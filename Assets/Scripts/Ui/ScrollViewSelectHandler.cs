using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Ui
{
    public class ScrollViewSelectHandler : MonoBehaviour, ISelectHandler
    {

        public GameObject target;
        private ScrollRect _scrollRect;
        private RectTransform _rectTransform;
    
        void Start()
        {
            _scrollRect = GetComponentInParent<ScrollRect>();
            if (target == null)
                _rectTransform = GetComponent<RectTransform>();
            else
                _rectTransform = target.GetComponent<RectTransform>();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void OnSelect(BaseEventData eventData)
        {
            _scrollRect.BringChildIntoView(_rectTransform);
        }
    }
}
