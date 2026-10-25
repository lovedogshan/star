using UnityEngine;

public class EvaCapability : MonoBehaviour
{
    [SerializeField]
    private bool hasSuit;
    [SerializeField]
    private bool hasOxygen;
    [SerializeField]
    private bool hasThruster;
    [SerializeField]
    private bool hasTether;
    [SerializeField]
    private float oxygenRemaining = 300f;
    [SerializeField]
    private float returnSafetySeconds = 30f;

    public bool HasSuit => hasSuit;
    public bool HasOxygen => hasOxygen && oxygenRemaining > 0f;
    public bool HasThruster => hasThruster;
    public bool HasTether => hasTether;
    public float OxygenRemaining => oxygenRemaining;
    public bool CanEva => HasSuit && HasOxygen && HasThruster;

    public bool HasOxygenBudget(float estimatedTravelSeconds, float workSeconds)
    {
        return CanEva && oxygenRemaining >= estimatedTravelSeconds + workSeconds + returnSafetySeconds;
    }

    public void ConsumeOxygen(float seconds)
    {
        if (seconds <= 0f)
        {
            return;
        }

        oxygenRemaining = Mathf.Max(0f, oxygenRemaining - seconds);
    }
}
