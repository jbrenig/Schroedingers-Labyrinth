using System;
using System.Runtime.InteropServices;
using Lib;
using UnityEngine;

namespace Shader
{
    public class FFTShader : System.IDisposable
    {
        public enum Direction
        {
            Forward = 0,
            Backward
        }

        public const int KernelNthreadsInGroup = 8;

        public const int KernelKernelBitReversalX = 0;
        public const int KernelKernelBitReversalY = 1;
        public const int KernelKernelDitXForward = 2;
        public const int KernelKernelDitYForward = 3;
        public const int KernelKernelDitXBackward = 4;
        public const int KernelKernelDitYBackward = 5;
        public const int KernelKernelScale = 6;

        public const string KernelShaderWidth = "width";
        public const string KernelShaderNs = "Ns";
        public const string KernelShaderNs2 = "_Ns2";
        public const string KernelShaderScale = "ScaleFact";

        public const string KernelShaderBITReversal = "BitReversal";
        public const string KernelShaderFFTIn = "FftIn";
        public const string KernelShaderFFTOut = "FftOut";

        private int _width = -1;
        private int _height = -1;
        private int _nGroupsX;
        private int _nGroupsY;
        private readonly ComputeShader _shader;

        private ComputeBuffer _bitReversalBufX;
        private ComputeBuffer _bitReversalBufY;

        public FFTShader(ComputeShader shader)
        {
            _shader = shader;
        }

        #region IDisposable implementation

        public void Dispose()
        {
            Release();
        }

        #endregion

        public void Forward(BufferedRenderTexture spaceTex)
        {
            Init(spaceTex);

            FftX(Direction.Forward, spaceTex);
            FftY(Direction.Forward, spaceTex);
            Normalize(spaceTex);
        }

        public void Backward(BufferedRenderTexture freqTex)
        {
            Init(freqTex);

            FftX(Direction.Backward, freqTex);
            FftY(Direction.Backward, freqTex);
            Normalize(freqTex);
        }

        void Release()
        {
            _bitReversalBufX?.Release();
        }

        public void Init(BufferedRenderTexture texIn)
        {
            if (!Mathf.IsPowerOfTwo(texIn.Width) || !Mathf.IsPowerOfTwo(texIn.Height))
                throw new ArgumentException("Texture dimensions must be a power of two!");
            if (_width == texIn.Width && _height == texIn.Height)
                return;

            Release();
            _width = texIn.Width;
            _height = texIn.Height;
            _nGroupsX = _width / KernelNthreadsInGroup;
            _nGroupsY = _height / KernelNthreadsInGroup;
            
            // FIXME: remove unused 
            _shader.SetInt(KernelShaderWidth, _width); 
            // FIXME: verify scaling
            _shader.SetFloat(KernelShaderScale, 1f / Mathf.Sqrt(_width * _height));

            // Setup BitReversal indices
            var bitReversalsX = BitReversal.Reverse(BitReversal.Sequence(_width));
            _bitReversalBufX = new ComputeBuffer(bitReversalsX.Length, Marshal.SizeOf(bitReversalsX[0]));
            _bitReversalBufX.SetData(bitReversalsX);
            
            var bitReversalsY = BitReversal.Reverse(BitReversal.Sequence(_height));
            _bitReversalBufY = new ComputeBuffer(bitReversalsY.Length, Marshal.SizeOf(bitReversalsY[0]));
            _bitReversalBufY.SetData(bitReversalsY);
        }

        void FftX(Direction dir, BufferedRenderTexture texIn)
        {
            var dit = (dir == Direction.Forward ? KernelKernelDitXForward : KernelKernelDitXBackward);

            _shader.SetTexture(KernelKernelBitReversalX, KernelShaderFFTIn, texIn.Read);
            _shader.SetTexture(KernelKernelBitReversalX, KernelShaderFFTOut, texIn.Write);
            _shader.SetBuffer(KernelKernelBitReversalX, KernelShaderBITReversal, _bitReversalBufX);
            texIn.Write.DiscardContents();
            _shader.Dispatch(KernelKernelBitReversalX, _nGroupsX, _nGroupsY, 1);
            texIn.Swap();

            // TODO: this can possible be optimized to loop on the gpu using barrier sync
            var ns = 1;
            while ((ns <<= 1) <= _width)
            {
                _shader.SetInt(KernelShaderNs, ns);
                _shader.SetInt(KernelShaderNs2, ns / 2);
                _shader.SetTexture(dit, KernelShaderFFTIn, texIn.Read);
                _shader.SetTexture(dit, KernelShaderFFTOut, texIn.Write);
                texIn.Write.DiscardContents();
                _shader.Dispatch(dit, _nGroupsX, _nGroupsY, 1);
                texIn.Swap();
            }
        }

        void FftY(Direction dir, BufferedRenderTexture texIn)
        {
            var dit = (dir == Direction.Forward ? KernelKernelDitYForward : KernelKernelDitYBackward);

            _shader.SetTexture(KernelKernelBitReversalY, KernelShaderFFTIn, texIn.Read);
            _shader.SetTexture(KernelKernelBitReversalY, KernelShaderFFTOut, texIn.Write);
            _shader.SetBuffer(KernelKernelBitReversalY, KernelShaderBITReversal, _bitReversalBufY);
            texIn.Write.DiscardContents();
            _shader.Dispatch(KernelKernelBitReversalY, _nGroupsX, _nGroupsY, 1);
            texIn.Swap();
            
            // TODO: this can possible be optimized to loop on the gpu using barrier sync
            var ns = 1;
            while ((ns <<= 1) <= _height)
            {
                _shader.SetInt(KernelShaderNs, ns);
                _shader.SetInt(KernelShaderNs2, ns / 2);
                _shader.SetTexture(dit, KernelShaderFFTIn, texIn.Read);
                _shader.SetTexture(dit, KernelShaderFFTOut, texIn.Write);
                texIn.Write.DiscardContents();
                _shader.Dispatch(dit, _nGroupsX, _nGroupsY, 1);
                texIn.Swap();
            }
        }

        void Normalize(BufferedRenderTexture tex)
        {
            // TODO: this pass can be optimized away by integrating it into the y pass
            _shader.SetTexture(KernelKernelScale, KernelShaderFFTIn, tex.Read);
            _shader.SetTexture(KernelKernelScale, KernelShaderFFTOut, tex.Write);
            tex.Write.DiscardContents();
            _shader.Dispatch(KernelKernelScale, _nGroupsX, _nGroupsY, 1);
            tex.Swap();
        }
    }
}