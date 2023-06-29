using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ui
{
    public class MainMenuController : MonoBehaviour
    {
        public GameObject errorMsgNoCompute;
        
        // Start is called before the first frame update
        void Start()
        {
            errorMsgNoCompute.SetActive(!SystemInfo.supportsComputeShaders);
        }
        
        // Update is called once per frame
        void Update()
        {
        
        }

        public void BtnExit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void BtnStart()
        {
            SceneManager.LoadScene("Scenes/Story/0_Intro/0_Intro");
        }

        public void BtnLevelSelect()
        {
            SceneManager.LoadScene("Level Select");
        }
    }
}
