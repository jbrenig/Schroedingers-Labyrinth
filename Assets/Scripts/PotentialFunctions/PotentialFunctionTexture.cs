using System;
using Lib;
using UnityEngine;

namespace PotentialFunctions
{
    public class PotentialFunctionTexture : MonoBehaviour, PotentialManager.IPotential
    {
        public Texture2D texture;

        private void Start()
        {
            PotentialManager.Instance.RegisterPotentialFunction(this);
        }

        private void OnValidate()
        {
            PotentialManager.Instance.RegisterPotentialFunction(this);
        }
        
        private void OnDestroy()
        {
            // ReSharper disable once Unity.NoNullPropagation
            PotentialManager.Instance?.UnregisterPotentialFunction(this);
        }

        public void ApplyPotential(BufferedRenderTexture renderTexture)
        {
            if (texture == null) throw new InvalidOperationException("texture cannot be null!");
        
            Graphics.Blit(texture, renderTexture.Write);
            renderTexture.Swap();
        }

        public int GetPriority()
        {
            return int.MinValue;
        }


        public bool IsValid()
        {
            return gameObject != null && gameObject.activeInHierarchy;
        }
    }
}
