using System;
using UnityEngine;

namespace Shader
{
    public class DoubleSlitShader
    {
        private const string KernelNameX = "DoubleSlitX";
        private const string KernelNameY = "DoubleSlitY";
        
        private const string ParamNameInput = "Input";
        private const string ParamNameResult = "Result";
        private const string ParamNamePosition = "pos";
        private const string ParamNameSize = "size";
        private const string ParamNameHeight = "height";
        private const string ParamNameSlitWidth = "slit_width";
        private const string ParamNameSlitDistance = "slit_distance";
        private const string ParamNameUseMax = "use_max";
        
        
        private const int KernelNthreadsInGroup = 8;
        
        private readonly ComputeShader _shader;
        
        private readonly int _indexX;
        private readonly int _indexY;
        
        public DoubleSlitShader(ComputeShader shader)
        {
            _shader = shader;
            _indexX = _shader.FindKernel(KernelNameX);
            _indexY = _shader.FindKernel(KernelNameY);
        }


        public void DoubleSlit(Texture input, RenderTexture output, Vector2Int position, Vector2Int size, float height, int slitWidth, int slitDistance, bool directionY, bool useMax)
        {
            if (input.width != output.width ||
                input.height != output.height)
            {
                throw new InvalidOperationException("Textures need to be the same size!");
            }

            var index = directionY ? _indexY : _indexX;
            
            _shader.SetTexture(index, ParamNameInput, input);
            _shader.SetTexture(index, ParamNameResult, output);
            _shader.SetInt(ParamNameSlitWidth, slitWidth);
            _shader.SetBool(ParamNameUseMax, useMax);
            _shader.SetInt(ParamNameSlitDistance, slitDistance);
            _shader.SetFloat(ParamNameHeight, height);
            _shader.SetInts(ParamNamePosition, position.x, position.y);
            _shader.SetInts(ParamNameSize, size.x, size.y);
            output.DiscardContents();
            
            var nGroupsX = input.width / KernelNthreadsInGroup;
            var nGroupsY = input.height / KernelNthreadsInGroup;
            
            _shader.Dispatch(index, nGroupsX, nGroupsY,1);
        }
    }
}