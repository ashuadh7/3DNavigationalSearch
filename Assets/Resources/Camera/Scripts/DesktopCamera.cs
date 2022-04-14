using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesktopCamera : BaseCamera
{

    // Update during Setup State
    protected override void OnSetupState()
    {
        base.OnSetupState();
        // Nothing to setup
        BeginPendingState();
    }

    // Update during Play State
    protected override void OnPlayState()
    {
        base.OnPlayState();
    }

    // Get player (usually the camera main if 1st person camera, otherwise, another character if 3rd person camera)
    public override GameObject GetPlayer()
    {
        return this.gameObject.GetComponentInChildren<Camera>().gameObject;
    }

    // Get play area (the virtual frame travelling with player)
    public override GameObject GetPlayArea()
    {
        return this.transform.Find("Play Area").gameObject;
    }

    // This object
    public override GameObject GetPlayerSystem()
    {
        return this.gameObject;
    }
}
