using System;
using System.Collections;
using Lib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class GameController : MonoBehaviour
    {
        public GameObject escapeMenu;
        public GameObject levelCompleteMenu;
        public GameObject restartConfirmDialog;
    
        public bool canPauseSimulation = true;
        public int showRetryIntroAfter = 0;
        
        /// <summary>
        /// Current game state
        /// </summary>
        public GameState CurrentGameState { get; private set; } = GameState.InGame;

        /// <summary>
        /// Stores the gamestate prior to pausing by some kind of ui (other than scripted dialogs)
        /// </summary>
        private GameState _previousGameState = GameState.Invalid;

        public GameObject CurrentForegroundUi { get; set; } = null;

        /// <summary>
        /// Set to true to enable camera during intro dialogs
        /// </summary>
        public bool CameraTutorialOverwrite { get; set; } = false;

        /// <summary>
        /// Set to true to enable labyrinth controls during intro dialogs
        /// </summary>
        public bool LabyrinthTutorialOverwrite { get; set; } = false;
        
        [NonSerialized] public int score = 0;
        [NonSerialized] public GameResult gameResult = GameResult.Lost;

        public event Action<GameState> OnGameStateChangedListener;  

        public enum GameResult
        {
            Lost = 0, WonTier1 = 1, WonTier2 = 2, WonTier3 = 3
        }
    
        public enum GameState
        {
            Invalid, Loading, ShowingIntro, ShowingRetryIntro, InGame, InGamePaused, Intermission, GameOver, PausedUi
        }

        /// <summary>
        /// Common number used by target areas to spread heavy computing across frames
        /// </summary>
        public int DelayedUpdaters { get; set; }= 0;

        /// <summary>
        /// Stores the async load of the next level
        /// </summary>
        private AsyncOperation _asyncLoad = null;
        
    
        #region Singleton

        private static GameController _instance = null;

        public static GameController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameController>();
                }

                return _instance;
            }
        }

        public void OnCurrentForegroundUiClosed()
        {
            CurrentForegroundUi = null;
        }
    
        private void OnDestroy()
        {
            _instance = null;
        }

        private void OnApplicationQuit()
        {
            _instance = null;
        }

        #endregion

        private void Start()
        {
            // init default values
            escapeMenu.SetActive(false);
            
            // Do not show intro when restarting level
            if (SessionStorage.IsSceneRestart)
            {
                // show extra dialog when restarting multiple times and the level was not already beaten
                if (showRetryIntroAfter > 0 && SessionStorage.LevelRestartCounter >= showRetryIntroAfter && UserPreferences.GetLevelStars(Levels.Story.GetCurrentStoryLevelSceneNumber()) == 0)
                {
                    GoToState(GameState.ShowingRetryIntro);
                }
                else
                {
                    GoToState(GameState.InGame);
                }
            }
            else
            {
                SessionStorage.LevelRestartCounter = 0;
                GoToState(GameState.ShowingIntro);
            }
            
            SessionStorage.IsSceneRestart = false;
        }

        void Update()
        {
            if (!restartConfirmDialog.activeSelf && Input.GetAxis("Restart") > 0.2 && IsInGame() && CurrentForegroundUi == null)
            {
                GoToState(GameState.PausedUi);
                restartConfirmDialog.SetActive(true);
            }
        }

        public void UserPlayPause()
        {
            switch (CurrentGameState)
            {
                case GameState.InGame:
                    GoToState(GameState.InGamePaused);
                    break;
                case GameState.InGamePaused:
                    GoToState(GameState.InGame);
                    break;
            }
        }

        public void GoToState(GameState newState)
        {
            var oldState = CurrentGameState;
            CurrentGameState = newState;
            GpuQuantumSimulator.Instance.isRunning = IsSimulationRunning();
            switch (CurrentGameState)
            {
                case GameState.ShowingIntro:
                    break;
                case GameState.ShowingRetryIntro:
                    break;
                case GameState.InGame:
                    break;
                case GameState.InGamePaused:
                    break;
                case GameState.GameOver:
                    levelCompleteMenu.SetActive(true);
                    break;
                case GameState.Invalid:
                    throw new InvalidOperationException("Cannot go to state INVALID!");
                case GameState.Loading:
                    throw new NotImplementedException();
                case GameState.Intermission:
                    break;
                case GameState.PausedUi:
                    if (oldState != GameState.PausedUi) 
                        _previousGameState = oldState;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            OnGameStateChangedListener?.Invoke(CurrentGameState);
        }
        
        public void NextLevelAsync(bool allowActivation=true)
        {
            StartCoroutine(NextLevelAsyncImpl(allowActivation));
        }

        public IEnumerator NextLevelAsyncImpl(bool allowActivation=true)
        {
            _asyncLoad = SceneManager.LoadSceneAsync(Levels.Story.IsLastStoryLevel()
                ? Levels.HighscoreOverview
                : Levels.Story.GetNextStoryLevel());

            _asyncLoad.allowSceneActivation = allowActivation;
            
            while (!_asyncLoad.isDone)
            {
                yield return null;
            }
        }

        public void ActivatePreloadedLevel()
        {
            if (_asyncLoad != null)
            {
                _asyncLoad.allowSceneActivation = true;
            }
            else
            {
                Debug.LogWarning("Tried to activate a preloaded level, but no level got preloaded!");
            }
        }

        /// <summary>
        /// Toggles the pause menu
        /// </summary>
        public void HandleMenuKey()
        {
            if (DismissUi())
                return;
            
            
            ToggleExitMenu();
        }

        public bool DismissUi()
        {
            if (CurrentForegroundUi != null)
            {
                CurrentForegroundUi.SetActive(false);
                CurrentForegroundUi = null;
                return true;
            }

            return false;
        }

        public void ToggleExitMenu()
        {
            if (CurrentGameState == GameState.PausedUi)
            {
                escapeMenu.SetActive(false);
                restartConfirmDialog.SetActive(false);
                ReturnToPreviousGameState();
            }
            else
            {
                GoToState(GameState.PausedUi);
                escapeMenu.SetActive(true);
            }
        }

        public void ReturnToPreviousGameState()
        {
            GoToState(_previousGameState);
            _previousGameState = GameState.Invalid;
        }

        public void OnLevelComplete()
        {
            var level = Levels.Story.GetCurrentStoryLevelSceneNumber();
            UserPreferences.UpdateLevelStars(level, gameResult);
            UserPreferences.UpdateLevelHighscore(level, score);
            UserPreferences.Save();
            GoToState(GameState.GameOver);
            // Make sure escape menu is not active
            escapeMenu.SetActive(false);
        }

        public bool IsInGame() => CurrentGameState == GameState.InGame || CurrentGameState == GameState.InGamePaused;

        public bool CameraControlsEnabled() =>
            CurrentForegroundUi == null && (IsInGame() ||  CameraTutorialOverwrite);
        
        public bool LabyrinthControlsEnabled() =>
            CurrentForegroundUi == null && (IsInGame() || LabyrinthTutorialOverwrite);

        public bool InGameKeybindingsEnabled() => CurrentForegroundUi == null && IsInGame(); 

        public bool IsSimulationRunning()
        {
            return CurrentGameState == GameState.InGame;
        }

        public void RestartLevel()
        {
            SessionStorage.IsSceneRestart = true;
            SessionStorage.LevelRestartCounter++;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        
    }
}
