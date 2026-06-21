using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DragonCeltas
{
    public class MejoraManager : MonoBehaviour
    {
        [Header("Canvas")]
        [SerializeField] private GameObject canvasMejora;

        [Header("Botones")]
        [SerializeField] private Button btnOpcion1;
        [SerializeField] private Button btnOpcion2;
        [SerializeField] private Button btnOpcion3;

        [Header("Textos de Botones")]
        [SerializeField] private TextMeshProUGUI txtNombre1;
        [SerializeField] private TextMeshProUGUI txtNombre2;
        [SerializeField] private TextMeshProUGUI txtNombre3;

        [SerializeField] private TextMeshProUGUI txtDesc1;
        [SerializeField] private TextMeshProUGUI txtDesc2;
        [SerializeField] private TextMeshProUGUI txtDesc3;

        [Header("Iconos")]
        [SerializeField] private Image imgIcono1;
        [SerializeField] private Image imgIcono2;
        [SerializeField] private Image imgIcono3;

        [Header("Selector de Buffs")]
        [SerializeField] private BuffSelector buffSelector;

        private List<BuffInfo> buffsActuales;
        private System.Action<BuffInfo> onBuffElegido;

        void Start()
        {
            if (canvasMejora != null)
                canvasMejora.SetActive(false);
        }

        public void MostrarMejora(int cantidad, System.Action<BuffInfo> callback)
        {
            if (buffSelector == null)
            {
                callback?.Invoke(null);
                return;
            }

            onBuffElegido = callback;
            buffsActuales = buffSelector.SeleccionarBuffs(cantidad);
            MostrarConBuffs(callback);
        }

        public void MostrarMejoraConBuffs(System.Action<BuffInfo> callback, List<BuffInfo> buffs)
        {
            buffsActuales = buffs;
            MostrarConBuffs(callback);
        }

        private void MostrarConBuffs(System.Action<BuffInfo> callback)
        {
            onBuffElegido = callback;

            for (int i = 0; i < 3; i++)
            {
                var btn = i == 0 ? btnOpcion1 : (i == 1 ? btnOpcion2 : btnOpcion3);
                var txtN = i == 0 ? txtNombre1 : (i == 1 ? txtNombre2 : txtNombre3);
                var txtD = i == 0 ? txtDesc1 : (i == 1 ? txtDesc2 : txtDesc3);
                var img = i == 0 ? imgIcono1 : (i == 1 ? imgIcono2 : imgIcono3);

                if (i < buffsActuales.Count && buffsActuales[i] != null)
                {
                    var b = buffsActuales[i];
                    var color = RarezaColores.ObtenerColor(b.rareza);

                    if (btn != null) btn.gameObject.SetActive(true);
                    if (txtN != null) { txtN.text = b.nombre; txtN.color = color; }
                    if (txtD != null) txtD.text = $"{b.descripcion} ({b.rareza})";
                    if (img != null && b.icono != null)
                    {
                        img.sprite = buffsActuales[i].icono;
                        img.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (btn != null) btn.gameObject.SetActive(false);
                }
            }

            if (canvasMejora != null)
                canvasMejora.SetActive(true);

            if (btnOpcion1 != null) btnOpcion1.onClick.RemoveAllListeners();
            if (btnOpcion2 != null) btnOpcion2.onClick.RemoveAllListeners();
            if (btnOpcion3 != null) btnOpcion3.onClick.RemoveAllListeners();

            if (btnOpcion1 != null) btnOpcion1.onClick.AddListener(() => ElegirBuff(0));
            if (btnOpcion2 != null) btnOpcion2.onClick.AddListener(() => ElegirBuff(1));
            if (btnOpcion3 != null) btnOpcion3.onClick.AddListener(() => ElegirBuff(2));
        }

        private void ElegirBuff(int index)
        {
            if (canvasMejora != null)
                canvasMejora.SetActive(false);

            BuffInfo elegido = null;
            if (buffsActuales != null && index < buffsActuales.Count)
                elegido = buffsActuales[index];

            onBuffElegido?.Invoke(elegido);
        }
    }
}
