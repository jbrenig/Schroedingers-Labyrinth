using Lib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ui
{
    public class HighscoreOverviewController : MonoBehaviour
    {
        public GameObject prefab;
        public GameObject target;
        
        void Start()
        {
            for (int i = 0; i < Levels.Story.LevelList.Length; i++)
            {
                var obj = Instantiate(prefab, target.transform);
                var comp = obj.GetComponent<HighscoreOverviewComponent>();
                comp.levelIndex = i;
            }
        }
        
        public void BtnMainMenu()
        {
            SceneManager.LoadSceneAsync(Levels.MainMenu);
        }
    }
}
