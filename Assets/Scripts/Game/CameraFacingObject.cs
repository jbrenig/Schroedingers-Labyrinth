using UnityEngine;

namespace Game
{
    [ExecuteAlways]
    public class CameraFacingObject : MonoBehaviour
    {
        private UnityEngine.Camera _camera;
        // Start is called before the first frame update
        void Start()
        {
            _camera = UnityEngine.Camera.main;
        }

        // Update is called once per frame
        void Update()
        {
            gameObject.transform.LookAt(_camera.gameObject.transform);
        }
    }
}
