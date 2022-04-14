using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseTarget : BaseComponent 
{

    // Phases of target during Play State
    public enum Phase
    {
        Normal,
        Effective
    }

    // Current phase
    protected Phase phase;

    // ------------------------- Information ----------------------------

    // Index of target in the field
    private int index = -1;
    //Index property
    public int Index
    {
        get { return index; }
        // Setter is only allowed during Initial state (before Setup state begins)
        set
        {
            if (state == ComponentState.Initial)
            {
                index = value;
            }
        }
    }

    // Set to special target (having core by default)
    private bool isSpecial = false;
    // Speciality property
    public bool IsSpecial
    {
        get { return isSpecial; }
        // Setter is only allowed during Initial state (before Setup state begins)
        set
        {
            if (state == ComponentState.Initial)
            {
                isSpecial = value;
                isEmpty = !isSpecial;
            }
        }
    }

    // Check if the core has been collected or not
    private bool isEmpty = true;
    // Empty-checking property
    public bool IsEmpty { get { return isEmpty; } }

    // Get the default core's position even if target is not special
    // MUST BE OVERRIDEN
    public abstract Vector3 GetDefaultCorePosition();

    // ------------------------ Player-Target events ---------------------------

    // This function must be called when player approaches the target (enter the effective field of the target)
    // Target should do something to show its core
    // Usually NEED to OVERRIDE
    public virtual void OnBeingApproached(GameObject player)
    {
        phase = Phase.Effective;
    }

    // This function must be called when player leaves the target (exit the effective field of the target)
    // Target should do something to hide its core
    // Usually NEED to OVERRIDE
    public virtual void OnBeingLeft(GameObject player)
    {
        phase = Phase.Normal;
    }

    // This function must be called when the core of the target is collected (if the core exists)
    // Target should hide the core permenantly
    // Usually NEED to OVERRIDE
    public virtual void OnBeingCollected(GameObject player)
    {
        isEmpty = true;
    }

    // This function must be called during the period the player visits the target EXCEPT collecting moment
    // Usually NO need to override
    public virtual void OnBeingVisited(GameObject player) { }

    // This function must be called to check if this target does have a core
    // and that core is ready to be collected (1st condition: in effective phase, 2nd condition: having a core)
    // Usually NO need to override unless additional conditions definded
    public virtual bool IsReadyToBeCollected(GameObject player)
    {
        return phase == Phase.Effective && !isEmpty;
    }

    // ------------------------------- Phases ---------------------------------------

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
