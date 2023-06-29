using UnityEngine;

namespace Game
{
    public class IntroController : MonoBehaviour
    {
        public GameObject dialog;
        
        void Start()
        {
            if (GameController.Instance.CurrentGameState != GameController.GameState.ShowingIntro)
            {
                gameObject.SetActive(false);
                return;
            }

            var animator = UnityEngine.Camera.main!.GetComponent<Animator>();
            animator.enabled = true;
            animator.Play("CameraLevelEnter");
        }
    }
}
