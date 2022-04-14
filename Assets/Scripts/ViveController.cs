﻿using UnityEngine;
using System.Collections;

public class ViveController : SteamVR_TrackedController
{
	// Types of controller
	public enum ControllerType
	{
		General,
		Active,
		Passive
	};

	// Type
	public ControllerType controllerType;

    // Use this for initialization
    protected override void Start ()
	{
		base.Start ();
		// Default type is general
		controllerType = ControllerType.General;
	}

	// Update is called once per frame
	protected override void Update ()
	{
		base.Update ();
        // Selecting
        if (isOnSelectingActiveController)
        {
            // Prevent running the following script twice in the same frame within update of 2 controllers
            if (frameIndex != Time.frameCount)
            {
                // Get controller with triggered pressed
                ViveController activeController = GetByTriggerPressed();
                if (activeController != null)
                {
                    SetActive(activeController);
                    isOnSelectingActiveController = false;
                    frameIndex = -1;
                }

                // Update frame index
                frameIndex = Time.frameCount;
            }
        }
	}

    // Flag for selection
    private static bool isOnSelectingActiveController = false;

    // Frame index for selection
    private static int frameIndex = -1;

    // Start selectin active controller
    public static void StartSelectingActiveController()
    {
        if (GetByType(ControllerType.Active) == null)
        {
            isOnSelectingActiveController = true;
            frameIndex = -1;
        }
    }

	// Get number of visible controllers
	public static int GetTotal() {
		return GameObject.FindObjectsOfType<ViveController> ().Length;
	}

	// Get controller by a given type
	public static ViveController GetByType(ControllerType controllerType) {
		ViveController[] gameControllers = GameObject.FindObjectsOfType<ViveController> ();
		foreach (ViveController gameController in gameControllers) {
			if (gameController.controllerType == controllerType) {
				return gameController;
			}
		}
		return null;
	}

	// Get controllers with trigger pressed
	public static ViveController GetByTriggerPressed() {
		ViveController[] gameControllers = GameObject.FindObjectsOfType<ViveController> ();
		foreach (ViveController gameController in gameControllers) {
			if (gameController.triggerPressed) {
				return gameController;
			}
		}
		return null;
	}

	// Set active state for the given controller and passive for the other
	public static void SetActive(ViveController activeController) {
		ViveController[] gameControllers = GameObject.FindObjectsOfType<ViveController> ();
		foreach (ViveController gameController in gameControllers) {
			gameController.controllerType = gameController == activeController ? ControllerType.Active : ControllerType.Passive;
		}
	}

    // Check if active controller has been assigned
    public static bool HasActive()
    {
        return GetByType(ControllerType.Active) != null;
    }

}

