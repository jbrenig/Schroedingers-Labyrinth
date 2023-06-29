using System;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class TargetArea : MonoBehaviour
    {
    
        public float likelihoodMultiplier = 100;
        public float targetScore = 50;
        public float bonusScore1 = 50;
        public float bonusScore2 = 50;
    
    
        public float bonusTime = 2;
    
        /// <summary>
        /// set to true to enable scoring functionality such as bonus score. Otherwise the area will just function as a measurement tool.
        /// </summary>
        public bool scoringArea = true;

        /// <summary>
        /// set to true to update the gamecontroller score values
        /// </summary>
        public bool publishScore = true;
    
        public UnityEvent<int> onScoreComplete;
        public UnityEvent<int> onScoreTargetReached;
        public UnityEvent<int> onScoreBonusReached1;
        public UnityEvent<int> onScoreBonusReached2;

        public bool multiZoneScoringEnabled = false;
        
        /// <summary>
        /// Set if multiple scorezones need to be fullfilled.
        /// Ignored if scoring area is false. Ignored if multiZone score is disabled.
        ///
        /// Will use minimum of both scores and only start counting towards bonus score, when both target scores are reached. 
        /// </summary>
        public TargetArea multiScoreZone = null;

        public Texture area;

        private State _state = State.Normal;
        private float _likelihood = 0;
        private float _currentBonusTime = 0;

        public int CurrentScore => (int) (_likelihood * likelihoodMultiplier);

        public bool IsTargetScoreReached => CurrentScore >= targetScore;
        public bool IsBonus1ScoreReached => CurrentScore >= bonusScore1;
        public bool IsBonus2ScoreReached => CurrentScore >= bonusScore2;

        public float NextScoreTarget
        {
            get
            {
                var current = CurrentScore;
                if (current < targetScore) return targetScore;
                if (current < bonusScore1) return bonusScore1;
                return bonusScore2;
            }
        }

        public bool IsCurrentlyScoring => _state != State.Normal;
        
        public float CurrentLikelihood => _likelihood;
        public float CurrentTimeProgress => bonusTime > 0 ? (_currentBonusTime / bonusTime) : (_state == State.Finished ? 1 : 0);

        private int _frameUpdateOffset = 0;

        private enum State
        {
            Normal, Bonus1, Bonus2, Bonus3, Finished
        }

        private void Start()
        {
            _state = State.Normal;
            _likelihood = 0;
            _currentBonusTime = 0;

            if (multiZoneScoringEnabled)
            {
                // Make sure other zone has correct parameters
                multiScoreZone.multiZoneScoringEnabled = true;
                multiScoreZone.multiScoreZone = this;
                multiScoreZone.bonusTime = bonusTime;
            }

            _frameUpdateOffset = GameController.Instance.DelayedUpdaters;
            GameController.Instance.DelayedUpdaters++;
        }

        private void OnValidate()
        {
            if (multiZoneScoringEnabled && multiScoreZone == null) throw new InvalidOperationException();
        }

        void Update()
        {
            if (_state == State.Finished) return;
            if (!GameController.Instance.IsSimulationRunning()) return;

            if (_state != State.Normal)
            {
                // advance bonus time when in any bonus field
                _currentBonusTime += Time.deltaTime;
            }
            
            // skip every x frames to allow other target areas to do their updates (spreading out the work)
            if (Time.frameCount % GameController.Instance.DelayedUpdaters != _frameUpdateOffset) return;
        
            var currentLikelihood = GpuQuantumSimulator.Instance.SampleProbabilities(area);
            switch (_state)
            {
                case State.Normal:
                    _likelihood = currentLikelihood;
                    if (scoringArea && CurrentScore >= targetScore)
                    {
                        if (multiZoneScoringEnabled && !multiScoreZone.IsTargetScoreReached)
                        {
                            // do not score if the other score is not reached.
                            break;
                        }
                        _state = State.Bonus1;
                        onScoreTargetReached?.Invoke(CurrentScore);
                    }
                    break;
                // TODO: refactor the following to be less copy paste
                case State.Bonus1:
                    if (_currentBonusTime >= bonusTime)
                    {
                        Finish();
                    }
                    else
                    {
                        _likelihood = Mathf.Max(currentLikelihood, _likelihood);
                        if (CurrentScore >= bonusScore1)
                        {
                            if (multiZoneScoringEnabled && !multiScoreZone.IsBonus1ScoreReached)
                            {
                                // do not score if the other bonus score is not reached.
                                break;
                            }
                            onScoreBonusReached1.Invoke(CurrentScore);
                            _state = State.Bonus2;
                        }
                    }
                    break;
                case State.Bonus2:
                    if (_currentBonusTime >= bonusTime)
                    {
                        Finish();
                    }
                    else
                    {
                        _likelihood = Mathf.Max(currentLikelihood, _likelihood);
                        if (CurrentScore >= bonusScore2)
                        {
                            if (multiZoneScoringEnabled && !multiScoreZone.IsBonus2ScoreReached)
                            {
                                // do not score if the other bonus score is not reached.
                                break;
                            }
                            onScoreBonusReached2.Invoke(CurrentScore);
                            _state = State.Bonus3;
                        }
                    }
                    break;
                case State.Bonus3:
                    if (_currentBonusTime >= bonusTime)
                    {
                        Finish();
                    }
                    else
                    {
                        _likelihood = Mathf.Max(currentLikelihood, _likelihood);
                    }
                    break;
                case State.Finished:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Finish()
        {
            _state = State.Finished;
            
            var achievedScore = multiZoneScoringEnabled ? Mathf.Min(CurrentScore, multiScoreZone.CurrentScore) : CurrentScore;
            if (multiZoneScoringEnabled)
            {
                // deactivate other score zone
                multiScoreZone._state = State.Finished;
                multiScoreZone.onScoreComplete?.Invoke(achievedScore);
            }
            
            onScoreComplete?.Invoke(achievedScore);
            if (publishScore)
            {
                GameController.Instance.score = achievedScore;
                if (achievedScore >= bonusScore2)
                {
                    GameController.Instance.gameResult = GameController.GameResult.WonTier3;
                } 
                else if (achievedScore >= bonusScore1)
                {
                    GameController.Instance.gameResult = GameController.GameResult.WonTier2;
                }
                else if (achievedScore >= targetScore)
                {
                    GameController.Instance.gameResult = GameController.GameResult.WonTier1;
                } 
                else
                {
                    GameController.Instance.gameResult = GameController.GameResult.Lost;
                }
                GameController.Instance.OnLevelComplete();
            }
        }
    
        public void SetAreaTexture(Texture texture)
        {
            area = texture;
        }

        /// <summary>
        /// Stop scoring, trigger exit animation and disable area after animation completion.  
        /// </summary>
        public void DisableOnComplete()
        {
            var anim = GetComponent<Animator>();
            anim.Play("zone_complete");
        }
    }
}
