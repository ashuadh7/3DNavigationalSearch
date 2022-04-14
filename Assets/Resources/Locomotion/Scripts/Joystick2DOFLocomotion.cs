using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Joystick2DOFLocomotion : BaseLocomotion
{
    /*
    public float smoothingFactor = 0.04f;
    public float speedMagnifier = 0.013f;
    */
    public float smoothingFactor = 0.04f;
    public float maxVelocity = 5f;

    // Internals
    private Vector3 lastVelocity;
    private BaseCamera baseCamera;
    private GameObject player;
    private GameObject playerSystem;

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
        baseCamera = GameObject.FindObjectOfType<BaseCamera>();
        player = baseCamera.GetPlayer();
        playerSystem = baseCamera.GetPlayerSystem();
    }

    // Update during Play State
    protected override void OnPlayState()
    {
        base.OnPlayState();

        Vector2 joystickXY = new Vector2(-Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
        Vector3 forwardDirection = player.transform.forward;
        forwardDirection.y = 0f;
        float rotateAngle = -AngleInDeg(Vector2.up, joystickXY);
        joystickXY = GetXZ(RotateInDeg(forwardDirection, rotateAngle, Vector3.up).normalized) * joystickXY.magnitude;

        if (joystickXY.magnitude == 0)
        {
            
        }
        float radiusExp = joystickXY.magnitude == 0f ? 0f : Mathf.Pow(Mathf.Max(0f, joystickXY.magnitude), 1.53f);
        float vForward = joystickXY.magnitude == 0f ? 0f : -radiusExp / joystickXY.magnitude * joystickXY.x * maxVelocity;
        float vSideway = joystickXY.magnitude == 0f ? 0f : radiusExp / joystickXY.magnitude * joystickXY.y * maxVelocity;

        Vector3 expectedVelocity = new Vector3(vSideway, 0f, vForward) * Time.deltaTime;
        Vector3 currentVelocity = smoothingFactor * expectedVelocity + (1f - smoothingFactor) * lastVelocity;
        playerSystem.transform.position += currentVelocity;
        // Update last velocity
        lastVelocity = currentVelocity;

        //playerSystem.transform.position += new Vector3(vSideway, 0f, vForward) * Time.deltaTime;

        /*
        // Calculate speed
        float forwardSpeed = -speedMagnifier * joystickXY.x;
        float sidewaySpeed = speedMagnifier * joystickXY.y;

        // Update position
        Vector3 expectedVelocity = new Vector3(sidewaySpeed, 0f, forwardSpeed);
        Vector3 currentVelocity = smoothingFactor * expectedVelocity + (1f - smoothingFactor) * lastVelocity;
        playerSystem.transform.position += currentVelocity;

        // Update last velocity
        lastVelocity = currentVelocity;
        */

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
