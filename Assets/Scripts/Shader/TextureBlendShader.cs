using System;
using UnityEngine;

namespace Shader
{
    /// <summary>
    /// Helper class for the Blend compute shader
    /// </summary>
    public class TextureBlendShader
    {
        private const string KernelNameBlendMax = "BlendMax";
        private const string KernelNameBlendMin = "BlendMin";
        private const string ParamNameInput1 = "Input1";
        private const string ParamNameInput2 = "Input2";
        private const string ParamNameResult = "Result";
        private const string ParamNameOffset = "offset";
        private const string ParamNameSize = "input2_size";
        private const string ParamNameFlipX = "flip_x";
        private const string ParamNameFlipY = "flip_y";
        private const string ParamNameMultiplier = "multiplier";
        
        private const int KernelNthreadsInGroup = 8;
        
        private readonly ComputeShader _shader;

        
        private readonly int _indexMax;
        private readonly int _indexMin;
        
        public TextureBlendShader(ComputeShader shader)
        {
            _shader = shader;
            _indexMax = _shader.FindKernel(KernelNameBlendMax);
            _indexMin = _shader.FindKernel(KernelNameBlendMin);
        }

        public void MinBlend(Texture input1, Texture input2, RenderTexture output, Vector2Int offset, bool flipX, bool flipY, float multiplier)
        {
            Blend(_indexMax, input1, input2, output, offset, flipX, flipY, multiplier);
        }

        public void MaxBlend(Texture input1, Texture input2, RenderTexture output, Vector2Int offset, bool flipX, bool flipY, float multiplier)
        {
            Blend(_indexMax, input1, input2, output, offset, flipX, flipY, multiplier);
        }
        
        private void Blend(int index, Texture input1, Texture input2, RenderTexture output, Vector2Int offset, bool flipX, bool flipY, float multiplier)
        {
            if (input1.width != output.width ||
                input1.height != output.height)
            {
                throw new InvalidOperationException("Textures need to be the same size!");
            }
            
            _shader.SetTexture(index, ParamNameInput1, input1);
            _shader.SetTexture(index, ParamNameInput2, input2);
            _shader.SetTexture(index, ParamNameResult, output);
            _shader.SetBool(ParamNameFlipX, flipX);
            _shader.SetBool(ParamNameFlipY, flipY);
            _shader.SetFloat(ParamNameMultiplier, multiplier);
            _shader.SetInts(ParamNameOffset, offset.x, offset.y);
            _shader.SetInts(ParamNameSize, input2.width, input2.height);
            output.DiscardContents();
            
            var nGroupsX = input1.width / KernelNthreadsInGroup;
            var nGroupsY = input1.height / KernelNthreadsInGroup;
            
            _shader.Dispatch(index, nGroupsX, nGroupsY,1);
        }
        
    }
}