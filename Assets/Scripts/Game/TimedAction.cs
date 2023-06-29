using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class TimedAction : MonoBehaviour
    {

        public float time;
        public UnityEvent action;

        private void Start()
        {
            StartCoroutine(Time());
        }

        IEnumerator Time()
        {
            yield return new WaitForSeconds(time);
            action.Invoke();
        }
    }
}
