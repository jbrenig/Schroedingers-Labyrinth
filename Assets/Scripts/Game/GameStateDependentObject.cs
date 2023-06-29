using System.Linq;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// deactivates the current gameobject on start if not in the correct gamestate
    /// </summary>
    public class GameStateDependentObject : MonoBehaviour
    {
        public GameController.GameState[] gameState;
    
        void Start()
        {
            if (gameState.All(s => s != GameController.Instance.CurrentGameState)) gameObject.SetActive(false);
        }
    }
}
