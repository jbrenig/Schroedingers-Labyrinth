using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements.Experimental;

namespace Game
{
    /// <summary>
    /// Animates the target from it's current position and rotation to the target position and rotation using the specified easing.
    /// </summary>
    public class AnimateToPosition : MonoBehaviour
    {
        [FormerlySerializedAs("targetCamera")] public GameObject target;

        public Vector3 targetPosition;
        public Quaternion targetRotation;
        public float animationTime = 1;

        public AnimationEasing animationEasing = AnimationEasing.SineInOut;
        
        public enum AnimationEasing
        {
            SineInOut, QuadIn
        }
        
        private void OnEnable()
        {
            StartCoroutine(Animate());
        }

        IEnumerator Animate()
        {
            var camTransform = target.transform;
            var startPos = camTransform.localPosition;
            var startRotation = camTransform.localRotation;
            var passedTime = 0f;
            while (passedTime < animationTime)
            {
                passedTime += Time.deltaTime;
                var eased = Ease(passedTime / animationTime);
                var newPos = Vector3.Lerp(startPos, targetPosition, eased);
                var newRot = Quaternion.Lerp(startRotation, targetRotation, eased);
                camTransform.localPosition = newPos;
                camTransform.localRotation = newRot;
                yield return null;
            }
        }

        private float Ease(float time)
        {
            switch (animationEasing)
            {
                case AnimationEasing.SineInOut:
                    return Easing.InOutSine(time);
                case AnimationEasing.QuadIn:
                    return Easing.InQuad(time);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
