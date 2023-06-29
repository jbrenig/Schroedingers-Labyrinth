using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Ui
{
    public class GamepadAxisHotkey : MonoBehaviour
    {
        public string axis;

        /// <summary>
        /// Threshold after the hotkey will trigger.
        /// A negative threshold is passed by a smaller value, a positive threshold by a larger value.
        /// </summary>
        public float triggerThreshold = 0.8f;

        /// <summary>
        /// Threshold to reset to neutral
        /// </summary>
        public float resetThreshold = 0.4f;
        public float repeatTime = 0.5f;
        
        private Button _button;
        private float _lastValue = 0;
        private float _repeatCooldown = 0;

        private void Start()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            // Hack: Do not accept inputs directly after activating to prevent accidental triggers from previous ui. 
            _repeatCooldown = repeatTime;
        }

        void Update()
        {
            var newValue = Input.GetAxis(axis);

            if (Mathf.Abs(newValue) < resetThreshold)
            {
                // Reset
                _repeatCooldown = 0;
            }
            else if (_repeatCooldown > 0)
            {
                // Still on cooldown
                _repeatCooldown -= Time.deltaTime;
            }
            else if (triggerThreshold > 0 && newValue > triggerThreshold || triggerThreshold < 0 && newValue < triggerThreshold)
            {
                // trigger action
                
                _repeatCooldown = repeatTime;

                if (repeatTime > 0 || triggerThreshold > 0 && _lastValue < triggerThreshold || triggerThreshold < 0 && _lastValue > triggerThreshold)
                {
                    // repeats are allowed or new activation
                    _button.onClick.Invoke();
                }
            }

            _lastValue = newValue;

        }
    }
}