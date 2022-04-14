using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CubeTarget : BaseTarget
{
    // Components
    private GameObject front;
    private GameObject core;
    private GameObject pedestal;

    protected override void OnSetupState()
    {
        base.OnSetupState();
        // Get components
        front = this.transform.Find("Front").gameObject;
        core = this.transform.Find("Ball").gameObject;

        // Hide core
        core.SetActive(false);

        // Done
        BeginPendingState();
    }

    public override void BeginPlayState()
    {
        base.BeginPlayState();
        // Load pedestal from prefab
        // Object pedestalPrefab = Resources.Load(Path.Combine("Target", Path.Combine("TwistedPillar", "Pillar-1m")));
        // pedestal = Instantiate(pedestalPrefab) as GameObject;
        // pedestal.transform.parent = this.transform;
        // pedestal.name = "Pedestal";
        // pedestal.transform.position = new Vector3(this.transform.position.x, 0f, this.transform.position.z);
        // pedestal.transform.localScale = new Vector3(1f, this.transform.position.y - 0.1f, 1f);

    }

    public override void OnBeingApproached(GameObject player)
    {
        base.OnBeingApproached(player);
        // Hide front
        front.SetActive(false);
        if (!IsEmpty)
        {
            // Show core
            core.SetActive(true);
        }
    }

    public override void OnBeingLeft(GameObject player)
    {
        base.OnBeingLeft(player);
        // Show front
        front.SetActive(true);
        // Hide core
        core.SetActive(false);
    }

    public override void OnBeingCollected(GameObject player)
    {
        base.OnBeingCollected(player);
        // Hide core
        core.SetActive(false);
    }

    public override Vector3 GetDefaultCorePosition()
    {
        return core.transform.position;
    }
}
