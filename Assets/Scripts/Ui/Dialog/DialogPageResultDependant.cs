using System.Linq;
using Game;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ui.Dialog
{
    public class DialogPageResultDependant : MonoBehaviour, DialogController.IDialogPage
    {
        public AudioClip audioClip;
        public int minScore = 0;
        public int maxScore = 100;
        public GameController.GameResult[] allowedResults; 
            
        public bool enableCameraControls = false;
        public bool enableLabyrinthControls = false;
        
        [FormerlySerializedAs("pageNumber")] public int pagePriority;

        public int GetPageNumber() => pagePriority;

        public void SetPageActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public bool IsPageValid()
        {
            var score = GameController.Instance.score;
            return allowedResults.Contains(GameController.Instance.gameResult) && score >= minScore && score <= maxScore;
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
