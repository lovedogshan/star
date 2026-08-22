using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlayerInteractor : MonoBehaviour
{
    public float interactRadius = 1.2f;
    public LayerMask interactLayer;

    private IInteractable currentInteractable;

    private void Awake()
    {
        if (interactLayer.value == 0)
        {
            interactLayer = Physics2D.AllLayers;
        }
    }

    void Update()
    {
        FindInteractable();

        if (currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable.Interact(this);
        }
    }

    void FindInteractable()
    {
        currentInteractable = FindNearestInteractable();
    }

    private IInteractable FindNearestInteractable()
    {
        IInteractable nearestInteractable = null;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRadius, interactLayer);

        float nearestDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();
            if (interactable == null)
            {
                interactable = hit.GetComponentInParent<IInteractable>();
            }

            if (interactable == null) continue;

            float dis = Vector2.Distance(transform.position, hit.transform.position);
            if (dis < nearestDistance)
            {
                nearestDistance = dis;
                nearestInteractable = interactable;
            }
        }

        return nearestInteractable;
    }

    public IInteractable GetCurrentInteractable()
    {
        return currentInteractable;
    }

    public bool HasNearbyInteractable()
    {
        return FindNearestInteractable() != null;
    }
}
