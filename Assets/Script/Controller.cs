using UnityEngine;

public class Controller : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform playerTarget;
    public Transform shipTarget;

    private Transform target;
    private Vector3 velocity = Vector3.zero;

    [Range(0, 1)]
    public float smoothTime;
    public Vector3 positionOffset;

    private void Awake()
    {
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTarget = player.transform;
        }
    }

    private void LateUpdate()
    {
        UpdateTarget();
        if (target == null) return;

        Vector3 targetposition = target.position + positionOffset;
        transform.position = Vector3.SmoothDamp(transform.position, targetposition, ref velocity, smoothTime);
    }

    private void UpdateTarget()
    {
        if (GameControlManager.Instance != null &&
            GameControlManager.Instance.CurrentMode == GameControlManager.ControlMode.Piloting)
        {
            if (shipTarget != null)
            {
                target = shipTarget;
                return;
            }
        }

        target = playerTarget != null ? playerTarget : shipTarget;
    }
}
