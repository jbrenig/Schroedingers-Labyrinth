using System;
using UnityEngine;

namespace Shader
{
    public class BlockShader
    {
        private const string KernelNameBlock = "Block";
        private const string KernelNameTriangle1 = "Triangle1";
        private const string KernelNameTriangle2 = "Triangle2";
        private const string KernelNameTriangle1Inv = "Triangle1Inv";
        private const string KernelNameTriangle2Inv = "Triangle2Inv";
        private const string ParamNameInput = "Input";
        private const string ParamNameResult = "Result";
        private const string ParamNameStart = "start";
        private const string ParamNameEnd = "end";
        private const string ParamNameUseMax = "use_max";
        private const string ParamNameValue = "height";
        
        
        private const int KernelNthreadsInGroup = 8;
        
        private readonly ComputeShader _shader;
        
        private readonly int _indexBlock;
        private readonly int _indexTriangle1;
        private readonly int _indexTriangle2;
        private readonly int _indexTriangle1Inv;
        private readonly int _indexTriangle2Inv;
        
        public enum TriangleForm
        {
            Tri1, Tri2, Tri1Inv, Tri2Inv
        }

        public BlockShader(ComputeShader shader)
        {
            _shader = shader;
            _indexBlock = _shader.FindKernel(KernelNameBlock);
            _indexTriangle1 = _shader.FindKernel(KernelNameTriangle1);
            _indexTriangle2 = _shader.FindKernel(KernelNameTriangle2);
            _indexTriangle1Inv = _shader.FindKernel(KernelNameTriangle1Inv);
            _indexTriangle2Inv = _shader.FindKernel(KernelNameTriangle2Inv);
        }

        public void Wall(Texture input, RenderTexture output, Vector2Int start, Vector2Int end)
        {
            Block(input, output, start, end,  1,true);
        }

        public void Hole(Texture input, RenderTexture output, Vector2Int start, Vector2Int end)
        {
            Block(input, output, start, end, 0, false);
        }

        public void Block(Texture input, RenderTexture output, Vector2Int start, Vector2Int end, float height, bool useMax)
        {
            if (input.width != output.width ||
                input.height != output.height)
            {
                throw new InvalidOperationException("Textures need to be the same size!");
            }
            
            _shader.SetTexture(_indexBlock, ParamNameInput, input);
            _shader.SetTexture(_indexBlock, ParamNameResult, output);
            _shader.SetBool(ParamNameUseMax, useMax);
            _shader.SetFloat(ParamNameValue, height);
            _shader.SetInts(ParamNameStart, Mathf.Max(0, start.x), Mathf.Max(0, start.y));
            _shader.SetInts(ParamNameEnd, Mathf.Min(input.width, end.x), Mathf.Min(input.height, end.y));
            output.DiscardContents();
            
            var nGroupsX = input.width / KernelNthreadsInGroup;
            var nGroupsY = input.height / KernelNthreadsInGroup;
            
            _shader.Dispatch(_indexBlock, nGroupsX, nGroupsY,1);
        }

        public void Triangle(Texture input, RenderTexture output, Vector2Int start, Vector2Int end, float height, bool useMax, TriangleForm triangle)
        {
            if (input.width != output.width ||
                input.height != output.height)
            {
                throw new InvalidOperationException("Textures need to be the same size!");
            }

            int index;
            
            switch (triangle)
            {
                case TriangleForm.Tri1:
                    index = _indexTriangle1;
                    break;
                case TriangleForm.Tri2:
                    index = _indexTriangle2;
                    break;
                case TriangleForm.Tri1Inv:
                    index = _indexTriangle1Inv;
                    break;
                case TriangleForm.Tri2Inv:
                    index = _indexTriangle2Inv;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(triangle), triangle, null);
            }
            
            _shader.SetTexture(index, ParamNameInput, input);
            _shader.SetTexture(index, ParamNameResult, output);
            _shader.SetBool(ParamNameUseMax, useMax);
            _shader.SetFloat(ParamNameValue, height);
            _shader.SetInts(ParamNameStart, start.x, start.y);
            _shader.SetInts(ParamNameEnd, end.x, end.y);
            output.DiscardContents();
            
            var nGroupsX = input.width / KernelNthreadsInGroup;
            var nGroupsY = input.height / KernelNthreadsInGroup;
            
            _shader.Dispatch(index, nGroupsX, nGroupsY,1);
        }
    }
}