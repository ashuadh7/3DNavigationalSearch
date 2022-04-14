using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoystickLocomotion : BaseLocomotion
{

    // Internalc
    public float smoothingFactor = 0.04f;
    public float speedMagnifier = 0.013f;
    public float angularSmoothingFactor = 0.04f;
    public float angularMagnifier = 0.5f;

    // Internals
    protected Vector3 lastVelocity;
    protected float lastAngularVelocity;
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
        baseCamera = GameObject.FindObjectOfType<BaseCamera>();
        playerSystem = baseCamera.GetPlayerSystem();
        player = baseCamera.GetPlayer();
    }

    // Update during Play State
    protected override void OnPlayState()
    {
        base.OnPlayState();

        Vector2 joystickXY = new Vector2(-Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
        Vector3 forwardDirection = playerSystem.transform.forward;
        forwardDirection.y = 0f;
        float rotateAngle = -AngleInDeg(Vector2.up, joystickXY);
        joystickXY = GetXZ(RotateInDeg(forwardDirection, rotateAngle, Vector3.up).normalized) * joystickXY.magnitude;

        // Calculate speed
        float forwardSpeed = -speedMagnifier * (float)joystickXY.x;
        float sidewaySpeed = speedMagnifier * (float)joystickXY.y;

        // Update position
        Vector3 expectedVelocity = new Vector3(sidewaySpeed, 0f, forwardSpeed);
        Vector3 currentVelocity = smoothingFactor * expectedVelocity + (1f - smoothingFactor) * lastVelocity;
        playerSystem.transform.position += currentVelocity;
        // Update last velocity
        this.lastVelocity = currentVelocity;

        // Rotation
        float expectedAngularVelocity = Input.GetAxis("Joy Z") * angularMagnifier;
        float currentAngularVelocity = angularSmoothingFactor * expectedAngularVelocity + (1f - angularSmoothingFactor) * lastAngularVelocity;
        playerSystem.transform.RotateAround(player.transform.position, Vector3.up, currentAngularVelocity);
        // Update last angular velocty
        lastAngularVelocity = currentAngularVelocity;
    }

	// Return trigonometric angle between two vectors.
	protected static float AngleInDeg(Vector2 lhs, Vector2 rhs)
    {
        float res = Vector2.Angle(lhs, rhs);
        float cross = lhs.x * rhs.y - lhs.y * rhs.x;
        if (cross != 0f)
        {
            res *= Mathf.Sign(cross);
        }
        return res;
    }

	// Return new vector with X, Z components.
	protected static Vector2 GetXZ(Vector3 vector)
    {
        return new Vector2(vector.x, vector.z);
    }

	// Rotate the specified vector with givan angle around given axis.
	protected static Vector3 RotateInDeg(Vector3 vector, float angle, Vector3 axis)
    {
        return Quaternion.AngleAxis(angle, axis) * vector;
    }

}
