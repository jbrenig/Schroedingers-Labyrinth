using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Ui.CatCharacter
{
    public class EyeAnimation : MonoBehaviour
    {
        public float chanceToPlayClip = 0.3f;
        public float minIdleTime = 1f;
        public float maxIdleTime = 6f;

        public Emotion emotion = Emotion.None;

        public enum Emotion
        {
            None, Bored, Panicked
        }
        
        private Animator _animator;

        private float _remainingIdleTime = 0;

        private static readonly string[] Animations = {"Eyes_1", "Eyes_2", "Eyes_bored_1"};
        private static readonly string[] AnimationsPanic = {"Eyes_1", "Eyes_2", "Eyes_panic_1"};
        private static readonly string[] AnimationsBored = {"Eyes_1", "Eyes_2", "Eyes_bored_1"};
        
        void Start()
        {
            _remainingIdleTime = maxIdleTime;
            _animator = GetComponent<Animator>();
        }

        void Update()
        {
            if (_remainingIdleTime > 0)
            {
                _remainingIdleTime -= Time.deltaTime;
                return;
            }

            if (Random.value < chanceToPlayClip)
            {
                PlayRandomAnimation();
                var clips = _animator.GetCurrentAnimatorClipInfo(0);
                float clipLength = clips.Length > 0 ? clips[0].clip.length : 1f; // fallback when unity stuff fails
                _remainingIdleTime = Random.Range(minIdleTime, maxIdleTime) + clipLength;
            }
            else
            {
                _remainingIdleTime = Random.Range(minIdleTime, maxIdleTime);
            }
        }

        private void PlayRandomAnimation()
        {
            string[] src = emotion switch
            {
                Emotion.None => Animations,
                Emotion.Bored => AnimationsBored,
                Emotion.Panicked => AnimationsPanic,
                _ => throw new ArgumentOutOfRangeException()
            };
            _animator.Play(src[Random.Range(0, src.Length)]);
        }
    }
}
