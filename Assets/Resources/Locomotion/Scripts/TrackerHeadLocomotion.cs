using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackerHeadLocomotion : BaseLocomotion
{

    /******************************************************************************************************************************/
    /**********             Speed Parameters: You can change them to move faster/slower toward each direction           ***********/
    /******************************************************************************************************************************/
    public float speedLimit = 10;
    public float speedSensitivity = 5;
    [Range(0, 0.5f)]
    public float deadZoneRadius = 0.1f;

    /******************************************************************************************************************************/
    /**** 								Internal Variables: Don't change these variables										***/
    /******************************************************************************************************************************/
    float locomotionDirection = 0;
    Vector3 euler;
    Quaternion quat;
    bool interfaceIsReady = false, viveControllerTriggerStatus = false;
    bool handBrakeActivated = false, viveControllerPadStatus = false;
    float handBrakeDecceleratrion = 3;
    Vector3 headZero, headCurrent;
    float headWidth = .09f, headHeight = .07f;
    float exponentialTransferFuntionPower = 1.53f;
    int initializeStep = 0;

    // Game Manager
    private GameManager gameManager;
    // Usually main camera
    private GameObject player;
    // Player system
    private GameObject playerSystem;
    // Trcaker
    private ViveTracker tracker;


    //0 = before printing PressSpace message, 1 = after PressSpace message waiting for space, 2 = after space press and when the user can fly

    public override void BeginSetupState()
    {
        base.BeginSetupState();
        gameManager = GameObject.FindObjectOfType<GameManager>();
        // Get player and player system
        BaseCamera baseCamera = GameObject.FindObjectOfType<BaseCamera>();
        player = baseCamera.GetPlayer();
        playerSystem = baseCamera.GetPlayerSystem();
    }

    protected override void OnSetupState()
    {
        base.OnSetupState();

        // To optimize performance, only find tracker when it is not found before.
        if (tracker == null)
        {
            tracker = GameObject.FindObjectOfType<ViveTracker>();
        }

        // Show notification if tracker is turned off
        if (tracker == null)
        {
            BaseNotifier.ShowInAnyNotifier("Turn on tracker");
            return;
        }

        // Show instruction
        if (gameManager.isStanding)
        {
            BaseNotifier.ShowInAnyNotifier("Stand straight");
        }
        else
        {
            BaseNotifier.ShowInAnyNotifier("Sit straight");
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Read the Vive Controller data to calculate the neck position
            // TED: Use global position and rotation instead
            float headYaw = player.transform.rotation.y;
            float headXo = player.transform.position.x - headWidth * Mathf.Sin(headYaw * Mathf.PI / 180); //Calculate the Neck x Position
            float headYo = player.transform.position.y + headHeight * Mathf.Sin(player.transform.rotation.eulerAngles.x * Mathf.PI / 180); //Calculate the Neck y Position;
            float headZo = player.transform.position.z - headWidth * Mathf.Cos(headYaw * Mathf.PI / 180); //Calculate the Neck y Position
            // TED: Compute relative position of head in the local coordinate system of tracker
            headZero = new Vector3(headXo, headYo, headZo);
            // Compute local headZero in tracker's coordinate
            headZero -= tracker.transform.position;
            headZero = Quaternion.AngleAxis(tracker.transform.eulerAngles.y + 180f, Vector3.up) * headZero;

            BaseNotifier.ShowInAnyNotifier("");
            BeginPendingState();
        }
    }

    protected override void OnPlayState()
    {
        base.OnPlayState();

        // ***************************  Caqlculate the forward locomotion  *******************************************

        // TED: Use global position and rotation instead
        float headYaw = player.transform.rotation.y;
        float headY = player.transform.position.y + headHeight * Mathf.Sin(player.transform.rotation.eulerAngles.x * Mathf.PI / 180); //Calculate the Neck y Position
        float headX = player.transform.position.x - headWidth * Mathf.Sin(headYaw * Mathf.PI / 180); //Calculate the Neck x Position
        float headZ = player.transform.position.z - headWidth * Mathf.Cos(headYaw * Mathf.PI / 180); //Calculate the Neck y Position
        headCurrent = new Vector3(headX, headY, headZ);
        // TED: Compute new world coordinate of headZero
        Vector3 worldHeadZero = Quaternion.AngleAxis(-tracker.transform.eulerAngles.y - 180f, Vector3.up) * headZero;
        worldHeadZero += tracker.transform.position;

        // **************************** Calculate polar coordinates of head ***********************************************

        // TED: Use Vector3 delta instead of deltaX, deltaY, deltaZ
        Vector3 delta = (headCurrent - worldHeadZero) * speedSensitivity;
        float radius = Vector3.Distance(headCurrent, worldHeadZero) * speedSensitivity; // polar r (radious)
        float Fi = (radius == 0) ? 0 : Mathf.Asin((float)(delta.y / radius)); // Fi in radian
        float Tetta = (delta.x == 0 && delta.z == 0) ? 0 : Mathf.Atan2(delta.z, delta.x); //Tetta in radian

        // **************************** Apply exponential transfer function ***********************************************

        // TED: Replace absolute radius with deadZone-relative value
        float radiousExp = Mathf.Pow(Mathf.Max(0f, radius - deadZoneRadius), exponentialTransferFuntionPower);

        // **************************** Limiting the speed if needed ***********************************************
        if (speedLimit >= 0 && radiousExp > speedLimit)
            radiousExp = speedLimit;

        // **************************** Calculate polar coordinates of head ***********************************************
        float velocityX = radiousExp * Mathf.Cos(Fi) * Mathf.Cos(Tetta);
        float velocityY = radiousExp * Mathf.Sin(Fi);
        float velocityZ = radiousExp * Mathf.Cos(Fi) * Mathf.Sin(Tetta);

        Vector3 velocity = new Vector3(velocityX, velocityY, velocityZ);
        Vector3 translate = velocity * Time.deltaTime;
        //translate.y = (transform.position.y + translate.y < 0) ? 0 : translate.y; // the player should not go beneath the ground
        translate.y = 0;

        //  ****************************** Calculate the Rotatoion locomotion *****************************************
        //Debug.Log ("Exp Distance = " + radiousExp + " - velocity = " + velocity.ToString ());

        playerSystem.transform.position += translate;
    }


}
