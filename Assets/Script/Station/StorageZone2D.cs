using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class StorageZone2D : MonoBehaviour
{
    [SerializeField] private Vector2 cellSize = new Vector2(1.2f, 1.2f);
    [SerializeField] private int columns = 6;
    [SerializeField] private int rows = 4;

    private readonly List<CargoCrate2D> storedCargo = new List<CargoCrate2D>();

    public IReadOnlyList<CargoCrate2D> StoredCargo => storedCargo;
    public int Capacity => Mathf.Max(0, columns * rows);
    public bool HasSpace => storedCargo.Count < Capacity;

    private void Reset()
    {
        Collider2D zoneCollider = GetComponent<Collider2D>();
        zoneCollider.isTrigger = true;
    }

    private void Awake()
    {
        Collider2D zoneCollider = GetComponent<Collider2D>();
        zoneCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CargoCrate2D cargo = other.GetComponent<CargoCrate2D>();
        if (cargo == null)
        {
            cargo = other.GetComponentInParent<CargoCrate2D>();
        }

        if (cargo == null || storedCargo.Contains(cargo) || !HasSpace) return;

        storedCargo.Add(cargo);
        cargo.StoreIn(this, GetSlotPosition(storedCargo.Count - 1));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        CargoCrate2D cargo = other.GetComponent<CargoCrate2D>();
        if (cargo == null)
        {
            cargo = other.GetComponentInParent<CargoCrate2D>();
        }

        if (cargo == null || !storedCargo.Remove(cargo)) return;

        cargo.RemoveFromStorage(this);
        ReflowCargo();
    }

    private void ReflowCargo()
    {
        for (int i = 0; i < storedCargo.Count; i++)
        {
            if (storedCargo[i] != null)
            {
                storedCargo[i].StoreIn(this, GetSlotPosition(i));
            }
        }
    }

    private Vector3 GetSlotPosition(int index)
    {
        int column = index % Mathf.Max(1, columns);
        int row = index / Mathf.Max(1, columns);

        Vector2 origin = new Vector2(
            -(columns - 1) * cellSize.x * 0.5f,
            (rows - 1) * cellSize.y * 0.5f
        );

        Vector2 localPosition = origin + new Vector2(column * cellSize.x, -row * cellSize.y);
        return transform.TransformPoint(localPosition);
    }
}
