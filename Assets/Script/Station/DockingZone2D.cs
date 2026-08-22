using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DockingZone2D : MonoBehaviour
{
    [Header("Docking Point")]
    [SerializeField] private Transform dockPoint;
    [SerializeField] private float approachDistance = 3f;

    [Header("Docking Conditions")]
    [SerializeField] private string shipTag = "Player";
    [SerializeField] private float maxDockSpeed = 1.5f;
    [SerializeField] private bool requirePiloting = true;

    [Header("Input")]
    [SerializeField] private KeyCode dockingKey = KeyCode.F;

    public DockableShip2D CurrentShip { get; private set; }
    public bool HasShip => CurrentShip != null;
    public Transform DockPoint => dockPoint;
    public bool CanDock => CurrentShip != null && CanDockShip(CurrentShip);
    public bool CanTakeoff => CurrentShip != null && CurrentShip.IsDocked && IsPilotAllowed(CurrentShip);

    public event Action<DockableShip2D> ShipEntered;
    public event Action<DockableShip2D> ShipExited;
    public event Action<DockableShip2D, DockableShip2D.DockState> ShipStateChanged;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;

        if (dockPoint == null)
        {
            dockPoint = transform;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(dockingKey))
        {
            ToggleLandingTakeoff();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        DockableShip2D ship = ResolveShip(other);
        if (ship == null) return;

        if (CurrentShip == null || CurrentShip == ship)
        {
            bool wasEmpty = CurrentShip == null;
            CurrentShip = ship;

            if (wasEmpty)
            {
                ShipEntered?.Invoke(ship);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        DockableShip2D ship = ResolveShip(other);
        if (ship == null || ship != CurrentShip) return;

        // Keep the reference while automatic docking moves the ship.
        if (ship.State != DockableShip2D.DockState.Flying) return;

        CurrentShip = null;
        ShipExited?.Invoke(ship);
    }

    public void ToggleLandingTakeoff()
    {
        if (CurrentShip == null || CurrentShip.IsTransitioning) return;

        if (CurrentShip.IsDocked)
        {
            if (CanTakeoff)
            {
                CurrentShip.BeginTakeoff();
            }

            return;
        }

        if (CanDock)
        {
            CurrentShip.BeginLanding(this, dockPoint, approachDistance);
        }
    }

    public bool CanDockShip(DockableShip2D ship)
    {
        if (ship == null || ship != CurrentShip || ship.State != DockableShip2D.DockState.Flying)
        {
            return false;
        }

        if (!IsPilotAllowed(ship)) return false;
        if (ship.CurrentSpeed > maxDockSpeed) return false;

        // Orientation is intentionally not a docking condition.
        return true;
    }

    public string GetDockingFailureReason()
    {
        if (CurrentShip == null) return "No ship in docking zone";
        if (!IsPilotAllowed(CurrentShip)) return "Piloting mode is required";
        if (CurrentShip.State != DockableShip2D.DockState.Flying) return "Ship cannot dock now";
        if (CurrentShip.CurrentSpeed > maxDockSpeed) return "Ship speed is too high";
        return string.Empty;
    }

    public void NotifyDockedStateChanged(DockableShip2D ship, DockableShip2D.DockState state)
    {
        if (ship != CurrentShip) return;

        ShipStateChanged?.Invoke(ship, state);

        if (state == DockableShip2D.DockState.Flying)
        {
            CurrentShip = null;
            ShipExited?.Invoke(ship);
        }
    }

    private bool IsPilotAllowed(DockableShip2D ship)
    {
        if (!requirePiloting) return true;

        ShipVisualController visualController = ship.GetComponent<ShipVisualController>();
        return visualController == null || visualController.isPiloting;
    }

    private DockableShip2D ResolveShip(Collider2D other)
    {
        DockableShip2D ship = other.GetComponent<DockableShip2D>();
        if (ship == null)
        {
            ship = other.GetComponentInParent<DockableShip2D>();
        }

        if (ship == null && other.attachedRigidbody != null)
        {
            ship = other.attachedRigidbody.GetComponent<DockableShip2D>();
        }

        if (ship == null || !MatchesShipTag(ship.gameObject)) return null;
        return ship;
    }

    private bool MatchesShipTag(GameObject shipObject)
    {
        return string.IsNullOrEmpty(shipTag) || shipObject.CompareTag(shipTag);
    }
}
