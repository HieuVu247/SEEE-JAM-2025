using UnityEngine;

namespace SEEEJam.Combat
{
    public class BaseBullet : MonoBehaviour
    {
        [Header("Flight Settings")]
        [Min(0f)]
        [Tooltip("Tốc độ bay của viên đạn.")]
        [SerializeField] private float speed = 12f;

        [Min(0.05f)]
        [Tooltip("Thời gian đạn bay trước khi tự nổ.")]
        [SerializeField] private float timeTillExplosion = 1.5f;

        [Header("Explosion Settings")]
        [Min(0)]
        [Tooltip("Lượng sát thương gây ra khi đạn nổ.")]
        [SerializeField] private int damage = 1;

        [Min(0f)]
        [Tooltip("Cường độ vụ nổ (bán kính ảnh hưởng).")]
        [SerializeField] private float explosionRadius = 1.5f;

        [Min(0f)]
        [Tooltip("Độ mạnh của lực hất văng lên kẻ địch.")]
        [SerializeField] private float knockBackIntensity = 5f;

        [Tooltip("Layer của Enemy để xác định mục tiêu bị ảnh hưởng.")]
        [SerializeField] private LayerMask enemyLayer;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite sprite;

        private Vector2 travelDirection;
        private float lifetimeTimer;
        private bool hasExploded;

        private void Awake()
        {
            lifetimeTimer = timeTillExplosion;
            CacheRenderer();
            ApplySprite();
        }

        private void CacheRenderer()
        {
            if (spriteRenderer != null)
            {
                return;
            }

            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void ApplySprite()
        {
            if (spriteRenderer != null && sprite != null)
            {
                spriteRenderer.sprite = sprite;
            }
        }

        private void Update()
        {
            if (hasExploded)
            {
                return;
            }

            transform.position += (Vector3)(travelDirection * speed * Time.deltaTime);
            lifetimeTimer -= Time.deltaTime;
            if (lifetimeTimer <= 0f)
            {
                Explode();
            }
        }

        public void Initialize(Vector2 direction)
        {
            travelDirection = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.right;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (hasExploded)
            {
                return;
            }

            if (((1 << collision.gameObject.layer) & enemyLayer.value) != 0)
            {
                Explode();
            }
        }

        private void Explode()
        {
            hasExploded = true;
            Vector2 center = transform.position;
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, explosionRadius, enemyLayer);

            foreach (Collider2D hit in hits)
            {
                if (hit.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(damage);
                }

                if (hit.attachedRigidbody != null)
                {
                    Vector2 knockDirection = ((Vector2)hit.transform.position - center).normalized;
                    hit.attachedRigidbody.AddForce(knockDirection * knockBackIntensity, ForceMode2D.Impulse);
                }
            }

            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }

        #region Editor Helpers
        private void OnValidate()
        {
            CacheRenderer();
            ApplySprite();

            if (timeTillExplosion < 0.05f)
            {
                timeTillExplosion = 0.05f;
            }
        }
        #endregion

        #region Exposed Properties
        public int Damage
        {
            get => damage;
            set => damage = Mathf.Max(0, value);
        }

        public float Speed
        {
            get => speed;
            set => speed = Mathf.Max(0f, value);
        }

        public float KnockBackIntensity
        {
            get => knockBackIntensity;
            set => knockBackIntensity = Mathf.Max(0f, value);
        }

        public float ExplosionRadius
        {
            get => explosionRadius;
            set => explosionRadius = Mathf.Max(0f, value);
        }

        public float TimeTillExplosion
        {
            get => timeTillExplosion;
            set => timeTillExplosion = Mathf.Max(0.05f, value);
        }

        public Sprite Sprite
        {
            get => sprite;
            set
            {
                sprite = value;
                ApplySprite();
            }
        }
        #endregion
    }
}
