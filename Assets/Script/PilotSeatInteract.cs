using UnityEngine;

public class PilotSeatInteract : MonoBehaviour, IInteractable
{
    public ShipVisualController shipVisualController;
    public Transform returnPoint;
    public KeyCode enterPilotKey = KeyCode.E;
    public KeyCode exitPilotKey = KeyCode.Q;
    public Vector2 autoTriggerSize = new Vector2(3.85f, 2.67f);

    private void Awake()
    {
        EnsureTriggerCollider();
    }

    private void EnsureTriggerCollider()
    {
        Collider2D existingCollider = GetComponent<Collider2D>();
        if (existingCollider != null)
        {
            existingCollider.isTrigger = true;
            return;
        }

        BoxCollider2D triggerCollider = gameObject.AddComponent<BoxCollider2D>();
        triggerCollider.isTrigger = true;
        triggerCollider.size = autoTriggerSize;
    }

    private void Update()
    {
        if (shipVisualController != null && shipVisualController.isPiloting && Input.GetKeyDown(exitPilotKey))
        {
            shipVisualController.ExitPiloting(returnPoint);
        }
    }

    public string GetPrompt()
    {
        return "按 E 驾驶飞船";
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (shipVisualController != null)
        {
            InteractionInputGuard.ConsumeCurrentFrame();
            shipVisualController.EnterPiloting();
        }
    }

}
