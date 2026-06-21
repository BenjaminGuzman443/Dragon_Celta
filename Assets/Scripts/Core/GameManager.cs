using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DragonCeltas
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [Header("UI Game Over")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI gameOverText;

        [Header("Botones")]
        [SerializeField] private Button reiniciarButton;
        [SerializeField] private Button menuButton;

        private bool gameOverActivo = false;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        void Start()
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            if (reiniciarButton != null)
                reiniciarButton.onClick.AddListener(ReiniciarEscena);

            if (menuButton != null)
                menuButton.onClick.AddListener(IrAlMenu);
        }

        void Update()
        {
            if (!gameOverActivo)
                return;

            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.rKey.wasPressedThisFrame)
                ReiniciarEscena();

            if (kb.mKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame)
                IrAlMenu();
        }

        public void GameOver()
        {
            if (gameOverActivo)
                return;

            gameOverActivo = true;

            Time.timeScale = 0f;

            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);

            if (gameOverText != null)
            {
                gameOverText.text =
                    "GAME OVER\n\n" +
                    "R - Reiniciar\n" +
                    "M - Menu Principal";
            }

            Debug.Log("GAME OVER");
        }

        public void ReiniciarEscena()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void IrAlMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
    }
}