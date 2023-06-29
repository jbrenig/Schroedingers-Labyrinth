using System;
using Game;
using Lib;
using PotentialFunctions;
using Shader;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class GpuQuantumSimulator : MonoBehaviour, IDisposable
{
    /// <summary>
    /// Width of the simulation (the simulation is always square)
    /// Should be a power of 2
    /// </summary>
    [Tooltip("Width of the simulation (the simulation is always square)\nShould be a power of 2")]
    public int width = 1024;
    
    /// <summary>
    /// Height of the simulation (the simulation is always square)
    /// Should be a power of 2
    /// </summary>
    [Tooltip("Height of the simulation (the simulation is always square)\nShould be a power of 2")]
    public int height = 1024;

    [Tooltip("Width of the labyrinth in world space")]
    public float worldWidth = 256;
    [Tooltip("Height of the labyrinth in world space")]
    public float worldHeight = 256;
    
    [Tooltip("Material associated associated with each terrain.")]
    public Material[] materials;

    [Header("Physics Settings")]
    [Tooltip("Spatial scale of the simulation (in meters per pixel)")]
    public float scale = (float) (1e-1 * Constants.Scale.Nano);
    
    [Tooltip("Time of one simulation step")]
    public float timeStep = (float) (1e-1 * Constants.Scale.Femto);

    [FormerlySerializedAs("timeStepQualityMultiplier")] [Tooltip("Multiplier for timestep.")]
    public float timeStepMultiplier = 2;

    [Tooltip("Multiplier for timestep. Used in fastforward mode.")]
    public int iterationMultiplier = 1;

    [Tooltip("Normalize wavefunction?  Gets overriden by user settings.")]
    public bool normalizeAfterEveryFrame = false;
    
    [Tooltip("Mass of the simulated particle in kg")]
    public float mass = (float) Constants.Mass.Electron;
    
    [Tooltip("Scale of the potential. Default is 1eV. The unscaled potential will range from 0 to 1.")]
    public float potentialScale = (float) (1 * Constants.ElectronVolt);

    [Tooltip("Scale multiplier of the potential. Use this to easily modify the potential scale. Default is 2.")]
    public float potentialScaleMultiplier = 2;
    
    [Tooltip("Offset of the potential. Will be applied before scaling.")]
    public float potentialOffset = 1;

    [Tooltip("Rotation of the potential around the y axis. Will be applied before scaling.\nThis value represents the additional offset at the +x end of the simulation.")]
    public float potentialRotationX = 0;
    
    [Tooltip("Rotation of the potential around the x axis. Will be applied before scaling.\nThis value represents the additional offset at the +y end of the simulation.")]
    public float potentialRotationY = 0;

    [Header("Wave Function")]
    [Tooltip("Initial position of the spawned wavepacket. In pixel coordinates.")]
    public Vector2 wavePacketStart = new Vector2(200, 512);
    
    [Tooltip("Initial size of the spawned wavepacket. In pixel coordinates.")]
    public Vector2 wavePacketSize = new Vector2(50*50, 50*50);
    
    [Tooltip("Initial average energy of the spawned wavepacket in electron volt.")]
    public float initialEnergy = 1;
    
    [Tooltip("Initial wave number k. (In pixel coordinates)")]
    public Vector2 initialWaveNumber;

    [Tooltip("If set to true, wavefunction will be initialized using the given wave number instead of energy.")]
    public bool initializeWithWaveNumber = false;
    

    [Header("Simulator")]
    [Tooltip("Whether to run the simulation on startup.")]
    public bool isRunning = false;
    
    [Tooltip("Whether to render the wavefunction (showPhase = true) or the probability density (showPhase = false).")]
    public bool showPhase = false;

    [Tooltip("Whether to draw a grid on top of the particle wavefunction texture.")]
    public bool drawGrid = false;
    
    [Tooltip("Simulation iterations per frame. Use this to speed up the simulation (at cost of computing power).")]
    [Range(1, 10)]
    public int iterations = 1;

    [Header("Shaders and target material")]
    public ComputeShader fftShader;
    public ComputeShader quantumShader;
    public ComputeShader convertShader;
    public ComputeShader maxMagnitudeShader;
    public ComputeShader parallelSumShader;
    public ComputeShader waveFunctionShader;
    public ComputeShader rescaleShader;
    
    public Vector2Int Size => new Vector2Int(width, height);

    public float ActualTimeStep => timeStepMultiplier * timeStep;

    public BufferedRenderTexture CurrentWaveFunction => _waveFunction;
    
    public float LastMaxMagnitude { get; private set; }
    public Action<BufferedRenderTexture> FFTTextureHandler { get; set; }

#if UNITY_EDITOR
    public Material debugFftMaterial1;
    public Material debugFftMaterial2;

    private RenderTexture debugFftTexture1;
    private RenderTexture debugFftTexture2;
#endif
    
    #region Private Data

    private BufferedRenderTexture _waveFunction;
    
    private FFTShader _fftShader;
    private SplitOperatorShader _quantumShader;
    private ComplexToTextureShader _convertShader;
    private MaxMagnitudeShader _maxMagnitudeShader;
    private ParallelSumShader _parallelSumShader;
    private WaveFunctionShader _waveFunctionShader;
    private RescaleShader _rescaleShader;

    private double _timePassed = 0;

    private volatile bool _init = false;

    private static readonly int EmissionMap = UnityEngine.Shader.PropertyToID("_EmissionMap");

    #endregion

    #region Singleton

    private static GpuQuantumSimulator _instance;
    public static GpuQuantumSimulator Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GpuQuantumSimulator>();
            }
            
            _instance.Init();
            return _instance;
        }
    }

    #endregion

    #region Unity Events

    private void OnEnable()
    {
        Init();
    }

    private void OnDisable()
    {
        Dispose();
    }

    private void OnValidate()
    {
        Assert.IsTrue(Mathf.IsPowerOfTwo(width));
        Assert.IsTrue(Mathf.IsPowerOfTwo(height));
        
        if (Application.isPlaying) return;
        Init();
        if (_init && _waveFunctionShader == null) return;
        ResetLevel();
    }

    private void Reset()
    {
        Dispose();
        Init();
        ResetLevel();
    }

    private void Start()
    {
        ResetLevel();
    }

    private void FixedUpdate()
    {
        DoSimulationStep();
        CreateTexture();
    }

    private void OnDestroy()
    {
        Dispose();
    }

    private void OnApplicationQuit()
    {
        Dispose();
    }

    #endregion

    private void LoadUserData()
    {
        normalizeAfterEveryFrame = UserPreferences.NormalizeEveryFrame;
        drawGrid = UserPreferences.DrawGrid;
    }

    public void OnUserSettingsChanged()
    {
        LoadUserData();
        // update shader info
        _quantumShader.TimeStep = ActualTimeStep;
    }

    private void Init()
    {
        if (_init) return;
        _init = true;
        
        // Load User Values
        LoadUserData();
        
        // Init shaders
        _waveFunction = new BufferedRenderTexture(new Vector2Int(width, height));
        _waveFunction.Init();

        _fftShader = new FFTShader(fftShader);

        _quantumShader = new SplitOperatorShader(quantumShader, (float) Constants.H)
        {
            Scale = scale,
            Mass = mass,
            TimeStep = ActualTimeStep,
            PotentialScale = potentialScale * potentialScaleMultiplier,
            PotentialOffset = potentialOffset,
            PotentialRotationX = potentialRotationX,
            PotentialRotationY = potentialRotationY
        };

        _convertShader = new ComplexToTextureShader(convertShader);
        _maxMagnitudeShader = new MaxMagnitudeShader(maxMagnitudeShader);
        _parallelSumShader = new ParallelSumShader(parallelSumShader);
        _waveFunctionShader = new WaveFunctionShader(waveFunctionShader);
        _rescaleShader = new RescaleShader(rescaleShader);
        
        _quantumShader.Init(_waveFunction);
        _fftShader.Init(_waveFunction);
        _convertShader.Init(_waveFunction);
        _maxMagnitudeShader.Init(_waveFunction);
        _parallelSumShader.Init(_waveFunction);
        _waveFunctionShader.Init(_waveFunction);
        _rescaleShader.Init(_waveFunction);
    }

    private void CreateTexture()
    {
        // Create and set texture
        LastMaxMagnitude = _maxMagnitudeShader.GetMax(_waveFunction);
        if (LastMaxMagnitude == 0) LastMaxMagnitude = 1;

        var textures = showPhase ? 
            _convertShader.CreateTexture(_waveFunction, LastMaxMagnitude, drawGrid) 
            : _convertShader.CreateTextureSq(_waveFunction, LastMaxMagnitude, drawGrid);
        
        for (var i = 0; i < textures.Count; i++)
        {
            materials[i].mainTexture = textures[i];
            materials[i].SetTexture(EmissionMap, textures[i]);
        }
    }

    public void DoSimulationStep()
    {
        if (!isRunning) return;
        

        for (int i = 0; i < iterations * iterationMultiplier; i++)
        {
            _quantumShader.ApplyPotential(_waveFunction, PotentialManager.Instance.Potential.Read);
            _fftShader.Forward(_waveFunction);
            
            
            
#if UNITY_EDITOR && CREATE_DEBUG_TEXTURES
            
            if (debugFftTexture1 == null)
            {
                debugFftTexture1 = new RenderTexture(512, 512, 1);
                debugFftTexture1.enableRandomWrite = true;
                debugFftTexture1.Create();
            }
        
            if (debugFftTexture2 == null)
            {
                debugFftTexture2 = new RenderTexture(512, 512, 1);
                debugFftTexture2.enableRandomWrite = true;
                debugFftTexture2.Create();
            }
            // Create and set texture
            var maxMag = _maxMagnitudeShader.GetMax(_waveFunction);
            if (maxMag == 0) maxMag = 1;

            _convertShader.CreateDebugTexture(_waveFunction, 0,  maxMag, debugFftTexture1);
            if (_convertShader.TextureCount() > 1)
                _convertShader.CreateDebugTexture(_waveFunction, 1, maxMag, debugFftTexture2);

            if (debugFftMaterial1 != null)
                debugFftMaterial1.mainTexture = debugFftTexture1;
            if (debugFftMaterial2 != null)
                debugFftMaterial2.mainTexture = debugFftTexture2;
#endif
            _quantumShader.ApplyMomentum(_waveFunction);
            FFTTextureHandler?.Invoke(_waveFunction);
            
            _fftShader.Backward(_waveFunction);
            _quantumShader.ApplyPotential(_waveFunction, PotentialManager.Instance.Potential.Read);
        }

        _timePassed += iterations * ActualTimeStep;

        if (normalizeAfterEveryFrame)
        {
            Normalize();
        }
    }
    

    public void SetPotentialRotationX(float value)
    {
        potentialRotationX = value;
        _quantumShader.PotentialRotationX = value;
    }
    
    public void SetPotentialRotationY(float value)
    {
        potentialRotationY = value;
        _quantumShader.PotentialRotationY = value;
    }

    public float SampleProbabilities(Texture scoringArea)
    {
        return _parallelSumShader.GetSumOfComplexSq(_waveFunction, scoringArea);
    }

    public void ApplyMask(Texture mask, bool inverted)
    {
        if (inverted)
        {
            _rescaleShader.MaskInverted(_waveFunction, mask);
        }
        else
        {
            _rescaleShader.Mask(_waveFunction, mask);
        }
    }

    public double GetTimePassed() => _timePassed;

    public void ResetLevel()
    {
        _timePassed = 0;
        if (initializeWithWaveNumber)
        {
            initialEnergy = (float) ((initialWaveNumber.SqrMagnitude() * Constants.HBarSq) / (Constants.ElectronVolt * Constants.Mass.Electron * scale * scale));
        }
        else
        {
            var kx = Math.Sqrt(Constants.ElectronVolt * initialEnergy * Constants.Mass.Electron * scale * scale / Constants.HBarSq);
            initialWaveNumber = new Vector2((float) kx, 0);
        }
        
        _waveFunctionShader.CreateWavePacket(_waveFunction, 
            wavePacketStart, 
            wavePacketSize, 
            initialWaveNumber);
        Normalize();
        CreateTexture();
    }
    
    public void Normalize()
    {
        var sum = _parallelSumShader.GetSumOfComplexSq(_waveFunction);
        _rescaleShader.Rescale(_waveFunction, (float) (1.0 / Math.Sqrt(sum)));
    }

    public Vector2Int WorldToSimulationCoordinate(Vector3 worldCoord)
    {
        var origin = transform.position;
        var halfWidth = worldWidth / 2;
        var halfHeight = worldHeight / 2;
        // coordinate clamped and translated
        var world2D = new Vector2(Mathf.Clamp(worldCoord.x - origin.x, -halfWidth, halfWidth) + halfWidth, Mathf.Clamp(worldCoord.z - origin.z, -halfHeight, halfHeight) + halfHeight);
        var rescaleX = width / worldWidth;
        var rescaleY = height / worldHeight;
        return new Vector2Int((int) (world2D.x * rescaleX), (int) (world2D.y * rescaleY));
    }

    public Vector3 SimulationToWorldCoordinate(Vector2Int simulationCoord)
    {
        var origin = transform.position;
        var halfWidth = worldWidth / 2;
        var halfHeight = worldHeight / 2;
        var rescaleX = width / worldWidth;
        var rescaleY = height / worldHeight;
        // rescaled and translated
        var world2D = new Vector2(simulationCoord.x / rescaleX - halfWidth, simulationCoord.y / rescaleY - halfHeight);
        return new Vector3(origin.x + world2D.x, origin.y, origin.z + world2D.y);
    }

    public void OnEditorUpdate()
    {
        if (_init) Dispose();
        Init();
        ResetLevel();
    }

    public void Dispose()
    {
        _init = false;
        _instance = null;
        _waveFunction?.Dispose();
        _fftShader?.Dispose();
        _convertShader?.Dispose();
        _maxMagnitudeShader?.Dispose();
        _parallelSumShader?.Dispose();
    }
}
