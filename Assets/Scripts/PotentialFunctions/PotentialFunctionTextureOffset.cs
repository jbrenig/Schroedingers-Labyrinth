using System;
using Lib;
using Shader;
using UnityEngine;

namespace PotentialFunctions
{
    [ExecuteInEditMode]
    public class PotentialFunctionTextureOffset : MonoBehaviour, PotentialManager.IPotential
    {
        public Texture texture;
        public int priority = 0;

        public Mode mode = Mode.Max;
        public bool flipX = false;
        public bool flipY = false;

        [Range(0, 1)]
        public float multiplier = 1;

        public ComputeShader shader;

        private TextureBlendShader _shaderImpl;

        private Vector3 _editorLastPos = new Vector3();

        public enum Mode
        {
            Max, Min
        }
        
        private void Start()
        {
            _shaderImpl = new TextureBlendShader(shader);
            PotentialManager.Instance.RegisterPotentialFunction(this);
        }

        private void Update()
        {
            if (Application.isEditor && _editorLastPos != transform.position)
            {
                _editorLastPos = transform.position;
                PotentialManager.Instance.RegisterPotentialFunction(this);
                PotentialManager.Instance.RequestUpdate();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying) return;
            PotentialManager.Instance.RegisterPotentialFunction(this);
            PotentialManager.Instance.RequestUpdate();
        }
#endif
        
        private void OnDestroy()
        {
            // ReSharper disable once Unity.NoNullPropagation
            PotentialManager.Instance?.UnregisterPotentialFunction(this);
        }

        public void ApplyPotential(BufferedRenderTexture renderTexture)
        {
            _shaderImpl ??= new TextureBlendShader(shader);
            
            var offset = new Vector2Int(texture.width / 2, texture.height / 2) - GpuQuantumSimulator.Instance.WorldToSimulationCoordinate(transform.position);
            
            switch (mode)
            {
                case Mode.Max:
                    _shaderImpl.MaxBlend(renderTexture.Read, texture, renderTexture.Write, offset, flipX, flipY, multiplier);
                    break;
                case Mode.Min:
                    _shaderImpl.MinBlend(renderTexture.Read, texture, renderTexture.Write, offset, flipX, flipY, multiplier);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            renderTexture.Swap();
        }

        public int GetPriority()
        {
            return priority;
        }

        public bool IsValid()
        {
            return gameObject != null && gameObject.activeInHierarchy;
        }
    }
}
