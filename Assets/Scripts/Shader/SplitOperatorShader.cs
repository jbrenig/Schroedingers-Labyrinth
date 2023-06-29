using System;
using Lib;
using UnityEngine;

namespace Shader
{
    public class SplitOperatorShader
    {
        public const int KernelNthreadsInGroup = 8;
        public const string KernelShaderData = "Input";
        public const string KernelShaderDataOut = "Output";
        public const string KernelShaderPotential = "Potential";
        public const string KernelShaderSize = "Size";
        public const string KernelShaderHalfSize = "HalfSize";
        public const string KernelShaderFourierFactor = "FourierFactor";
        public const string KernelShaderFourierScaleSqX = "FourierScaleSqX";
        public const string KernelShaderFourierScaleSqY = "FourierScaleSqY";
        public const string KernelShaderMass = "ParticleMass";
        public const string KernelShaderTimeStep = "TimeStep";
        
        public const string KernelShaderH = "H";
        public const string KernelShaderHBar = "HBAR";
        
        public const string KernelShaderPotentialScale = "PotentialScale";
        public const string KernelShaderPotentialOffset = "PotentialOffset";
        public const string KernelShaderPotentialRotationX = "PotentialRotationX";
        public const string KernelShaderPotentialRotationY = "PotentialRotationY";
        
        private readonly ComputeShader _shader;
        
        private int _width = -1;
        private int _height = -1;
        private int _nGroupsX, _nGroupsY;

        public float Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                CalculateFourierScale();
            }
        }

        public float Mass
        {
            get => _mass;
            set
            {
                _mass = value;
                _shader.SetFloat(KernelShaderMass, value);
                CalculateFourierScale();
            }
        }

        public float TimeStep
        {
            get => _timeStep;
            set
            {
                _timeStep = value;
                _shader.SetFloat(KernelShaderTimeStep, value);
                CalculateFourierScale();
            }
        }

        public float PotentialScale
        {
            get => _potentialScale;
            set
            {
                _potentialScale = value;
                _shader.SetFloat(KernelShaderPotentialScale, value);
            }
        }

        public float PotentialOffset
        {
            get => _potentialOffset;
            set { _potentialOffset = value;
                _shader.SetFloat(KernelShaderPotentialOffset, value); }
        }
        
        public float PotentialRotationX
        {
            get => _potentialRotationX;
            set { _potentialRotationX = value;
                _shader.SetFloat(KernelShaderPotentialRotationX, value); }
        }
        
        public float PotentialRotationY
        {
            get => _potentialRotationY;
            set { _potentialRotationY = value;
                _shader.SetFloat(KernelShaderPotentialRotationY, value); }
        }

        private float _fourierFactor = -1;
        private float _fourierScaleSqX = -1;
        private float _fourierScaleSqY = -1;
        private readonly float _h;
        private readonly float _hBar;


        private readonly int _indexMomentum;
        private readonly int _indexPotential;
        
        private float _scale;
        private float _mass;
        private float _timeStep;
        private float _potentialScale;
        private float _potentialOffset;
        private float _potentialRotationX;
        private float _potentialRotationY;

        public SplitOperatorShader(ComputeShader shader, float h)
        {
            _shader = shader;
            _h = h;
            _hBar = h / (2 * Mathf.PI);
            
            _indexPotential = _shader.FindKernel("HalfPotential");
            _indexMomentum = _shader.FindKernel("Momentum");
        }
        private void CalculateFourierScale()
        {
            
            // calculate constant exponent part (only missing k wavenumber Index squared as factor)
            _fourierScaleSqX = Mathf.Pow(2.0f * Mathf.PI / (_width * Scale), 2);
            _fourierScaleSqY = Mathf.Pow(2.0f * Mathf.PI / (_height * Scale), 2);
            double expRealWithoutKSq = -TimeStep * _hBar/ (2 * Mass);
            // modulo PI, since it will be passed into a sin function. 
            // sin functions on the gpu are optimized for small values around 0
            // so we make sure we pass in a small value to avoid precision loss
            _fourierFactor = (float) (expRealWithoutKSq % (2 * Math.PI));
            _shader.SetFloat(KernelShaderFourierFactor, _fourierFactor);
            _shader.SetFloat(KernelShaderFourierScaleSqX, _fourierScaleSqX);
            _shader.SetFloat(KernelShaderFourierScaleSqY, _fourierScaleSqY);
        }
        
        public void Init(BufferedRenderTexture texIn) {
            if (_width == texIn.Width && _height == texIn.Height)
                return;

            _width = texIn.Width;
            _height = texIn.Height;
            
            _nGroupsX = _width / KernelNthreadsInGroup;
            _nGroupsY = _height / KernelNthreadsInGroup;
            _shader.SetInts(KernelShaderSize, _width, _height);
            _shader.SetInts(KernelShaderHalfSize, _width >> 1, _height >> 1);
            
            // Set all parameters
            CalculateFourierScale();
            _shader.SetFloat(KernelShaderMass, Mass);
            _shader.SetFloat(KernelShaderTimeStep, TimeStep);
            _shader.SetFloat(KernelShaderH, _h);
            _shader.SetFloat(KernelShaderHBar, _hBar);
            _shader.SetFloat(KernelShaderPotentialScale, PotentialScale);
            _shader.SetFloat(KernelShaderPotentialOffset, PotentialOffset);
            _shader.SetFloat(KernelShaderPotentialRotationX, PotentialRotationX);
            _shader.SetFloat(KernelShaderPotentialRotationY, PotentialRotationY);
        }

        public void ApplyMomentum(BufferedRenderTexture tex)
        {
            Init(tex);
            _shader.SetTexture(_indexMomentum, KernelShaderData, tex.Read);
            _shader.SetTexture(_indexMomentum, KernelShaderDataOut, tex.Write);
            _shader.Dispatch(_indexMomentum, _nGroupsX, _nGroupsY, 1);
            tex.Swap();
        }
        
        public void ApplyPotential(BufferedRenderTexture tex, Texture potential)
        {
            Init(tex);
            _shader.SetTexture(_indexPotential, KernelShaderData, tex.Read);
            _shader.SetTexture(_indexPotential, KernelShaderDataOut, tex.Write);
            _shader.SetTexture(_indexPotential, KernelShaderPotential, potential);
            _shader.Dispatch(_indexPotential, _nGroupsX, _nGroupsY, 1);
            tex.Swap();
        }
        
    }
}