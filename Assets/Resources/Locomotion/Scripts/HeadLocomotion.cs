using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HeadLocomotion : BaseLocomotion
{

    public string headCenterFilePath = "center.txt";

    /******************************************************************************************************************************/
    /**********             Speed Parameters: You can change them to move faster/slower toward each direction           ***********/
    /******************************************************************************************************************************/
    public float speedLimit = 10;
    public float speedSensitivity = 5;
    [Range(0, 2f)]
    public float deadZoneRadius = 0.3f;

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

    //0 = before printing PressSpace message, 1 = after PressSpace message waiting for space, 2 = after space press and when the user can fly

    public override void BeginSetupState()
    {
        base.BeginSetupState();
        gameManager = GameObject.FindObjectOfType<GameManager>();

        // Get player and player system
        BaseCamera baseCamera = GameObject.FindObjectOfType<BaseCamera>();
        player = baseCamera.GetPlayer();
        playerSystem = baseCamera.GetPlayerSystem();

        deadZoneRadius = isStanding ? 0.15f : 0.10f; 
    }

    protected override void OnSetupState()
    {
        base.OnSetupState();

        // Show instruction
        if (gameManager.isStanding)
        {
            BaseNotifier.ShowInAnyNotifier("Stand straight");
        }
        else
        {
            BaseNotifier.ShowInAnyNotifier("Sit straight");
        }

        if (Input.GetKeyDown(KeyCode.Space) || File.Exists(headCenterFilePath))
        {
            if (!File.Exists(headCenterFilePath))
            {
                //Read the Vive Controller data to calculate the neck position
                float headYaw = player.transform.localRotation.y;
                float headXo = player.transform.localPosition.x - headWidth * Mathf.Sin(headYaw * Mathf.PI / 180); //Calculate the Neck x Position
                float headYo = player.transform.localPosition.y + headHeight * Mathf.Sin(player.transform.rotation.eulerAngles.x * Mathf.PI / 180); //Calculate the Neck y Position;
                float headZo = player.transform.localPosition.z - headWidth * Mathf.Cos(headYaw * Mathf.PI / 180); //Calculate the Neck y Position
                headZero = new Vector3(headXo, headYo, headZo);
            }
            else
            {
                using (StreamReader reader = new StreamReader(headCenterFilePath))
                {
                    float headXo = Convert.ToSingle(reader.ReadLine());
                    reader.ReadLine();
                    float headYo = GameObject.FindObjectOfType<BaseCamera>().GetPlayer().transform.position.y;
                    float headZo = Convert.ToSingle(reader.ReadLine());
                    headZero = new Vector3(headXo, headYo, headZo);
                    reader.Close();
                }
            }

            BaseNotifier.ShowInAnyNotifier("");
            BeginPendingState();
        }
    }

    protected override void OnPlayState()
    {
        base.OnPlayState();

        // ***************************  Caqlculate the forward locomotion  *******************************************

        // TED: Use global position and rotation instead
        float headYaw = player.transform.localRotation.y;
        float headY = player.transform.localPosition.y + headHeight * Mathf.Sin(player.transform.rotation.eulerAngles.x * Mathf.PI / 180); //Calculate the Neck y Position
        float headX = player.transform.localPosition.x - headWidth * Mathf.Sin(headYaw * Mathf.PI / 180); //Calculate the Neck x Position
        float headZ = player.transform.localPosition.z - headWidth * Mathf.Cos(headYaw * Mathf.PI / 180); //Calculate the Neck y Position
        headCurrent = new Vector3(headX, headY, headZ);

        // **************************** Calculate polar coordinates of head ***********************************************

        // TED: Use Vector3 delta instead of deltaX, deltaY, deltaZ
        Vector3 delta = headCurrent - headZero;
        //float radius = delta.magnitude; // polar r (radious)
        //float Fi = (radius == 0) ? 0 : Mathf.Asin((float)(delta.y / radius)); // Fi in radian
        //float Tetta = (delta.x == 0 && delta.z == 0) ? 0 : Mathf.Atan2(delta.z, delta.x); //Tetta in radian

        // **************************** Apply exponential transfer function ***********************************************

        // TED: Replace absolute radius with deadZone-relative value
        float velocity = Mathf.Pow(Mathf.Max(0f, delta.magnitude - deadZoneRadius) * speedSensitivity, exponentialTransferFuntionPower)*speedLimit;

        // **************************** Limiting the speed if needed ***********************************************
        if (speedLimit >= 0 && velocity > speedLimit)
            velocity = speedLimit;

        // **************************** Calculate polar coordinates of head ***********************************************
        //float velocityX = velocity * Mathf.Cos(Fi) * Mathf.Cos(Tetta);
        //float velocityY = velocity * Mathf.Sin(Fi);
        //float velocityZ = velocity * Mathf.Cos(Fi) * Mathf.Sin(Tetta);

        //Vector3 velocity = new Vector3(velocityX, velocityY, velocityZ);
        //Vector3 translate = velocity * Time.deltaTime;

        Vector3 translate = Vector3.ProjectOnPlane(delta.normalized * velocity, Vector3.up) * Time.deltaTime;
        
        //translate.y = (transform.position.y + translate.y < 0) ? 0 : translate.y; // the player should not go beneath the ground
        //translate.y = 0;

        //  ****************************** Calculate the Rotatoion locomotion *****************************************
        //Debug.Log ("Exp Distance = " + radiousExp + " - velocity = " + velocity.ToString ());

        playerSystem.transform.position += translate;
    }


}
