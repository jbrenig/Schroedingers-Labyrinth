using Lib;
using Shader;
using UnityEngine;

namespace PotentialFunctions
{
    [ExecuteInEditMode]
    public class PotentialFunctionTriangle : MonoBehaviour, PotentialManager.IPotential
    {
        public bool useMax = true;
        
        [Range(0, 1)]
        public float height = 1;
        public int priority = 0;

        public BlockShader.TriangleForm triangleForm;

        public Vector2Int size = new Vector2Int(100, 100);
        
        public ComputeShader shader;

        private BlockShader _shaderImpl;

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

        private void OnDestroy()
        {
            // ReSharper disable once Unity.NoNullPropagation
            PotentialManager.Instance?.UnregisterPotentialFunction(this);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(Application.isPlaying) return;
            PotentialManager.Instance.RegisterPotentialFunction(this);
            PotentialManager.Instance.RequestUpdate();
        }
#endif

        public void ApplyPotential(BufferedRenderTexture renderTexture)
        {
            _shaderImpl ??= new BlockShader(shader);

            var pos = GpuQuantumSimulator.Instance.WorldToSimulationCoordinate(transform.position);
            var start = pos - new Vector2Int(size.x / 2, size.y / 2);
            var end = start + size;
            
            _shaderImpl.Triangle(renderTexture.Read, renderTexture.Write, start, end, height, useMax, triangleForm);
            renderTexture.Swap();
        }

        public int GetPriority()
        {
            return priority;
        }

        public bool IsValid()
        {
            return isActiveAndEnabled 
                   && gameObject != null 
                   && gameObject.activeInHierarchy;
        }
    }
}
