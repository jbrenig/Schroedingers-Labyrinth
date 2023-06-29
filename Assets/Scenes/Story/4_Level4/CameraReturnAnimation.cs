using System.Collections;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class CameraReturnAnimation : MonoBehaviour
{
    public float animationTimeReturnToCenter = 1f;
    public float animationTimeDisable = 1f;
    public float animationTimeEnable = 1f;


    public float enabledXRotationAmount = -60;

    public AudioSource audioSource;
    public AudioClip soundCameraRotate;

    private enum State
    {
        DisableAnimation, EnableAnimation, IdleDisabled, IdleEnabled, 
    }


    private State _state = State.IdleDisabled;
    private float _startYRotation = 0;
    private float _startXRotation = 360;

    private float _returnFromXRotation;
    private float _returnFromYRotation;
    
    void Start()
    {
        var angles = transform.eulerAngles;
        _startXRotation = angles.x;
        _startYRotation = angles.y;
    }

    public IEnumerator DoDisableAnimationImpl()
    {
        if (audioSource != null) audioSource.PlayOneShot(soundCameraRotate, .7f);
        var timePassed = 0f;
        // return to neutral y rotation
        while (timePassed < animationTimeReturnToCenter)
        {
            timePassed += Time.deltaTime;
            var progress = timePassed / animationTimeReturnToCenter;
            var easedProgress = Easing.InOutCirc( progress);
            var yRotation = Mathf.LerpAngle(_returnFromYRotation, _startYRotation, easedProgress);
            var eulerAngles = transform.eulerAngles;
            eulerAngles.x = _returnFromXRotation;
            eulerAngles.y = yRotation;
            transform.eulerAngles = eulerAngles;
            yield return null;
        }
        if (audioSource != null) audioSource.PlayOneShot(soundCameraRotate, .6f);

        timePassed = 0;

        // return to disable x rotation
        while (timePassed < animationTimeDisable)
        {
            timePassed += Time.deltaTime;
            var progress2 = timePassed / animationTimeDisable;
            var easedProgress = Easing.InOutSine(progress2);
            var xRotation = Mathf.LerpAngle(_returnFromXRotation, _startXRotation, easedProgress);
            var eulerAngles = transform.eulerAngles;
            eulerAngles.x = xRotation;
            eulerAngles.y = _startYRotation;
            transform.eulerAngles = eulerAngles;
            yield return null;
        }

        _state = State.IdleDisabled;
    }

    public void DoDisableAnimation()
    {
        StopAllCoroutines();
        _state = State.DisableAnimation;
        _returnFromXRotation = transform.eulerAngles.x;
        _returnFromYRotation = transform.eulerAngles.y;
        StartCoroutine(DoDisableAnimationImpl());
    }

    public void DoEnableAnimation()
    {
        StopAllCoroutines();
        _state = State.EnableAnimation;
        _returnFromXRotation = transform.eulerAngles.x;
        _returnFromYRotation = transform.eulerAngles.y;
        StartCoroutine(DoEnableAnimationImpl());
    }

    public IEnumerator DoEnableAnimationImpl()
    {
        var timePassed = 0f;
        // do x rotation to enable
        while (timePassed < animationTimeEnable)
        {
            timePassed += Time.deltaTime;
            var progress = timePassed / animationTimeReturnToCenter;
            var easedProgress = Easing.OutCirc(progress);
            var xRotation = Mathf.LerpAngle(_returnFromXRotation, _startXRotation + enabledXRotationAmount, easedProgress);
            var eulerAngles = transform.eulerAngles;
            eulerAngles.x = xRotation;
            eulerAngles.y = _returnFromYRotation;
            transform.eulerAngles = eulerAngles;
            yield return null;
        }
        _state = State.IdleEnabled;
    }
}
