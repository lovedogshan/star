using UnityEngine;
using CodeMonkey.CameraSystem;

public class ShipVisualController : MonoBehaviour
{
    [Header("Display Roots")]
    public GameObject exteriorRoot;
    public GameObject interiorRoot;

    [Header("Optional Cameras")]
    public Camera exteriorCamera;
    public Camera interiorCamera;

    [Header("Display Rules")]
    public bool useCameraSwitch = false;
    public bool hideExteriorWhenOnFoot = true;
    public bool hidePlayerWhenPiloting = true;

    [Header("Control Scripts")]
    public MonoBehaviour shipMoveScript;
    public MonoBehaviour playerMoveScript;
    public GameObject playerObject;
    public DockableShip2D dockableShip;

    [Header("Physics")]
    public Rigidbody2D shipRigidbody;
    public bool makeShipKinematicWhenOnFoot = true;

    [Header("Camera Follow")]
    public CameraSystem2D cameraSystem;

    public bool isPiloting;

    private Renderer[] exteriorRenderers;
    private Renderer[] interiorRenderers;
    private Canvas[] exteriorCanvases;
    private Canvas[] interiorCanvases;
    private Transform originalPlayerParent;
    private bool cachedOriginalPlayerParent;
    private bool subscribedToDockState;

    private void Awake()
    {
        ResolveCameraSystem();
        ResolveVisualRoots();
        ResolveDockableShip();
        SubscribeToDockState();
        CacheVisualComponents();
        ApplyControlState();
        ApplyVisualState();
        RegisterCameraTargets();
    }

    private void OnEnable()
    {
        SubscribeToDockState();
    }

    private void OnDisable()
    {
        if (subscribedToDockState && dockableShip != null)
        {
            dockableShip.StateChanged -= OnDockStateChanged;
            subscribedToDockState = false;
        }
    }

    private void SubscribeToDockState()
    {
        if (subscribedToDockState || dockableShip == null) return;

        dockableShip.StateChanged += OnDockStateChanged;
        subscribedToDockState = true;
    }

    private void OnDockStateChanged(DockableShip2D.DockState state)
    {
        ApplyControlState();
    }

    private void ResolveCameraSystem()
    {
        if (cameraSystem == null)
        {
            cameraSystem = FindObjectOfType<CameraSystem2D>();
        }
    }

    private void ResolveDockableShip()
    {
        if (dockableShip == null)
        {
            dockableShip = GetComponent<DockableShip2D>();
        }
    }

    private void ResolveVisualRoots()
    {
        if (interiorRoot == null)
        {
            Transform interior = FindSiblingOrChild("Interior");
            if (interior != null)
            {
                interiorRoot = interior.gameObject;
            }
        }

        if (exteriorRoot == null)
        {
            Transform exterior = FindSiblingOrChild("Exterior");
            if (exterior != null)
            {
                exteriorRoot = exterior.gameObject;
            }
        }
    }

    private Transform FindSiblingOrChild(string childName)
    {
        Transform directChild = transform.Find(childName);
        if (directChild != null) return directChild;

        return transform.parent != null ? transform.parent.Find(childName) : null;
    }

    private void RegisterCameraTargets()
    {
        if (cameraSystem == null) return;

        if (playerObject != null)
        {
            cameraSystem.SetPlayerTarget(playerObject.transform);
        }

        cameraSystem.SetPilotingTarget(transform);
    }

    private void CacheVisualComponents()
    {
        if (exteriorRoot != null)
        {
            exteriorRenderers = exteriorRoot.GetComponentsInChildren<Renderer>(true);
            exteriorCanvases = exteriorRoot.GetComponentsInChildren<Canvas>(true);
        }

        if (interiorRoot != null)
        {
            interiorRenderers = interiorRoot.GetComponentsInChildren<Renderer>(true);
            interiorCanvases = interiorRoot.GetComponentsInChildren<Canvas>(true);
        }
    }

    private void SetVisualGroupVisible(Renderer[] renderers, Canvas[] canvases, bool visible)
    {
        if (renderers != null)
        {
            foreach (var r in renderers)
            {
                if (r != null) r.enabled = visible;
            }
        }

        if (canvases != null)
        {
            foreach (var c in canvases)
            {
                if (c != null) c.enabled = visible;
            }
        }
    }

    private void ApplyVisualState()
    {
        bool showInterior = !isPiloting;
        bool showExterior = isPiloting || !hideExteriorWhenOnFoot;

        SetVisualGroupVisible(interiorRenderers, interiorCanvases, showInterior);
        SetVisualGroupVisible(exteriorRenderers, exteriorCanvases, showExterior);

        if (useCameraSwitch)
        {
            if (interiorCamera != null) interiorCamera.gameObject.SetActive(showInterior);
            if (exteriorCamera != null) exteriorCamera.gameObject.SetActive(isPiloting);
        }
    }

    private void ApplyControlState()
    {
        if (playerMoveScript != null)
        {
            playerMoveScript.enabled = !isPiloting;
        }

        if (shipMoveScript != null)
        {
            shipMoveScript.enabled = isPiloting &&
                                     (dockableShip == null || dockableShip.CanAcceptManualControl);
        }

        if (playerObject != null)
        {
            if (isPiloting)
            {
                AttachPlayerToShip();
            }

            playerObject.SetActive(!isPiloting || !hidePlayerWhenPiloting);
        }

        if (shipRigidbody != null && makeShipKinematicWhenOnFoot)
        {
            if (isPiloting)
            {
                shipRigidbody.bodyType = RigidbodyType2D.Dynamic;
            }
            else
            {
                shipRigidbody.velocity = Vector2.zero;
                shipRigidbody.angularVelocity = 0f;
                shipRigidbody.bodyType = RigidbodyType2D.Kinematic;
            }
        }
    }

    private void AttachPlayerToShip()
    {
        if (playerObject == null) return;

        if (!cachedOriginalPlayerParent)
        {
            originalPlayerParent = playerObject.transform.parent;
            cachedOriginalPlayerParent = true;
        }

        if (playerObject.transform.parent != transform)
        {
            playerObject.transform.SetParent(transform, true);
        }
    }

    private void RestorePlayerParent()
    {
        if (playerObject == null || !cachedOriginalPlayerParent) return;

        if (playerObject.transform.parent != originalPlayerParent)
        {
            playerObject.transform.SetParent(originalPlayerParent, true);
        }
    }

    public void EnterPiloting()
    {
        isPiloting = true;

        ApplyControlState();
        ApplyVisualState();

        if (cameraSystem != null)
        {
            cameraSystem.EnterPilotingView(transform);
        }
    }

    public void ExitPiloting(Transform returnPoint)
    {
        isPiloting = false;

        ApplyControlState();
        ApplyVisualState();

        if (playerObject != null)
        {
            RestorePlayerParent();

            if (returnPoint != null)
            {
                playerObject.transform.position = returnPoint.position;
            }

            if (cameraSystem != null)
            {
                cameraSystem.ExitPilotingView(playerObject.transform);
            }
        }
    }
}
