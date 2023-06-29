using Lib;
using UnityEngine;
using UnityEngine.Audio;

namespace Game
{
    public class MusicPlayer : MonoBehaviour
    {
        public AudioMixer mixer;
        private AudioSource _audioSource;

        private static MusicPlayer _instance = null;
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                _instance = this;
            }
            
            _audioSource = GetComponent<AudioSource>();
            
        }

        private void Start()
        {
            // initialize volume settings
            MusicUtils.SetMusicVolumeFromPreferences(mixer, UserPreferences.VolumeCategory.Master);
            MusicUtils.SetMusicVolumeFromPreferences(mixer, UserPreferences.VolumeCategory.Effects);
            MusicUtils.SetMusicVolumeFromPreferences(mixer, UserPreferences.VolumeCategory.Music);
            MusicUtils.SetMusicVolumeFromPreferences(mixer, UserPreferences.VolumeCategory.Speech);
        }

        public void PlayMusic()
        {
            if (_audioSource.isPlaying) return;
            _audioSource.Play();
        }
 
        public void StopMusic()
        {
            _audioSource.Stop();
        }
    }
}
