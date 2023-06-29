using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

namespace Scenes.Story._4_Level4
{
    public class ScannerAnimation : MonoBehaviour
    {
        public float startOffset = -40;
        public float endOffset = 40;
        
        public float animationTimeOffset = .8f;
        public float animationLength = 1;
        public int animationIterations = 1;

        public CameraReturnAnimation cameraToRotate;
        public float rotationAmountStart = -20;
        public float rotationAmountEnd = 20;
        public float rotationAmountX = -60;

        public AudioSource audioSource;
        public AudioClip audioTurnFull;
        public AudioClip audioTurnHalf;

        private float _startRotationX = 0;
        private float _startRotationY = 0;

        private bool _hasStarted = false;
        
        private readonly List<LineRenderer> _list = new List<LineRenderer>();
        
        // Start is called before the first frame update
        void Start()
        {
            _hasStarted = false;
            
            _list.Clear();
            _list.AddRange(GetComponentsInChildren<LineRenderer>());
            foreach (var lineRenderer in _list)
            {
                var oldPos = lineRenderer.GetPosition(1);
                oldPos.x = (startOffset + endOffset) / 2;
                lineRenderer.SetPosition(1, oldPos);
                lineRenderer.enabled = false;
            }

            _startRotationX = cameraToRotate.transform.eulerAngles.x;
            _startRotationY = cameraToRotate.transform.eulerAngles.y;
        }

        private IEnumerator DoScannerAnimation()
        {
            var cameraTransform = cameraToRotate.transform;
            foreach (var lineRenderer in _list)
            {
                // enable all lines
                lineRenderer.enabled = true;
            }

            if (audioSource != null)
            {
                audioSource.PlayOneShot(audioTurnHalf);
            }
            
            var time = 0f;
            // half movement
            while (time < animationLength / 2)
            {
                time += Time.deltaTime;
                
                var progress = Easing.InOutCirc(time * 2 / animationLength);
                var newPosition = Mathf.Lerp(0, endOffset, progress);
                SetLinePositions(newPosition);

                var rotation = Mathf.Lerp(_startRotationY, _startRotationY + rotationAmountEnd, progress);
                var eulerAngles = cameraTransform.eulerAngles;
                eulerAngles.y = rotation;
                cameraTransform.eulerAngles = eulerAngles;
                yield return null;
            }

            var currentDirection = true;
            var currentIteration = 1;

            while (animationIterations <= 0 || currentIteration < animationIterations)
            {
                if (audioSource != null)
                {
                    audioSource.PlayOneShot(audioTurnFull);
                }

                currentIteration++;
                currentDirection = !currentDirection;
                time = 0;

                while (time < animationLength)
                {
                    time += Time.deltaTime;
                    var progress = Easing.InOutCirc(time / animationLength);
                    var start = currentDirection ? startOffset : endOffset;
                    var end = currentDirection ? endOffset : startOffset;
                    var newPosition = Mathf.Lerp(start, end, progress);
                    SetLinePositions(newPosition);
                    
                    var startR = _startRotationY + (currentDirection ? rotationAmountStart : rotationAmountEnd);
                    var endR = _startRotationY + (currentDirection ? rotationAmountEnd : rotationAmountStart);
                    var rotation = Mathf.Lerp(startR, endR, progress);
                    var eulerAngles = cameraTransform.eulerAngles;
                    eulerAngles.y = rotation;
                    cameraTransform.eulerAngles = eulerAngles;
                    yield return null;
                }
            }
            
            cameraToRotate.DoDisableAnimation();
            gameObject.SetActive(false);
        }

        private void SetLinePositions(float position)
        {
            foreach (var lineRenderer in _list)
            {
                // adjust line origins aswell
                var originPos = lineRenderer.GetPosition(0);
                originPos.x = 0.11f * position;
                lineRenderer.SetPosition(0, originPos);
                
                var oldPos = lineRenderer.GetPosition(1);
                oldPos.x = position;
                lineRenderer.SetPosition(1, oldPos);
            }
        }
        
        void Update()
        {
            if (!_hasStarted)
            {
                _hasStarted = true;
                StartCoroutine(DoScannerAnimation());
            }
        }
    }
}
