using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewLocomotion : BaseLocomotion
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

    private ViveTracker2 tracker2;

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

        if (tracker2 == null)
        {
            tracker2 = GameObject.FindObjectOfType<ViveTracker2>();
        }

        // Show notification if tracker is turned off
        if (tracker2 == null)
        {
            BaseNotifier.ShowInAnyNotifier("Turn on tracker 2");
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
            BaseNotifier.ShowInAnyNotifier("");
            BeginPendingState();
        }
    }

    protected override void OnPlayState()
    {
        base.OnPlayState();
        
        Vector3 neckPosition, basePosition;

        neckPosition = tracker.transform.position;
        basePosition = tracker2.transform.position;

        Vector3 delta;

        if (neckPosition.y<basePosition.y)
        {
        delta = Vector3.ProjectOnPlane(neckPosition, Vector3.up) - Vector3.ProjectOnPlane(basePosition, Vector3.up);
        }
        else
        {
        delta = Vector3.ProjectOnPlane(basePosition, Vector3.up) - Vector3.ProjectOnPlane(neckPosition, Vector3.up);
        }

       // delta = Vector3.ProjectOnPlane(basePosition, Vector3.up) - Vector3.ProjectOnPlane(neckPosition, Vector3.up);
        
         float velocity = 10/3*(delta.magnitude-(1/10))*speedLimit*Mathf.Pow(Mathf.Max(0f, delta.magnitude) * speedSensitivity, exponentialTransferFuntionPower);
            if (speedLimit >= 0 && velocity > speedLimit)
            {velocity = speedLimit;}
            Vector3 translate = Vector3.ProjectOnPlane(delta.normalized*velocity, Vector3.up)* Time.deltaTime;
  
		    //File.AppendAllText (path, content);
            playerSystem.transform.position += translate;
    }
}
