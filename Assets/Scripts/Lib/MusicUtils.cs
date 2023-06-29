using System;
using Game;
using UnityEngine;
using UnityEngine.Audio;

namespace Lib
{
    public static class MusicUtils
    {
        public static string VolumeCategoryToMixerVolumeName(UserPreferences.VolumeCategory category)
        {
            switch (category)
            {
                case UserPreferences.VolumeCategory.Music:
                    return "VolumeMusic";
                case UserPreferences.VolumeCategory.Effects:
                    return "VolumeSFX";
                case UserPreferences.VolumeCategory.Speech:
                    return "VolumeSpeech";
                case UserPreferences.VolumeCategory.Master:
                    return "VolumeMaster";
                default:
                    throw new ArgumentOutOfRangeException(nameof(category), category, null);
            }
        }
        
        public static void VolumeLin(AudioMixer mixer, UserPreferences.VolumeCategory category, float vol)
        {
            var clamped = Mathf.Clamp(vol, 0.0001f, 2f);
            UserPreferences.SetVolumeLin(category, clamped);
            mixer.SetFloat(VolumeCategoryToMixerVolumeName(category), Mathf.Log10(clamped) * 20);
        }

        public static void SetMusicVolumeFromPreferences(AudioMixer mixer, UserPreferences.VolumeCategory category)
        {
            VolumeLin(mixer, category, UserPreferences.GetVolumeLin(category));
        }
    }
}