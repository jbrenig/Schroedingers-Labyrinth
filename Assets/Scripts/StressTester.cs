using System;
using System.Diagnostics;
using Game;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class StressTester : MonoBehaviour
{
    public Texture targetArea;

    public const float TargetFrameRate = 40;
    public const float TargetFrameTimeMs = 1000f / TargetFrameRate;
    
    public const float MinFrameRate = 10;
    public const float MinFrameTimeMs = 1000f / MinFrameRate;
    public const int TestIterations = 10;

    public const float TargetAccuracyDelta = 0.01f;
    
    void Start()
    {
        if (UserPreferences.IsNotFirstLaunch)
        {
            gameObject.SetActive(false);
            return;
        }

        UserPreferences.IsNotFirstLaunch = true;
    }

    void Update()
    {
        var sim = GpuQuantumSimulator.Instance;
        sim.isRunning = true;
        
        // do performance testing
        sim.iterations = 2;
        sim.timeStepMultiplier = 1;

        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < TestIterations; i++)
        {
            sim.DoSimulationStep();
        }
        stopwatch.Stop();
        var milliseconds = stopwatch.ElapsedMilliseconds;
        var avgPerFrame = milliseconds / (float) TestIterations;
        
        // TODO: show warnings to user
        if (avgPerFrame > TargetFrameTimeMs)
        {
            Debug.LogWarning("System not reaching recommended target frame-rate");
        }
        if (avgPerFrame > TargetFrameTimeMs)
        {
            Debug.LogError("System not reaching minimum frame-rate!");
        }

        // determine simulation accuracy
        var probabilities = sim.SampleProbabilities(targetArea);
        UserPreferences.NormalizeEveryFrame = !(Math.Abs(probabilities - 1) < TargetAccuracyDelta);
        UserPreferences.Save();
        
        Debug.Log($"Updated default settings to: accuracy control: {UserPreferences.NormalizeEveryFrame}");
        
        sim.isRunning = false;
        gameObject.SetActive(false);
    }

}
