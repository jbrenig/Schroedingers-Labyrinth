using Game;
using Lib;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Ui
{
    public class UserInterface : MonoBehaviour
    {
    
        public GpuQuantumSimulator simulator;

        private bool _menuKeyDown = true;
        
        public void BtnRestartLevel()
        {
            GameController.Instance.CurrentForegroundUi = GameController.Instance.restartConfirmDialog;
            GameController.Instance.restartConfirmDialog.SetActive(true);
        }

        public void BtnQuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }

        public void BtnMainMenu()
        {
            SceneManager.LoadScene(Levels.MainMenu);
        }

        public void BtnResume()
        {
            GameController.Instance.ToggleExitMenu();
        }

        public void BtnPlayPause()
        {
            GameController.Instance.UserPlayPause();
        }

        public void BtnSkipLevel()
        {
            GameController.Instance.score = 0;
            GameController.Instance.gameResult = GameController.GameResult.Lost;
            
            GameController.Instance.OnLevelComplete();
        }

        // Update is called once per frame
        void Update()
        {
            if (!_menuKeyDown && Input.GetAxis("Menu") > 0.1f)
            {
                _menuKeyDown = true;
                GameController.Instance.HandleMenuKey();
            }
            else if (Input.GetAxis("Menu") < 0.1f)
            {
                _menuKeyDown = false;
            }

#if UNITY_EDITOR || ENABLE_CHEATS
            if (Input.GetKeyDown(KeyCode.L))
            {
                // finish level with current gameresult
                GameController.Instance.OnLevelComplete();
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                // finish level with 3 star game result
                GameController.Instance.gameResult = GameController.GameResult.WonTier3;
                GameController.Instance.score = 50;
                GameController.Instance.OnLevelComplete();
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                // clear user score of the current level
                var level = Levels.Story.GetCurrentStoryLevelSceneNumber();
                UserPreferences.ClearLevelHighscore(level);
                UserPreferences.Save();
            }
#endif
            // txtTimePassed.text = "Time passed: " + TimeUtils.TimeToUnitsString(simulator.GetTimePassed());
        }
    }
}
