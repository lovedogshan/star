using UnityEngine;

public class CargoCrate2D : MonoBehaviour
{
    public string cargoId;
    public int quantity = 1;

    public StorageZone2D CurrentStorage { get; private set; }

    public void StoreIn(StorageZone2D storage, Vector3 position)
    {
        CurrentStorage = storage;
        transform.position = position;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    public void RemoveFromStorage(StorageZone2D storage)
    {
        if (CurrentStorage == storage)
        {
            CurrentStorage = null;
        }
    }
}
