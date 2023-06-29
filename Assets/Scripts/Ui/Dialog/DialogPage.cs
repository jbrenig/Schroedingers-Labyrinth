using UnityEngine;

namespace Ui.Dialog
{
    public class DialogPage : MonoBehaviour, DialogController.IDialogPage
    {
        public AudioClip audioClip;
        public int page;

        public bool enableCameraControls = false;
        public bool enableLabyrinthControls = false;
    
        public int GetPageNumber()
        {
            return page;
        }

        public void SetPageActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public bool IsPageValid()
        {
            return true;
        }

        public AudioClip GetAudio()
        {
            return audioClip;
        }

        public bool EnableCameraControls()
        {
            return enableCameraControls;
        }

        public bool EnableLabyrinthControls()
        {
            return enableLabyrinthControls;
        }
    }
}
