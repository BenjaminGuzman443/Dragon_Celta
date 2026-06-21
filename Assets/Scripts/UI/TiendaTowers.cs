using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DragonCeltas
{
    public enum EfectoTiendaTowers
    {

    }

    [System.Serializable]
    public class ItemTiendaTowers
    {
        public string nombre;
        public int costo = 100;
        public int incremento = 25;
        public Button boton;
        public TextMeshProUGUI txtCosto;
        public EfectoTiendaTowers efecto;
    }

    public class TiendaTowers : MonoBehaviour, ITienda
    {
        [Header("Canvas")]
        [SerializeField] private GameObject canvasTienda;

        [Header("Items de la Tienda")]
        [SerializeField] private ItemTiendaTowers[] itemsTienda;

        [SerializeField] private Button btnCerrar;

        [Header("Player")]
        [SerializeField] private PlayerUpgrades playerUpgrades;

        public bool EstaAbierta { get; private set; }

        void Start()
        {
            if (canvasTienda != null)
                canvasTienda.SetActive(false);
        }

        public void Mostrar()
        {
            if (EstaAbierta) return;

            EstaAbierta = true;
            Time.timeScale = 0f;

            ConfigurarBotones();

            if (canvasTienda != null)
                canvasTienda.SetActive(true);
        }

        public void Ocultar()
        {
            if (!EstaAbierta) return;

            EstaAbierta = false;
            Time.timeScale = 1f;

            if (canvasTienda != null)
                canvasTienda.SetActive(false);
        }

        private void ConfigurarBotones()
        {
            int oro = playerUpgrades != null ? playerUpgrades.OroDisponible : 0;

            for (int i = 0; i < itemsTienda.Length; i++)
            {
                var item = itemsTienda[i];
                if (item == null || item.boton == null) continue;

                if (item.txtCosto != null)
                    item.txtCosto.text = $"{item.costo} oro";

                int index = i;
                item.boton.onClick.RemoveAllListeners();
                item.boton.onClick.AddListener(() => ComprarItem(index));
                item.boton.interactable = oro >= item.costo;
            }

            if (btnCerrar != null)
            {
                btnCerrar.onClick.RemoveAllListeners();
                btnCerrar.onClick.AddListener(Ocultar);
            }
        }

        private void ComprarItem(int index)
        {
            if (itemsTienda == null || index < 0 || index >= itemsTienda.Length) return;

            var item = itemsTienda[index];
            if (item == null || playerUpgrades == null) return;

            if (!playerUpgrades.GastarOro(item.costo)) return;

            item.costo += item.incremento;
            if (item.txtCosto != null)
                item.txtCosto.text = $"{item.costo} oro";

            Ocultar();

            switch (item.efecto)
            {

            }
        }

        public void CerrarTienda()
        {
            Ocultar();
        }
    }
}
