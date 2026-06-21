using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DragonCeltas
{
    public class CameraFocus : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private Camera camara;
        [SerializeField] private Transform castillo;
        [SerializeField] private Button botonEnfocar;

        [Header("Zoom Castillo")]
        [SerializeField] private float zoomAlejado = 7.5f;
        [SerializeField] private float velocidadTransicion = 6f;

        private CameraFollow cameraFollow;
        private float zoomNormal;
        private bool presionado;
        private Vector3 posCastillo;

        void Start()
        {
            if (camara == null)
                camara = Camera.main;
            if (camara != null)
            {
                cameraFollow = camara.GetComponent<CameraFollow>();
                zoomNormal = camara.orthographicSize;
            }

            if (botonEnfocar != null)
            {
                var trigger = botonEnfocar.gameObject.AddComponent<EventTrigger>();
                var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
                down.callback.AddListener((data) => EmpezarEnfoque());
                trigger.triggers.Add(down);

                var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
                up.callback.AddListener((data) => TerminarEnfoque());
                trigger.triggers.Add(up);
            }
        }

        public void EmpezarEnfoque()
        {
            if (castillo == null || camara == null) return;
            posCastillo = new Vector3(castillo.position.x, castillo.position.y, camara.transform.position.z);
            presionado = true;
            if (cameraFollow != null)
                cameraFollow.enabled = false;
        }

        public void TerminarEnfoque()
        {
            presionado = false;
            camara.orthographicSize = zoomNormal;
            if (cameraFollow != null)
                cameraFollow.enabled = true;
        }

        void Update()
        {
            if (!presionado) return;

            if (cameraFollow != null)
                cameraFollow.enabled = false;

            camara.transform.position = Vector3.Lerp(camara.transform.position, posCastillo, Time.deltaTime * velocidadTransicion);
            camara.orthographicSize = Mathf.Lerp(camara.orthographicSize, zoomAlejado, Time.deltaTime * velocidadTransicion);
        }
    }
}
