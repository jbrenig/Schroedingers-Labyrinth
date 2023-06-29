using UnityEngine;
using UnityEngine.EventSystems;



namespace Ui
{

    public class OptionalControllerHint : MonoBehaviour
    {
        public UIBehaviour controllerHintGraphic;

        private bool active = false;

        // Start is called before the first frame update
        void Start()
        {
            controllerHintGraphic.enabled = this.active;
        }

        void Update()
        {
            // TODO: find a better way of handling changes in input
            var hasGamepad = Input.GetJoystickNames().Length > 0;

            if (this.active != hasGamepad)
            {
                this.active = hasGamepad;
                this.controllerHintGraphic.enabled = hasGamepad;
            }
        }
    }
}
