using System.Collections;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

namespace Ui.Animation
{
    public class SlideInPanel : MonoBehaviour
    {
        public int startY;

        public float animationOffset = 0;
        public float animationDuration = 0.6F;
        
        private float _targetY;

        private void Awake()
        {
            var rectTransform = GetComponent<RectTransform>();
            // get current position and offset position
            var startPosition = rectTransform.anchoredPosition;
            _targetY = startPosition.y;
        }

        void OnEnable()
        {
            StartCoroutine(Animate());
        }

        IEnumerator Animate()
        {
            var rectTransform = GetComponent<RectTransform>();
            // get current position and offset position
            var startPosition = rectTransform.anchoredPosition;
            startPosition.y = startY;
            rectTransform.anchoredPosition = startPosition;

            yield return new WaitForSeconds(animationOffset);
            
            float timePassed = 0f;
            while (timePassed < animationDuration)
            {
                timePassed += Time.deltaTime;
                var progress = Easing.OutBounce(timePassed / animationDuration);
                var position = rectTransform.anchoredPosition;
                position.y = Mathf.Lerp(startY, _targetY, progress);
                rectTransform.anchoredPosition = position;
                yield return null;
            }
        }
    }
}
