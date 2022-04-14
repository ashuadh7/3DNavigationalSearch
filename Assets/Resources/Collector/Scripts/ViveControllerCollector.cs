using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveControllerCollector : BaseCollector
{
    // Maximum required distance to target's core
    public float requiredDistanceToCore = 0.13f;

    // All controllers
    private ViveController[] controllers;

    // Show notification
    public override void BeginSetupState()
    {
        base.BeginSetupState();
        if (ViveController.GetTotal() == 0)
        {
            BaseNotifier.ShowInAnyNotifier("Turn on controller");
        }
    }

    // Check if at least one Vive Controller exists
    protected override void OnSetupState()
    {
        base.OnSetupState();
        if (ViveController.GetTotal() > 0)
        {
            BaseNotifier.ShowInAnyNotifier("");
            BeginPendingState();
        }
    }

    // When Play state is started, assign active controller
    public override void BeginPlayState()
    {
        base.BeginPlayState();
        controllers = GameObject.FindObjectsOfType<ViveController>();
    }

    // Ready controller
    private int readyControllerIndex = -1;

    // Use to check if player is ready to collect a core (if it exists)
    public override bool IsReadyToCollect(BaseTarget target)
    {
        readyControllerIndex = -1;
        if (!base.IsReadyToCollect(target))
        {
            return false;
        }
        // One of the controllers touching target's core
        foreach (ViveController controller in controllers)
        {
            if (Vector3.Distance(controller.transform.position, target.GetDefaultCorePosition()) <= requiredDistanceToCore)
            {
                readyControllerIndex = (int)controller.controllerIndex;
                return true;
            }
        }
        return false;
    }

    // Haptic feedback
    public override void OnCollecting(BaseTarget target)
    {
        base.OnCollecting(target);
        Debug.Log(readyControllerIndex);
        if (readyControllerIndex != -1)
        {
            SteamVR_Controller.Device device = SteamVR_Controller.Input(readyControllerIndex);
            StartCoroutine(Vibrate(0.1f, device));
        }
    }

    // Vibrate
    private IEnumerator Vibrate(float duration, SteamVR_Controller.Device device)
    {
        float currentDuration = 0;
        while (currentDuration < duration)
        {
            device.TriggerHapticPulse(4000);
            currentDuration += Time.deltaTime;
            yield return null;
        }
    }
}
