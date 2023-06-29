using Shader;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    /// <summary>
    /// Use to align pillars & Particle system of a resizable area inside the xz-plane
    /// </summary>
    public class ResizableArea : MonoBehaviour
    {
        public UnityEvent<Texture> target;
        
        public Vector2Int size;

        public GameObject movingCornerLowerLeft;
        public GameObject movingCornerLowerRight;
        public GameObject movingCornerUpperLeft;
        public GameObject movingCornerUpperRight;
        public GameObject movingCenter;

        public float particleSpeed = 5;

        public ComputeShader shader;

        private RenderTexture _renderTexture;
        
        private void Start()
        {
            UpdateValues();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying) return;
            UpdateValues();
        }
#endif

        private void UpdateValues()
        {
            var halfX = size.x / 2;
            var halfY = size.y / 2;
            movingCornerLowerLeft.transform.localPosition = new Vector3(-halfX, 0, -halfY);
            movingCornerLowerRight.transform.localPosition = new Vector3(halfX, 0, -halfY);
            movingCornerUpperLeft.transform.localPosition = new Vector3(-halfX, 0, halfY);
            movingCornerUpperRight.transform.localPosition = new Vector3(halfX, 0, halfY);
            movingCenter.transform.localPosition = new Vector3(0, 0, 0);

            var timeForWidth = 1f / (particleSpeed / size.x);
            var timeForHeight = 1f / ( particleSpeed / size.y);

            var borderBottomMain = movingCornerLowerLeft.GetComponentInChildren<ParticleSystem>().main;
            var borderRightMain = movingCornerLowerRight.GetComponentInChildren<ParticleSystem>().main;
            var borderLeftMain = movingCornerUpperLeft.GetComponentInChildren<ParticleSystem>().main;
            var borderTopMain = movingCornerUpperRight.GetComponentInChildren<ParticleSystem>().main;
            
            borderBottomMain.startLifetime = new ParticleSystem.MinMaxCurve(timeForWidth);
            borderBottomMain.startSpeed = new ParticleSystem.MinMaxCurve(particleSpeed);
            borderTopMain.startLifetime = new ParticleSystem.MinMaxCurve(timeForWidth);
            borderTopMain.startSpeed = new ParticleSystem.MinMaxCurve(particleSpeed);
            borderRightMain.startLifetime = new ParticleSystem.MinMaxCurve(timeForHeight);
            borderRightMain.startSpeed = new ParticleSystem.MinMaxCurve(particleSpeed);
            borderLeftMain.startLifetime = new ParticleSystem.MinMaxCurve(timeForHeight);
            borderLeftMain.startSpeed = new ParticleSystem.MinMaxCurve(particleSpeed);

            if (target != null && shader != null)
            {
                var sim = GpuQuantumSimulator.Instance;
                var lower = sim.WorldToSimulationCoordinate(movingCornerLowerLeft.transform.position);
                var upper = sim.WorldToSimulationCoordinate(movingCornerUpperRight.transform.position);
                var maskShader = new CreateMaskShader( shader);
                
                if (_renderTexture != null)
                {
                    _renderTexture.Release();
                }   
                _renderTexture = new RenderTexture(sim.width, sim.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
                _renderTexture.enableRandomWrite = true;
                _renderTexture.Create();
                
                maskShader.Create(_renderTexture, lower.x, lower.y, upper.x, upper.y);
                target?.Invoke((Texture) _renderTexture);
            }
        }

        public void OnEditorUpdate()
        {
            UpdateValues();
        }
    }
}
