using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Ui
{
    public class ButtonHotkey : MonoBehaviour
    {
        public KeyCode[] keys;

        private bool[] _isPressed;
        private Button _button;


        private void Awake()
        {
            _button = GetComponent<Button>();
            _isPressed = new bool[keys.Length];
        }

        void Update()
        {
            for (var i = 0; i < keys.Length; i++)
            {
                var key = keys[i];
                if (Input.GetKeyUp(key))
                {
                    if (_isPressed[i])
                    {
                        _button.onClick.Invoke();
                        Array.Clear(_isPressed, 0, _isPressed.Length);
                        return;
                    }

                    _isPressed[i] = false;
                }
                else if (Input.GetKeyDown(key))
                {
                    _isPressed[i] = true;
                }
            }
        }
    }
}