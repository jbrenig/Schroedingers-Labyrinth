using System;
using Game;
using UnityEngine;
using UnityEngine.UI;

namespace Ui
{
    /// <summary>
    /// Ui Controller for Time Control buttons. Play/Pause/FastForward
    /// </summary>
    public class TimeControllerUi : MonoBehaviour
    {
        public const int FastForwardTimeStepMultiplier = 2;
        
        public Button btnPause; 
        public Button btnPlay; 
        public Button btnFastForward;
        
        private bool _lastIsRunning = false;
        private bool _isFastForwarding = false;

        private bool _isTemporaryFastForwarding = false;
        private bool _submitButtonDown = true;

        private void OnEnable()
        {
            // Hack to prevent accidental ui triggering hotkeys
            _submitButtonDown = true;
        }

        void Update()
        {
            if (_lastIsRunning != GameController.Instance.IsSimulationRunning())
            {
                _lastIsRunning = GameController.Instance.IsSimulationRunning();
                UpdateBtnStates();
            }

            if (GameController.Instance.InGameKeybindingsEnabled() && GameController.Instance.canPauseSimulation)
            {
                if (!_submitButtonDown && Input.GetAxis("Submit") > 0.4f)
                {
                    _submitButtonDown = true;
                    if (_lastIsRunning)
                    {
                        BtnPause();
                    }
                    else
                    {
                        BtnPlay();
                    }
                } 
                else if (_submitButtonDown && Input.GetAxis("Submit") < 0.2f)
                {
                    _submitButtonDown = false;
                }

                if (Input.GetAxis("Fast Forward") > 0.4f)
                {
                    if (!_isTemporaryFastForwarding)
                    {
                        if (_lastIsRunning && _isFastForwarding)
                        {
                            BtnPlay();
                        }
                        else if (_lastIsRunning)
                        {
                            BtnFastForward();
                        }

                        _isTemporaryFastForwarding = true;
                    }
                } 
                else if (_isTemporaryFastForwarding)
                {
                    // Switch back
                    if (_lastIsRunning && _isFastForwarding)
                    {
                        BtnPlay();
                    }
                    else if (_lastIsRunning)
                    {
                        BtnFastForward();
                    }
                    _isTemporaryFastForwarding = false;
                }
            }
            else
            {
                // Hack: reset submit button to prevent accidential trigger after ui
                _submitButtonDown = true;
            }
        }

        private void UpdateBtnStates()
        {
            btnPause.interactable = _lastIsRunning; // only interactable if is running
            btnPlay.interactable = !_lastIsRunning || _isFastForwarding; // only interactable if is not running or is fast forwarding
            btnFastForward.interactable = !_lastIsRunning || !_isFastForwarding; // only interactable if not fast forwarding or paused
        }

        public void BtnPause()
        {
            GameController.Instance.GoToState(GameController.GameState.InGamePaused);
            _lastIsRunning = false;
            _isFastForwarding = false;
            GpuQuantumSimulator.Instance.iterationMultiplier = 1;
            GpuQuantumSimulator.Instance.showPhase = true;
            UpdateBtnStates();
        }
        
        public void BtnPlay()
        {
            GameController.Instance.GoToState(GameController.GameState.InGame);
            _lastIsRunning = true;
            _isFastForwarding = false;
            GpuQuantumSimulator.Instance.iterationMultiplier = 1;
            GpuQuantumSimulator.Instance.showPhase = true;
            UpdateBtnStates();
        }

        public void BtnFastForward()
        {
            GameController.Instance.GoToState(GameController.GameState.InGame);
            _lastIsRunning = true;
            _isFastForwarding = true;
            GpuQuantumSimulator.Instance.iterationMultiplier = FastForwardTimeStepMultiplier;
            GpuQuantumSimulator.Instance.showPhase = false;
            UpdateBtnStates();
        }
    }
}
