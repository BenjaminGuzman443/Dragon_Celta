using UnityEngine;
using TMPro;

namespace DragonCeltas
{
    [RequireComponent(typeof(Collider2D))]
    public class TorreVigia : MonoBehaviour
    {
        [Header("Compra Global")]
        [SerializeField] private int[] precios = new int[] { 100, 500, 1000, 2000 };

        private static int indicePrecioActual;

        [RuntimeInitializeOnLoadMethod]
        private static void ResetearPrecios()
        {
            indicePrecioActual = 0;
        }

        [Header("Compra")]
        [SerializeField] private GameObject prefabArquero;
        [SerializeField] private Vector2 offsetSpawn = new Vector2(0, 1f);

        [Header("Referencias")]
        [SerializeField] private PlayerUpgrades playerUpgrades;

        [Header("Estado Visual")]
        [SerializeField] private GameObject torreVaciaVisual;
        [SerializeField] private GameObject torreOcupadaVisual;

        private bool jugadorCerca;
        private bool arqueroComprado;
        private TextMeshProUGUI textoIndicador;
        private GameObject canvasIndicador;

        void Start()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;

            CrearIndicadorTexto();

            if (playerUpgrades == null)
                playerUpgrades = FindAnyObjectByType<PlayerUpgrades>();

            ActualizarEstadoVisual();
        }

        void Update()
        {
            if (!jugadorCerca || arqueroComprado) return;

            ActualizarIndicador();

            if (UnityEngine.InputSystem.Keyboard.current != null &&
                UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
            {
                IntentarComprarArquero();
            }
        }

        private void CrearIndicadorTexto()
        {
            canvasIndicador = new GameObject("IndicadorCosto");
            canvasIndicador.transform.SetParent(transform);
            canvasIndicador.transform.localPosition = new Vector3(0, 1.5f, 0);

            var canvas = canvasIndicador.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 100;
            var rectCanvas = canvas.GetComponent<RectTransform>();
            rectCanvas.sizeDelta = new Vector2(100, 25);

            var textoObj = new GameObject("TextoCosto");
            textoObj.transform.SetParent(canvasIndicador.transform);
            textoObj.transform.localPosition = Vector3.zero;
            textoObj.transform.localScale = Vector3.one;

            textoIndicador = textoObj.AddComponent<TextMeshProUGUI>();
            textoIndicador.fontSize = 0.375f;
            textoIndicador.alignment = TextAlignmentOptions.Center;
            textoIndicador.color = Color.white;
            textoIndicador.fontStyle = FontStyles.Bold;
            var rectTexto = textoIndicador.GetComponent<RectTransform>();
            rectTexto.sizeDelta = new Vector2(100, 25);

            canvasIndicador.SetActive(false);
        }

        private void ActualizarIndicador()
        {
            if (textoIndicador == null) return;

            int precioActual = PrecioActual();

            bool puedeComprar = playerUpgrades != null && playerUpgrades.OroDisponible >= precioActual;
            textoIndicador.color = puedeComprar ? Color.green : Color.red;
            textoIndicador.text = indicePrecioActual >= precios.Length
                ? "[E] MAX"
                : $"[E] {precioActual} oro";
        }

        private int PrecioActual()
        {
            if (indicePrecioActual >= precios.Length)
                return int.MaxValue;
            return precios[indicePrecioActual];
        }

        private void IntentarComprarArquero()
        {
            if (playerUpgrades == null) return;
            if (indicePrecioActual >= precios.Length) return;

            int precio = PrecioActual();
            if (!playerUpgrades.GastarOro(precio)) return;

            indicePrecioActual++;
            arqueroComprado = true;

            if (prefabArquero != null)
            {
                Instantiate(prefabArquero, (Vector2)transform.position + offsetSpawn, Quaternion.identity);
            }
            else
            {
                var arquero = new GameObject("ArqueroTorre");
                arquero.transform.position = (Vector2)transform.position + offsetSpawn;
                arquero.AddComponent<ArqueroTorre>();
                var sr = arquero.AddComponent<SpriteRenderer>();
                sr.sprite = CrearSpriteArquero();
                sr.sortingOrder = 10;
            }

            if (canvasIndicador != null)
                canvasIndicador.SetActive(false);

            ActualizarEstadoVisual();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (arqueroComprado) return;

            if (other.CompareTag("Player"))
            {
                jugadorCerca = true;
                ActualizarIndicador();

                if (canvasIndicador != null)
                    canvasIndicador.SetActive(true);
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                jugadorCerca = false;
                if (canvasIndicador != null)
                    canvasIndicador.SetActive(false);
            }
        }

        private void ActualizarEstadoVisual()
        {
            if (torreVaciaVisual != null)
                torreVaciaVisual.SetActive(!arqueroComprado);
            if (torreOcupadaVisual != null)
                torreOcupadaVisual.SetActive(arqueroComprado);
        }

        private Sprite CrearSpriteArquero()
        {
            int size = 24;
            var tex = new Texture2D(size, size);
            Color[] colores = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int cx = size / 2;
                    int cy = size / 2;
                    bool cabeza = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy + 3)) <= 4;
                    bool cuerpo = x >= cx - 3 && x <= cx + 3 && y >= cy - 5 && y <= cy;
                    bool arco = (x >= cx + 4 && x <= cx + 10) && Mathf.Abs(y - cy + 1) <= 2;
                    colores[y * size + x] = (cabeza || cuerpo || arco) ? new Color(0.3f, 0.8f, 1f) : Color.clear;
                }
            }

            tex.SetPixels(colores);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
