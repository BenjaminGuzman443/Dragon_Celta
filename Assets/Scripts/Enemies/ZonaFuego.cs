using UnityEngine;

namespace DragonCeltas
{
    public class ZonaFuego : MonoBehaviour
    {
        private float radio;
        private float danoPorTick;
        private float intervaloDano;
        private float duracion;
        private float timerDano;
        private float timerVida;
        private bool haceVeneno;
        private float duracionVeneno;
        private GameObject prefabFuego;
        private float timerFuego;

        public void Inicializar(float r, float dmg, float intervalo, float dur, bool veneno, float venenoDur, GameObject fuegoPrefab)
        {
            radio = r;
            danoPorTick = dmg;
            intervaloDano = intervalo;
            duracion = dur;
            timerVida = dur;
            haceVeneno = veneno;
            duracionVeneno = venenoDur;
            prefabFuego = fuegoPrefab;
            timerDano = 0f;
            timerFuego = 0f;

            CrearIndicador();
        }

        private void CrearIndicador()
        {
            var go = new GameObject("IndicadorFuego");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CrearCirculo(radio);
            sr.color = new Color(1f, 0.5f, 0f, 0.25f);
            sr.sortingOrder = 50;
        }

        private Sprite CrearCirculo(float radius)
        {
            int size = 256;
            var tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Bilinear;
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float pixelRadius = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    tex.SetPixel(x, y, dist <= pixelRadius ? Color.white : Color.clear);
                }
            }
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size / (radius * 2f));
        }

        void Update()
        {
            timerVida -= Time.deltaTime;
            if (timerVida <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            timerDano -= Time.deltaTime;
            if (timerDano <= 0f)
            {
                timerDano = intervaloDano;
                AplicarDano();
            }

            if (prefabFuego != null)
            {
                timerFuego -= Time.deltaTime;
                if (timerFuego <= 0f)
                {
                    timerFuego = 0.3f;
                    float x = Random.Range(-radio, radio) * 0.7f;
                    float y = Random.Range(-radio, radio) * 0.7f;
                    var fuego = Instantiate(prefabFuego, (Vector2)transform.position + new Vector2(x, y), Quaternion.identity);
                    Destroy(fuego, 0.5f);
                }
            }
        }

        private void AplicarDano()
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, radio);
            foreach (var hit in hits)
            {
                var castle = hit.GetComponentInParent<CastleHealth>();
                if (castle != null)
                {
                    castle.TakeDamage(danoPorTick);
                    continue;
                }

                var player = hit.GetComponentInParent<PlayerHealth>();
                if (player != null)
                {
                    player.TakeDamage(danoPorTick);
                    if (haceVeneno)
                        player.AplicarVeneno(duracionVeneno);
                }
            }
        }
    }
}
