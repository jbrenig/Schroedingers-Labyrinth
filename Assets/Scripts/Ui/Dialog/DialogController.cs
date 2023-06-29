using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Ui.Dialog
{
    [ExecuteAlways]
    public class DialogController : MonoBehaviour
    {
        public Text btnContinueText;
        public Button btnPrevious;
        
        public GameObject pageHolder;

        public string continueText = "Continue";
        public string finishText = "Start";

        public GameController.GameState validOnlyInGameState = GameController.GameState.ShowingIntro;
        public UnityEvent onDialogComplete;

        public bool closeDialogOnFinish = true;
        public bool keyboardControlEnabled = true;

        public AudioSource audioSource;

        private int _currentPage = 0;

        private bool isFirstUpdate = true;

        public interface IDialogPage
        {
            int GetPageNumber();
            void SetPageActive(bool active);
        
            /// <returns>true if the page can be shown. </returns>
            bool IsPageValid();

            AudioClip GetAudio();

            bool EnableCameraControls();
            bool EnableLabyrinthControls();
        }

        private readonly List<IDialogPage> _pages = new List<IDialogPage>();

        public void GoToState(string stateName)
        {
            if (!Enum.TryParse(stateName, true, out GameController.GameState state))
            {
                throw new InvalidOperationException();
            }
            GameController.Instance.GoToState(state);
        }

        public void ReturnToGame()
        {
            GameController.Instance.GoToState(GameController.GameState.InGame);
        }

        public void Start()
        {
            _pages.Clear();
            var candidatePages = pageHolder.GetComponentsInChildren<IDialogPage>(true);
            foreach (var page in candidatePages)
            {
                page.SetPageActive(false);
            }
            
            // only filter when playing, since gamestate is not available in editor
#if UNITY_EDITOR
            _pages.AddRange(Application.isPlaying
                ? candidatePages.Where(page => page.IsPageValid())
                : candidatePages);
#else
            _pages.AddRange(candidatePages.Where(page => page.IsPageValid()));
#endif
            _pages.Sort((p1, p2) => p1.GetPageNumber().CompareTo(p2.GetPageNumber()));
            for (var index = 0; index < _pages.Count; index++)
            {
                var page = _pages[index];
                page.SetPageActive(_currentPage == index);
            }

            UpdateButtonUiState();

#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            gameObject.SetActive(GameController.Instance.CurrentGameState == validOnlyInGameState);
        }

        private void UpdateButtonUiState()
        {
            btnContinueText.text = _currentPage >= _pages.Count - 1 ? finishText : continueText;
            btnPrevious.interactable = _currentPage > 0;

            if (_currentPage > _pages.Count - 1)
            {
                GameController.Instance.CameraTutorialOverwrite = false;
                GameController.Instance.LabyrinthTutorialOverwrite = false;
            }
            else
            {
                GameController.Instance.CameraTutorialOverwrite = _pages[_currentPage].EnableCameraControls();
                GameController.Instance.LabyrinthTutorialOverwrite = _pages[_currentPage].EnableLabyrinthControls();
            }
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            if (isFirstUpdate)
            {
                isFirstUpdate = false;
                PlayDialogAudio();
            }
            if(!keyboardControlEnabled) return;
            
            if (Input.GetKeyDown(KeyCode.Return))
            {
                BtnContinue();
            }
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                BtnPrevious();
            }
        }

        public void BtnSkip()
        {
            onDialogComplete.Invoke();
            if (closeDialogOnFinish)
            {
                gameObject.SetActive(false);
            }
            else
            {
                // deactivate all controls
                keyboardControlEnabled = false;
                foreach (var button in GetComponentsInChildren<Button>())
                {
                    button.interactable = false;
                }
            }
        }

        public void BtnContinue()
        {
#if UNITY_EDITOR

            if (!Application.isPlaying && _currentPage + 1 >= _pages.Count)
            {
                return;
            }
#endif
            _currentPage++;
            UpdateButtonUiState();
            if (_currentPage >= _pages.Count)
            {
                onDialogComplete.Invoke();
                if (closeDialogOnFinish)
                {
                    gameObject.SetActive(false);
                }
                else
                {
                    // deactivate all controls
                    keyboardControlEnabled = false;
                    foreach (var button in GetComponentsInChildren<Button>())
                    {
                        button.interactable = false;
                    }
                }
            }
            else
            {
                _pages[_currentPage - 1].SetPageActive(false);
                _pages[_currentPage].SetPageActive(true);
                PlayDialogAudio();
            }
        }

        public void BtnPrevious()
        {
            _currentPage--;
            if (_currentPage < 0)
            {
                _currentPage = 0;
            }
            else
            {
                _pages[_currentPage + 1].SetPageActive(false);
                _pages[_currentPage].SetPageActive(true);
                UpdateButtonUiState();
                PlayDialogAudio();
            }
        }

        private void PlayDialogAudio()
        {
            var page = _pages[_currentPage];
            var audio = page.GetAudio();
            
            if (audioSource.isPlaying)
            {
                // TODO maybe duck out of old dialog
                audioSource.Stop();
            }

            if (audio != null)
            {
                audioSource.clip = audio;
                audioSource.Play();
            }
        }
    }
}