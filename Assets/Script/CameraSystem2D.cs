using UnityEngine;
using Cinemachine;

namespace CodeMonkey.CameraSystem
{
    public class CameraSystem2D : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;
        [SerializeField] private bool useEdgeScrolling = false;
        [SerializeField] private bool useDragPan = false;
        [SerializeField] private float orthographicSizeMin = 10f;
        [SerializeField] private float orthographicSizeMax = 50f;

        [Header("Control Mode Follow")]
        [SerializeField] private bool followControlMode = true;
        [SerializeField] private Transform playerTarget;
        [SerializeField] private Transform pilotingTarget;
        [SerializeField] private float onFootOrthographicSize = 18f;
        [SerializeField] private float pilotingOrthographicSize = 50f;
        [SerializeField] private float commandOrthographicSize = 28f;
        [SerializeField] private float zoomSpeed = 10f;
        [SerializeField] private float zoomStep = 5f;

        private bool dragPanMoveActive;
        private Vector2 lastMousePosition;
        private float targetOrthographicSize = 50f;
        private Transform defaultTarget;
        private Transform activeTarget;
        private ShipVisualController visualController;
        private bool lastPilotingState;
        private bool commandViewActive;

        private void Awake()
        {
            if (cinemachineVirtualCamera == null)
            {
                cinemachineVirtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
            }

            if (cinemachineVirtualCamera != null)
            {
                defaultTarget = cinemachineVirtualCamera.Follow;
                activeTarget = cinemachineVirtualCamera.Follow;
                targetOrthographicSize = cinemachineVirtualCamera.m_Lens.OrthographicSize;
            }

            playerTarget = ResolvePlayerTarget();
            pilotingTarget = ResolvePilotingTarget();
            visualController = ResolveShipVisualController();

            bool isPiloting = IsPiloting();
            lastPilotingState = isPiloting;
            ApplyControlModeCamera(isPiloting, true);
        }

        private void Update()
        {
            if (followControlMode)
            {
                UpdateControlModeCamera();
            }

            if (useEdgeScrolling)
            {
                HandleCameraMovementEdgeScrolling();
            }

            if (useDragPan)
            {
                HandleCameraMovementDragPan();
            }

            HandleCameraZoomOrthographicSize();
        }

        private void HandleCameraMovementEdgeScrolling()
        {
            Vector3 inputDir = Vector3.zero;

            int edgeScrollSize = 20;

            if (Input.mousePosition.x < edgeScrollSize)
            {
                inputDir.x = -1f;
            }
            if (Input.mousePosition.y < edgeScrollSize)
            {
                inputDir.z = -1f;
            }
            if (Input.mousePosition.x > Screen.width - edgeScrollSize)
            {
                inputDir.x = 1f;
            }
            if (Input.mousePosition.y > Screen.height - edgeScrollSize)
            {
                inputDir.z = 1f;
            }

            Vector3 moveDir = transform.forward * inputDir.z + transform.right * inputDir.x;

            float moveSpeed = 50f;
            transform.position += moveDir * moveSpeed * Time.deltaTime;
        }

        private void HandleCameraMovementDragPan()
        {
            Vector3 inputDir = Vector3.zero;

            if (Input.GetMouseButtonDown(1))
            {
                dragPanMoveActive = true;
                lastMousePosition = Input.mousePosition;
            }
            if (Input.GetMouseButtonUp(1))
            {
                dragPanMoveActive = false;
            }

            if (dragPanMoveActive)
            {
                Vector2 mouseMovementDelta = (Vector2)Input.mousePosition - lastMousePosition;

                float dragPanSpeed = 1f;
                inputDir.x = mouseMovementDelta.x * dragPanSpeed;
                inputDir.z = mouseMovementDelta.y * dragPanSpeed;

                lastMousePosition = Input.mousePosition;
            }

            Vector3 moveDir = transform.forward * inputDir.z + transform.right * inputDir.x;

            float moveSpeed = 50f;
            transform.position += moveDir * moveSpeed * Time.deltaTime;
        }

        private void HandleCameraZoomOrthographicSize()
        {
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                LerpCameraZoom();
                return;
            }

            if (Input.mouseScrollDelta.y > 0)
            {
                targetOrthographicSize -= zoomStep;
            }
            if (Input.mouseScrollDelta.y < 0)
            {
                targetOrthographicSize += zoomStep;
            }

            targetOrthographicSize = Mathf.Clamp(targetOrthographicSize, orthographicSizeMin, orthographicSizeMax);

            LerpCameraZoom();
        }

        private void LerpCameraZoom()
        {
            if (cinemachineVirtualCamera == null) return;

            cinemachineVirtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(
                cinemachineVirtualCamera.m_Lens.OrthographicSize,
                targetOrthographicSize,
                Time.deltaTime * zoomSpeed
            );
        }

        private void UpdateControlModeCamera()
        {
            bool isPiloting = IsPiloting();

            if (isPiloting != lastPilotingState)
            {
                ApplyControlModeCamera(isPiloting, false);
                lastPilotingState = isPiloting;
                return;
            }

            Transform desiredTarget = isPiloting ? ResolvePilotingTarget() : ResolvePlayerTarget();
            if (desiredTarget != null && desiredTarget != activeTarget)
            {
                ApplyFollowTarget(desiredTarget);
            }
        }

        private void ApplyControlModeCamera(bool isPiloting, bool instant)
        {
            Transform desiredTarget = isPiloting ? ResolvePilotingTarget() : ResolvePlayerTarget();
            ApplyFollowTarget(desiredTarget);

            float desiredSize = isPiloting
                ? pilotingOrthographicSize
                : commandViewActive ? commandOrthographicSize : onFootOrthographicSize;
            targetOrthographicSize = Mathf.Clamp(desiredSize, orthographicSizeMin, orthographicSizeMax);

            if (instant && cinemachineVirtualCamera != null)
            {
                cinemachineVirtualCamera.m_Lens.OrthographicSize = targetOrthographicSize;
            }
        }

        private void ApplyFollowTarget(Transform target)
        {
            if (target == null || cinemachineVirtualCamera == null) return;

            activeTarget = target;
            cinemachineVirtualCamera.Follow = target;
            cinemachineVirtualCamera.LookAt = target;
        }

        private bool IsPiloting()
        {
            if (visualController == null)
            {
                visualController = ResolveShipVisualController();
            }

            if (GameControlManager.Instance != null)
            {
                return GameControlManager.Instance.CurrentMode == GameControlManager.ControlMode.Piloting ||
                       (visualController != null && visualController.isPiloting);
            }

            return visualController != null && visualController.isPiloting;
        }

        private ShipVisualController ResolveShipVisualController()
        {
            if (visualController != null) return visualController;

            if (GameControlManager.Instance != null &&
                GameControlManager.Instance.currentShipVisualController != null)
            {
                visualController = GameControlManager.Instance.currentShipVisualController;
                return visualController;
            }

            visualController = FindObjectOfType<ShipVisualController>();
            return visualController;
        }

        private Transform ResolvePlayerTarget()
        {
            if (playerTarget != null) return playerTarget;

            if (GameControlManager.Instance != null && GameControlManager.Instance.player != null)
            {
                playerTarget = GameControlManager.Instance.player.transform;
                return playerTarget;
            }

            Move playerMove = FindObjectOfType<Move>();
            if (playerMove != null)
            {
                playerTarget = playerMove.transform;
                return playerTarget;
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
            }

            return playerTarget;
        }

        private Transform ResolvePilotingTarget()
        {
            if (pilotingTarget != null) return pilotingTarget;

            if (GameControlManager.Instance != null)
            {
                if (GameControlManager.Instance.currentShip != null)
                {
                    pilotingTarget = GameControlManager.Instance.currentShip.transform;
                    return pilotingTarget;
                }

                if (GameControlManager.Instance.currentShipVisualController != null)
                {
                    pilotingTarget = GameControlManager.Instance.currentShipVisualController.transform;
                    return pilotingTarget;
                }
            }

            ShipMove shipMove = FindObjectOfType<ShipMove>();
            if (shipMove != null)
            {
                pilotingTarget = shipMove.transform;
                return pilotingTarget;
            }

            pilotingTarget = defaultTarget;
            return pilotingTarget;
        }

        public void SetPlayerTarget(Transform target)
        {
            playerTarget = target;
            if (followControlMode && !IsPiloting())
            {
                ApplyControlModeCamera(false, false);
            }
        }

        public void SetPilotingTarget(Transform target)
        {
            pilotingTarget = target;
            if (followControlMode && IsPiloting())
            {
                ApplyControlModeCamera(true, false);
            }
        }

        public void EnterPilotingView(Transform target)
        {
            if (target != null)
            {
                pilotingTarget = target;
            }

            lastPilotingState = true;
            ApplyControlModeCamera(true, false);
        }

        public void ExitPilotingView(Transform target)
        {
            if (target != null)
            {
                playerTarget = target;
            }

            lastPilotingState = false;
            ApplyControlModeCamera(false, false);
        }

        public void SetCommandView(bool active)
        {
            commandViewActive = active && !IsPiloting();
            ApplyControlModeCamera(IsPiloting(), false);
        }
    }
}
