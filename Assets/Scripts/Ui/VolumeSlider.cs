using Game;
using Lib;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public UserPreferences.VolumeCategory category;
    public AudioMixer mixer;
    
    private Slider _slider;
    


    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        _slider.value = UserPreferences.GetVolumeLin(category);
    }
    
    public void UpdateSlider(float vol) => MusicUtils.VolumeLin(mixer, category, vol);
}
