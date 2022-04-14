using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneLocomotion : BaseLocomotion
{
    
    /******************************************************************************************************************************/
    /**********             Speed Parameters: You can change them to move faster/slower toward each direction           ***********/
    /******************************************************************************************************************************/
    public float speedLimit = 10;
    [Range(0, 0.5f)]
    public float deadZoneRadius = 0.1f;
    public float scaleFactor = 2f;

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
	private float maxforward = 0f;
	private float maxstrafe = 0f;
	private float maxup = 0f;
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
		// GameObject.FindGameObjectWithTag("cockpit").SetActive(false);
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
		if(gameManager.scaleSize == 1)
		{
			correctionPosition = correctionPosition = new Vector3(0,-(player.transform.position.y-0.3f),0);			
		}
		else
		{
			correctionPosition = new Vector3(0,-(player.transform.position.y+0.3f+0.3f*(gameManager.scaleSize-1)/4),0);
		}
		
    }

    protected override void OnPlayState()
    {
        base.OnPlayState();
        
       // *************************** Forward Motion Calculation *************************** 

			float ChairDirectionYaw = 180 + tracker.transform.localRotation.eulerAngles.y;
			if (ChairDirectionYaw > 360) {ChairDirectionYaw -= 360;}
			
			// Console Log
			// Debug.Log("ChairDirectionYaw: " + ChairDirectionYaw);

			float headXnow = player.transform.localPosition.z*Mathf.Cos(ChairDirectionYaw*Mathf.PI/180) + player.transform.localPosition.x* Mathf.Sin(ChairDirectionYaw*Mathf.PI/180);
			headXnow += headHeight * (1- Mathf.Abs(Mathf.Cos(player.transform.rotation.eulerAngles.x*Mathf.PI / 180)));
			
			// Console Log
			// Debug.Log("headXnow:" + headXnow);


			float headP3 = -tracker.transform.localRotation.eulerAngles.x;
			Vector3 headController3;
			headController3 = tracker.transform.localPosition;
			
			// Console Log
			// Debug.Log("headController3:" + headController3);
			
			
			float headX3 = headController3.z*Mathf.Cos(ChairDirectionYaw*Mathf.PI/180) + headController3.x * Mathf.Sin(ChairDirectionYaw*Mathf.PI / 180);
			float chairAlpha = (headP3 - angularDifference) * Mathf.PI/180;
			float headXo = headX3 + headChairRadius * Mathf.Cos(chairAlpha);
			float headYo = headController3.y - headChairRadius*Mathf.Sin(chairAlpha);
			
			// Console Log
			// Debug.Log("headXo: "+ headXo);
			
			float forwardDistance = headXo - headXnow;

			// Console Log
			// Debug.Log("forwardDistance: "+ forwardDistance);

			
			float forwardInputrate = forwardDistance - forwardDistanceZero;

			// Console Log
			// Debug.Log("forwardInputrate: "+ forwardInputrate);

			//  *************************** Sideways Motion Calculation *************************** 

			float ChairYaw = 360 - tracker.transform.rotation.eulerAngles.y;
			float HeadYaw = 360 - player.transform.rotation.eulerAngles.y;
			float HeadX = -player.transform.position.z + headWidth*Mathf.Cos(HeadYaw*Mathf.PI/180);
			float HeadY = player.transform.position.x + headWidth*Mathf.Sin(HeadYaw*Mathf.PI/180);
			float ChairX = -tracker.transform.position.z;
			float ChairY = tracker.transform.position.x;

			float LeaningDistanceSideway = Mathf.Sin(ChairYaw * Mathf.PI / 180) * (HeadX - ChairX) - Mathf.Cos(ChairYaw*Mathf.PI/180)*(HeadY - ChairY) - 0.005f;

			float sidewayInputRate = -LeaningDistanceSideway;
			
			// Console Log
			// Debug.Log("sidewayInputRate: " + sidewayInputRate);

			// *************************** Upward Motion Calculation ***************************
			
			float CurrentY = player.transform.localPosition.y;
			CurrentY += headHeight*Mathf.Sin(player.transform.rotation.eulerAngles.x * Mathf.PI/180);
			float PreviousY = intialHeadPosition.y;	
			PreviousY += headHeight*Mathf.Sin(intialHeadTransform.rotation.eulerAngles.x*Mathf.PI / 180);
			float leaningUpwardDistance = CurrentY - PreviousY;
			float upwardInputRate = leaningUpwardDistance;

			// Console Log

			// Debug.Log("Displacement Value:" + Mathf.Sin(cameraRig.transform.rotation.eulerAngles.x * Mathf.PI/180));
			// Debug.Log("CurrentY: " + CurrentY);
			// Debug.Log("Modified CurrentY: "+ CurrentY);
			// Debug.Log("Previous Y: " + intialHeadPosition.y);
			// Debug.Log("Previous Y modified:" + PreviousY );
			// float leaningUpwardDistance = CurrentY - intialHeadPosition.y;
			// Debug.Log("upward: " + check);
			// Debug.Log("upwardInputRate: " + upwardInputRate);

			// *************************** Apply Speed Sensitivity ***************************
			
			Vector3 netInputRate = new Vector3(sidewayInputRate, upwardInputRate, forwardInputrate);
			netInputRate *= speedSensitivity;
			netInputRate.y *= 2;

			// *************************** Apply Exponential Transfer Function ***************************

			float radius = netInputRate.magnitude;
			float Fi = (radius == 0) ? 0 : Mathf.Asin((float)(netInputRate.y / radius));
			float Theta = (netInputRate.x == 0 && netInputRate.z == 0) ? 0: Mathf.Atan2(netInputRate.z, netInputRate.x);

			float radiusExp = Mathf.Pow(radius, 1.53f);
			// float radiusExp = Mathf.Pow(radius, 1f);

			float sidewayVelocity = radiusExp * Mathf.Cos(Fi) * Mathf.Cos(Theta);
			float upwardVelocity = radiusExp * Mathf.Sin(Fi);
			float forwardVelocity = radiusExp * Mathf.Cos(Fi) * Mathf.Sin(Theta);

			if (sidewayVelocity>10) {sidewayVelocity = 10;}
			if (sidewayVelocity<-10) {sidewayVelocity = -10;}
			if (upwardVelocity>10) {upwardVelocity = 10;}
			if (upwardVelocity<-10) {upwardVelocity = -10;}
			if (forwardVelocity>10) {forwardVelocity = 10;}
			if (forwardVelocity<-10) {forwardVelocity = -10;}

			returnVectorx = forwardVelocity/10;
			returnVectory = upwardVelocity/5;
			returnVectorz = sidewayVelocity/10;

			_forward = returnVectorx * scaleFactor;
			_strafe = returnVectorz * scaleFactor;
			_up = returnVectory * scaleFactor;
			_turnInput = tracker.transform.rotation;

			if (_forward > maxforward)
			{
				maxforward = _forward;
			}
			if (_strafe > maxstrafe)
			{
				maxstrafe = _strafe;
			}
			if (_up > maxup)
			{
				maxup = _up;
			}			
			Debug.Log("maxforward: " + maxforward);
			Debug.Log("maxstrafe: " + maxstrafe);
			Debug.Log("maxup: " + maxup);

			
			// correctionPosition.x = 0.5f * Mathf.Cos(tracker.transform.rotation.eulerAngles.y);
			// correctionPosition.y = -(player.transform.position.y-0.5f);
			// correctionPosition.z =  0.5f * Mathf.Sin(tracker.transform.rotation.eulerAngles.y);

            playerSystem.transform.position = drone.transform.position + correctionPosition;

            // Debug.Log("drone transform: " + drone.transform.position);
            // Debug.Log("player: " + player.transform.position);
    }
}
