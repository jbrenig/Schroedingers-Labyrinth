using System;
using Lib;
using UnityEngine;

namespace Shader
{
    public class ParallelSumShader : IDisposable
    {
        public const int KernelNthreadsInGroup = 128;
        public const string KernelShaderTexture = "InputTexture";
        public const string KernelShaderTextureMask = "MaskTexture";
        public const string KernelShaderInputBuffer = "InputBuffer";
        public const string KernelShaderOutputBuffer = "Result";
        public const string KernelShaderTextureWidth = "TextureWidth";
        
        private readonly ComputeShader _shader;
        private int _nGroups;
        private ComputeBuffer _shaderBufferResult;
        private ComputeBuffer _shaderBufferTemp;
        private float[] _ouputArray;

        private int KernelIndexScan;
        private int KernelIndexScanMasked;
        private int KernelIndexScanBucket;
        
        private int _n = -1;

        public ParallelSumShader(ComputeShader shader)
        {
            _shader = shader;
            KernelIndexScan = _shader.FindKernel("ScanPerGroup");
            KernelIndexScanMasked = _shader.FindKernel("ScanPerGroupMasked");
            KernelIndexScanBucket = _shader.FindKernel("ScanBuckets");
        }
        
        #region IDisposable implementation
        public void Dispose () { Release(); }
        #endregion
        
        void Release() {
            if (_shaderBufferResult != null)
                _shaderBufferResult.Release();
            if (_shaderBufferTemp != null)
                _shaderBufferTemp.Release();
        }

        public void Init(BufferedRenderTexture texIn) {
            if (_n == texIn.Width * texIn.Height)
                return;

            Release();
            _n = texIn.Width * texIn.Height;
            _nGroups = _n / KernelNthreadsInGroup;
			
            _shaderBufferResult = new ComputeBuffer(_n , sizeof(float));
            _shaderBufferTemp = new ComputeBuffer(_n, sizeof(float));
            _ouputArray = new float[_n];
            _shader.SetInt(KernelShaderTextureWidth, texIn.Width);
        }
        
        public float GetSumOfComplexSq(BufferedRenderTexture tex)
        {
            Init(tex);
            _shader.SetTexture(KernelIndexScan, KernelShaderTexture, tex.Read);
            _shader.SetBuffer(KernelIndexScan, KernelShaderOutputBuffer, _shaderBufferResult);
            _shader.Dispatch(KernelIndexScan, _nGroups, 1, 1);

            return GetSumOfComplexSqBuckets(tex);
        }

        public float GetSumOfComplexSq(BufferedRenderTexture tex, Texture mask)
        {
            Init(tex);
            _shader.SetTexture(KernelIndexScanMasked, KernelShaderTexture, tex.Read);
            _shader.SetTexture(KernelIndexScanMasked, KernelShaderTextureMask, mask);
            _shader.SetBuffer(KernelIndexScanMasked, KernelShaderOutputBuffer, _shaderBufferResult);
            _shader.Dispatch(KernelIndexScanMasked, _nGroups, 1, 1);

            return GetSumOfComplexSqBuckets(tex);
        }

        private float GetSumOfComplexSqBuckets(BufferedRenderTexture tex)
        {
            // calculate necessary groups
            var lastBucketGroups = _nGroups;
            var bucketGroups = (int) Math.Ceiling((_nGroups) / ((float) KernelNthreadsInGroup));
            
            Swap();
            _shader.SetBuffer(KernelIndexScanBucket, KernelShaderInputBuffer, _shaderBufferTemp);
            _shader.SetBuffer(KernelIndexScanBucket, KernelShaderOutputBuffer, _shaderBufferResult);
            _shader.Dispatch(KernelIndexScanBucket, bucketGroups, 1, 1);

            while (bucketGroups > 1)
            {
                lastBucketGroups = bucketGroups;
                bucketGroups = (int) Math.Ceiling(bucketGroups / ((float) KernelNthreadsInGroup));
                Swap();
                _shader.SetBuffer(KernelIndexScanBucket, KernelShaderInputBuffer, _shaderBufferTemp);
                _shader.SetBuffer(KernelIndexScanBucket, KernelShaderOutputBuffer, _shaderBufferResult);
                _shader.Dispatch(KernelIndexScanBucket, bucketGroups, 1, 1);
            }
            
            _shaderBufferResult.GetData(_ouputArray);
            
            var index = lastBucketGroups - 1;
            return _ouputArray[index];
        }

        private void Swap()
        {
            var old = _shaderBufferTemp;
            _shaderBufferTemp = _shaderBufferResult;
            _shaderBufferResult = old;
        }
    }
}