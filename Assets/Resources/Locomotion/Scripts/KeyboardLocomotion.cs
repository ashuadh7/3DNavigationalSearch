using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardLocomotion : BaseLocomotion
{

    // Internalc
    public float maxVelocityInMps = 1f;
    public float smoothingFactor = 0.08f;
    public float maxAngularVelocityInDegreesPerSecond = 30f;
    public float angularSmoothingFactor = 0.06f;

    // Internals
    protected Vector3 lastVelocity;
    protected float lastAngularVelocity;
    protected float lastUpDownVelocity;
    protected BaseCamera baseCamera;
    protected GameObject playerSystem;
    protected GameObject player;

    // Update during Setup State
    protected override void OnSetupState()
    {
        base.OnSetupState();
        // Nothing to setup
        BeginPendingState();
    }

    // When begin playing
    public override void BeginPlayState()
    {
        base.BeginPlayState();
        lastVelocity = Vector3.zero;
        lastAngularVelocity = 0f;
        lastUpDownVelocity = 0f;
        baseCamera = GameObject.FindObjectOfType<BaseCamera>();
        playerSystem = baseCamera.GetPlayerSystem();
        player = baseCamera.GetPlayer();
    }

    // Update during Play State
    protected override void OnPlayState()
    {
        base.OnPlayState();
        // Determine rotation
        float expectedAngularVelocity = 0f;
        if (Input.GetKey(KeyCode.A))
        {
            expectedAngularVelocity -= 1f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            expectedAngularVelocity += 1f;
        }
        expectedAngularVelocity *= maxAngularVelocityInDegreesPerSecond * Time.deltaTime;
        float currentAngularVelocity = angularSmoothingFactor * expectedAngularVelocity + (1f - angularSmoothingFactor) * lastAngularVelocity;
        playerSystem.transform.RotateAround(player.transform.position, Vector3.up, currentAngularVelocity);
        lastAngularVelocity = currentAngularVelocity;

        // Up-down rotation
        float expectedUpDownVelocity = 0f;
        if (Input.GetKey(KeyCode.W))
        {
            expectedUpDownVelocity += 1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            expectedUpDownVelocity -= 1f;
        }
        expectedUpDownVelocity *= maxAngularVelocityInDegreesPerSecond * Time.deltaTime;
        float currentUpDownVelocity = angularSmoothingFactor * expectedUpDownVelocity + (1f - angularSmoothingFactor) * lastUpDownVelocity;
        player.transform.Rotate(Vector3.left, currentUpDownVelocity, Space.Self);
        lastUpDownVelocity = currentUpDownVelocity;

        // Find direction defined by pressed keys
        Vector3 expectedVelocity = new Vector3();
        if (Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.LeftShift))
        {
            expectedVelocity += new Vector3(baseCamera.transform.forward.x, 0, baseCamera.transform.forward.z);
        }
        if (Input.GetKey(KeyCode.DownArrow) && !Input.GetKey(KeyCode.LeftShift))
        {
            expectedVelocity -= new Vector3(baseCamera.transform.forward.x, 0, baseCamera.transform.forward.z);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            expectedVelocity += new Vector3(baseCamera.transform.right.x, 0, baseCamera.transform.right.z);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            expectedVelocity -= new Vector3(baseCamera.transform.right.x, 0, baseCamera.transform.right.z);
        }
        if (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.LeftShift))
        {
            expectedVelocity += Vector3.up;
        }
        if (Input.GetKey(KeyCode.DownArrow) && Input.GetKey(KeyCode.LeftShift))
        {
            expectedVelocity -= Vector3.up;
        }
        expectedVelocity = expectedVelocity.normalized * this.maxVelocityInMps * Time.deltaTime;

        Vector3 currentVelocity = this.smoothingFactor * expectedVelocity + (1f - smoothingFactor) * lastVelocity;
        this.lastVelocity = currentVelocity;
        playerSystem.transform.localPosition += currentVelocity;
    }

}
