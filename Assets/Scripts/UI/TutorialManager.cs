using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DragonCeltas
{
    public class TutorialManager : MonoBehaviour
    {
        [Header("Paginas")]
        [SerializeField] private PaginaTutorial[] paginas;

        [Header("Al Finalizar")]
        [SerializeField] private GameObject canvasTutorial;
        [SerializeField] private GameObject canvasHUD;
        [SerializeField] private PortalManager portalManager;

        private int paginaActual = -1;
        private int pasoActual;
        private bool escribiendo;
        private Coroutine escrituraCoroutine;
        private float velocidadEscritura = 0.04f;

        [System.Serializable]
        public class PaginaTutorial
        {
            public GameObject panel;
            public TextMeshProUGUI texto;
            public Button btnSiguiente;
            public Button btnOmitir;
            public bool mostrarHUD;
            [TextArea(3, 10)] public string[] pasos;
        }

        void Start()
        {
            if (canvasTutorial != null)
            {
                var canvas = canvasTutorial.GetComponent<Canvas>();
                if (canvas != null)
                    canvas.sortingOrder = 1;
            }
            if (canvasTutorial != null)
                canvasTutorial.SetActive(true);

            for (int i = 0; i < paginas.Length; i++)
            {
                if (paginas[i].panel != null)
                    paginas[i].panel.SetActive(false);
            }

            if (paginas.Length > 0)
                MostrarPagina(0, 0);
        }

        private void MostrarPagina(int indexPagina, int indexPaso)
        {
            if (indexPagina >= paginas.Length)
            {
                FinalizarTutorial();
                return;
            }

            var pag = paginas[indexPagina];

            if (pag.pasos == null || pag.pasos.Length == 0)
            {
                MostrarPagina(indexPagina + 1, 0);
                return;
            }

            if (indexPaso >= pag.pasos.Length)
            {
                MostrarPagina(indexPagina + 1, 0);
                return;
            }

            bool cambioDePagina = indexPagina != paginaActual;

            if (cambioDePagina)
            {
                if (paginaActual >= 0 && paginaActual < paginas.Length && paginas[paginaActual].panel != null)
                    paginas[paginaActual].panel.SetActive(false);

                if (pag.panel != null)
                    pag.panel.SetActive(true);

                if (canvasHUD != null)
                    canvasHUD.SetActive(pag.mostrarHUD);

                if (pag.btnSiguiente != null)
                {
                    pag.btnSiguiente.onClick.RemoveAllListeners();
                    pag.btnSiguiente.onClick.AddListener(Siguiente);
                }

                if (pag.btnOmitir != null)
                {
                    pag.btnOmitir.onClick.RemoveAllListeners();
                    pag.btnOmitir.onClick.AddListener(Omitir);
                }
            }

            paginaActual = indexPagina;
            pasoActual = indexPaso;

            Escribir(pag.pasos[pasoActual]);
        }

        private void Escribir(string texto)
        {
            if (escrituraCoroutine != null)
                StopCoroutine(escrituraCoroutine);
            escrituraCoroutine = StartCoroutine(EscribirTexto(texto));
        }

        private IEnumerator EscribirTexto(string texto)
        {
            escribiendo = true;
            var pag = paginas[paginaActual];
            if (pag.texto != null)
                pag.texto.text = "";

            foreach (char c in texto)
            {
                if (pag.texto != null)
                    pag.texto.text += c;
                yield return new WaitForSecondsRealtime(velocidadEscritura);
            }

            escribiendo = false;
        }

        public void Siguiente()
        {
            if (escribiendo)
            {
                if (escrituraCoroutine != null)
                    StopCoroutine(escrituraCoroutine);
                var pag = paginas[paginaActual];
                if (pag.texto != null && pasoActual < pag.pasos.Length)
                    pag.texto.text = pag.pasos[pasoActual];
                escribiendo = false;
                return;
            }

            MostrarPagina(paginaActual, pasoActual + 1);
        }

        public void Omitir()
        {
            if (escrituraCoroutine != null)
                StopCoroutine(escrituraCoroutine);
            FinalizarTutorial();
        }

        private void FinalizarTutorial()
        {
            if (paginaActual >= 0 && paginaActual < paginas.Length && paginas[paginaActual].panel != null)
                paginas[paginaActual].panel.SetActive(false);

            if (canvasTutorial != null)
                canvasTutorial.SetActive(false);

            if (canvasHUD != null)
                canvasHUD.SetActive(true);

            if (portalManager != null)
                portalManager.IniciarTodos();
        }
    }
}
