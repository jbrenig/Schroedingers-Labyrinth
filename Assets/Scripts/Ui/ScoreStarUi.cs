using System;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class ScoreStarUi : MonoBehaviour
{
    public Visibility state = Visibility.Enabled;

    public Image enabledImage;
    public Image disabledImage;
    
    public enum Visibility
    {
        None, Enabled, Disabled
    }

    public void SetVisibility(Visibility visibility)
    {
        state = visibility;

        switch (visibility)
        {
            case Visibility.None:
                enabledImage.enabled = false;
                disabledImage.enabled = false;
                break;
            case Visibility.Enabled:
                enabledImage.enabled = true;
                disabledImage.enabled = false;
                break;
            case Visibility.Disabled:
                enabledImage.enabled = false;
                disabledImage.enabled = true;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(visibility), visibility, null);
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        SetVisibility(state);
    }

    private void OnValidate()
    {
        SetVisibility(state);
    }
}
