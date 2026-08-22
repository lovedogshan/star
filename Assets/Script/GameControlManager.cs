using UnityEngine;


public class GameControlManager : MonoBehaviour
{
    public static GameControlManager Instance;
    public ControlMode CurrentMode { get; private set; } = ControlMode.OnFoot;

    [Header("全局引用")]
    public GameObject player;
    public ShipMove currentShip;
    public ShipVisualController currentShipVisualController;
    
    public enum ControlMode
    {
        OnFoot,
        Piloting
    }
    void Awake()
    {
        Instance = this;
    }

    public void EnterPiloting(ShipMove ship, ShipVisualController visualController = null)
    {
        CurrentMode = ControlMode.Piloting;
        currentShip = ship;
        currentShipVisualController = visualController;

        if (currentShipVisualController != null)
        {
            currentShipVisualController.EnterPiloting();
        }
        else
        {
            if (player != null) player.SetActive(false);
            if (currentShip != null) currentShip.enabled = true;
        }
    }

    public void ExitPiloting(Transform standPoint)
    {
        CurrentMode = ControlMode.OnFoot;

        if (currentShipVisualController != null)
        {
            currentShipVisualController.ExitPiloting(standPoint);
        }
        else
        {
            if (currentShip != null)
                currentShip.enabled = false;

            if (player != null)
            {
                if (standPoint != null)
                    player.transform.position = standPoint.position;
                player.SetActive(true);
            }
        }

        currentShip = null;
        currentShipVisualController = null;
    }
}
