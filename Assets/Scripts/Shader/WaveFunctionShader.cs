using Lib;
using UnityEngine;

namespace Shader
{
    public class WaveFunctionShader
    {
        public const int KernelNthreadsInGroup = 8;
        public const string KernelNameResult = "Result";
        public const string KernelNameVarX = "varX";
        public const string KernelNameVarY = "varY";
        public const string KernelNameMeanX = "meanX";
        public const string KernelNameMeanY = "meanY";
        public const string KernelNameKX = "kx";
        public const string KernelNameKY = "ky";
        
        private readonly int _indexWavePacket;
        private readonly int _indexGaussian;
        private readonly int _indexPlaneWave;
        
        private readonly ComputeShader _shader;
        
        private int _nGroupsX, _nGroupsY;
        private int _width = -1;
        private int _height = -1;

        public WaveFunctionShader(ComputeShader shader)
        {
            _shader = shader;
            _indexWavePacket = _shader.FindKernel("WavePacket");
            _indexGaussian = _shader.FindKernel("Gaussian");
            _indexPlaneWave = _shader.FindKernel("PlaneWave");
        }
        
        public void Init(BufferedRenderTexture texIn) {
            if (_width == texIn.Width && _height == texIn.Height)
                return;

            _width = texIn.Width;
            _height = texIn.Height;
            _nGroupsX = _width / KernelNthreadsInGroup;
            _nGroupsY = _height / KernelNthreadsInGroup;
        }

        public void CreateWavePacket(BufferedRenderTexture tex, Vector2 mean, Vector2 variance, Vector2 k)
        {
            Init(tex);
            _shader.SetTexture(_indexWavePacket, KernelNameResult, tex.Write);
            _shader.SetFloat(KernelNameKX, k.x);
            _shader.SetFloat(KernelNameKY, k.y);
            _shader.SetFloat(KernelNameMeanX, mean.x);
            _shader.SetFloat(KernelNameMeanY, mean.y);
            _shader.SetFloat(KernelNameVarX, variance.x);
            _shader.SetFloat(KernelNameVarY, variance.y);
            
            tex.Write.DiscardContents();
            _shader.Dispatch(_indexWavePacket, _nGroupsX, _nGroupsY, 1);
            tex.Swap();
        }
        
        public void CreatePlaneWave(BufferedRenderTexture tex, Vector2 k)
        {
            Init(tex);
            _shader.SetTexture(_indexPlaneWave, KernelNameResult, tex.Write);
            _shader.SetFloat(KernelNameKX, k.x);
            _shader.SetFloat(KernelNameKY, k.y);
            
            tex.Write.DiscardContents();
            _shader.Dispatch(_indexPlaneWave, _nGroupsX, _nGroupsY, 1);
            tex.Swap();
        }
    }
}