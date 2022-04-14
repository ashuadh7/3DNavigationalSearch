using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandingBoxTarget : BaseTarget
{
    // Target's components
    protected GameObject box;
    protected GameObject cover;
    protected GameObject core;
    protected Material boxMaterial;

    // Assign target's components in setup state
    protected override void OnSetupState()
    {
        base.OnSetupState();
        // Find the box
        box = this.transform.Find("Box").gameObject;
        boxMaterial = box.GetComponent<Renderer>().material;
        // Find the inner cover
        cover = this.transform.Find("Cover").gameObject;
        // Find the core
        core = this.transform.Find("Ball").gameObject;

        // Show front side by default
        ToggleFrontSideAppearance(true);
        // Hide ball by default
        ToggleCoreAppearance(false);

        // Setup done
        BeginPendingState();
    }

    // Get position of the core
    public override Vector3 GetDefaultCorePosition()
    {
        return core.transform.position;
    }

    // Called when player approaches target
    public override void OnBeingApproached(GameObject player)
    {
        base.OnBeingApproached(player);
        // Hide front side
        ToggleFrontSideAppearance(false);
        // Show core if there is a core inside target (target is not empty)
        ToggleCoreAppearance(true);
    }

    // Called when player leaves target
    public override void OnBeingLeft(GameObject player)
    {
        base.OnBeingLeft(player);
        // Show front side again
        ToggleFrontSideAppearance(true);
        // Hide the core
        ToggleCoreAppearance(false);
    }

    // Called when player tries to collect target's core
    public override void OnBeingCollected(GameObject player)
    {
        base.OnBeingCollected(player);
        // Hide the core
        ToggleCoreAppearance(false);
    }

    // True to show front side, false to hide it
    protected void ToggleFrontSideAppearance(bool status)
    {
        boxMaterial.SetFloat("_Enable", status ? 0 : 1);
        cover.SetActive(status);
    }

    // True to show the core, false to hide it
    protected void ToggleCoreAppearance(bool status)
    {
        core.SetActive(status && !IsEmpty);
    }
}
