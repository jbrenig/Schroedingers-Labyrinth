using Game;
using Lib;
using UnityEngine;
using UnityEngine.UI;

namespace Ui
{
    public class TextFillerFullScore : MonoBehaviour
    {
        private void OnEnable()
        {
            float completeScore = 1f;
            foreach (var level in Levels.Story.LevelList)
            {
                var score = UserPreferences.GetLevelHighscore(Levels.Story.GetStoryLevelSceneNumber(level)) / 100f;
                if (score > 0)
                {
                    completeScore *= score;
                }
            }

            var text = GetComponent<Text>();
            var newText = text.text.Replace("$fullscore", $"{completeScore * 100:F}");
            newText = newText.Replace("$chance", $"{1f / completeScore:F0}");
            text.text = newText;
        }
    }
}
