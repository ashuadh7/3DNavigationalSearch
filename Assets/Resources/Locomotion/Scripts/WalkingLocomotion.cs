using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingLocomotion : BaseLocomotion
{
    // Nothing to setup
    protected override void OnSetupState()
    {
        base.OnSetupState();
        BeginPendingState();
    }
}
