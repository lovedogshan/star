using System;
using UnityEngine;

public class DockableShip2D : MonoBehaviour
{
    public enum DockState
    {
        Flying,
        Approaching,
        Landing,
        Docked,
        TakingOff
    }

    [Header("Automatic Docking")]
    [SerializeField] private float approachSpeed = 4f;
    [SerializeField] private float landingSpeed = 1.5f;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float positionTolerance = 0.05f;
    [SerializeField] private float takeoffDistance = 4f;
    [SerializeField] private float takeoffSpeed = 3f;

    [Header("Control")]
    [SerializeField] private ShipMove shipMove;
    [SerializeField] private MonoBehaviour[] disableWhileDocked;
    [SerializeField] private TurretFire[] turrets;

    public DockState State { get; private set; } = DockState.Flying;
    public DockingZone2D CurrentDockingZone { get; private set; }
    public bool IsDocked => State == DockState.Docked;
    public bool IsTransitioning => State == DockState.Approaching ||
                                   State == DockState.Landing ||
                                   State == DockState.TakingOff;
    public float CurrentSpeed => rb != null ? rb.velocity.magnitude : 0f;
    public bool CanAcceptManualControl => State == DockState.Flying;

    public event Action<DockState> StateChanged;

    private Rigidbody2D rb;
    private Transform dockPoint;
    private Vector2 approachPosition;
    private bool shipMoveWasEnabled;

    private void Reset()
    {
        shipMove = GetComponent<ShipMove>();
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (shipMove == null)
        {
            shipMove = GetComponent<ShipMove>();
        }

        RefreshTurrets();
    }

    private void FixedUpdate()
    {
        switch (State)
        {
            case DockState.Approaching:
                MoveTowardsApproachPoint();
                break;
            case DockState.Landing:
                MoveIntoDockPoint();
                break;
            case DockState.TakingOff:
                MoveOutOfDockingZone();
                break;
        }
    }

    public bool BeginLanding(DockingZone2D zone, Transform target, float approachDistance)
    {
        if (State != DockState.Flying || target == null) return false;

        CurrentDockingZone = zone;
        dockPoint = target;
        approachPosition = (Vector2)dockPoint.position - (Vector2)dockPoint.up * Mathf.Max(0f, approachDistance);

        BeginAutomaticControl();
        SetState(DockState.Approaching);
        return true;
    }

    public bool BeginTakeoff()
    {
        if (State != DockState.Docked || dockPoint == null) return false;

        BeginAutomaticControl();
        SetDockedBehaviours(false);
        SetState(DockState.TakingOff);
        return true;
    }

    private void MoveTowardsApproachPoint()
    {
        MovePosition(approachPosition, approachSpeed);
        RotateTowards(dockPoint.up);

        if (Vector2.Distance(rb.position, approachPosition) <= positionTolerance)
        {
            SetState(DockState.Landing);
        }
    }

    private void MoveIntoDockPoint()
    {
        MovePosition(dockPoint.position, landingSpeed);
        RotateTowards(dockPoint.up);

        if (Vector2.Distance(rb.position, dockPoint.position) <= positionTolerance)
        {
            rb.position = dockPoint.position;
            rb.rotation = dockPoint.eulerAngles.z;
            CompleteLanding();
        }
    }

    private void MoveOutOfDockingZone()
    {
        Vector2 exitPosition = (Vector2)dockPoint.position + (Vector2)dockPoint.up * takeoffDistance;
        MovePosition(exitPosition, takeoffSpeed);
        RotateTowards(dockPoint.up);

        if (Vector2.Distance(rb.position, exitPosition) <= positionTolerance)
        {
            CompleteTakeoff();
        }
    }

    private void CompleteLanding()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        SetTurretsDocked(true);
        SetDockedBehaviours(true);
        SetState(DockState.Docked);
    }

    private void CompleteTakeoff()
    {
        rb.velocity = (Vector2)dockPoint.up * takeoffSpeed;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Dynamic;
        SetTurretsDocked(false);
        RestoreAutomaticControl();
        SetState(DockState.Flying);
    }

    private void BeginAutomaticControl()
    {
        if (shipMove != null)
        {
            shipMoveWasEnabled = shipMove.enabled;
            shipMove.enabled = false;
        }

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    private void RestoreAutomaticControl()
    {
        if (shipMove != null)
        {
            ShipVisualController visualController = GetComponent<ShipVisualController>();
            if (visualController != null)
            {
                // Piloting state is the source of truth after docking has taken control.
                shipMove.enabled = visualController.isPiloting;
            }
            else
            {
                shipMove.enabled = shipMoveWasEnabled;
            }
        }
    }

    private void SetDockedBehaviours(bool docked)
    {
        foreach (MonoBehaviour behaviour in disableWhileDocked)
        {
            if (behaviour != null)
            {
                behaviour.enabled = !docked;
            }
        }
    }

    private void SetTurretsDocked(bool docked)
    {
        RefreshTurrets();

        foreach (TurretFire turret in turrets)
        {
            if (turret != null)
            {
                turret.SetDocked(docked);
            }
        }
    }

    private void RefreshTurrets()
    {
        TurretFire[] childTurrets = GetComponentsInChildren<TurretFire>(true);
        if (childTurrets.Length > 0)
        {
            turrets = childTurrets;
        }
        else if (turrets == null)
        {
            turrets = new TurretFire[0];
        }
    }

    private void MovePosition(Vector2 target, float moveSpeed)
    {
        if (rb == null) return;

        rb.velocity = Vector2.zero;
        rb.MovePosition(Vector2.MoveTowards(rb.position, target, moveSpeed * Time.fixedDeltaTime));
    }

    private void RotateTowards(Vector2 direction)
    {
        if (rb == null || direction.sqrMagnitude <= 0.0001f) return;

        float targetRotation = Mathf.Atan2(-direction.x, direction.y) * Mathf.Rad2Deg;
        float nextRotation = Mathf.MoveTowardsAngle(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(nextRotation);
    }

    private void SetState(DockState nextState)
    {
        if (State == nextState) return;

        State = nextState;
        StateChanged?.Invoke(State);

        if (CurrentDockingZone != null)
        {
            CurrentDockingZone.NotifyDockedStateChanged(this, State);
        }
    }
}
