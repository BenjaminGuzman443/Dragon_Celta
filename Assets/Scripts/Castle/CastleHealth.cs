using UnityEngine;

namespace DragonCeltas
{
    public class CastleHealth : MonoBehaviour
    {
        [Header("Vida")]
        [SerializeField] private float maxHp = 200f;

        [Header("Barra de Vida")]
        [SerializeField] private float barWidth = 4f;
        [SerializeField] private float barHeight = 0.3f;
        [SerializeField] private Vector2 barOffset = new Vector2(0f, 2f);

        private float hp;
        private Transform fillTransform;
        private SpriteRenderer fillRenderer;

        public float HpNormalized => Mathf.Clamp01(hp / maxHp);
        public float Hp => hp;

        void Start()
        {
            hp = maxHp;
            CreateHealthBar();
        }

        private void CreateHealthBar()
        {
            var bgGO = new GameObject("HealthBar_BG");
            bgGO.transform.SetParent(transform);
            bgGO.transform.localPosition = barOffset;
            var bgSR = bgGO.AddComponent<SpriteRenderer>();
            bgSR.sprite = SpriteUtils.CreatePixelSprite(new Vector2(0.5f, 0.5f));
            bgSR.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            bgSR.sortingOrder = 100;
            bgGO.transform.localScale = new Vector3(barWidth, barHeight, 1f);

            var fillGO = new GameObject("HealthBar_Fill");
            fillGO.transform.SetParent(transform);
            fillGO.transform.localPosition = new Vector3(barOffset.x - barWidth / 2f, barOffset.y, 0f);
            fillRenderer = fillGO.AddComponent<SpriteRenderer>();
            fillRenderer.sprite = SpriteUtils.CreatePixelSprite(new Vector2(0f, 0.5f));
            fillRenderer.sortingOrder = 101;
            fillTransform = fillGO.transform;
            fillTransform.localScale = new Vector3(barWidth, barHeight, 1f);

            UpdateBar();
        }

        private void UpdateBar()
        {
            if (fillTransform == null) return;

            float n = HpNormalized;
            fillTransform.localScale = new Vector3(barWidth * n, barHeight, 1f);

            if (n > 0.5f)
                fillRenderer.color = Color.Lerp(Color.yellow, Color.green, (n - 0.5f) * 2f);
            else
                fillRenderer.color = Color.Lerp(Color.red, Color.yellow, n * 2f);
        }

        public void TakeDamage(float damage)
        {
            hp -= damage;

            if (hp < 0f)
                hp = 0f;

            UpdateBar();

            if (hp <= 0f)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GameOver();
                }

                enabled = false;
            }
        }
    }
}