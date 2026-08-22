using UnityEngine;

public class ShipMove : MonoBehaviour
{
    public float speed;
    public float acceleration;
    public float degreespeed;

    private Vector2 targetPosition;
    private float currentSpeed;
    private Vector2 currentVector;
    private bool cruiseMode = true;
    private bool cruiseTarget;
    private Rigidbody2D rb;
    private Camera mainCamera;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.angularDrag = 0f;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }

    private void OnEnable()
    {
        mainCamera = Camera.main;
        targetPosition = rb != null ? rb.position : (Vector2)transform.position;
        StopShip();
    }

    private void Update()
    {
        if (SpaceshipAssemblyUI.IsUIOpen)
        {
            StopShip();
            return;
        }

        HandleModeInput();
        UpdateTargetInput();
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        if (SpaceshipAssemblyUI.IsUIOpen)
        {
            StopShip();
            return;
        }

        if (cruiseMode)
        {
            HandleCruiseMode();
        }
        else
        {
            HandleBattleMode();
        }
    }

    private void HandleModeInput()
    {
        if (!Input.GetKeyDown(KeyCode.X)) return;

        cruiseMode = !cruiseMode;
        cruiseTarget = false;
        StopShip();

        if (!cruiseMode)
        {
            UpdateMouseTarget();
        }
    }

    private void UpdateTargetInput()
    {
        if (cruiseMode)
        {
            if (IsPointerOverUI()) return;

            if (Input.GetMouseButtonDown(1) && TryGetMouseWorldPosition(out Vector2 mousePosition))
            {
                targetPosition = mousePosition;
                cruiseTarget = true;
            }

            return;
        }

        UpdateMouseTarget();
    }

    private void HandleCruiseMode()
    {
        if (!cruiseTarget)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        Vector2 toTarget = targetPosition - rb.position;
        float distance = toTarget.magnitude;

        if (distance <= 0.1f)
        {
            cruiseTarget = false;
            StopShip();
            return;
        }

        Vector2 direction = toTarget / distance;
        RotateTowards(direction);

        float decelerationDistance = currentSpeed * currentSpeed / Mathf.Max(2f * acceleration, 0.001f);
        if (distance <= decelerationDistance)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, speed, acceleration * Time.fixedDeltaTime);
        }

        rb.velocity = direction * currentSpeed;
    }

    private void HandleBattleMode()
    {
        Vector2 toMouse = targetPosition - rb.position;
        if (toMouse.sqrMagnitude > 0.0001f)
        {
            RotateTowards(toMouse.normalized);
        }

        MoveWithKeyboard();
    }

    private void MoveWithKeyboard()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector2 inputDirection = new Vector2(horizontal, vertical).normalized;

        if (inputDirection.sqrMagnitude > 0.25f)
        {
            currentVector += inputDirection * acceleration * Time.fixedDeltaTime;
            currentVector = Vector2.ClampMagnitude(currentVector, speed);
        }
        else
        {
            currentVector = Vector2.MoveTowards(currentVector, Vector2.zero, acceleration * Time.fixedDeltaTime);
        }

        rb.velocity = transform.TransformDirection(currentVector);
    }

    private void RotateTowards(Vector2 direction)
    {
        rb.angularVelocity = 0f;

        float targetRotation = Mathf.Atan2(-direction.x, direction.y) * Mathf.Rad2Deg;
        float newRotation = Mathf.MoveTowardsAngle(
            rb.rotation,
            targetRotation,
            degreespeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(newRotation);
    }

    private void UpdateMouseTarget()
    {
        if (TryGetMouseWorldPosition(out Vector2 mousePosition))
        {
            targetPosition = mousePosition;
        }
    }

    private bool TryGetMouseWorldPosition(out Vector2 mousePosition)
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            mousePosition = targetPosition;
            return false;
        }

        mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        return true;
    }

    private bool IsPointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current != null &&
               UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    private void StopShip()
    {
        currentSpeed = 0f;
        currentVector = Vector2.zero;
        if (rb != null) rb.velocity = Vector2.zero;
    }
}
