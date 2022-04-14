using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneGamePadLocomotion : BaseLocomotion 
{

	 
    /******************************************************************************************************************************/
    /**********             Speed Parameters: You can change them to move faster/slower toward each direction           ***********/
    /******************************************************************************************************************************/
    public float speedLimit = 10;
    [Range(0, 0.5f)]
    public float deadZoneRadius = 0.1f;
    public float scaleFactor = 1f;

    /******************************************************************************************************************************/
    /**** 								Internal Variables: Don't change these variables										***/
    /******************************************************************************************************************************/
    float headHeight = 0.07f;
	float headWidth = 0.09f;
	float angularDifference = 0f;
	float headChairRadius;
	float speedSensitivity = 10f;
	float forwardDistanceZero = 0;
	bool FirstCheck = true;
	bool StartLocomotion = false;
    float exponentialTransferFuntionPower = 1.53f;
    
    /******************************************************************************************************************************/
    /**** 								            Variables for Returning Value        										***/
    /******************************************************************************************************************************/

    float returnVectorx;
	float returnVectory;
	float returnVectorz;
    private Quaternion _turnInput;
	private float _forward;
	private float _strafe;
	private float _up;
	public float strafe
	{
		get
		{
			return _strafe;
		}
	}
	public float up
	{
		get
		{
			return _up;
		}
	}

	public Quaternion turnInput
	{
		get
		{
			return _turnInput;
		}
	}

	public float forward
	{
		get
		{
			return _forward;
		}
	}

    /******************************************************************************************************************************/
    /**** 					        	 Initial transformation data for the purpose of Calibration       		  				***/
    /******************************************************************************************************************************/
    
	private Transform intialHeadTransform;
	private Transform intialTrackerTransform;
	private Vector3 intialDistance;
	private Vector3 intialHeadPosition;
	private Vector3 intialTrackerPosition;
    public Vector3 correctionPosition;
    // Game Manager
    private GameManager gameManager;
    // Usually main camera
    private GameObject player;
    // Player system
    private GameObject playerSystem;
    // Trcaker
    private ViveTracker tracker;
    private GameObject drone;

    //0 = before printing PressSpace message, 1 = after PressSpace message waiting for space, 2 = after space press and when the user can fly

    public override void BeginSetupState()
    {
        base.BeginSetupState();
        gameManager = GameObject.FindObjectOfType<GameManager>();
        // Get player and player system
        BaseCamera baseCamera = GameObject.FindObjectOfType<BaseCamera>();
        player = baseCamera.GetPlayer();
        playerSystem = baseCamera.GetPlayerSystem();
        drone = GameObject.FindGameObjectWithTag("Drone");
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
        // if (tracker == null)
        // {
        //     BaseNotifier.ShowInAnyNotifier("Turn on tracker");
        //     return;
        // }
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
            
		// *************************** Initial Relaxed Position *************************** 
		intialHeadTransform = player.transform;
		intialTrackerTransform = tracker.transform;
		intialHeadPosition = intialHeadTransform.localPosition;
		intialTrackerPosition = intialTrackerTransform.localPosition;

		// *************************** Initial Relaxed Constants *************************** 
		intialDistance = intialHeadTransform.localPosition - intialTrackerTransform.localPosition;
		headChairRadius = Mathf.Sqrt(intialDistance.x*intialDistance.x + intialDistance.z*intialDistance.z);

		float headP2 = -intialTrackerTransform.localRotation.eulerAngles.x;
		float headControllerX2 = intialTrackerPosition.x;
		float headControllerZ2 = intialTrackerPosition.z;
	    
		float headControllerYaw = 180 + intialTrackerTransform.localRotation.eulerAngles.y;
		if(headControllerYaw>360)
			headControllerYaw-=360;
		float headX2 = headControllerZ2 * Mathf.Cos(headControllerYaw * Mathf.PI / 180) + headControllerX2 * Mathf.Sin(headControllerYaw * Mathf.PI / 180);
		float headAlpha = (headP2 - angularDifference)*Mathf.PI/180;
		float headXo = headX2 + headChairRadius*Mathf.Cos(headAlpha);
		float headXz = intialHeadPosition.z*Mathf.Cos(headControllerYaw*Mathf.PI/180)+intialTrackerPosition.x*Mathf.Sin(headControllerYaw*Mathf.PI/180);
		headXz += headHeight*(1 - Mathf.Abs(Mathf.Cos(intialHeadTransform.rotation.eulerAngles.x*Mathf.PI/180)));
		forwardDistanceZero = headXo - headXz;

		// Console Log
		
		// Debug.Log("intialHeadTransform: " + intialHeadPosition);
		// Debug.Log("intialTrackerTransform: " + intialTrackerPosition);
		// Debug.Log("headChairRadius: " + headChairRadius);
		
		// *************************** Drone Orientation  *************************** 

		_turnInput = tracker.transform.rotation;
		

            BaseNotifier.ShowInAnyNotifier("");
            BeginPendingState();
        }
	}

    protected override void OnPlayState()
    {
        base.OnPlayState();
        
       // *************************** Forward Motion Calculation *************************** 

	
			// _up = returnVectory * scaleFactor;
			_turnInput = tracker.transform.rotation;
			
			Vector3 temp = drone.transform.position + correctionPosition;

            playerSystem.transform.position = temp;

			

            // Debug.Log("drone transform: " + drone.transform.position);
            // Debug.Log("player: " + player.transform.position)
	}
}
