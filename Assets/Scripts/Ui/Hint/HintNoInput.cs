using Lib;
using UnityEngine;

namespace Ui
{
    public class HintNoInput : MonoBehaviour
    {
        public float showTime = 2;
        public GameObject target;
        public double simulationTimePassed = 1 * Constants.Scale.Pico;

        private bool _completed = false;
        private Animator _animator;
        
        private static readonly int StartTrigger = Animator.StringToHash("Show");
        private static readonly int HideTrigger = Animator.StringToHash("Hide");

        private void Start()
        {
            _completed = false;
            target.SetActive(false);
            _animator = target.GetComponent<Animator>();
        }

        
        /// <summary>
        /// Very inefficient way of checking whether the use tries to play the game
        /// </summary>
        /// <returns></returns>
        bool CheckForInputs()
        {
            return Input.GetKey(KeyCode.RightArrow)
                   || Input.GetKey(KeyCode.LeftArrow)
                   || Input.GetKey(KeyCode.UpArrow)
                   || Input.GetKey(KeyCode.DownArrow)
                   || (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1)
                   || (Mathf.Abs(Input.GetAxis("Vertical")) > 0.1);
        }

        void Update()
        {
            if (!_completed && (CheckForInputs()) && GpuQuantumSimulator.Instance.GetTimePassed() > 0.1*simulationTimePassed)
            {
                _completed = true;
                gameObject.SetActive(false);
            }
            
            if (!_completed && GpuQuantumSimulator.Instance.GetTimePassed() > simulationTimePassed)
            {
                _completed = true;
                target.SetActive(true);
                _animator.SetTrigger(StartTrigger);
                Invoke(nameof(Hide), showTime);
            }
        }

        void Hide()
        {
            _animator.SetTrigger(HideTrigger);
        }
    }
}