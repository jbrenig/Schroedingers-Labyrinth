using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Ui
{
    public class ControllerButtonHintUpdater : MonoBehaviour
    {
        public Vector3 offset;
        public UIBehaviour controllerHintGraphic;

        private RectTransform _transform;
        private bool active = false;
        private GameObject lastSelected = null;

        private int _ticksSinceInit = 0;
    
        // Start is called before the first frame update
        void Start()
        {
            controllerHintGraphic.enabled = this.active;
            _transform = controllerHintGraphic.GetComponent<RectTransform>();
            _ticksSinceInit = 0;
        }


        private void updatePosition(GameObject newSelection)
        {
            var btnRect = newSelection.GetComponent<RectTransform>();
                        
            var corners = new Vector3[4];
            btnRect.GetWorldCorners(corners);

            var leftEdge = (corners[0] + corners[1]) / 2;
                        
            this._transform.position = leftEdge + offset;
        }
        
        // Update is called once per frame
        void Update()
        {
            if (_ticksSinceInit < 4)
            {
                // Hack to give ui time to init first
                _ticksSinceInit++;
                return;
            } 
            
            // TODO: find a better way of handling changes in input
            var newActive = Input.GetJoystickNames().Length > 0;

            if (newActive)
            {
                // Now move to the location
                var selected = EventSystem.current.currentSelectedGameObject;
                if (selected == null)
                {
                    this.lastSelected = null;
                    newActive = false;
                }
                else
                {
                    var btnComponent = selected.GetComponent<Button>();
                    if (btnComponent == null)
                    {
                        newActive = false;
                    }
                    else if (selected != lastSelected)
                    {
                        this.lastSelected = selected;
                        updatePosition(selected);
                    }
                }

            }
            
            if (this.active != newActive)
            {
                this.active = newActive;
                this.controllerHintGraphic.enabled = newActive;
                
                if (this.lastSelected != null)
                {
                    updatePosition(lastSelected);
                }
            }
        }
    }
}
