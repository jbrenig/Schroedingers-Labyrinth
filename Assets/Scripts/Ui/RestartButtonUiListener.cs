using Game;
using UnityEngine;
using UnityEngine.UI;

namespace Ui
{
    /// <summary>
    /// Add this script to a button that should restart the level on click
    /// </summary>
    public class RestartButtonUiListener : MonoBehaviour
    {
        void Start()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(BtnRestartLevel);
        }

        public void BtnRestartLevel()
        {
            GameController.Instance.RestartLevel();
        }
    }
}
