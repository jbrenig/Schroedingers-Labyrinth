using Lib;
using UnityEngine;

namespace Ui
{
    public class HintController : MonoBehaviour
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

        void Update()
        {
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
