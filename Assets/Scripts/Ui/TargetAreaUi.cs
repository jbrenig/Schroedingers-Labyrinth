using Game;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

namespace Ui
{
    [ExecuteAlways]
    public class TargetAreaUi : MonoBehaviour
    {
        public Text text;
        public Image progressBar;

        public TargetArea targetArea;

        private int _lastScore = -1;

        private const float AnimationTime = 0.5f;
        private float _currentAnimationTime = 0;
        
        public void Start()
        {
            _lastScore = -1;
            _currentAnimationTime = 0;
            progressBar.fillAmount = 0;
        }
        

        void Update()
        {
            int currentScore = targetArea.CurrentScore;

            // update text
            if (_lastScore != currentScore)
            {
                _lastScore = currentScore;
                text.text = targetArea.IsBonus2ScoreReached ? $"{currentScore}" : $"{currentScore}/{targetArea.NextScoreTarget}";
            }
            
            // update progressbar
            if (targetArea.scoringArea)
            {
                float currentProgress = targetArea.CurrentTimeProgress;
                progressBar.fillAmount = currentProgress;
            }
            else
            {
                if (targetArea.IsTargetScoreReached)
                {
                    _currentAnimationTime += Time.deltaTime;
                }
                else
                {
                    _currentAnimationTime -= Time.deltaTime;
                }
                
                _currentAnimationTime = Mathf.Clamp(_currentAnimationTime, 0, AnimationTime);
                var progress = _currentAnimationTime / AnimationTime;
                progressBar.fillAmount = Easing.InOutCirc(progress);
            }
        }
    }
}
