using Game;
using UnityEngine;

namespace Ui
{
    public class ScoreStarAddon : MonoBehaviour
    {
        public ScoreStarUi star1;
        public ScoreStarUi star2;
        public ScoreStarUi star3;

        private TargetArea _targetArea;
    
        void Start()
        {
            star1.SetVisibility(ScoreStarUi.Visibility.None);
            star2.SetVisibility(ScoreStarUi.Visibility.None);
            star3.SetVisibility(ScoreStarUi.Visibility.None);
            _targetArea = GetComponentInParent<TargetArea>();
            _targetArea.onScoreTargetReached.AddListener(arg0 => star1.SetVisibility(ScoreStarUi.Visibility.Enabled));
            _targetArea.onScoreBonusReached1.AddListener(arg0 => star2.SetVisibility(ScoreStarUi.Visibility.Enabled));
            _targetArea.onScoreBonusReached2.AddListener(arg0 => star3.SetVisibility(ScoreStarUi.Visibility.Enabled));
        }

        private void Update()
        {
            if (!_targetArea.IsCurrentlyScoring)
            {
                // Show unused stars
                star1.SetVisibility(_targetArea.IsTargetScoreReached
                    ? ScoreStarUi.Visibility.Disabled
                    : ScoreStarUi.Visibility.None);
                star2.SetVisibility(_targetArea.IsBonus1ScoreReached
                    ? ScoreStarUi.Visibility.Disabled
                    : ScoreStarUi.Visibility.None);
                star3.SetVisibility(_targetArea.IsBonus2ScoreReached
                    ? ScoreStarUi.Visibility.Disabled
                    : ScoreStarUi.Visibility.None);
            }
        }
    }
}
