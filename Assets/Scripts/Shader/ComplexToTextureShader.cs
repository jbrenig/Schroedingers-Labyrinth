using System;
using System.Collections.Generic;
using Lib;
using UnityEngine;

namespace Shader
{
    public class ComplexToTextureShader : IDisposable
    {
        public const int KernelNthreadsInGroup = 8;
        public const string KernelShaderData = "Data";
        public const string KernelShaderDataOut = "Result";
        public const string KernelShaderMaxSq = "MaxSq";
        public const string KernelShaderDrawGrid = "DrawGrid";
        public const string KernelShaderGridSize = "GridSize";
        public const string KernelShaderOffset = "offset";
        
        public const string KernelShaderGridColor = "GridColor";
        public const string KernelShaderSaturation = "Saturation";
        
        private readonly ComputeShader _shader;
        private int _nGroupsX, _nGroupsY;
        private readonly List<RenderTexture> _shaderTexOut = new List<RenderTexture>();

        private readonly int _indexComplex;
        private int _width = -1;
        private int _height = -1;

        private int _flagOffsetX = -1;
        private int _flagOffsetY = -1;

        private Color _gridColor = new Color(0.3f, 0.3f, 0.3f);
        public Color GridColor
        {
            set
            {
                _gridColor = value;
                _shader.SetFloats(KernelShaderGridColor, value.r, value.g, value.b, value.a);
            }
            get => _gridColor;
        }

        private int _gridSize = 32;
        public int GridSize
        {
            get => _gridSize;
            set
            {
                _gridSize = value;
                _shader.SetInt(KernelShaderGridSize, value);
            }
        }

        public ComplexToTextureShader(ComputeShader shader)
        {
            _shader = shader;
            _indexComplex = _shader.FindKernel("ConvertComplex");
        }
        
        #region IDisposable implementation
        public void Dispose () { Release(); }
        #endregion
        
        void Release() {
            foreach (var texture in _shaderTexOut)
            {
                texture.Release();
            }
            _shaderTexOut.Clear();
            _width = -1;
            _height = -1;
        }
        
        public void Init(BufferedRenderTexture texIn) {
            if (_width == texIn.Width && _height == texIn.Height)
                return;

            Release();
            _width = texIn.Width;
            _height = texIn.Height;
            _nGroupsX = _width / KernelNthreadsInGroup;
            _nGroupsY = _height / KernelNthreadsInGroup;

            _flagOffsetX = _width > _height ? 1 : 0;
            _flagOffsetY = _width < _height ? 1 : 0;

            var min = Mathf.Min(_width, _height);
            var max = Mathf.Max(_width, _height);
            var count = max / min;

            for (int i = 0; i < count; i++)
            {
                var tex = new RenderTexture(min, min, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                tex.filterMode = FilterMode.Bilinear;
                tex.wrapMode = TextureWrapMode.Repeat;
                tex.enableRandomWrite = true;
                tex.Create();
                _shaderTexOut.Add(tex);
            }

            // init params
            GridColor = _gridColor;
            GridSize = _gridSize;
        }

        public List<RenderTexture> CreateTextureSq(BufferedRenderTexture tex, float maxSq, bool drawGrid = true)
        {
            return CreateTexture(tex, maxSq, drawGrid, 0);
        }

        public List<RenderTexture> CreateTexture(BufferedRenderTexture tex, float maxSq, bool drawGrid = true, float saturation = 1)
        {
            Init(tex);
            _shader.SetTexture(_indexComplex, KernelShaderData, tex.Read);
            _shader.SetFloat(KernelShaderMaxSq, maxSq);
            _shader.SetFloat(KernelShaderSaturation, saturation);
            _shader.SetBool(KernelShaderDrawGrid, drawGrid);
            
            for (int i = 0; i < _shaderTexOut.Count; i++)
            {
                _shader.SetInts(KernelShaderOffset, _flagOffsetX * i * _height, _flagOffsetY * i * _width);
                _shader.SetTexture(_indexComplex, KernelShaderDataOut, _shaderTexOut[i]);
                _shaderTexOut[i].DiscardContents();
                _shader.Dispatch(_indexComplex, _nGroupsX, _nGroupsY, 1);
            }
            return _shaderTexOut;
        }

        public int TextureCount() => _shaderTexOut.Count;
        
        public List<RenderTexture> CreateDebugTexture(BufferedRenderTexture tex, int index, float maxSq, RenderTexture targetTexture)
        {
            Init(tex);
            _shader.SetTexture(_indexComplex, KernelShaderData, tex.Read);
            _shader.SetFloat(KernelShaderMaxSq, maxSq);
            _shader.SetFloat(KernelShaderSaturation, 1);
            _shader.SetBool(KernelShaderDrawGrid, false);
            
            _shader.SetInts(KernelShaderOffset, _flagOffsetX * index * _height, _flagOffsetY * index * _width);
            _shader.SetTexture(_indexComplex, KernelShaderDataOut, targetTexture);
            _shader.Dispatch(_indexComplex, _nGroupsX, _nGroupsY, 1);
            return _shaderTexOut;
        }
    }
}