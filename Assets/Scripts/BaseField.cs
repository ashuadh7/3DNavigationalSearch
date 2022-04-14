using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base class of target field
public abstract class BaseField : BaseComponent {

    // Getter and setter of number of targets
    public virtual int NumberOfTargets { get; set; }

    // Number of special targets
    public virtual int NumberOfSpecialTargets { get; set; }

    // Prefab of targets
    protected Object targetPrefab = null;
    // Target-prefab property
    public Object TargetPrefab
    {
        get { return targetPrefab; }
        set
        {
            // Only allow set prefab in Initial State
            if (state == ComponentState.Initial)
            {
                targetPrefab = value;
            } 
        }
    }

    // List of targets
    protected List<BaseTarget> targetList = null;
    // Target-list property
    public List<BaseTarget> TargetList { get { return targetList; } }

    // Target height
    private float targetHeight = 0f;
    public float TargetHeight
    {
        get { return targetHeight; }
        set { targetHeight = value; }
    }

    // Start playing all targets
    public override void BeginPlayState()
    {
        base.BeginPlayState();
        foreach(BaseTarget target in targetList)
        {
            target.BeginPlayState();
        }
    }

    // Stop playing all targets
    public override void BeginFinishedState()
    {
        base.BeginFinishedState();
        foreach (BaseTarget target in targetList)
        {
            target.BeginFinishedState();
        }
    }

    // Reset height of target
    public virtual void ResetTargetHeight(float height = 0f)
    {
        foreach (BaseTarget target in targetList)
        {
            target.transform.position = new Vector3(target.transform.position.x, height, target.transform.position.z);
        }
    }

}
