using System;
using Lib;
using UnityEngine;

namespace Shader
{
    public class RescaleShader
    {
        public const int KernelNthreadsInGroup = 8;
        
        public const string KernelNameInput = "Input";
        public const string KernelNameMask = "MaskInput";
        public const string KernelNameResult = "Result";
        public const string KernelNameScale = "scale";
        
        private readonly ComputeShader _shader;
        private readonly int _indexRescale;
        private readonly int _indexMask;
        private readonly int _indexMaskInverted;

        private int _nGroupsX, _nGroupsY;
        private int _width = -1;
        private int _height = -1;

        public RescaleShader(ComputeShader shader)
        {
            _shader = shader;
            _indexRescale = _shader.FindKernel("Rescale");
            _indexMask = _shader.FindKernel("Mask");
            _indexMaskInverted = _shader.FindKernel("MaskInverted");
        }

        public void Init(BufferedRenderTexture texIn) {
            if (_width == texIn.Width && _height == texIn.Height)
                return;

            _width = texIn.Width;
            _height = texIn.Height;
            _nGroupsX = _width / KernelNthreadsInGroup;
            _nGroupsY = _height / KernelNthreadsInGroup;
        }
        
        public void Init(Texture texIn) {
            if (_width == texIn.width && _height == texIn.height)
                return;

            _width = texIn.width;
            _height = texIn.height;
            _nGroupsX = _width / KernelNthreadsInGroup;
            _nGroupsY = _height / KernelNthreadsInGroup;
        }
        
        public void Rescale(Texture input, RenderTexture output, float scale)
        {
            if (_width != input.width ||_width != output.width) throw new InvalidOperationException();
            _shader.SetTexture(_indexRescale, KernelNameMask, input); // Set all textures just to be sure
            _shader.SetTexture(_indexRescale, KernelNameInput, input);
            _shader.SetTexture(_indexRescale, KernelNameResult, output);
            _shader.SetFloat(KernelNameScale, scale);
            output.DiscardContents();
            _shader.Dispatch(_indexRescale, _nGroupsX, _nGroupsY, 1);
        }

        public void Rescale(BufferedRenderTexture tex, float scale)
        {
            Init(tex);
            _shader.SetTexture(_indexRescale, KernelNameMask, tex.Read); // Set all textures just to be sure
            _shader.SetTexture(_indexRescale, KernelNameInput, tex.Read);
            _shader.SetTexture(_indexRescale, KernelNameResult, tex.Write);
            _shader.SetFloat(KernelNameScale, scale);
            tex.Write.DiscardContents();
            _shader.Dispatch(_indexRescale, _nGroupsX, _nGroupsY, 1);
            tex.Swap();
        }
        
        public void Mask(BufferedRenderTexture tex, Texture mask)
        {
            Init(tex);
            _shader.SetTexture(_indexMask, KernelNameInput, tex.Read);
            _shader.SetTexture(_indexMask, KernelNameMask, mask);
            _shader.SetTexture(_indexMask, KernelNameResult, tex.Write);
            _shader.Dispatch(_indexMask, _nGroupsX, _nGroupsY, 1);
            tex.Swap();
        }
        
        public void MaskInverted(BufferedRenderTexture tex, Texture mask)
        {
            Init(tex);
            _shader.SetTexture(_indexMaskInverted, KernelNameInput, tex.Read);
            _shader.SetTexture(_indexMaskInverted, KernelNameMask, mask);
            _shader.SetTexture(_indexMaskInverted, KernelNameResult, tex.Write);
            _shader.Dispatch(_indexMaskInverted, _nGroupsX, _nGroupsY, 1);
            tex.Swap();
        }
    }
}