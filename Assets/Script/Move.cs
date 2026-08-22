using UnityEngine;

public class Move : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 4f;

    [Header("背包")]
    public GameObject cargo;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isopen;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }

    private void Update()
    {
        bool disableMove = SpaceshipAssemblyUI.IsUIOpen ||
                           (GameControlManager.Instance != null &&
                            GameControlManager.Instance.CurrentMode == GameControlManager.ControlMode.Piloting);

        if (disableMove)
        {
            moveInput = Vector2.zero;
            if (rb != null) rb.velocity = Vector2.zero;
            return;
        }

        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        OpenMyBag();
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        Vector2 targetPosition = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
    }

    private void OpenMyBag()
    {
        if (Input.GetKeyDown(KeyCode.I) && cargo != null)
        {
            isopen = !isopen;
            cargo.SetActive(isopen);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Home"))
        {
            Debug.Log("玩家碰到了 Home");
        }

        if (collision.gameObject.CompareTag("Planet"))
        {
            Debug.Log("玩家碰到了 Planet");
        }
    }
}
