using UnityEngine;

namespace Game
{
    public class SkipToState : MonoBehaviour
    {
        public GameController.GameState gameState;
    
        void Start()
        {
            GameController.Instance.GoToState(gameState);    
        }
    }
}
