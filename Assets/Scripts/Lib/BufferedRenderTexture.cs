using System;
using UnityEngine;

namespace Lib
{
    public class BufferedRenderTexture : IDisposable
    {
        public RenderTexture Read { get; private set; }
        public RenderTexture Write { get; private set; }

        private bool _isInit = false;
        private bool _isDisposed = false;

        public readonly Vector2Int Size;
        public int Width => Size.x;
        public int Height => Size.y;

        public BufferedRenderTexture(Vector2Int size)
        {
            Size = size;
        }

        public BufferedRenderTexture(int simulatorWidth, int simulatorHeight)
        {
            Size = new Vector2Int(simulatorWidth, simulatorHeight);
        }

        public void Init(RenderTextureFormat format = RenderTextureFormat.RGFloat, RenderTextureReadWrite readWrite = RenderTextureReadWrite.Linear, FilterMode filterMode = FilterMode.Point, TextureWrapMode wrapMode = TextureWrapMode.Clamp)
        {
            if (_isInit)
            {
                throw new InvalidOperationException();
            }

            _isInit = true;

            Read = new RenderTexture(Size.x, Size.y, 0, format, readWrite)
            {
                enableRandomWrite = true, 
                filterMode = filterMode, 
                wrapMode = wrapMode
            };
            Read.Create();

            Write = new RenderTexture(Size.x, Size.y, 0, format, readWrite)
            {
                enableRandomWrite = true, 
                filterMode = filterMode, 
                wrapMode = wrapMode
            };
            Write.Create();
        }

        public void Swap()
        {
            if (!_isInit) throw new InvalidOperationException();
            var oldRead = Read;
            Read = Write;
            Write = oldRead;
        }

        public void Dispose()
        {
            if (!_isInit) return;
            if (_isDisposed) return;
            _isDisposed = true;
            Read.Release();
            Write.Release();
        }
    }
}