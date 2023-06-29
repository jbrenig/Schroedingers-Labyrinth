using System;
using System.Linq;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Behaviour that enables the given gameobject only when in the correct game state
    /// </summary>
    public class GameStateLifecycle : MonoBehaviour
    {

        public GameController.GameState[] allowedStates;
        public GameObject target;
    
        // Start is called before the first frame update
        void Start()
        {
            GameController.Instance.OnGameStateChangedListener += OnGameStateUpdated;
            if (target == null)
            {
                target = gameObject;
            }
            OnGameStateUpdated(GameController.Instance.CurrentGameState);
        }

        private void OnGameStateUpdated(GameController.GameState state)
        {
            // Set target active iff current game state is in allowedStates
            target.SetActive(allowedStates.Any(s => s == state));
        }

        private void OnDestroy()
        {
            if (GameController.Instance == null) return;
            GameController.Instance.OnGameStateChangedListener -= OnGameStateUpdated;
        }
    }
}
