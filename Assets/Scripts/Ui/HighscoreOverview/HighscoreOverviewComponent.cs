using Game;
using Lib;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Ui
{
    public class HighscoreOverviewComponent : MonoBehaviour
    {
        [FormerlySerializedAs("level")] public int levelIndex;

        public Text txtLevel;
        public Text txtScore;

        public ScoreStarUi star1;
        public ScoreStarUi star2;
        public ScoreStarUi star3;

        void Start()
        {
            var levelSceneNumber = Levels.Story.GetStoryLevelSceneNumber(levelIndex);
            txtLevel.text = levelSceneNumber == 0 ? "Intro" : "Level " + levelIndex;
            txtScore.text = UserPreferences.GetLevelHighscore(levelSceneNumber) + " points";
            var stars = UserPreferences.GetLevelStars(levelSceneNumber);
            star1.SetVisibility(stars > 0 ? ScoreStarUi.Visibility.Enabled : ScoreStarUi.Visibility.Disabled);
            star2.SetVisibility(stars > 1 ? ScoreStarUi.Visibility.Enabled : ScoreStarUi.Visibility.Disabled);
            star3.SetVisibility(stars > 2 ? ScoreStarUi.Visibility.Enabled : ScoreStarUi.Visibility.Disabled);
        }

        public void BtnRetry()
        {
            SceneManager.LoadSceneAsync(Levels.Story.GetStoryLevel(levelIndex));
        }
    }
}
