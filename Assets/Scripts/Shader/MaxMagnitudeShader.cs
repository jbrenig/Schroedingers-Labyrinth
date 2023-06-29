using System;
using Lib;
using UnityEngine;

namespace Shader
{
    public class MaxMagnitudeShader : IDisposable
    {
        private const int NthreadsInGroup = 128;
        private const string ShaderTexture = "InputTexture";
        private const string ShaderInputBuffer = "InputBuffer";
        private const string ShaderOutputBuffer = "Result";
        private const string ShaderTextureWidth = "TextureWidth";
        
        private readonly ComputeShader _shader;
        private int _nGroups;
        private ComputeBuffer _shaderBufferResult;
        private ComputeBuffer _shaderBufferTemp;
        private readonly float[] _outputArray = {0f};

        private readonly int _indexScan;
        private readonly int _indexScanBucket;
        
        private int _n = -1;

        public MaxMagnitudeShader(ComputeShader shader)
        {
            _shader = shader;
            _indexScan = _shader.FindKernel("ScanPerGroup");
            _indexScanBucket = _shader.FindKernel("ScanBuckets");
        }
        
        #region IDisposable implementation
        public void Dispose () { Release(); }
        #endregion
        
        void Release() {
            _shaderBufferResult?.Release();
            _shaderBufferTemp?.Release();
        }
        
        public void Init(BufferedRenderTexture texIn) {
            if (_n == texIn.Width * texIn.Height)
                return;

            Release();
            _n = texIn.Width * texIn.Height;
            _nGroups = _n / NthreadsInGroup;
			
            _shaderBufferResult = new ComputeBuffer(_n , sizeof(float));
            _shaderBufferTemp = new ComputeBuffer(_n, sizeof(float));
            _shader.SetInt(ShaderTextureWidth, texIn.Width);
        }

        private void Swap()
        {
            var old = _shaderBufferTemp;
            _shaderBufferTemp = _shaderBufferResult;
            _shaderBufferResult = old;
        }

        public float GetMax(BufferedRenderTexture tex)
        {
            Init(tex);
            _shader.SetTexture(_indexScan, ShaderTexture, tex.Read);
            _shader.SetBuffer(_indexScan, ShaderOutputBuffer, _shaderBufferResult);
            _shader.Dispatch(_indexScan, _nGroups, 1, 1);

            // calculate necessary groups
            var bucketGroups = (int) Math.Ceiling((_nGroups) / ((float) NthreadsInGroup));
            
            Swap();
            _shader.SetBuffer(_indexScanBucket, ShaderInputBuffer, _shaderBufferTemp);
            _shader.SetBuffer(_indexScanBucket, ShaderOutputBuffer, _shaderBufferResult);
            _shader.Dispatch(_indexScanBucket, bucketGroups, 1, 1);
            
            while (bucketGroups > 1)
            {
                bucketGroups = (int) Math.Ceiling(bucketGroups / ((float) NthreadsInGroup));
                Swap();
                _shader.SetBuffer(_indexScanBucket, ShaderInputBuffer, _shaderBufferTemp);
                _shader.SetBuffer(_indexScanBucket, ShaderOutputBuffer, _shaderBufferResult);
                _shader.Dispatch(_indexScanBucket, bucketGroups, 1, 1);
            }
            
            var index = NthreadsInGroup - 1;
            _shaderBufferResult.GetData(_outputArray, 0, index, 1);
            
            return _outputArray[0];
        }
    }
}