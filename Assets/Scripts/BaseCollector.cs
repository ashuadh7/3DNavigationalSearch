using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCollector : BaseComponent
{

    // Phases of target during Play State
    public enum Phase
    {
        Normal,
        Effective
    }

    // Current phase
    protected Phase phase;

    // ---------------------------------- Events -----------------------------------

    // This function must be called when player approaches the target (enter the effective field of the target)
    // Player should do some animation (e.g sound playing)
    // Usually NEED to OVERRIDE
    public virtual void OnApproaching(BaseTarget target)
    {
        phase = Phase.Effective;
    }

    // This function must be called when player leaves the target (exit the effective field of the target)
    // Player should stop some animation (e.g sound stopping)
    // Usually NEED to OVERRIDE
    public virtual void OnLeaving(BaseTarget target)
    {
        phase = Phase.Normal;
    }

    // This function must be called when player collects a core of a target
    // Player should do some animation
    // Usually NEED to OVERRIDE
    public virtual void OnCollecting(BaseTarget target) { }

    // Use to check if player is ready to collect a core (if it exists)
    public virtual bool IsReadyToCollect(BaseTarget target)
    {
        // Must be in effective phase
        return phase == Phase.Effective;
    }

    // This function must be called during the period the player visits the target EXCEPT collecting moment
    // Usually NO need to override
    public virtual void OnVisiting(BaseTarget target) { }

    // ---------------------- Phases ------------------------------

    // This function is automatically called during normal periods of the target
    // Target may show some animationa here
    protected virtual void OnNormalPhase() { }

    // This function is automatically called during the time player visits the target (effective)
    // Target may show some animationa here
    protected virtual void OnEffectivePhase() { }

    // When play state begins
    public override void BeginPlayState()
    {
        base.BeginPlayState();
        phase = Phase.Normal;
    }

    // Update during Play State
    protected override void OnPlayState()
    {
        base.OnPlayState();
        switch (phase)
        {
            case Phase.Normal:
                OnNormalPhase();
                break;
            case Phase.Effective:
                OnEffectivePhase();
                break;
            default:
                break;
        }
    }
    
}
