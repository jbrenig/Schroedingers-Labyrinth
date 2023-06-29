using Lib;
using UnityEngine;

namespace PotentialFunctions
{
    public class PotentialFunctionPowerWell : MonoBehaviour, PotentialManager.IPotential
    {
        public Vector2 mean;
        public Vector2 size;

        public float power = 2;
        public float amplitude = 1;

        public int priority = 0; 
        
        public ComputeShader shader;

        private const string KernelName = "CSMain"; 
        private const string KernelNameResult = "Result";
        private const string KernelNameMean = "mean";
        private const string KernelNameSize = "size";
        private const string KernelNameAmplitude = "amplitude";
        private const string KernelNamePower = "power";
    
        private const int KernelNthreadsInGroup = 8;

        private int _index = -1;
    
        void Start()
        {
            PotentialManager.Instance.RegisterPotentialFunction(this);
            Init();
        }

        private void OnValidate()
        {
            PotentialManager.Instance.RegisterPotentialFunction(this);
            PotentialManager.Instance.RequestUpdate();
        }
        
        public void Init()
        {
            _index = shader.FindKernel(KernelName);
            shader.SetFloats(KernelNameMean, mean.x, mean.y);
            shader.SetFloats(KernelNameSize, size.x, size.y);
            shader.SetFloat(KernelNameAmplitude, amplitude);
            shader.SetFloat(KernelNamePower, power);
        }

        #region IPotential

        public void ApplyPotential(BufferedRenderTexture renderTexture)
        {
            if (_index < 0) Init();
            
            var simulator = GpuQuantumSimulator.Instance;
            shader.SetTexture(_index, KernelNameResult, renderTexture.Write);
            var nGroupsX = simulator.width / KernelNthreadsInGroup;
            var nGroupsY = simulator.height / KernelNthreadsInGroup;
            renderTexture.Write.DiscardContents();
            shader.Dispatch(_index, nGroupsX, nGroupsY, 1);
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

        #endregion
    }
}
