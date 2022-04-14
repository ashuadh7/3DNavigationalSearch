using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceBarCollector : BaseCollector
{

    // Update during Setup State
    protected override void OnSetupState()
    {
        base.OnSetupState();
        // Nothing to setup
        BeginPendingState();
    }

    // Use to check if player is ready to collect a core (if it exists)
    public override bool IsReadyToCollect(BaseTarget target)
    {
        return base.IsReadyToCollect(target) && Input.GetKeyDown(KeyCode.Space);
    }

}
