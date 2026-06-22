using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonCeltas
{
    public class PlayerUpgrades : MonoBehaviour
    {
        [SerializeField] private MejoraManager mejoraManager;

        public static int Score { get; private set; }
        public static float XpNormalized => instance != null ? Mathf.Clamp01((float)Score / instance.nextUpgradeScore) : 0f;
        public int OroDisponible { get; private set; }

        private int nextUpgradeScore = 10;
        private bool isChoosingUpgrade;
        private static PlayerUpgrades instance;

        private PlayerHealth playerHealth;
        private PlayerMovement playerMovement;
        private PlayerCombat playerCombat;
        private float bonusOro;

        void Awake()
        {
            instance = this;
            Score = 0;
            OroDisponible = 20;
        }

        void Start()
        {
            playerHealth = GetComponent<PlayerHealth>();
            playerMovement = GetComponent<PlayerMovement>();
            playerCombat = GetComponent<PlayerCombat>();
        }

        void OnEnable()
        {
            GameEvents.OnScoreGained += HandleScoreGained;
            GameEvents.OnGoldGained += AgregarOro;
        }

        void OnDisable()
        {
            GameEvents.OnScoreGained -= HandleScoreGained;
            GameEvents.OnGoldGained -= AgregarOro;
        }

        private void HandleScoreGained(int points)
        {
            var portal = FindAnyObjectByType<PortalManager>();
            int multiplicador = portal != null ? portal.MultiplicadorXp : 1;

            float bonus = playerCombat != null ? playerCombat.bonusXp / 100f : 0f;
            int total = points * multiplicador + Mathf.RoundToInt(points * multiplicador * bonus);
            Score += total;
            if (!isChoosingUpgrade && Score >= nextUpgradeScore)
                ActivarMejora();
        }

        public static void AddScore(int points)
        {
            GameEvents.ScoreGained(points);
        }

        private void ActivarMejora()
        {
            isChoosingUpgrade = true;
            Time.timeScale = 0f;

            if (mejoraManager != null)
            {
                StartCoroutine(MostrarConDelay());
            }
        }

        public void ActivarMejoraGratis()
        {
            if (isChoosingUpgrade) return;
            isChoosingUpgrade = true;
            Time.timeScale = 0f;

            if (mejoraManager != null)
            {
                StartCoroutine(MostrarConDelayGratis());
            }
        }

        public void ActivarMejoraBuenaSuerte()
        {
            if (isChoosingUpgrade) return;
            isChoosingUpgrade = true;
            Time.timeScale = 0f;

            if (mejoraManager != null)
            {
                StartCoroutine(MostrarBuenaSuerte());
            }
        }

        private IEnumerator MostrarBuenaSuerte()
        {
            yield return new WaitForSecondsRealtime(1f);

            var selector = FindAnyObjectByType<BuffSelector>();
            if (selector == null)
            {
                isChoosingUpgrade = false;
                Time.timeScale = 1f;
                yield break;
            }

            var pesos = new System.Collections.Generic.Dictionary<RarezaBuff, float>
            {
                { RarezaBuff.Epico, 60f },
                { RarezaBuff.Legendario, 35f },
                { RarezaBuff.Mitico, 5f }
            };

            var buffs = selector.SeleccionarBuffsConPesos(3, pesos);
            mejoraManager.MostrarMejoraConBuffs(OnBuffGratisElegido, buffs);
        }

        private IEnumerator MostrarConDelayGratis()
        {
            yield return new WaitForSecondsRealtime(1f);
            mejoraManager.MostrarMejora(3, OnBuffGratisElegido);
        }

        private void OnBuffGratisElegido(BuffInfo buff)
        {
            isChoosingUpgrade = false;
            Time.timeScale = 1f;

            if (buff == null) return;
            AplicarBuff(buff);
        }

        private IEnumerator MostrarConDelay()
        {
            yield return new WaitForSecondsRealtime(1f);
            mejoraManager.MostrarMejora(3, OnBuffElegido);
        }

        private void OnBuffElegido(BuffInfo buff)
        {
            isChoosingUpgrade = false;
            nextUpgradeScore += 5;
            Time.timeScale = 1f;

            if (buff == null) return;
            AplicarBuff(buff);
        }

        private void AplicarRngBuffs(int cantidad)
        {
            var selector = FindAnyObjectByType<BuffSelector>();
            if (selector == null) return;

            var buffs = selector.SeleccionarBuffs(cantidad);
            foreach (var buff in buffs)
            {
                if (buff != null)
                    AplicarBuff(buff);
            }
        }

        private void AplicarBuff(BuffInfo buff)
        {
            AplicarEfecto(buff.tipo, buff.valor);
            if (buff.tipoSecundario != 0)
                AplicarEfecto(buff.tipoSecundario, buff.valorSecundario);
        }

        public void AplicarMejora(BuffInfo buff)
        {
            if (buff == null) return;
            AplicarBuff(buff);
        }

        public void AgregarOro(int cantidad)
        {
            int total = cantidad + Mathf.RoundToInt(cantidad * (bonusOro / 100f));
            OroDisponible += total;
        }

        public bool GastarOro(int cantidad)
        {
            if (OroDisponible < cantidad) return false;
            OroDisponible -= cantidad;
            return true;
        }

        private void AplicarEfecto(BuffInfo.TipoBuff tipo, float valor)
        {
            if (valor == 0) return;

            switch (tipo)
            {
                case BuffInfo.TipoBuff.Dano:
                    if (playerCombat != null) playerCombat.IncreaseDamage(valor);
                    break;
                case BuffInfo.TipoBuff.VidaMaxima:
                    if (playerHealth != null) { playerHealth.SetMaxHp(playerHealth.MaxHp + valor); if (valor > 0) playerHealth.Heal(valor); }
                    break;
                case BuffInfo.TipoBuff.RegeneracionPasiva:
                    if (playerHealth != null) playerHealth.regeneracionPasiva = Mathf.Max(0, playerHealth.regeneracionPasiva + valor);
                    break;
                case BuffInfo.TipoBuff.Velocidad:
                    if (playerMovement != null) playerMovement.IncreaseSpeed(valor);
                    break;
                case BuffInfo.TipoBuff.VelocidadAtaque:
                    if (playerCombat != null) playerCombat.SetBonusVelocidadAtaque(valor);
                    break;
                case BuffInfo.TipoBuff.EscudoCooldown:
                    if (playerCombat != null) playerCombat.SetReduccionCooldownEscudo(playerCombat.GetShieldCooldown() + valor); // valor = segundos a restar
                    break;
                case BuffInfo.TipoBuff.XpBoost:
                    if (playerCombat != null) playerCombat.bonusXp += valor;
                    break;
                case BuffInfo.TipoBuff.AtaqueArea:
                    if (playerCombat != null) playerCombat.SetBonusRangoAtaque(playerCombat.AttackRadius * (valor / 100f));
                    break;
                case BuffInfo.TipoBuff.VidaPorcentaje:
                    if (playerHealth != null) playerHealth.SetMaxHpPorcentaje(valor);
                    break;
                case BuffInfo.TipoBuff.DanoMultiplicativo:
                    if (playerCombat != null) playerCombat.SetMultiplicadorDano(valor);
                    break;
                case BuffInfo.TipoBuff.RangoAtaque:
                    if (playerCombat != null) playerCombat.SetBonusRangoAtaque(valor / 100f);
                    break;
                case BuffInfo.TipoBuff.CuracionFinRonda:
                    if (playerHealth != null) playerHealth.curacionFinRondaPorcentaje += valor;
                    break;
                case BuffInfo.TipoBuff.StaminaMaxima:
                    if (playerMovement != null) playerMovement.BoostStamina(valor);
                    break;
                case BuffInfo.TipoBuff.PenalizacionCero:
                    if (playerCombat != null) playerCombat.penalizacionCero = true;
                    if (playerMovement != null) playerMovement.penalizacionCero = true;
                    break;
                case BuffInfo.TipoBuff.Oro:
                    AgregarOro((int)valor);
                    break;
                case BuffInfo.TipoBuff.Velocista:
                    if (playerMovement != null) playerMovement.IncreaseSpeed(valor);
                    if (playerCombat != null) playerCombat.velocistaActivo = true;
                    break;
                case BuffInfo.TipoBuff.OroBoost:
                    bonusOro += valor;
                    break;
                case BuffInfo.TipoBuff.Extasis:
                    if (playerHealth != null) playerHealth.extasisActivo = true;
                    break;
                case BuffInfo.TipoBuff.Vampirico:
                    if (playerCombat != null) playerCombat.vampiricoCuracion += valor;
                    break;
                case BuffInfo.TipoBuff.VampiricoPorcentaje:
                    if (playerCombat != null) playerCombat.vampiricoPorcentaje += valor;
                    break;
                case BuffInfo.TipoBuff.RngBuffs:
                    AplicarRngBuffs((int)valor);
                    break;
                case BuffInfo.TipoBuff.ReduccionDelayRegeneracion:
                    if (playerHealth != null) playerHealth.reduccionDelayRegeneracion += valor;
                    break;
                case BuffInfo.TipoBuff.Semidios:
                    float pct = valor / 100f;
                    if (playerHealth != null) playerHealth.SetMaxHpPorcentaje(valor);
                    if (playerMovement != null) playerMovement.IncreaseSpeed(playerMovement.MoveSpeed * pct);
                    if (playerCombat != null)
                    {
                        playerCombat.IncreaseDamage(playerCombat.AttackDamage * pct);
                        playerCombat.SetBonusRangoAtaque(playerCombat.AttackRadius * pct);
                    }
                    break;
            }
        }
    }
}

