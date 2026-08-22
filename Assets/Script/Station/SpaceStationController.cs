using UnityEngine;

public class SpaceStationController : MonoBehaviour
{
    [SerializeField] private DockingZone2D dockingZone;
    [SerializeField] private StorageZone2D storageZone;

    public DockingZone2D DockingZone => dockingZone;
    public StorageZone2D StorageZone => storageZone;

    private void Awake()
    {
        if (dockingZone == null)
        {
            dockingZone = GetComponentInChildren<DockingZone2D>();
        }

        if (storageZone == null)
        {
            storageZone = GetComponentInChildren<StorageZone2D>();
        }
    }

    public void ToggleLandingTakeoff()
    {
        if (dockingZone != null)
        {
            dockingZone.ToggleLandingTakeoff();
        }
    }
}
