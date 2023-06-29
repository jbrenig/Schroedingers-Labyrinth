using System;
using System.Collections.Generic;
using System.IO;
using Lib;
using Shader;
using UnityEngine;


namespace PotentialFunctions
{
    /// <summary>
    /// Manages multiple potential functions that will be combined.
    ///
    /// Also applies potential to the heightmap of the terrains
    /// </summary>
    [ExecuteAlways]
    public class PotentialManager : MonoBehaviour
    {
        public ComputeShader rescaleShader;

        public Material gaussFilterMaterial;
        public int gaussianFilterIterations = 4;
        
        /// <summary>
        /// Potential Functions to apply. Will be applied top to bottom.
        /// </summary>
        private readonly List<IPotential> _potentialFunctions = new List<IPotential>();

        private bool _isDirty = true;
        
        private BufferedRenderTexture _potentialHeightmap;
        public BufferedRenderTexture Potential { get; private set; }

        [Tooltip("Editor only. Export path for the export potential button")]
        public string ExportTexturePath;

        private bool _terrainTilingDirectionIsX = false;
        private int _terrainTilingCount;
        private Terrain _rootTerrain;
        
        private RescaleShader _rescaleShader;

        public interface IPotential
        {
            void ApplyPotential(BufferedRenderTexture renderTexture);
            int GetPriority();

            /// <summary>
            /// Used in editor to check whether the potential belongs to the current scene.
            /// </summary>
            bool IsValid();
        }

        private class PotentialSorter : IComparer<IPotential>
        {
            public int Compare(IPotential x, IPotential y)
            {
                if (x == null || y == null) throw new InvalidOperationException();
                return x.GetPriority().CompareTo(y.GetPriority());
            }
        }

        private static readonly PotentialSorter Sorter = new PotentialSorter();

        #region Singleton

        private static PotentialManager _instance = null;

        public static PotentialManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<PotentialManager>();
                }

                return _instance;
            }
        }

        #endregion

        public void RegisterPotentialFunction(IPotential potential)
        {
            if (!_potentialFunctions.Contains(potential))
            {
                _potentialFunctions.Add(potential);
            }

            _potentialFunctions.Sort(Sorter);
            _isDirty = true;
        }
        
        public void UnregisterPotentialFunction(IPotential potential)
        {
            _potentialFunctions.Remove(potential);
            _isDirty = true;
        }

        /// <summary>
        /// Updates terrain heightmaps
        /// </summary>
        public void UpdatePotentialTextureToTerrainHeightmap()
        {
            if (_rootTerrain == null)
            {
                return;
            }
            // rescale potential between 0 and 0.5 due to limitations of unity terrain
            // due to inaccuracies of different gpus, actually scale to a value slightly lower than 0.5
            _rescaleShader.Rescale(Potential.Read, _potentialHeightmap.Write, 0.4999f);
            _potentialHeightmap.Swap();

            for (int i = 0; i < gaussianFilterIterations; i++)
            {
                Graphics.Blit(_potentialHeightmap.Read, _potentialHeightmap.Write, gaussFilterMaterial);
                _potentialHeightmap.Swap();
            }
            
            RenderTexture.active = _potentialHeightmap.Read;
            var simulator = GpuQuantumSimulator.Instance;
            var terrainResolution = Mathf.Min(simulator.width, simulator.height);
            var currentTerrain = _rootTerrain;
            for (int i = 0; i < _terrainTilingCount; i++)
            {
                // Use 2^n+1 sizes for heightmaps
                // only the last heightmap will not be able to use this size, since the potential has dimensions of 2^n
                var sizeX = _terrainTilingDirectionIsX && i < _terrainTilingCount - 1 ? terrainResolution + 1 : terrainResolution;
                var sizeY = !_terrainTilingDirectionIsX && i < _terrainTilingCount - 1 ? terrainResolution + 1 : terrainResolution;
                // calculate offsets
                var x = _terrainTilingDirectionIsX ? i : 0;
                var y = !_terrainTilingDirectionIsX ? i : 0;
                currentTerrain.terrainData.CopyActiveRenderTextureToHeightmap(new RectInt(x * terrainResolution, y * terrainResolution, sizeX, sizeY), new Vector2Int(0, 0), TerrainHeightmapSyncControl.HeightAndLod);
                currentTerrain = _terrainTilingDirectionIsX ? currentTerrain.rightNeighbor : currentTerrain.topNeighbor;
                // TODO: texture tiling in the y-direction might be incorrect
            }
            RenderTexture.active = null;
        }

        private void Awake()
        {
            var simulator = GpuQuantumSimulator.Instance;
            // Create Heightmap to apply to the terrain
            _potentialHeightmap?.Dispose();
            _potentialHeightmap = new BufferedRenderTexture(simulator.width, simulator.height);
            _potentialHeightmap.Init(format:RenderTextureFormat.RFloat);
            
            // Create potential texture
            Potential?.Dispose();
            Potential = new BufferedRenderTexture(simulator.width, simulator.height);
            Potential.Init(format:RenderTextureFormat.RFloat);
            
            // Init shaders
            _rescaleShader = new RescaleShader(rescaleShader);
            _rescaleShader.Init(Potential);
            
            // connect Terrain
            UnityEngine.TerrainUtils.TerrainUtility.AutoConnect();
            _rootTerrain = Terrain.activeTerrain;
            if (_rootTerrain != null)
            {
                while (_rootTerrain.bottomNeighbor != null)
                {
                    _rootTerrain = _rootTerrain.bottomNeighbor;
                }

                while (_rootTerrain.leftNeighbor != null)
                {
                    _rootTerrain = _rootTerrain.leftNeighbor;
                }
            }

            _terrainTilingDirectionIsX = simulator.width > simulator.height;
            _terrainTilingCount = Mathf.Max(simulator.width, simulator.height) / Mathf.Min(simulator.width, simulator.height);
            _isDirty = true;
        }

        private void OnDestroy()
        {
            _instance = null;
            _potentialFunctions.Clear();
        }

        public void Update()
        {
#if UNITY_EDITOR
            if (UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null) return;
#endif
            
            if (_isDirty || !Application.isPlaying)
            {
                _isDirty = false;
                UpdatePotential();
            }
        }

        public void RequestUpdate()
        {
            _isDirty = true;
        }

        public void UpdatePotential()
        {
            if (Potential == null) Awake();

            if (Application.isEditor)
            {
                // sanity check in editor to remove dead references
                for (int i = _potentialFunctions.Count - 1; i >= 0; i--)
                {
                    if (_potentialFunctions[i] == null || !_potentialFunctions[i].IsValid())
                    {
                        _potentialFunctions.RemoveAt(i);
                    }
                }
            }

            foreach (var potential in _potentialFunctions)
            {
                potential.ApplyPotential(Potential);
            }

            UpdatePotentialTextureToTerrainHeightmap();
        }

        public void BtnExportTexture()
        {
            if (string.IsNullOrEmpty(ExportTexturePath)) return;

            Texture2D outTex = new Texture2D(Potential.Width, Potential.Height, TextureFormat.RFloat, false, true)
            {
                filterMode = FilterMode.Point, 
                wrapMode = TextureWrapMode.Clamp
            };

            RenderTexture.active = Potential.Read;
            
            outTex.ReadPixels(new Rect(0, 0, Potential.Width, Potential.Height), 0, 0, false);
            outTex.Apply(false);
            var result = outTex.EncodeToPNG();

            if (!ExportTexturePath.EndsWith(".png"))
            {
                if (Directory.Exists(ExportTexturePath))
                {
                    ExportTexturePath += "/exported.png";
                }
                else
                {
                    ExportTexturePath += ".png";
                }
            }
            
            File.WriteAllBytes(ExportTexturePath, result);
        }
    }
}