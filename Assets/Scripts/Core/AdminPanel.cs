// ============================================================
// TEMPORAL — ELIMINAR ANTES DE RELEASE
// F1: buffs  F2: pausa  F3: spawn enemigos
// ============================================================
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DragonCeltas
{
    public class AdminPanel : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private BuffSelector buffSelector;
        [SerializeField] private PlayerUpgrades playerUpgrades;
        [SerializeField] private PortalManager portalManager;

        private List<BuffInfo> todosLosBuffs;
        private List<GameObject> todosLosEnemigos;
        private int indiceActual;
        private int cantidadSpawn = 1;
        private bool panelAbierto;
        private bool modoSpawn;

        private GameObject panelAdmin;
        private TextMeshProUGUI txtNombre;
        private TextMeshProUGUI txtDescripcion;
        private TextMeshProUGUI txtIndice;

        void Start()
        {
            CrearCanvas();
            CerrarPanel();

            if (buffSelector != null)
                todosLosBuffs = buffSelector.ObtenerTodos();
            else
                todosLosBuffs = new List<BuffInfo>();

            todosLosEnemigos = new List<GameObject>();
        }

        private void CrearCanvas()
        {
            var canvasGO = new GameObject("AdminCanvas");
            canvasGO.transform.SetParent(transform);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            var panelGO = new GameObject("AdminPanel");
            panelGO.transform.SetParent(canvasGO.transform);
            var panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(600, 250);
            panelRect.anchoredPosition = Vector2.zero;
            var panelImg = panelGO.AddComponent<UnityEngine.UI.Image>();
            panelImg.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);
            panelAdmin = panelGO;

            var nombreGO = new GameObject("Nombre");
            nombreGO.transform.SetParent(panelGO.transform);
            var nombreRect = nombreGO.AddComponent<RectTransform>();
            nombreRect.anchorMin = new Vector2(0, 1);
            nombreRect.anchorMax = new Vector2(1, 1);
            nombreRect.sizeDelta = new Vector2(0, 40);
            nombreRect.anchoredPosition = new Vector2(0, -30);
            txtNombre = nombreGO.AddComponent<TextMeshProUGUI>();
            txtNombre.fontSize = 28;
            txtNombre.alignment = TextAlignmentOptions.Center;
            txtNombre.fontStyle = FontStyles.Bold;

            var descGO = new GameObject("Descripcion");
            descGO.transform.SetParent(panelGO.transform);
            var descRect = descGO.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0, 0.5f);
            descRect.anchorMax = new Vector2(1, 0.5f);
            descRect.sizeDelta = new Vector2(-40, 60);
            descRect.anchoredPosition = new Vector2(0, 10);
            txtDescripcion = descGO.AddComponent<TextMeshProUGUI>();
            txtDescripcion.fontSize = 18;
            txtDescripcion.alignment = TextAlignmentOptions.Center;

            var indiceGO = new GameObject("Indice");
            indiceGO.transform.SetParent(panelGO.transform);
            var indiceRect = indiceGO.AddComponent<RectTransform>();
            indiceRect.anchorMin = new Vector2(0, 0);
            indiceRect.anchorMax = new Vector2(1, 0);
            indiceRect.sizeDelta = new Vector2(0, 40);
            indiceRect.anchoredPosition = new Vector2(0, 50);
            txtIndice = indiceGO.AddComponent<TextMeshProUGUI>();
            txtIndice.fontSize = 16;
            txtIndice.alignment = TextAlignmentOptions.Center;
            txtIndice.color = new Color(0.7f, 0.7f, 0.7f);
        }

        void Update()
        {
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb == null) return;

            if (kb.f2Key.wasPressedThisFrame && portalManager != null)
                portalManager.PausarReanudar();

            if (kb.f3Key.wasPressedThisFrame)
            {
                if (panelAbierto && modoSpawn)
                    CerrarPanel();
                else
                    AbrirModoSpawn();
            }

            if (kb.f1Key.wasPressedThisFrame)
            {
                if (panelAbierto && !modoSpawn)
                    CerrarPanel();
                else
                    AbrirModoBuffs();
            }

            if (!panelAbierto) return;

            if (kb.leftArrowKey.wasPressedThisFrame)
            {
                int total = modoSpawn ? todosLosEnemigos.Count : todosLosBuffs.Count;
                if (total == 0) return;
                indiceActual--;
                if (indiceActual < 0) indiceActual = total - 1;
                MostrarActual();
            }

            if (kb.rightArrowKey.wasPressedThisFrame)
            {
                int total = modoSpawn ? todosLosEnemigos.Count : todosLosBuffs.Count;
                if (total == 0) return;
                indiceActual++;
                if (indiceActual >= total) indiceActual = 0;
                MostrarActual();
            }

            if (kb.enterKey.wasPressedThisFrame)
            {
                if (modoSpawn) SpawnearActual(); else AplicarBuffActual();
            }

            if (kb.upArrowKey.wasPressedThisFrame && modoSpawn)
            {
                cantidadSpawn++;
                if (cantidadSpawn > 50) cantidadSpawn = 50;
                MostrarActual();
            }

            if (kb.downArrowKey.wasPressedThisFrame && modoSpawn)
            {
                cantidadSpawn--;
                if (cantidadSpawn < 1) cantidadSpawn = 1;
                MostrarActual();
            }
        }

        private void AbrirModoBuffs()
        {
            modoSpawn = false;
            panelAbierto = true;
            Time.timeScale = 0f;
            indiceActual = 0;
            MostrarActual();
            if (panelAdmin != null) panelAdmin.SetActive(true);
        }

        private void AbrirModoSpawn()
        {
            if (portalManager != null)
                todosLosEnemigos = portalManager.ObtenerTodosLosPrefabs();

            cantidadSpawn = 1;
            modoSpawn = true;
            panelAbierto = true;
            Time.timeScale = 0f;
            indiceActual = 0;
            MostrarActual();
            if (panelAdmin != null) panelAdmin.SetActive(true);
        }

        private void CerrarPanel()
        {
            panelAbierto = false;
            modoSpawn = false;
            Time.timeScale = 1f;
            if (panelAdmin != null) panelAdmin.SetActive(false);
        }

        private void MostrarActual()
        {
            if (modoSpawn)
                MostrarEnemigoActual();
            else
                MostrarBuffActual();
        }

        private void MostrarBuffActual()
        {
            if (todosLosBuffs.Count == 0) return;

            var buff = todosLosBuffs[indiceActual];

            if (txtNombre != null)
            {
                txtNombre.text = buff.nombre;
                txtNombre.color = RarezaColores.ObtenerColor(buff.rareza);
            }

            if (txtDescripcion != null)
                txtDescripcion.text = buff.descripcion;

            if (txtIndice != null)
                txtIndice.text = $"[BUFFS] {indiceActual + 1}/{todosLosBuffs.Count}  ← → navegar  Enter aplicar  F1 cerrar  F3 enemigos";
        }

        private void MostrarEnemigoActual()
        {
            if (todosLosEnemigos.Count == 0) return;

            var prefab = todosLosEnemigos[indiceActual];

            if (txtNombre != null)
            {
                txtNombre.text = prefab.name;
                txtNombre.color = Color.red;
            }

            if (txtDescripcion != null)
                txtDescripcion.text = $"Spawnea x{cantidadSpawn} al frente del jugador";

            if (txtIndice != null)
                txtIndice.text = $"[SPAWN] {indiceActual + 1}/{todosLosEnemigos.Count}  ← → enemigo  ↑↓ cantidad  Enter spawn  F3 cerrar  F1 buffs";
        }

        private void AplicarBuffActual()
        {
            if (todosLosBuffs.Count == 0 || playerUpgrades == null) return;

            playerUpgrades.AplicarMejora(todosLosBuffs[indiceActual]);
            CerrarPanel();
        }

        private void SpawnearActual()
        {
            if (todosLosEnemigos.Count == 0 || portalManager == null) return;

            var player = GameObject.FindGameObjectWithTag("Player");
            Vector3 posicionBase;

            if (player != null)
            {
                var sr = player.GetComponent<SpriteRenderer>();
                float direccion = sr != null && sr.flipX ? -1f : 1f;
                posicionBase = player.transform.position + new Vector3(direccion * 3f, 0f, 0f);
            }
            else
            {
                posicionBase = Vector3.zero;
            }

            for (int i = 0; i < cantidadSpawn; i++)
            {
                float offsetY = (i - (cantidadSpawn - 1) / 2f) * 1.5f;
                var pos = posicionBase + new Vector3(0f, offsetY, 0f);
                portalManager.SpawnearPrefab(todosLosEnemigos[indiceActual], pos);
            }

            CerrarPanel();
        }
    }
}
