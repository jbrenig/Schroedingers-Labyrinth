using Game;
using UnityEngine;
using UnityEngine.UI;

namespace Ui
{
    public class ScoreTextReplacer : MonoBehaviour
    {
        private void OnEnable()
        {
            var text = GetComponent<Text>();
            var newText = text.text.Replace("$score", GameController.Instance.score.ToString());
            text.text = newText;
        }
    }
}
