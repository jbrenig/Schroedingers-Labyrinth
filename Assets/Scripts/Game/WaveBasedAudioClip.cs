using System;
using Lib;
using UnityEngine;
using UnityEngine.Audio;

namespace Game
{
    public class WaveBasedAudioClip : MonoBehaviour
    {
        public AudioMixer mixer;
        public int sampleRate = 44100;
        public float frequency = 440;

        public ComputeShader shader;

        public float maxPitchShift = 0.2f / 60f;
        public float pitchShiftFactor = 1f;
        public float pitchShiftOffset = 0f;

        private int _sampleCount;
        private int _waveSampleCount;
        private int _size;

        private int _kernelIndexProject;
        private int _kernelIndexWeightedAverages;
        private int _kernelIndexWaveNumbers;
        private ComputeBuffer _bufferIndices;
        private ComputeBuffer _bufferValues;
        private float[] _waveDataIndices;
        private float[] _waveDataValues;
        private int _kernelGroupsX;

        private const string KernelNameHalfSize = "HalfSize";
        private const string KernelNameSize = "Size";
        
        private const string KernelNameRadius = "radius";
        private const string KernelNamePhaseSamples = "phaseSamples";
        
        private const string KernelNameInput = "Input";
        private const string KernelNameResult1 = "ResultIndices";
        private const string KernelNameResult2 = "ResultValues";
        private const int KernelThreadsX = 64;

        private int _position = 0;

        private float _lastPitch = 1f;
        private float _currentTargetPitch = 1f;

        void Start()
        {
            _sampleCount = sampleRate * 2;
            _waveSampleCount = _sampleCount / 2;
            _kernelIndexWeightedAverages = shader.FindKernel("WeightedAverages");
            _size = GpuQuantumSimulator.Instance.width;

            _kernelGroupsX = _size / KernelThreadsX;
            shader.SetInts(KernelNameHalfSize, GpuQuantumSimulator.Instance.width / 2, GpuQuantumSimulator.Instance.height / 2);
            shader.SetInts(KernelNameSize, GpuQuantumSimulator.Instance.width, GpuQuantumSimulator.Instance.height);
            shader.SetInt(KernelNameRadius, _size);
            shader.SetInt(KernelNamePhaseSamples, 360);
            _bufferIndices = new ComputeBuffer(_size, sizeof(float));
            _bufferValues = new ComputeBuffer(_size, sizeof(float));
            _waveDataIndices = new float[_size];
            _waveDataValues = new float[_size];
            _bufferIndices.SetData(_waveDataIndices);
            _bufferValues.SetData(_waveDataValues);

            // AudioClip myClip = AudioClip.Create("WaveAudio", _sampleCount, 1, sampleRate, true, OnAudioRead, OnAudioSetPosition);
            // AudioSource aud = GetComponent<AudioSource>();
            // aud.clip = myClip;
            // aud.Play();

            GpuQuantumSimulator.Instance.FFTTextureHandler = UpdateWaveData;

            _currentTargetPitch = 1f;
            mixer.GetFloat("AmbiencePitch", out _lastPitch);
        }

        private void UpdateWaveData(BufferedRenderTexture tex)
        {
            if (UserPreferences.DisableDynamicAmbiance) return;
            
            shader.SetTexture(_kernelIndexWeightedAverages, KernelNameInput, tex.Read);
            shader.SetBuffer(_kernelIndexWeightedAverages, KernelNameResult1, _bufferIndices);
            shader.SetBuffer(_kernelIndexWeightedAverages, KernelNameResult2, _bufferValues);
            shader.Dispatch(_kernelIndexWeightedAverages, _kernelGroupsX, 1, 1);

            _bufferIndices.GetData(_waveDataIndices);
            _bufferValues.GetData(_waveDataValues);

            // TODO: ideally this operation would be done on the GPU to prevent expensive transfers
            float weightsSum = 0;
            float weightedIndices = 0;
            for (int i = 0; i < _size; i++)
            {
                // if (maxVal < _waveData[i])
                // {
                    // maxVal = _waveData[i];
                    // maxIndex = i;
                // }
                weightedIndices += _waveDataIndices[i] * _waveDataValues[i];
                weightsSum += _waveDataValues[i];
            }

            weightedIndices /= weightsSum;

            // calculate maximum possible k magnitude
            float maxIndex = Mathf.Sqrt(GpuQuantumSimulator.Instance.width * GpuQuantumSimulator.Instance.width +
                                        GpuQuantumSimulator.Instance.height * GpuQuantumSimulator.Instance.height);
            float scaledIndex = (float) weightedIndices  / maxIndex;
            _currentTargetPitch = 1f + (scaledIndex + pitchShiftOffset) * pitchShiftFactor;
        }

        private void Update()
        {
            if (UserPreferences.DisableDynamicAmbiance) return;
            
            var clamped = Mathf.Clamp(_currentTargetPitch, _lastPitch - maxPitchShift, _lastPitch + maxPitchShift);
            _lastPitch = clamped;
            mixer.SetFloat("AmbiencePitch", clamped);
            // UpdateWaveData(GpuQuantumSimulator.Instance.CurrentWaveFunction);
        }

        private float InterpolatedArrayAccess(int i, int max)
        {
            float adjustedIndex = (((float) i) / max) * _size;
            int index1 = (int) adjustedIndex;
            int index2 = (int) Mathf.Min(_size - 1, adjustedIndex + 1);
            float index1Weight = 1 - (adjustedIndex - index1);
            return Mathf.Lerp(_waveDataValues[index1], _waveDataValues[index2], index1Weight) /
                   GpuQuantumSimulator.Instance.LastMaxMagnitude;
        }

        private void OnDestroy()
        {
            _bufferIndices.Dispose();
            _bufferValues.Dispose();
        }

        void OnAudioRead(float[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                var index = (i + _position) % _waveSampleCount;
                data[i] = InterpolatedArrayAccess(index, _waveSampleCount);
                _position++;
            }
        }

        void OnAudioSetPosition(int newPosition)
        {
            _position = newPosition;
        }
    }
}