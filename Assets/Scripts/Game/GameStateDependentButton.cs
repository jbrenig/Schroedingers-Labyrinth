using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{    
    /// <summary>
    /// makes the current ui button not interactable on start iff not in the correct gamestate
    /// </summary>
    public class GameStateDependentButton : MonoBehaviour
    {
        public GameController.GameState[] gameState;
    
        void OnEnable()
        {
            gameObject.GetComponent<Button>().interactable = gameState.Any(s => s == GameController.Instance.CurrentGameState);
        }
    }
}
