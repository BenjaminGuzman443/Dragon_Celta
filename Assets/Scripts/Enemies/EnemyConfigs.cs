using UnityEngine;

namespace DragonCeltas
{
    [System.Serializable]
    public class EfectosSecundariosConfig
    {
        [Header("Veneno")]
        public bool venenoActivo;
        [Range(1f, 10f)]
        public float duracionVeneno = 2f;

        [Header("Camuflaje")]
        public bool camuflajeActivo;
        public float camuflajeRango = 8f;
        [Range(0f, 1f)]
        public float camuflajeTransparencia = 0.3f;

        [Header("Avaricia")]
        [Tooltip("Por cada 10 de oro del jugador: +2 daño, +10 vida al enemigo")]
        public bool avariciaActivo;

        [Header("Colera")]
        [Tooltip("Al estar a mitad de vida o menos, gana +30% de daño")]
        public bool coleraActivo;
    }
}
