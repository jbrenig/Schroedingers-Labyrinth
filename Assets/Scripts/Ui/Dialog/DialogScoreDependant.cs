using Game;
using UnityEngine;

namespace Ui.Dialog
{
    public class DialogScoreDependant : MonoBehaviour, DialogController.IDialogPage
    {
        public AudioClip audioClip;
        public int minScore = 50;
        public int maxScore = 100;
            
        public int pageNumber;

        public bool enableCameraControls = false;
        public bool enableLabyrinthControls = false;
        
        public int GetPageNumber() => pageNumber;

        public void SetPageActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public bool IsPageValid()
        {
            var score = GameController.Instance.score;
            return score >= minScore && score <= maxScore;
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
