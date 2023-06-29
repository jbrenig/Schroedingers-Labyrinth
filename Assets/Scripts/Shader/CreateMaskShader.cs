using UnityEngine;

namespace Shader
{
    public class CreateMaskShader
    {
        public const int KernelNthreadsInGroup = 8;
        
        public const string KernelNameResult = "Result";
        public const string KernelNameStartX = "startX";
        public const string KernelNameStartY = "startY";
        public const string KernelNameEndX = "endX";
        public const string KernelNameEndY = "endY";
        
        private readonly ComputeShader _shader;
        private readonly int _index;

        public CreateMaskShader(ComputeShader shader)
        {
            _shader = shader;
            _index = _shader.FindKernel("CreateMask");
        }
        
        public void Create(Texture output, int startX, int startY, int endX, int endY)
        {
            var nGroupsX = output.width / KernelNthreadsInGroup;
            var nGroupsY = output.height / KernelNthreadsInGroup;
            _shader.SetTexture(_index, KernelNameResult, output);
            _shader.SetInt(KernelNameStartX, startX);
            _shader.SetInt(KernelNameStartY, startY);
            _shader.SetInt(KernelNameEndX, endX);
            _shader.SetInt(KernelNameEndY, endY);
            _shader.Dispatch(_index, nGroupsX, nGroupsY, 1);
        }
    }
}