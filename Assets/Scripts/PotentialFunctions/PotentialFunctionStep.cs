using Lib;
using UnityEngine;
using UnityEngine.UI;

namespace PotentialFunctions
{
    public class PotentialFunctionStep : MonoBehaviour, PotentialManager.IPotential
    {
        public int location;
        public bool inverted = false;

        public Axis axis = Axis.X;

        public int priority = 0;
        
        public ComputeShader shader;

        private const string KernelNameX = "StepX"; 
        private const string KernelNameY = "StepY"; 
        private const string KernelNameXMax = "StepXMax"; 
        private const string KernelNameYMax = "StepYMax"; 
        private const string KernelNameResult = "Result";
        private const string KernelNameInput = "Merge";
        private const string KernelNameLocation = "location";
        private const string KernelNameInverted = "inverted";
    
        private const int KernelNthreadsInGroup = 8;
        private int _indexX = -1;
        private int _indexY = -1;
        private int _indexXMax = -1;
        private int _indexYMax = -1;


        public enum Axis
        {
            X, Y
        }

        #region Unity Events

        private void Start()
        {
            PotentialManager.Instance.RegisterPotentialFunction(this);
            if (_indexX < 0) Init();
        }

        private void OnValidate()
        {
            PotentialManager.Instance.RegisterPotentialFunction(this);
            PotentialManager.Instance.RequestUpdate();
        }

        #endregion

        private void Init()
        {
            _indexX = shader.FindKernel(KernelNameX);
            _indexY = shader.FindKernel(KernelNameY);
            _indexXMax = shader.FindKernel(KernelNameXMax);
            _indexYMax = shader.FindKernel(KernelNameYMax);
        }
        
        public void OnSliderChanged(Slider slider)
        {
            location = (int) slider.value;
            PotentialManager.Instance.RequestUpdate();
        }

        #region IPotential

        public int GetPriority()
        {
            return priority;
        }


        public bool IsValid()
        {
            return gameObject != null && gameObject.activeInHierarchy;
        }

        public void ApplyPotential(BufferedRenderTexture renderTexture)
        {
            if (_indexX < 0) Init();
            
            var index = axis == Axis.X ? _indexXMax : _indexYMax;
            
            var nGroupsX = GpuQuantumSimulator.Instance.width / KernelNthreadsInGroup;
            var nGroupsY = GpuQuantumSimulator.Instance.height / KernelNthreadsInGroup;
            shader.SetInt(KernelNameLocation, location);
            shader.SetBool(KernelNameInverted, inverted);
            shader.SetTexture(index, KernelNameInput, renderTexture.Read);
            shader.SetTexture(index, KernelNameResult, renderTexture.Write);
            shader.Dispatch(index, nGroupsX, nGroupsY, 1);
            
            renderTexture.Swap();
        }

        #endregion
    }
}