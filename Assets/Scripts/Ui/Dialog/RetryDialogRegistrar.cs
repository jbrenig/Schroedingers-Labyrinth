using Game;
using UnityEngine;

/// <summary>
/// Utility to set the number of retries needed for the retry dialog to show up. Changes the value of <see cref="GameController.showRetryIntroAfter"/> to the value specified.
/// </summary>
public class RetryDialogRegistrar : MonoBehaviour
{
    public int showRetryIntroAfter = 2;
    void Awake()
    {
        GameController.Instance.showRetryIntroAfter = showRetryIntroAfter;
    }
}
