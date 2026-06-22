using System.Collections.Generic;
using UnityEngine;

namespace DragonCeltas
{
    public class PortalManager : MonoBehaviour
    {
        [SerializeField] private Spawner[] spawners;

        [Header("Ciclo de Portales")]
        [SerializeField] private float duracionCiclo = 300f;

        private float tiempoPartida;
        private float tiempoCiclo;
        private bool pausado = true;
        private int multiplicadorXp = 1;

        public int MultiplicadorXp => multiplicadorXp;

        public string DificultadActual
        {
            get
            {
                int mins = Mathf.FloorToInt(tiempoPartida / 60f);
                if (mins < 4) return "Fácil";
                if (mins < 8) return "Normal";
                if (mins < 12) return "Difícil";
                if (mins < 14) return "Locura";
                return "UN DRAGON";
            }
        }

        public int EnemigosVivosTotales
        {
            get
            {
                int total = 0;
                foreach (var s in spawners)
                    if (s != null) total += s.EnemigosVivos;
                return total;
            }
        }

        public string TiempoFormateado
        {
            get
            {
                int mins = Mathf.FloorToInt(tiempoPartida / 60f);
                int segs = Mathf.FloorToInt(tiempoPartida % 60f);
                return $"{mins}:{segs:00}";
            }
        }

        public bool EstaPausado => pausado;

        public void IniciarTodos()
        {
            pausado = false;
            tiempoCiclo = 0f;
            multiplicadorXp = 1;
            foreach (var s in spawners)
                if (s != null) s.Activar();
        }

        public void PausarReanudar()
        {
            pausado = !pausado;
            if (!pausado)
            {
                foreach (var s in spawners)
                    if (s != null) s.Activar();
            }
        }

        public void SpawnearEnemigo()
        {
            foreach (var s in spawners)
                if (s != null && s.EstaSpawneando)
                    s.SpawnearUno();
        }

        public List<GameObject> ObtenerTodosLosPrefabs()
        {
            var lista = new List<GameObject>();
            foreach (var s in spawners)
            {
                if (s == null) continue;
                foreach (var prefab in s.ObtenerPrefabs())
                    if (prefab != null && !lista.Contains(prefab))
                        lista.Add(prefab);
            }
            return lista;
        }

        public void SpawnearPrefab(GameObject prefab, Vector3 posicion)
        {
            var enemy = Instantiate(prefab, posicion, Quaternion.identity);
            var skull = enemy.GetComponent<BasicEnemy>();
            if (skull != null)
                skull.spawner = spawners.Length > 0 ? spawners[0] : null;
        }

        void Update()
        {
            if (pausado) return;

            tiempoPartida += Time.deltaTime;
            tiempoCiclo += Time.deltaTime;

            if (tiempoCiclo >= duracionCiclo)
            {
                tiempoCiclo = 0f;
                multiplicadorXp *= 2;
            }

            foreach (var s in spawners)
                if (s != null) s.Actualizar(tiempoCiclo);
        }
    }
}
