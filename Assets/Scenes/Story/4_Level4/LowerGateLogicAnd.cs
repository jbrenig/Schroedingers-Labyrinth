using System.Collections;
using Game;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scenes.Story._4_Level4
{
    public class LowerGateLogicAnd : MonoBehaviour
    {
        /// <summary>
        /// Time it take for the camera enable animation
        /// </summary>
        public float enableTime = .5f;
        public float measureAfter = 2;

        public float rotationAmount = -60;
        
        public TargetArea area1;
        public TargetArea area2;

        public GameObject targetGate;
        public GameObject newScoreZone;

        public GameObject dialog;

        public CameraReturnAnimation[] cameras;
        public GameObject[] observationAnimations;
        public GameObject[] scannerCompleteAnimations;

        public Texture observationZone;

        public AudioClip cameraEnableSound;
        public AudioClip cameraMeasureSound;
        public AudioSource audioSource;
        
        private bool _goalReached = false;
    
        private void Start()
        {
            _goalReached = false;
            newScoreZone.SetActive(false);
        }

        IEnumerator OnScoreReached()
        {
            foreach (var cameraToRotate in cameras)
            {
                cameraToRotate.animationTimeEnable = enableTime;
                cameraToRotate.DoEnableAnimation();
            }

            audioSource.PlayOneShot(cameraEnableSound, .8f);
            
            yield return new WaitForSeconds(enableTime);
            GameController.Instance.GoToState(GameController.GameState.Intermission);
            dialog.SetActive(true);
        }
        
        IEnumerator AfterDialogComplete()
        {
            GameController.Instance.GoToState(GameController.GameState.InGame);
            newScoreZone.SetActive(true);
            foreach (var a in observationAnimations)
            {
                a.SetActive(true);
            }
            yield return new WaitForSeconds(measureAfter);
            DoObservation();
            audioSource.PlayOneShot(cameraMeasureSound);
            foreach (var scannerCompleteAnimation in scannerCompleteAnimations)
            {
                scannerCompleteAnimation.SetActive(true);
            }
            yield return null;
        }

        public void OnDialogComplete()
        {
            StartCoroutine(nameof(AfterDialogComplete));
        }

        void Update()
        {
#if UNITY_EDITOR || ENABLE_CHEATS
            if (!_goalReached && area1.IsTargetScoreReached && area2.IsTargetScoreReached || !_goalReached && Input.GetKey("t"))
#else
            if (!_goalReached && area1.IsTargetScoreReached && area2.IsTargetScoreReached)
#endif
            {
                _goalReached = true;
                targetGate.SetActive(false);
                area1.DisableOnComplete();
                area2.DisableOnComplete();

                StartCoroutine(nameof(OnScoreReached));
            }
        }

        public void DoObservation()
        {
            float randVal = Random.value;
            float probabilities = GpuQuantumSimulator.Instance.SampleProbabilities(observationZone);
            bool invert = randVal > probabilities;
            GpuQuantumSimulator.Instance.ApplyMask(observationZone, invert);
            GpuQuantumSimulator.Instance.Normalize();
        }
    }
}
