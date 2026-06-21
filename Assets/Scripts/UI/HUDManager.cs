using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DragonCeltas
{
    public class HUDManager : MonoBehaviour
    {
        [Header("Barras")]
        [SerializeField] private Image healthFill;
        [SerializeField] private Image staminaFill;
        [SerializeField] private Image xpFill;
        [SerializeField] private Image escudoCooldownFill;
        [SerializeField] private Image ataqueCooldownFill;
        [SerializeField] private RectTransform staminaBarContainer;

        [Header("Temblor Stamina")]
        [SerializeField] private float maxShakeIntensity = 8f;

        [Header("Ronda")]
        [SerializeField] private TextMeshProUGUI rondaText;
        [SerializeField] private TextMeshProUGUI infoText;

        [Header("Puntaje")]
        [SerializeField] private TextMeshProUGUI puntosText;

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI atkText;
        [SerializeField] private TextMeshProUGUI spdText;

        [Header("Oro")]
        [SerializeField] private TextMeshProUGUI oroText;

        private PlayerHealth playerHealth;
        private PlayerMovement playerMovement;
        private PlayerCombat playerCombat;
        private PlayerUpgrades playerUpgrades;
        private PortalManager portalManager;

        private RectTransform staminaBarRect;
        private Vector3 staminaBarPosInicial;

        void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<PlayerHealth>();
                playerMovement = player.GetComponent<PlayerMovement>();
                playerCombat = player.GetComponent<PlayerCombat>();
                playerUpgrades = player.GetComponent<PlayerUpgrades>();
            }

            portalManager = FindAnyObjectByType<PortalManager>();

            if (staminaBarContainer != null)
            {
                staminaBarRect = staminaBarContainer;
                staminaBarPosInicial = staminaBarRect.localPosition;
            }

            if (ataqueCooldownFill != null)
                ataqueCooldownFill.fillAmount = 0f;
        }

        void Update()
        {
            ActualizarBarras();
            ActualizarEscudo();
            ActualizarAtaqueCooldown();
            ActualizarRonda();
            ActualizarPuntos();
            ActualizarStats();
            ActualizarOro();
        }

        private void ActualizarBarras()
        {
            if (playerHealth != null && healthFill != null)
            {
                healthFill.fillAmount = playerHealth.HpNormalized;
                healthFill.color = playerHealth.EstaSobreCurado
                    ? Color.green
                    : playerHealth.EstaEnvenenado
                        ? new Color(0.6f, 0.2f, 0.8f, 1f)
                        : Color.white;
            }

            if (xpFill != null)
                xpFill.fillAmount = PlayerUpgrades.XpNormalized;

            if (playerMovement != null && staminaFill != null)
            {
                staminaFill.fillAmount = playerMovement.StaminaNormalized;
                ActualizarTemblorStamina();
            }
        }

        private void ActualizarTemblorStamina()
        {
            if (staminaBarRect == null) return;

            float n = playerMovement.StaminaNormalized;
            bool debeTemblar = playerMovement.QuiereCorrer && n < 0.5f;

            if (debeTemblar)
            {
                float intensity = (0.5f - n) * 2f * maxShakeIntensity;
                float x = Random.Range(-intensity, intensity);
                float y = Random.Range(-intensity, intensity);
                staminaBarRect.localPosition = staminaBarPosInicial + new Vector3(x, y, 0f);
            }
            else
            {
                staminaBarRect.localPosition = staminaBarPosInicial;
            }
        }

        private void ActualizarEscudo()
        {
            if (playerCombat == null || escudoCooldownFill == null) return;

            escudoCooldownFill.fillAmount = playerCombat.ShieldEnCooldown
                ? playerCombat.ShieldCooldownNormalized
                : 0f;
        }

        private void ActualizarAtaqueCooldown()
        {
            if (playerCombat == null || ataqueCooldownFill == null) return;

            ataqueCooldownFill.fillAmount = playerCombat.AttackEnCooldown
                ? playerCombat.AttackCooldownNormalized
                : 0f;
        }

        private void ActualizarRonda()
        {
            if (portalManager == null) return;

            if (rondaText != null)
                rondaText.text = portalManager.DificultadActual;

            if (infoText != null)
                infoText.text = portalManager.TiempoFormateado;
        }

        private void ActualizarPuntos()
        {
            if (puntosText != null)
                puntosText.text = $"Puntos: {PlayerUpgrades.Score}";
        }

        private void ActualizarStats()
        {
            if (playerHealth != null && hpText != null)
                hpText.text = $"{playerHealth.CurrentHp:F0}/{playerHealth.MaxHp}";
            if (playerCombat != null && atkText != null)
                atkText.text = $"Ataque: {playerCombat.GetEffectiveDamage():F0}";
            if (playerMovement != null && spdText != null)
                spdText.text = $"Velocidad: {playerMovement.MoveSpeed:F1}\nSprint: {playerMovement.SprintSpeed:F1}";
        }

        private void ActualizarOro()
        {
            if (playerUpgrades != null && oroText != null)
                oroText.text = $"Oro: {playerUpgrades.OroDisponible}";
        }
    }
}
