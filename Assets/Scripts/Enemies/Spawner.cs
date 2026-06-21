using System.Collections.Generic;
using UnityEngine;

namespace DragonCeltas
{
    public class Spawner : MonoBehaviour
    {
        [Header("Fases del Portal")]
        [SerializeField] private FaseSpawn[] fases;

        private bool activo;
        private int enemigosVivos;
        private float timerSpawn;

        public int EnemigosVivos => enemigosVivos;
        public bool EstaSpawneando => activo;

        [System.Serializable]
        public class FaseSpawn
        {
            public int minutosInicio;
            public int segundosInicio;
            public int minutosFin;
            public int segundosFin;
            public float intervalo = 1.5f;
            public EnemigoInfo[] enemigos;

            public float TiempoInicio => minutosInicio * 60f + segundosInicio;
            public float TiempoFin => minutosFin * 60f + segundosFin;
        }

        [System.Serializable]
        public class EnemigoInfo
        {
            public GameObject prefab;
        }

        public void Activar() { activo = true; enemigosVivos = 0; timerSpawn = 0f; }
        public void Desactivar() { activo = false; }

        public void Actualizar(float tiempoJuego)
        {
            if (!activo) return;

            foreach (var fase in fases)
            {
                if (tiempoJuego < fase.TiempoInicio || tiempoJuego >= fase.TiempoFin) continue;
                if (fase.enemigos.Length == 0) continue;

                timerSpawn -= Time.deltaTime;
                if (timerSpawn <= 0f)
                {
                    timerSpawn = fase.intervalo;
                    var info = fase.enemigos[Random.Range(0, fase.enemigos.Length)];
                    var enemy = Instantiate(info.prefab, transform.position, Quaternion.identity);
                    var skull = enemy.GetComponent<BasicEnemy>();
                    if (skull != null)
                        skull.spawner = this;
                    enemigosVivos++;
                }
                break;
            }
        }

        public void HandleEnemyDeath()
        {
            enemigosVivos--;
            if (enemigosVivos < 0) enemigosVivos = 0;
        }

        public void SpawnearUno()
        {
            foreach (var fase in fases)
            {
                if (fase.enemigos.Length == 0) continue;
                var info = fase.enemigos[Random.Range(0, fase.enemigos.Length)];
                var enemy = Instantiate(info.prefab, transform.position, Quaternion.identity);
                var skull = enemy.GetComponent<BasicEnemy>();
                if (skull != null)
                    skull.spawner = this;
                enemigosVivos++;
                return;
            }
        }

        public List<GameObject> ObtenerPrefabs()
        {
            var lista = new List<GameObject>();
            foreach (var fase in fases)
                foreach (var info in fase.enemigos)
                    if (info.prefab != null && !lista.Contains(info.prefab))
                        lista.Add(info.prefab);
            return lista;
        }

        public void SpawnearPrefab(GameObject prefab)
        {
            var enemy = Instantiate(prefab, transform.position, Quaternion.identity);
            var skull = enemy.GetComponent<BasicEnemy>();
            if (skull != null)
                skull.spawner = this;
            enemigosVivos++;
        }
    }
}
