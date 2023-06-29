using Game;
using UnityEngine;

namespace Camera
{
    public class CameraAnimationEventHandler : MonoBehaviour
    {
        private Animator _animator;
        private AsyncOperation _nextLevelLoad;
        
        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void TriggerNextLevel()
        {
            GameController.Instance.NextLevelAsync(allowActivation: false);
        }

        public void ActivatePreloadedLevel()
        {
            GameController.Instance.ActivatePreloadedLevel();
        }

        public void FreeCameraFromAnimation()
        {
            _animator.enabled = false;
        }
    }
}
