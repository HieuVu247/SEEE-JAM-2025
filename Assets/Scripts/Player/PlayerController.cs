using System.Collections;
using UnityEngine;
using SEEEJam.Combat;

namespace SEEEJam.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private PlayerConfig config;
        [SerializeField] private Transform firePoint;
        [SerializeField] private BaseBullet bulletPrefab;

        [Header("Runtime References")]
        [SerializeField] private Rigidbody2D rb;

        private Vector2 moveInput;
        private Vector2 lastMoveDirection = Vector2.right;
        private bool isDashing;
        private float dashCooldownTimer;
        private float shootCooldownTimer;
        private Coroutine dashRoutine;
        private int currentHp;
        private Camera mainCamera;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            mainCamera = Camera.main;

            if (rb == null)
            {
                rb = GetComponent<Rigidbody2D>();
            }

            int startingHp = config != null ? config.maxHp : 3;
            currentHp = Mathf.Max(1, startingHp);
        }

        private void Update()
        {
            ReadMovementInput();

            if (!isDashing)
            {
                HandleDashInput();
            }

            HandleShootInput();

            if (dashCooldownTimer > 0f)
            {
                dashCooldownTimer -= Time.deltaTime;
                if (dashCooldownTimer < 0f)
                {
                    dashCooldownTimer = 0f;
                }
            }

            if (shootCooldownTimer > 0f)
            {
                shootCooldownTimer -= Time.deltaTime;
                if (shootCooldownTimer < 0f)
                {
                    shootCooldownTimer = 0f;
                }
            }
        }

        private void FixedUpdate()
        {
            if (isDashing)
            {
                return;
            }

            Move();
        }

        private void ReadMovementInput()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            moveInput = new Vector2(horizontal, vertical);

            if (moveInput.sqrMagnitude > 0.01f)
            {
                lastMoveDirection = moveInput.normalized;
            }
        }

        private void Move()
        {
            float speed = config != null ? config.moveSpeed : 5f;
            Vector2 desiredVelocity = moveInput.normalized * speed;
            rb.velocity = desiredVelocity;
        }

        private void HandleDashInput()
        {
            if (!Input.GetKeyDown(KeyCode.Space) || dashCooldownTimer > 0f)
            {
                return;
            }

            if (moveInput.sqrMagnitude <= 0.01f && lastMoveDirection.sqrMagnitude <= 0.01f)
            {
                return;
            }

            Vector2 dashDirection = moveInput.sqrMagnitude > 0.01f ? moveInput.normalized : lastMoveDirection.normalized;

            if (dashRoutine != null)
            {
                StopCoroutine(dashRoutine);
            }

            dashRoutine = StartCoroutine(DashRoutine(dashDirection));
        }

        private IEnumerator DashRoutine(Vector2 direction)
        {
            isDashing = true;
            float dashDuration = config != null ? config.dashDuration : 0.2f;
            float dashDistance = config != null ? config.dashDistance : 3f;
            float dashSpeed = dashDistance / Mathf.Max(0.01f, dashDuration);
            float elapsed = 0f;

            rb.velocity = Vector2.zero;

            while (elapsed < dashDuration)
            {
                rb.velocity = direction * dashSpeed;
                elapsed += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            rb.velocity = Vector2.zero;
            isDashing = false;
            dashCooldownTimer = config != null ? config.dashCooldown : 1.2f;
            dashRoutine = null;
        }

        private void HandleShootInput()
        {
            if (!Input.GetMouseButton(0) || shootCooldownTimer > 0f || bulletPrefab == null)
            {
                return;
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    return;
                }
            }

            Transform spawnPoint = firePoint != null ? firePoint : transform;

            Vector3 mousePosition = Input.mousePosition;
            Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(mousePosition);
            Vector2 shootDirection = ((Vector2)worldMousePosition - (Vector2)spawnPoint.position).normalized;

            if (shootDirection.sqrMagnitude < 0.001f)
            {
                return;
            }

            BaseBullet bulletInstance = Instantiate(bulletPrefab, spawnPoint.position, Quaternion.identity);
            bulletInstance.Initialize(shootDirection);

            shootCooldownTimer = config != null ? config.shootCooldown : 0.25f;
        }

        public void TakeDamage(int damage)
        {
            currentHp -= Mathf.Abs(damage);
            currentHp = Mathf.Clamp(currentHp, 0, MaxHp);

            if (currentHp == 0)
            {
                HandleDeath();
            }
        }

        private void HandleDeath()
        {
            Debug.Log("Player has died.");
            // TODO: Trigger death animation / game over logic.
        }

        public int CurrentHp => currentHp;
        public int MaxHp => config != null ? config.maxHp : 3;
    }
}
