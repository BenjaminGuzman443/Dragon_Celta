using UnityEngine;

namespace DragonCeltas
{
    [RequireComponent(typeof(Collider2D))]
    public class TiendaTrigger : MonoBehaviour
    {
        [Header("Tienda")]
        [SerializeField] private MonoBehaviour tienda;
        [SerializeField] private GameObject indicadorVisual;

        private bool jugadorCerca;
        private ITienda tiendaInterface;

        void Start()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;

            tiendaInterface = tienda as ITienda;

            if (indicadorVisual != null)
                indicadorVisual.SetActive(false);
        }

        void Update()
        {
            if (!jugadorCerca) return;

            if (tiendaInterface != null && tiendaInterface.EstaAbierta)
                return;

            if (UnityEngine.InputSystem.Keyboard.current != null &&
                UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
            {
                tiendaInterface?.Mostrar();
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                jugadorCerca = true;
                if (indicadorVisual != null)
                    indicadorVisual.SetActive(true);
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                jugadorCerca = false;
                if (indicadorVisual != null)
                    indicadorVisual.SetActive(false);
                tiendaInterface?.Ocultar();
            }
        }
    }
}
