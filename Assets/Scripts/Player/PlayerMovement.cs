using UnityEngine;
using UnityEngine.InputSystem;

namespace DragonCeltas
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Animator))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movimiento")]
        [SerializeField] private float moveSpeed = 3.2f;

        [Header("Sprint")]
        [SerializeField] private float sprintSpeed = 6f;
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float staminaDrainRate = 25f;
        [SerializeField] private float staminaRegenRate = 15f;

        public float MoveSpeed => moveSpeed;
        public float SprintSpeed => sprintSpeed;
        public float StaminaNormalized => Mathf.Clamp01(stamina / maxStamina);
        public bool EstaCorriendo => estaCorriendo;
        public bool QuiereCorrer => quiereCorrer;

        public bool penalizacionCero;

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;
        private Animator animator;
        private PlayerCombat combat;
        private PlayerHealth health;

        private Vector2 moveInput;
        private bool estaCorriendo;
        private bool quiereCorrer;
        private float stamina;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            combat = GetComponent<PlayerCombat>();
            health = GetComponent<PlayerHealth>();

            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            animator.applyRootMotion = false;

            stamina = maxStamina;
        }

        void Update()
        {
            if (PlayerHealth.IsDead) return;

            LeerMovimiento();
            ActualizarStamina();
            Mover();
        }

        private void LeerMovimiento()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            float h = 0f;
            float v = 0f;

            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) h = -1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h = 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v = 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v = -1f;

            moveInput = new Vector2(h, v).normalized;

            quiereCorrer = kb.shiftKey.isPressed && moveInput.magnitude > 0f;
            estaCorriendo = quiereCorrer && (penalizacionCero || stamina > 0f);

            if (h < 0f) spriteRenderer.flipX = true;
            if (h > 0f) spriteRenderer.flipX = false;

            bool blocked = !penalizacionCero && combat != null && (combat.IsGuarding || combat.IsAttacking);
            float speedParam = blocked ? 0f : moveInput.magnitude;
            animator.SetFloat("Speed", speedParam);
        }

        private void ActualizarStamina()
        {
            if (penalizacionCero)
            {
                stamina = maxStamina;
                return;
            }

            bool blocked = !penalizacionCero && combat != null && (combat.IsGuarding || combat.IsAttacking);
            if (estaCorriendo && !blocked)
            {
                stamina -= staminaDrainRate * Time.deltaTime;
                if (stamina < 0f)
                {
                    stamina = 0f;
                    estaCorriendo = false;
                }
            }
            else
            {
                stamina += staminaRegenRate * Time.deltaTime;
                if (stamina > maxStamina)
                    stamina = maxStamina;
            }
        }

        private void Mover()
        {
            bool blocked = !penalizacionCero && combat != null && (combat.IsGuarding || combat.IsAttacking);
            if (blocked)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            float speed = moveSpeed;
            if (estaCorriendo)
            {
                float staminaPercent = stamina / maxStamina;
                speed = moveSpeed + (sprintSpeed - moveSpeed) * staminaPercent;
            }

            rb.linearVelocity = moveInput * speed;
        }

        public void IncreaseSpeed(float amount)
        {
            moveSpeed += amount;
            sprintSpeed += amount;
        }

        public void BoostStamina(float porcentaje)
        {
            maxStamina += maxStamina * (porcentaje / 100f);
            stamina = maxStamina;
        }

    }
}