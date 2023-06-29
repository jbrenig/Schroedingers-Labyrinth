using System;
using Game;
using Lib;
using UnityEngine;
using UnityEngine.UI;

namespace Ui
{
    public class LevelCompleteUi : MonoBehaviour
    {
        public ScoreStarUi star1;
        public ScoreStarUi star2;
        public ScoreStarUi star3;

        public Text scoreText;

        public Text titleText;

        private void OnEnable()
        {
            if (GameController.Instance.CurrentGameState != GameController.GameState.GameOver)
            {
                // do not enable when the game is not finished yet.
                gameObject.SetActive(false);
                return;
            }
            
            switch (GameController.Instance.gameResult)
            {
                case GameController.GameResult.Lost:
                    star1.SetVisibility(ScoreStarUi.Visibility.None);
                    star2.SetVisibility(ScoreStarUi.Visibility.None);
                    star3.SetVisibility(ScoreStarUi.Visibility.None);
                    break;
                case GameController.GameResult.WonTier1:
                    star1.SetVisibility(ScoreStarUi.Visibility.Enabled);
                    star2.SetVisibility(ScoreStarUi.Visibility.Disabled);
                    star3.SetVisibility(ScoreStarUi.Visibility.Disabled);
                    break;
                case GameController.GameResult.WonTier2:
                    star1.SetVisibility(ScoreStarUi.Visibility.Enabled);
                    star2.SetVisibility(ScoreStarUi.Visibility.Enabled);
                    star3.SetVisibility(ScoreStarUi.Visibility.Disabled);
                    break;
                case GameController.GameResult.WonTier3:
                    star1.SetVisibility(ScoreStarUi.Visibility.Enabled);
                    star2.SetVisibility(ScoreStarUi.Visibility.Enabled);
                    star3.SetVisibility(ScoreStarUi.Visibility.Enabled);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // TODO better scoring
            scoreText.text = GameController.Instance.score + " percent";

            var level = Levels.Story.GetCurrentStoryLevelIndex();
            titleText.text = level == 0 ? "Intro Complete!" : $"Level {level} Complete!";
        }


        public void NextLevelNoAnim()
        {
            GameController.Instance.NextLevelAsync();
        }
        
        public void NextLevel()
        {
            var animator = UnityEngine.Camera.main!.GetComponent<Animator>();
            animator.enabled = true;
            animator.Play("CameraLevelLeave");
        }
    }
}
