using System.Collections.Generic;
using UnityEngine;

namespace DragonCeltas
{
    public enum RarezaBuff
    {
        Comun,
        PocoComun,
        Raro,
        Epico,
        Legendario,
        Mitico
    }

    public static class RarezaColores
    {
        public static Color ObtenerColor(RarezaBuff rareza)
        {
            return rareza switch
            {
                RarezaBuff.Comun => new Color(0.2f, 0.8f, 0.2f),
                RarezaBuff.PocoComun => new Color(0.3f, 0.7f, 1f),
                RarezaBuff.Raro => new Color(0.4f, 1f, 1f),
                RarezaBuff.Epico => new Color(0.7f, 0.3f, 1f),
                RarezaBuff.Legendario => new Color(1f, 0.9f, 0.2f),
                RarezaBuff.Mitico => new Color(1f, 0.2f, 0.2f),
                _ => Color.white
            };
        }
    }

    [System.Serializable]
    public class BuffInfo
    {
        public string nombre;
        public RarezaBuff rareza;
        [TextArea(2, 4)] public string descripcion;
        public Sprite icono;

        public TipoBuff tipo;
        public float valor;
        public TipoBuff tipoSecundario;
        public float valorSecundario;

        public enum TipoBuff
        {
            Dano,
            VidaMaxima,
            RegeneracionPasiva,
            Velocidad,
            VelocidadAtaque,
            EscudoCooldown,
            XpBoost,
            AtaqueArea,
            VidaPorcentaje,
            DanoMultiplicativo,
            RangoAtaque,
            CuracionFinRonda,
            StaminaMaxima,
            PenalizacionCero,
            Oro,
            Velocista,
            OroBoost,
            Extasis,
            Vampirico,
            VampiricoPorcentaje,
            RngBuffs,
            ReduccionDelayRegeneracion,
            Semidios
        }
    }

    public class BuffSelector : MonoBehaviour
    {
        [SerializeField] private BuffInfo[] buffs;

        void Awake()
        {
            if (buffs == null || buffs.Length == 0)
                GenerarBuffsPorDefecto();
        }

        [ContextMenu("Generar Buffs por Defecto")]
        private void GenerarBuffsPorDefecto()
        {
            var lista = new List<BuffInfo>();

            // COMUN
            lista.Add(new BuffInfo { nombre = "+2 Daño", rareza = RarezaBuff.Comun, descripcion = "Aumenta el daño en 2", tipo = BuffInfo.TipoBuff.Dano, valor = 2f });
            lista.Add(new BuffInfo { nombre = "+30 Vida", rareza = RarezaBuff.Comun, descripcion = "Aumenta la vida máxima en 30", tipo = BuffInfo.TipoBuff.VidaMaxima, valor = 30f });
            lista.Add(new BuffInfo { nombre = "+0.1 Velocidad", rareza = RarezaBuff.Comun, descripcion = "Aumenta la velocidad en 0.1", tipo = BuffInfo.TipoBuff.Velocidad, valor = 0.1f });
            lista.Add(new BuffInfo { nombre = "Regeneración Básica", rareza = RarezaBuff.Comun, descripcion = "Recuperas el 5% de tu vida máxima periódicamente", tipo = BuffInfo.TipoBuff.RegeneracionPasiva, valor = 5f });
            lista.Add(new BuffInfo { nombre = "+25% XP", rareza = RarezaBuff.Comun, descripcion = "25% más de XP al matar enemigos", tipo = BuffInfo.TipoBuff.XpBoost, valor = 25f });
            lista.Add(new BuffInfo { nombre = "Un vampiro mediocre", rareza = RarezaBuff.Comun, descripcion = "Por cada golpe te curas 1.5 de vida", tipo = BuffInfo.TipoBuff.Vampirico, valor = 1.5f });
            lista.Add(new BuffInfo { nombre = "Moneda de Oro", rareza = RarezaBuff.Comun, descripcion = "Obtienes 60 de oro al instante", tipo = BuffInfo.TipoBuff.Oro, valor = 60f });

            // POCO COMUN
            lista.Add(new BuffInfo { nombre = "+5 Daño", rareza = RarezaBuff.PocoComun, descripcion = "+5 daño pero -35 vida máxima", tipo = BuffInfo.TipoBuff.Dano, valor = 5f, tipoSecundario = BuffInfo.TipoBuff.VidaMaxima, valorSecundario = -35f });
            lista.Add(new BuffInfo { nombre = "+50 Vida", rareza = RarezaBuff.PocoComun, descripcion = "Aumenta la vida máxima en 50", tipo = BuffInfo.TipoBuff.VidaMaxima, valor = 50f });
            lista.Add(new BuffInfo { nombre = "+0.2 Velocidad", rareza = RarezaBuff.PocoComun, descripcion = "Aumenta la velocidad en 0.2", tipo = BuffInfo.TipoBuff.Velocidad, valor = 0.2f });
            lista.Add(new BuffInfo { nombre = "Regeneración Media", rareza = RarezaBuff.PocoComun, descripcion = "Recuperas el 10% de tu vida máxima periódicamente", tipo = BuffInfo.TipoBuff.RegeneracionPasiva, valor = 10f });
            lista.Add(new BuffInfo { nombre = "Ataque Rápido", rareza = RarezaBuff.PocoComun, descripcion = "0.2s más rápido al atacar", tipo = BuffInfo.TipoBuff.VelocidadAtaque, valor = 0.2f });
            lista.Add(new BuffInfo { nombre = "Velocista", rareza = RarezaBuff.PocoComun, descripcion = "+1.5 de velocidad. Tu velocidad se suma a tu daño", tipo = BuffInfo.TipoBuff.Velocista, valor = 1.5f });
            lista.Add(new BuffInfo { nombre = "Éxtasis", rareza = RarezaBuff.PocoComun, descripcion = "La curación extra se convierte en escudo que decae 1%/s", tipo = BuffInfo.TipoBuff.Extasis, valor = 1f });
            lista.Add(new BuffInfo { nombre = "Vampirismo", rareza = RarezaBuff.PocoComun, descripcion = "Robo de vida por golpe: +3", tipo = BuffInfo.TipoBuff.Vampirico, valor = 3f });
            lista.Add(new BuffInfo { nombre = "Puñado de Monedas", rareza = RarezaBuff.PocoComun, descripcion = "Obtienes 80 de oro al instante", tipo = BuffInfo.TipoBuff.Oro, valor = 80f });

            // RARO
            lista.Add(new BuffInfo { nombre = "+50% XP", rareza = RarezaBuff.Raro, descripcion = "50% más de XP", tipo = BuffInfo.TipoBuff.XpBoost, valor = 50f });
            lista.Add(new BuffInfo { nombre = "+7 Daño", rareza = RarezaBuff.Raro, descripcion = "+7 daño pero atacas 0.3s más lento", tipo = BuffInfo.TipoBuff.Dano, valor = 7f, tipoSecundario = BuffInfo.TipoBuff.VelocidadAtaque, valorSecundario = -0.3f });
            lista.Add(new BuffInfo { nombre = "+80 Vida", rareza = RarezaBuff.Raro, descripcion = "Aumenta la vida máxima en 80", tipo = BuffInfo.TipoBuff.VidaMaxima, valor = 80f });
            lista.Add(new BuffInfo { nombre = "Tanque de Guerra", rareza = RarezaBuff.Raro, descripcion = "-5 daño pero +100 vida máxima", tipo = BuffInfo.TipoBuff.Dano, valor = -5f, tipoSecundario = BuffInfo.TipoBuff.VidaMaxima, valorSecundario = 100f });
            lista.Add(new BuffInfo { nombre = "+10% Ataque en Área", rareza = RarezaBuff.Raro, descripcion = "10% más de ataque en área", tipo = BuffInfo.TipoBuff.AtaqueArea, valor = 10f });
            lista.Add(new BuffInfo { nombre = "Fiebre de ORO", rareza = RarezaBuff.Raro, descripcion = "Ganas 40% más de oro", tipo = BuffInfo.TipoBuff.OroBoost, valor = 40f });
            lista.Add(new BuffInfo { nombre = "Alto Vampiro", rareza = RarezaBuff.Raro, descripcion = "Robo de vida por golpe: +5", tipo = BuffInfo.TipoBuff.Vampirico, valor = 5f });
            lista.Add(new BuffInfo { nombre = "Bolsa de Monedas", rareza = RarezaBuff.Raro, descripcion = "Obtienes 140 de oro al instante", tipo = BuffInfo.TipoBuff.Oro, valor = 140f });

            // EPICO
            lista.Add(new BuffInfo { nombre = "+100% Vida", rareza = RarezaBuff.Epico, descripcion = "100% más de vida máxima", tipo = BuffInfo.TipoBuff.VidaPorcentaje, valor = 100f });
            lista.Add(new BuffInfo { nombre = "Recuperación Ágil", rareza = RarezaBuff.Epico, descripcion = "Reduce 1s el tiempo entre regeneraciones", tipo = BuffInfo.TipoBuff.ReduccionDelayRegeneracion, valor = 1f });
            lista.Add(new BuffInfo { nombre = "Poder Bruto", rareza = RarezaBuff.Epico, descripcion = "x1.3 de daño (solo una vez)", tipo = BuffInfo.TipoBuff.DanoMultiplicativo, valor = 1.3f });
            lista.Add(new BuffInfo { nombre = "Gran Vampiro", rareza = RarezaBuff.Epico, descripcion = "30% del daño hecho te cura", tipo = BuffInfo.TipoBuff.VampiricoPorcentaje, valor = 30f });
            lista.Add(new BuffInfo { nombre = "Caja de Monedas", rareza = RarezaBuff.Epico, descripcion = "Obtienes 300 de oro al instante", tipo = BuffInfo.TipoBuff.Oro, valor = 300f });

            // LEGENDARIO
            lista.Add(new BuffInfo { nombre = "Berserker", rareza = RarezaBuff.Legendario, descripcion = "-30% de vida pero x1.8 de daño", tipo = BuffInfo.TipoBuff.VidaPorcentaje, valor = -30f, tipoSecundario = BuffInfo.TipoBuff.DanoMultiplicativo, valorSecundario = 1.8f });
            lista.Add(new BuffInfo { nombre = "Francotirador", rareza = RarezaBuff.Legendario, descripcion = "100% más de rango de ataque pero x0.5 de daño", tipo = BuffInfo.TipoBuff.RangoAtaque, valor = 100f, tipoSecundario = BuffInfo.TipoBuff.DanoMultiplicativo, valorSecundario = 0.5f });
            lista.Add(new BuffInfo { nombre = "+100% XP", rareza = RarezaBuff.Legendario, descripcion = "100% más de XP", tipo = BuffInfo.TipoBuff.XpBoost, valor = 100f });
            lista.Add(new BuffInfo { nombre = "Coloso", rareza = RarezaBuff.Legendario, descripcion = "+100% de vida pero -1 de velocidad", tipo = BuffInfo.TipoBuff.VidaPorcentaje, valor = 100f, tipoSecundario = BuffInfo.TipoBuff.Velocidad, valorSecundario = -1f });
            lista.Add(new BuffInfo { nombre = "Ejecutor", rareza = RarezaBuff.Legendario, descripcion = "x2 de daño pero +2s de delay al atacar", tipo = BuffInfo.TipoBuff.DanoMultiplicativo, valor = 2f, tipoSecundario = BuffInfo.TipoBuff.VelocidadAtaque, valorSecundario = -2f });
            lista.Add(new BuffInfo { nombre = "Vampiro Real", rareza = RarezaBuff.Legendario, descripcion = "60% del daño hecho te cura", tipo = BuffInfo.TipoBuff.VampiricoPorcentaje, valor = 60f });
            lista.Add(new BuffInfo { nombre = "Gran Fortuna", rareza = RarezaBuff.Legendario, descripcion = "Obtienes 1000 de oro al instante", tipo = BuffInfo.TipoBuff.Oro, valor = 1000f });

            // MITICO
            lista.Add(new BuffInfo { nombre = "Semidiós", rareza = RarezaBuff.Mitico, descripcion = "+150% de estadísticas generales (vida, velocidad, daño y área de ataque)", tipo = BuffInfo.TipoBuff.Semidios, valor = 150f });
            lista.Add(new BuffInfo { nombre = "Guerrero Imparable", rareza = RarezaBuff.Mitico, descripcion = "Elimina penalización de stamina y el estado quieto al golpear. +100% stamina y +50% velocidad de ataque.", tipo = BuffInfo.TipoBuff.PenalizacionCero, valor = 1f, tipoSecundario = BuffInfo.TipoBuff.StaminaMaxima, valorSecundario = 100f });
            lista.Add(new BuffInfo { nombre = "Extintor de Vidas", rareza = RarezaBuff.Mitico, descripcion = "Te curas el 200% del daño hecho", tipo = BuffInfo.TipoBuff.VampiricoPorcentaje, valor = 200f });
            lista.Add(new BuffInfo { nombre = "RNG JAJA", rareza = RarezaBuff.Mitico, descripcion = "Ganas instantaneamente 7 buffs aleatorios", tipo = BuffInfo.TipoBuff.RngBuffs, valor = 7f });
            lista.Add(new BuffInfo { nombre = "EL TESORO DEL DRAGON", rareza = RarezaBuff.Mitico, descripcion = "Obtienes 6000 de oro al instante", tipo = BuffInfo.TipoBuff.Oro, valor = 6000f });

            buffs = lista.ToArray();
        }
        private static readonly Dictionary<RarezaBuff, float> probabilidades = new Dictionary<RarezaBuff, float>
        {
            { RarezaBuff.Comun, 59.9f },
            { RarezaBuff.PocoComun, 20f },
            { RarezaBuff.Raro, 10f },
            { RarezaBuff.Epico, 9f },
            { RarezaBuff.Legendario, 1f },
            { RarezaBuff.Mitico, 0.1f }
        };

        public BuffInfo SeleccionarBuffAleatorio()
        {
            return SeleccionarBuffConPesos(probabilidades);
        }

        public BuffInfo SeleccionarBuffConPesos(Dictionary<RarezaBuff, float> pesos)
        {
            float total = 0f;
            foreach (var p in pesos.Values)
                total += p;

            float tiro = Random.Range(0f, total);
            float acumulado = 0f;
            RarezaBuff rarezaSeleccionada = RarezaBuff.Comun;

            foreach (var kvp in pesos)
            {
                acumulado += kvp.Value;
                if (tiro <= acumulado)
                {
                    rarezaSeleccionada = kvp.Key;
                    break;
                }
            }

            var buffsDeRareza = new List<BuffInfo>();
            foreach (var b in buffs)
            {
                if (b.rareza == rarezaSeleccionada)
                    buffsDeRareza.Add(b);
            }

            if (buffsDeRareza.Count == 0)
            {
                foreach (var b in buffs)
                {
                    if (b.rareza == RarezaBuff.Comun)
                        buffsDeRareza.Add(b);
                }
            }

            if (buffsDeRareza.Count == 0 && buffs.Length > 0)
                return buffs[Random.Range(0, buffs.Length)];

            return buffsDeRareza[Random.Range(0, buffsDeRareza.Count)];
        }

        public List<BuffInfo> SeleccionarBuffs(int cantidad)
        {
            var seleccionados = new List<BuffInfo>();
            for (int i = 0; i < cantidad; i++)
            {
                var buff = SeleccionarBuffAleatorio();
                if (buff != null)
                    seleccionados.Add(buff);
            }
            return seleccionados;
        }

        public List<BuffInfo> SeleccionarBuffsConPesos(int cantidad, Dictionary<RarezaBuff, float> pesos)
        {
            var seleccionados = new List<BuffInfo>();
            for (int i = 0; i < cantidad; i++)
            {
                var buff = SeleccionarBuffConPesos(pesos);
                if (buff != null)
                    seleccionados.Add(buff);
            }
            return seleccionados;
        }

        public List<BuffInfo> ObtenerTodos()
        {
            return new List<BuffInfo>(buffs);
        }
    }
}
