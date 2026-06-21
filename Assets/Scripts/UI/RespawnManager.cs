using TMPro;
using UnityEngine;

namespace DragonCeltas
{
    public class RespawnManager : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private TextMeshProUGUI textoContador;

        private GameObject canvasRespawn;
        private TextMeshProUGUI textoAuto;
        private Canvas canvasExterno;

        void Start()
        {
            if (playerHealth == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    playerHealth = player.GetComponent<PlayerHealth>();
            }

            if (textoContador == null)
                CrearCanvas();
            else
            {
                canvasExterno = textoContador.GetComponentInParent<Canvas>();
                if (canvasExterno != null)
                    canvasExterno.sortingOrder = 500;
            }
        }

        private void CrearCanvas()
        {
            canvasRespawn = new GameObject("RespawnCanvas");
            canvasRespawn.transform.SetParent(transform);
            var canvas = canvasRespawn.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            canvasRespawn.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasRespawn.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            var panelGO = new GameObject("RespawnPanel");
            panelGO.transform.SetParent(canvasRespawn.transform);
            var panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.3f);
            panelRect.anchorMax = new Vector2(0.5f, 0.3f);
            panelRect.sizeDelta = new Vector2(400, 100);
            panelRect.anchoredPosition = Vector2.zero;
            var panelImg = panelGO.AddComponent<UnityEngine.UI.Image>();
            panelImg.color = new Color(0f, 0f, 0f, 0.8f);

            var textoGO = new GameObject("RespawnText");
            textoGO.transform.SetParent(panelGO.transform);
            var textoRect = textoGO.AddComponent<RectTransform>();
            textoRect.anchorMin = Vector2.zero;
            textoRect.anchorMax = Vector2.one;
            textoRect.sizeDelta = Vector2.zero;
            textoRect.anchoredPosition = Vector2.zero;
            textoAuto = textoGO.AddComponent<TextMeshProUGUI>();
            textoAuto.fontSize = 36;
            textoAuto.alignment = TextAlignmentOptions.Center;
            textoAuto.color = Color.white;
            textoAuto.fontStyle = FontStyles.Bold;

            canvasRespawn.SetActive(false);
        }

        void Update()
        {
            if (playerHealth == null) return;

            bool muerto = playerHealth.EstaMuerto;

            if (canvasRespawn != null)
            {
                canvasRespawn.SetActive(muerto);
            }
            else if (canvasExterno != null)
            {
                canvasExterno.gameObject.SetActive(muerto);
            }

            var texto = textoContador != null ? textoContador : textoAuto;
            if (muerto && texto != null)
                texto.text = $"Reapareciendo en {playerHealth.RespawnTimer:F0}s";
        }
    }
}
