using UnityEngine;

namespace Game
{
    public class SkipIntro : MonoBehaviour
    {
        void Start()
        {
            GameController.Instance.GoToState(GameController.GameState.InGame);
        }
    }
}
