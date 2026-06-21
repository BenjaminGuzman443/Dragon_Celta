using UnityEngine;
using UnityEngine.SceneManagement;

namespace DragonCeltas
{
    public class MainMenu : MonoBehaviour
    {
        public void PlayGame()
        {
            SceneManager.LoadScene("SampleScene");
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}