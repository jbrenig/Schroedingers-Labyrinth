using Lib;
using Shader;
using UnityEngine;

namespace PotentialFunctions
{
    [ExecuteInEditMode]
    public class PotentialFunctionDoubleSlit : MonoBehaviour, PotentialManager.IPotential
    {
        [Range(0, 1)]
        public float height = 1;

        public int priority = 0;

        public Vector2Int size = new Vector2Int(20, 100);
        
        public int slitWidth = 4;
        public int slitDistance = 40;

        public bool isVertical = true;
        public bool useMax = true;
        
        public ComputeShader shader;

        private DoubleSlitShader _shaderImpl;

        private Vector3 _editorLastPos = new Vector3();
        
        void Start()
        {
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

        private void OnEnable()
        {
            PotentialManager.Instance.RegisterPotentialFunction(this);
        }

        private void OnDisable()
        {
            // ReSharper disable once Unity.NoNullPropagation
            PotentialManager.Instance?.UnregisterPotentialFunction(this);
        }

        private void OnDestroy()
        {
            // ReSharper disable once Unity.NoNullPropagation
            PotentialManager.Instance?.UnregisterPotentialFunction(this);
        }

        private void OnValidate()
        {
            if (Application.isPlaying) return;
            PotentialManager.Instance.RegisterPotentialFunction(this);
            PotentialManager.Instance.RequestUpdate();
        }

        public void ApplyPotential(BufferedRenderTexture renderTexture)
        {
            _shaderImpl ??= new DoubleSlitShader(shader);

            var pos = GpuQuantumSimulator.Instance.WorldToSimulationCoordinate(transform.position);
            var start = pos - (size / 2);
            var adjustedSize = size;
            if (start.x < 0)
            {
                adjustedSize.x += start.x * 2;
                start.x = 0;
            }

            if (start.y < 0)
            {
                adjustedSize.y += start.y * 2;
                start.y = 0;
            }
            
            _shaderImpl.DoubleSlit(renderTexture.Read, renderTexture.Write, start, adjustedSize, height, slitWidth, slitDistance, isVertical, useMax);
            renderTexture.Swap();
        }

        public int GetPriority()
        {
            return priority;
        }

        public bool IsValid()
        {
            return gameObject != null && isActiveAndEnabled && gameObject.activeInHierarchy;
        }
    }
}
