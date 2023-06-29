using Game;
using UnityEngine;
using UnityEngine.Serialization;

namespace Camera
{
    public class StrateCamC : MonoBehaviour
    {
        // Public fields
        public Terrain terrain;

        [FormerlySerializedAs("panSpeed")] public float keyboardPanSpeed = 15.0f;
        [FormerlySerializedAs("zoomSpeed")] public float keyboardZoomSpeed = 200.0f;
        [FormerlySerializedAs("rotationSpeed")] public float keyboardRotationSpeed = 50.0f;

        public float mousePanMultiplier = 0.03f;
        public float mouseRotationMultiplier = 0.2f;
        public float mouseZoomMultiplier = 30f;

        public float gamepadRotationWidthHorizontal = 45;
        public float gamepadRotationWidthVertical = 45;
        public float gamepadRotationSpeed = 10;

        public float minZoomDistance = 50.0f;
        public float maxZoomDistance = 300.0f;
        public float smoothingFactor = 0.1f;
        public float targetFrameRate = 60;
        public float goToSpeed = 0.1f;

        public bool useKeyboardPanning = true;
        public bool useKeyboardZooming = false;
        public bool useMouseInput = true;
        public bool adaptToTerrainHeight = true;
        public bool increaseSpeedWhenZoomedOut = true;
        public bool correctZoomingOutRatio = true;
        [FormerlySerializedAs("smoothing")] public bool keyboardSmoothing = true;
        public bool allowDoubleClickMovement = false;
        public float doubleClickTimeWindow = 0.3f;

        public bool allowScreenEdgeMovement = true;
        public int screenEdgeSize = 10;
        public float screenEdgeSpeed = 15.0f;

        public GameObject objectToFollow;
        public Vector3 cameraTarget;
        public Vector3 cameraBorderStart;
        public Vector3 cameraBorderEnd;

        /// <summary>
        /// Zoom between 0 (close) and 1 (far)
        /// </summary>
        public float startZoom = 1;
    
        // private fields
        private float _currentCameraDistance;
        private Vector3 _lastMousePos;
        private Vector3 _lastPanSpeed = Vector3.zero;
        private Vector3 _goingToCameraTarget = Vector3.zero;
        private bool _doingAutoMovement = false;
        private DoubleClickDetectorC _doubleClickDetector;
        
        private float _lastHorizontalRotationGamepad = 0;
        private float _lastVerticalRotationGamepad = 0;
        private float _gamepadRotSpeedH = 0;
        private float _gamepadRotSpeedV = 0;


        // Use this for initialization
        public void Start()
        {
            _currentCameraDistance = minZoomDistance +  (maxZoomDistance - minZoomDistance) * startZoom;
            _lastMousePos = Vector3.zero;
            _doubleClickDetector = gameObject.AddComponent<DoubleClickDetectorC>();
            _doubleClickDetector.doubleClickTimeWindow = doubleClickTimeWindow;
        }

        public void Update()
        {
            if (!GameController.Instance.CameraControlsEnabled()) return;
            
            DoUpdate();
        }

        private void DoUpdate()
        {
            if (allowDoubleClickMovement)
            {
                //doubleClickDetector.Update();
                UpdateDoubleClick();
            }
            UpdatePanning();
            UpdateRotation();
            UpdateZooming();
            UpdatePosition();
            UpdateAutoMovement();
            _lastMousePos = Input.mousePosition;
        }

        public void GoTo(Vector3 position)
        {
            _doingAutoMovement = true;
            _goingToCameraTarget = position;
            objectToFollow = null;
        }

        public void Follow(GameObject gameObjectToFollow)
        {
            objectToFollow = gameObjectToFollow;
        }

        #region private functions
        private void UpdateDoubleClick()
        {
            if (_doubleClickDetector.IsDoubleClick() && terrain && terrain.GetComponent<Collider>())
            {
                var cameraTargetY = cameraTarget.y;

                var collider = terrain.GetComponent<Collider>();
                var ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit = new RaycastHit();
                Vector3 pos;

                if (collider.Raycast(ray, out hit, Mathf.Infinity))
                {
                    pos = hit.point;
                    pos.y = cameraTargetY;
                    GoTo(pos);
                }
            }
        }

        private void UpdatePanning()
        {
            Vector3 keyboardMoveVector = new Vector3(0, 0, 0);
            if (useKeyboardPanning)
            {
                keyboardMoveVector.x += Input.GetAxis("Pan Horizontal") * keyboardPanSpeed;
                keyboardMoveVector.z += Input.GetAxis("Pan Vertical") * keyboardPanSpeed;
            }
            if (allowScreenEdgeMovement)
            {
                if (Input.mousePosition.x < screenEdgeSize)
                {
                    keyboardMoveVector.x -= screenEdgeSpeed;
                }
                else if (Input.mousePosition.x > Screen.width - screenEdgeSize)
                {
                    keyboardMoveVector.x += screenEdgeSpeed;
                }
                if (Input.mousePosition.y < screenEdgeSize)
                {
                    keyboardMoveVector.z -= screenEdgeSpeed;
                }
                else if (Input.mousePosition.y > Screen.height - screenEdgeSize)
                {
                    keyboardMoveVector.z += screenEdgeSpeed;
                }
            }
            
            // Smooth acceleration of keyboard panning
            var effectiveKeyboardPanSpeed = keyboardMoveVector;
            if (keyboardSmoothing)
            {
                // Smooth acceleration
                effectiveKeyboardPanSpeed = Vector3.Lerp(_lastPanSpeed, keyboardMoveVector, smoothingFactor * (Time.deltaTime * targetFrameRate));
                _lastPanSpeed = effectiveKeyboardPanSpeed;
            }

            Vector3 mouseMoveVector = new Vector3(0, 0, 0);
            if (useMouseInput)
            {
                if (Input.GetMouseButton(2))
                {
                    Vector3 deltaMousePos = (Input.mousePosition - _lastMousePos);
                    mouseMoveVector = new Vector3(-deltaMousePos.x, 0, -deltaMousePos.y) * mousePanMultiplier;
                }
            }

            if (mouseMoveVector != Vector3.zero || effectiveKeyboardPanSpeed != Vector3.zero)
            {
                objectToFollow = null;
                _doingAutoMovement = false;
            }


            var oldXRotation = transform.localEulerAngles.x;

            // Set the local X rotation to 0;
            transform.SetLocalEulerAngles(0.0f);

            float panMultiplier = increaseSpeedWhenZoomedOut ? (Mathf.Sqrt(_currentCameraDistance)) : 1.0f;
            cameraTarget += transform.TransformDirection(effectiveKeyboardPanSpeed) * (panMultiplier * Time.deltaTime);
            cameraTarget += transform.TransformDirection(mouseMoveVector) * panMultiplier;
            cameraTarget = new Vector3(Mathf.Clamp(cameraTarget.x, cameraBorderStart.x, cameraBorderEnd.x),
                Mathf.Clamp(cameraTarget.y, cameraBorderStart.y, cameraBorderEnd.y),
                Mathf.Clamp(cameraTarget.z, cameraBorderStart.z, cameraBorderEnd.z));

            // Set the old x rotation.
            transform.SetLocalEulerAngles(oldXRotation);
        }

        private void UpdateRotation()
        {
            if (useKeyboardPanning)
            {
                var oldAngle = transform.localEulerAngles;
                float deltaAngleH = 0.0f;
                
                if (Input.GetKey(KeyCode.Q))
                {
                    deltaAngleH = -1.0f;
                }
                if (Input.GetKey(KeyCode.E))
                {
                    deltaAngleH = 1.0f;
                }
                transform.SetLocalEulerAngles(
                    Mathf.Clamp(oldAngle.x, 5.0f, 80.0f), oldAngle.y + deltaAngleH * Time.deltaTime * keyboardRotationSpeed);
            }


            if (useMouseInput)
            {
                if (Input.GetMouseButton(1))
                {
                    var oldAngle = transform.localEulerAngles;
                    var deltaMousePos = (Input.mousePosition - _lastMousePos);
                    var deltaAngleH = deltaMousePos.x * mouseRotationMultiplier;
                    var deltaAngleV = -deltaMousePos.y * mouseRotationMultiplier;
                    transform.SetLocalEulerAngles(
                        Mathf.Clamp(oldAngle.x + deltaAngleV, 5.0f, 80.0f),
                        oldAngle.y + deltaAngleH
                    );
                }
            }

            // TODO if (useGamepadInput)
            {
                var oldAngle = transform.localEulerAngles;
                
                var newAngleH = Input.GetAxis("Rotate Horizontal") * gamepadRotationWidthHorizontal;
                var deltaAngleH = newAngleH - _lastHorizontalRotationGamepad;
                
                deltaAngleH = Mathf.Clamp(deltaAngleH, -gamepadRotationSpeed * Time.deltaTime, gamepadRotationSpeed * Time.deltaTime);
                _lastHorizontalRotationGamepad += deltaAngleH;
                
                var newAngleV = Input.GetAxis("Rotate Vertical") * gamepadRotationWidthVertical;
                var deltaAngleV = newAngleV - _lastVerticalRotationGamepad;
                deltaAngleV = Mathf.Clamp(deltaAngleV, -gamepadRotationSpeed * Time.deltaTime, gamepadRotationSpeed * Time.deltaTime);
                _lastVerticalRotationGamepad += deltaAngleV;
                
                transform.SetLocalEulerAngles(
                    oldAngle.x + deltaAngleV,
                    oldAngle.y + deltaAngleH
                );
            }
        }

        private void UpdateZooming()
        {
            var zoomedOutRatio = correctZoomingOutRatio ? (_currentCameraDistance - minZoomDistance) / (maxZoomDistance - minZoomDistance) : 0.0f;
            
            if (useKeyboardZooming)
            {
                float deltaZoom = Input.GetAxis("Zoom");
                _currentCameraDistance = Mathf.Clamp(_currentCameraDistance + deltaZoom * Time.deltaTime * keyboardZoomSpeed * (zoomedOutRatio * 2.0f + 1.0f), minZoomDistance, maxZoomDistance);
            }
            if (useMouseInput)
            {
                var scroll = Input.GetAxis("Mouse ScrollWheel");
                float deltaZoom = -scroll * mouseZoomMultiplier;
                _currentCameraDistance = Mathf.Clamp(_currentCameraDistance + deltaZoom * (zoomedOutRatio * 2.0f + 1.0f), minZoomDistance, maxZoomDistance);
            }
            
        }

        private void UpdatePosition()
        {
            if (objectToFollow != null)
            {
                cameraTarget = Vector3.Lerp(cameraTarget, objectToFollow.transform.position, goToSpeed);
            }

            transform.localPosition = cameraTarget;
            transform.Translate(Vector3.back * _currentCameraDistance);

            if (adaptToTerrainHeight && terrain != null)
            {
                transform.SetPosition(
                    null,
                    Mathf.Max(terrain.SampleHeight(transform.position) + terrain.transform.position.y + 10.0f, transform.position.y)
                );
            }
        }

        private void UpdateAutoMovement()
        {
            if (_doingAutoMovement)
            {
                cameraTarget = Vector3.Lerp(cameraTarget, _goingToCameraTarget, goToSpeed);
                if (Vector3.Distance(_goingToCameraTarget, cameraTarget) < 1.0f)
                {
                    _doingAutoMovement = false;
                }
            }
        }
        #endregion
    }
}