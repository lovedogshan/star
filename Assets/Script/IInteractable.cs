using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    string GetPrompt();
    void Interact(PlayerInteractor interactor);
}

public static class InteractionInputGuard
{
    private static int consumedFrame = -1;

    public static void ConsumeCurrentFrame()
    {
        consumedFrame = UnityEngine.Time.frameCount;
    }

    public static bool WasConsumedThisFrame()
    {
        return consumedFrame == UnityEngine.Time.frameCount;
    }
}
