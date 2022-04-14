using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NaviBoardDemo : MonoBehaviour {

	public string headCenterFilePath = "center.txt";
	public GameObject cameraRig;
	public GameObject visualizer;

	/******************************************************************************************************************************/
	/**********             Speed Parameters: You can change them to move faster/slower toward each direction           ***********/
	/******************************************************************************************************************************/
	public float speedLimit = 10;
	[HideInInspector]
	public float speedSensitivity = 3;
	[Range(0, 2f)]
	public float deadZoneRadius = 0.7f;

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
    private bool locomotionEnabled = false;
    private bool eventHanderAdded = false;
    private GameObject player;

    // Use this for initialization
    void Start () {
        player = cameraRig.GetComponentInChildren<SteamVR_Camera>().gameObject;
        if (File.Exists(headCenterFilePath))
        {
            using (StreamReader reader = new StreamReader(headCenterFilePath))
            {
                float headXo = Convert.ToSingle(reader.ReadLine());
                reader.ReadLine();
                float headYo = 0f;
                float headZo = Convert.ToSingle(reader.ReadLine());
                headZero = new Vector3(headXo, headYo, headZo);
                reader.Close();

                visualizer.transform.localPosition = new Vector3(headXo, .01f, headZo);
            }
        }
    }

	// Update is called once per frame
	void Update () {
        if (locomotionEnabled)
        {
            // TED: Use global position and rotation instead
            float headYaw = player.transform.localRotation.y;
            float headY = player.transform.localPosition.y + headHeight * Mathf.Sin(player.transform.rotation.eulerAngles.x * Mathf.PI / 180); //Calculate the Neck y Position
            float headX = player.transform.localPosition.x - headWidth * Mathf.Sin(headYaw * Mathf.PI / 180); //Calculate the Neck x Position
            float headZ = player.transform.localPosition.z - headWidth * Mathf.Cos(headYaw * Mathf.PI / 180); //Calculate the Neck y Position
            headCurrent = new Vector3(headX, headY, headZ);
            // **************************** Calculate polar coordinates of head ***********************************************

            // TED: Use Vector3 delta instead of deltaX, deltaY, deltaZ
            Vector3 delta = Vector3.ProjectOnPlane(headCurrent, Vector3.up) - headZero;
            //float radius = delta.magnitude; // polar r (radious)
            //float Fi = (radius == 0) ? 0 : Mathf.Asin((float)(delta.y / radius)); // Fi in radian
            //float Tetta = (delta.x == 0 && delta.z == 0) ? 0 : Mathf.Atan2(delta.z, delta.x); //Tetta in radian

            // **************************** Apply exponential transfer function ***********************************************
            float velocity = Mathf.Pow(Mathf.Max(0f, delta.magnitude - deadZoneRadius) * speedSensitivity, exponentialTransferFuntionPower) * speedLimit;

            // **************************** Limiting the speed if needed ***********************************************
            if (speedLimit >= 0 && velocity > speedLimit)
                velocity = speedLimit;

            // **************************** Calculate polar coordinates of head ***********************************************
            Vector3 translate = Vector3.ProjectOnPlane(delta.normalized * velocity, Vector3.up) * Time.deltaTime;

            //  ****************************** Calculate the Rotatoion locomotion *****************************************
            cameraRig.transform.position += translate;
        }

        if (!eventHanderAdded)
        {
            foreach (var controller in FindObjectsOfType<SteamVR_TrackedController>())
            {
                controller.TriggerUnclicked += (sender, e) =>
                {
                    locomotionEnabled = true;
                };

                Debug.Log("Added envent handler!");
                eventHanderAdded = true;
            }
        }
	}

    public void startLocomotion()
    {
        locomotionEnabled = true;
    }
}
