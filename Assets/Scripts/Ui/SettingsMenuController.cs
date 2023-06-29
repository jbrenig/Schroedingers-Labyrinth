using System;
using Game;
using Lib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Ui
{
    public class SettingsMenuController : MonoBehaviour
    {
        public Dropdown dropdownQuality;
        public Dropdown dropdownPrecisionControl;
        public Dropdown dropdownDynamicAtmosphere;

        public Button saveButton;

        private const int PrecisionControlOptionEnabled = 0;
        private const int PrecisionControlOptionDisabled = 1;
        private const int DynamicAtmosphereEnabled = 0;
        private const int DynamicAtmosphereDisabled = 1;

        private void OnEnable()
        {
            dropdownPrecisionControl.value = UserPreferences.NormalizeEveryFrame
                ? PrecisionControlOptionEnabled
                : PrecisionControlOptionDisabled;
            dropdownDynamicAtmosphere.value = UserPreferences.DisableDynamicAmbiance
                ? DynamicAtmosphereDisabled
                : DynamicAtmosphereEnabled;
            
            EventSystem.current.SetSelectedGameObject(saveButton.gameObject);
        }

        public void BtnSave()
        {
            switch (dropdownPrecisionControl.value)
            {
                case PrecisionControlOptionEnabled:
                    UserPreferences.NormalizeEveryFrame = true;
                    break;
                case PrecisionControlOptionDisabled:
                    UserPreferences.NormalizeEveryFrame = false;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
            
            switch (dropdownDynamicAtmosphere.value)
            {
                case PrecisionControlOptionEnabled:
                    UserPreferences.DisableDynamicAmbiance = false;
                    break;
                case PrecisionControlOptionDisabled:
                    UserPreferences.DisableDynamicAmbiance = true;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
            UserPreferences.Save();

            if (Levels.IsSimulationLevel())
            {
                // Gpu Simulator exists --> update values
                GpuQuantumSimulator.Instance.OnUserSettingsChanged();
            }
            gameObject.SetActive(false);
        }
    }
}
