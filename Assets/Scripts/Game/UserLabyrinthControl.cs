using System;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

namespace Game
{
    public class UserLabyrinthControl : MonoBehaviour
    {
        public GameObject targetRotationObject;
    
        [Range(0, 180)]
        public int maxRotation = 15;
        public float rotationSpeed = 25f;

        private float _maxTime = 0;

        public float potentialStrengthX = 0.25f;
        public float potentialStrengthY = 0.25f;

        public bool btnLeftEnabled = true;
        public bool btnRightEnabled = true;
        public bool btnUpEnabled = true;
        public bool btnDownEnabled = true;
    
        enum Direction
        {
            Return, Up, Down
        }

        private Direction _directionX = Direction.Return;
        private Direction _directionY = Direction.Return;
        private float _startX = 0;
        private float _targetX = 0;
        private float _startY = 0;
        private float _targetY = 0;
        private float _currentTimeX = 0;
        private float _currentTimeY = 0;

        private float ClampAngle(float angle, float min, float max)
        {
            angle = (angle + 360) % 360;
            if (angle > 180)
            {
                angle -= 360;
            }
            return Mathf.Clamp(angle, min, max);
        }
        private float ClampAngle(float angle)
        {
            return ClampAngle(angle, -maxRotation, maxRotation);
        }

        private float AngleToLinearRotation(float angle)
        {
            angle = AngleToZeroedAngel(angle);

            return (angle / maxRotation);
        }

        private float AngleToZeroedAngel(float angle)
        {
            angle = (angle + 360) % 360;
            if (angle > 180)
            {
                angle -= 360;
            }

            return angle;
        }

        private float EaseAngle(float min, float max, float time)
        {
            return Mathf.LerpAngle(min, max, Easing.OutSine(time));
        }

        private void Start()
        {
            _maxTime = (maxRotation) / rotationSpeed;
        }

        void FixedUpdate()
        {
            if (!GameController.Instance.LabyrinthControlsEnabled()) return;

            var rotation = targetRotationObject.transform.localEulerAngles;

            if (Input.GetAxis("Horizontal") < 0 && btnLeftEnabled)
            {
                if (_directionX != Direction.Down)
                {
                    _currentTimeX = 0;
                    _startX = rotation.z;
                    _targetX = -maxRotation;
                    _directionX = Direction.Down;
                }
                
                // Retarget rotation to new axis input
                var lastTargetX = _targetX;
                _targetX = Input.GetAxis("Horizontal") * maxRotation;
                if (Math.Abs(lastTargetX - _targetX) > 0.1f)
                {
                    _startX = rotation.z;
                    _currentTimeX = 0;
                }
            }
            else if (Input.GetAxis("Horizontal") > 0 && btnRightEnabled)
            {
                if (_directionX != Direction.Up)
                {
                    _currentTimeX = 0;
                    _startX = rotation.z;
                    _targetX = maxRotation;
                    _directionX = Direction.Up;
                }

                // Retarget rotation to new axis input
                var lastTargetX = _targetX;
                _targetX = Input.GetAxis("Horizontal") * maxRotation;
                if (Math.Abs(lastTargetX - _targetX) > 0.1f)
                {
                    _startX = rotation.z;
                    _currentTimeX = 0;
                }
            }
            else
            {
                if (_directionX != Direction.Return)
                {
                    _currentTimeX = 0;
                    _startX = rotation.z;
                    _targetX = 0;
                    _directionX = Direction.Return;
                }
            }
        
            // do rotation
            if (_maxTime != 0 && _currentTimeX < _maxTime)
            {
                _currentTimeX += Time.deltaTime;
                _currentTimeX = Mathf.Min(_currentTimeX, _maxTime);
                
                rotation.z = EaseAngle(_startX, _targetX, _currentTimeX / _maxTime);
                // rotation axis and rotation potential are orthogonal (x -> y, etc.)
                // also this axis is inverted
                GpuQuantumSimulator.Instance.SetPotentialRotationX(-AngleToLinearRotation(rotation.z) * potentialStrengthX);
            }

            if (Input.GetAxis("Vertical") > 0 && btnUpEnabled)
            {
                if (_directionY != Direction.Down)
                {
                    _currentTimeY = 0;
                    _startY = rotation.x;
                    _targetY = -maxRotation;
                    _directionY = Direction.Down;
                }
                
                
                // Retarget rotation to new axis input
                var lastTargetY = _targetY;
                _targetY = -Input.GetAxis("Vertical") * maxRotation;
                if (Math.Abs(lastTargetY - _targetY) > 0.1f)
                {
                    _startY = rotation.x;
                    _currentTimeY = 0;
                }
            }
            else if (Input.GetAxis("Vertical") < 0 && btnDownEnabled)
            {
                if (_directionY != Direction.Up)
                {
                    _currentTimeY = 0;
                    _startY = rotation.x;
                    _targetY = maxRotation;
                    _directionY = Direction.Up;
                }
                
                // Retarget rotation to new axis input
                var lastTargetY = _targetY;
                _targetY = -Input.GetAxis("Vertical") * maxRotation;
                if (Math.Abs(lastTargetY - _targetY) > 0.1f)
                {
                    _startY = rotation.x;
                    _currentTimeY = 0;
                }
            }
            else
            {
                if (_directionY != Direction.Return)
                {
                    _currentTimeY = 0;
                    _startY = rotation.x;
                    _targetY = 0;
                    _directionY = Direction.Return;
                }
            }
            // do rotation
            if (_maxTime != 0 && _currentTimeY < _maxTime)
            {
                _currentTimeY += Time.deltaTime;
                _currentTimeY = Mathf.Min(_currentTimeY, _maxTime);

                rotation.x = EaseAngle(_startY, _targetY, _currentTimeY / _maxTime);
                // rotation axis and rotation potential are orthogonal (x -> y, etc.)
                // also this axis is inverted
                GpuQuantumSimulator.Instance.SetPotentialRotationY(AngleToLinearRotation(rotation.x) * potentialStrengthY);
            }

            targetRotationObject.transform.localEulerAngles = rotation;
        }

        public void RestartLevel()
        {
            var rotation = targetRotationObject.transform.localEulerAngles;
            rotation.x = 0;
            rotation.z = 0;
            targetRotationObject.transform.localEulerAngles = rotation;
        
            GpuQuantumSimulator.Instance.SetPotentialRotationX(0);
            GpuQuantumSimulator.Instance.SetPotentialRotationY(0);
        }

        private void Reset()
        {
            rotationSpeed = 25;
            maxRotation = 15;
            potentialStrengthX = 0.25f;
            potentialStrengthY = 0.25f;
        }
    }
}
