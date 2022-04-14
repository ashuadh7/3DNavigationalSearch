using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WiiBoardLocomotion : BaseLocomotion {

    // Publics
    public float SmoothingFactor = 0.04f;
    public float speedMagnifier = 0.013f;

    // Internals
    protected Vector3 lastVelocity;
    protected BaseCamera baseCamera;
    protected GameObject playArea;

    // When begin playing
    public override void BeginPlayState()
    {
        base.BeginPlayState();
        lastVelocity = Vector3.zero;
        baseCamera = GameObject.FindObjectOfType<BaseCamera>();
        playArea = baseCamera.GetPlayArea();
    }

    // Update during Play state
    protected override void OnPlayState()
    {
        base.OnPlayState();

        // Get Wii information
        Vector2 wiiXY;
        wiiXY = Wii.GetCenterOfBalance(0);
        //print("Wii Balance Board Data: X = " + wiiXY.x + " - Y = " + wiiXY.y);
        wiiXY.x *= 46.0f / 27.0f;

        // Calculate speed
        float forwardSpeed = speedMagnifier * wiiXY.y;
        float sidewaySpeed = speedMagnifier * wiiXY.x;

        // Update position
        Vector3 expectedVelocity = new Vector3(sidewaySpeed, 0f, forwardSpeed);
        Vector3 currentVelocity = SmoothingFactor * expectedVelocity + (1f - SmoothingFactor) * lastVelocity;
        lastVelocity = currentVelocity;
        playArea.transform.position += currentVelocity;
    }

}
