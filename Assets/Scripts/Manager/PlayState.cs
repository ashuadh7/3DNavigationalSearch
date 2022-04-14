using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayState : ManagerState {

    // Configuration variables
    private BaseCamera baseCamera;
    private BaseLocomotion baseLocomotion;
    private BaseCollector baseCollector;
    private BaseField baseField;

    // Player is main camera
    private GameObject player;

    // Last and current target
    private BaseTarget lastTarget = null;
    private BaseTarget currentTarget = null;

    // Number of core collected
    private int numberOfCollectedCores = 0;
    
    //Time management
    private float maxTime = 300f;
    private float timeSpent;
    GameManager gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();

    // Constructor with context of central manager
    public PlayState(GameManager.Context context) : base(context) { }

    // Initializer for the state
    protected override void Init()
    {
        base.Init();
        Debug.Log("Entering state 2 - Play ...");

        // Find options
        baseCamera = GameObject.FindObjectOfType<BaseCamera>();
        baseLocomotion = GameObject.FindObjectOfType<BaseLocomotion>();
        baseCollector = GameObject.FindObjectOfType<BaseCollector>();
        baseField = GameObject.FindObjectOfType<BaseField>();

        // Play
        baseCamera.BeginPlayState();
        baseLocomotion.BeginPlayState();
        baseCollector.BeginPlayState();
        baseField.BeginPlayState();

        // Extras
        foreach (BaseComponent extra in context.Extras)
        {
            extra.BeginPlayState();
        }

        // Set player
        player = baseCamera.GetPlayer();

        // Notify game manager
        context.OnPlayStateBeginning(player);

        // Show notifier
        BaseNotifier.ShowInAnyNotifier("0 ball collected");
    }

    // Internal execution called once per frame
    protected override void Execute()
    {
        base.Execute();
        // Debug.Log(string.Format("PlayerPos = ({0:F2}, {1:F2}, {2:F2})", player.transform.position.x, player.transform.position.y, player.transform.position.z));
        // Control player-target interaction
        ControlGameLogic();
    }

    // Deinitializer for the state
    protected override void Deinit()
    {
        base.Deinit();

        // Stop playing
        baseCamera.BeginFinishedState();
        baseLocomotion.BeginFinishedState();
        baseCollector.BeginFinishedState();
        baseField.BeginFinishedState();

        // Extras
        foreach (BaseComponent extra in context.Extras)
        {
            extra.BeginFinishedState();
        }

        // Notify game manager
        context.OnPlayStateFinishing(player);
    }

    // Condition to stop the state
    protected override bool ShouldBeEnded()
    {
        // Check if all target is empty
        foreach (BaseTarget target in baseField.TargetList)
        {
            if (!target.IsEmpty)
            {
                if (timeSpent>maxTime)
                {
                    return true;
                }
                return false;
            }
        }
        return true;
    }

    // ------------------- Logic support methods -----------------------

    // Control player-target interaction
    private void ControlGameLogic()
    {
        // Hide targets that too far away
        HideTooFarAwayTargets();
        timeSpent += Time.deltaTime;

        // Find target that is approached by player
        currentTarget = FindTargetVisitedByPlayer();

        // To prevent APPROACHING and LEAVING events happening in the same frame
        // Never let both last target and current target be non-null and different from each other
        if (lastTarget != null && currentTarget != null && lastTarget != currentTarget)
        {
            currentTarget = null;
        }

        // Checking if player is finding a target to approach
        bool findingTarget = true;

        // Check APPROACHING event
        if (lastTarget == null && currentTarget != null)
        {
            findingTarget = false;
            context.OnPlayerApproachingTarget(player, currentTarget);
            currentTarget.OnBeingApproached(player);
            baseCollector.OnApproaching(currentTarget);
        }

        // Check LEAVING event
        if (lastTarget != null && currentTarget == null)
        {
            findingTarget = false;
            context.OnPlayerLeavingTarget(player, lastTarget);
            lastTarget.OnBeingLeft(player);
            baseCollector.OnLeaving(lastTarget);
        }

        // Check VISITING event (after APPROACHING and before LEAVING)
        if (lastTarget != null && lastTarget == currentTarget)
        {
            findingTarget = false;
            // Check if core can be collected
            if (currentTarget.IsReadyToBeCollected(player) && baseCollector.IsReadyToCollect(currentTarget))
            {
                context.OnPlayerCollectingTarget(player, currentTarget);
                currentTarget.OnBeingCollected(player);
                baseCollector.OnCollecting(currentTarget);
                ++numberOfCollectedCores;
                BaseNotifier.ShowInAnyNotifier(string.Format("{0} ball{1} collected", 
                    numberOfCollectedCores, numberOfCollectedCores > 1 ? "s" : ""));
            }
            else
            {
                // Visiting
                context.OnPlayerVisitingTarget(player, currentTarget);
                currentTarget.OnBeingVisited(player);
                baseCollector.OnVisiting(currentTarget);
            }
        }

        // Inform event
        if (findingTarget)
        {
            context.OnPlayerFindingTarget(player);
        }

        // Check if player is in safe radius
        if (IsPlayerInSafeArea())
        {
            context.OnPlayerInSafeArea(player);
        }
        else
        {
            context.OnPlayerInUnsafeArea(player);
        }

        // Assign last target
        lastTarget = currentTarget;
    }

    // Check if player is in safe area
    private bool IsPlayerInSafeArea()
    {
        return Vector3.Distance(player.transform.position, baseField.transform.position) <= context.SafeRadius * gameManager.scaleSize;
    }

    // Hide targets that too far away from player (main camera)
    private void HideTooFarAwayTargets()
    {
        foreach (BaseTarget target in baseField.TargetList)
        {
            bool isVisible = Vector3.Distance(player.transform.position, target.GetDefaultCorePosition()) <= context.MaxDistanceToSeeTarget * gameManager.scaleSize;
            target.gameObject.SetActive(isVisible);
        }
    }

    // Find a target approached by player
    private BaseTarget FindTargetVisitedByPlayer()
    {
        foreach (BaseTarget target in baseField.TargetList)
        {
            if (IsInEffectiveFieldOfTarget(target))
            {
                return target;
            }
        }
        return null;
    }

    // Is player in effective field of a target
    // 1st: target must be active
    // 2nd: distance must be in required range
    // 3rd: view angles from both target and player are in required angular field
    private bool IsInEffectiveFieldOfTarget(BaseTarget target)
    {
        // Target must be visible
        if (!target.gameObject.activeSelf)
        {
            return false;
        }

        // Convert to Vector2
        Vector3 playerPosition = player.transform.position;
        Vector3 playerForward = player.transform.forward;
        Vector3 corePosition = target.GetDefaultCorePosition();
        Vector3 coreForward = target.transform.forward;

        // Get direction vectors
        Vector3 corePlayerVector = playerPosition - corePosition;
        Vector3 playerCoreVector = -corePlayerVector;

        // Check if target-player distance is in required range
        bool isInRequiredDistanceRange
            = corePlayerVector.magnitude >= context.MinDistanceToOpenTarget * gameManager.scaleSize
            && corePlayerVector.magnitude <= context.MaxDistanceToOpenTarget * gameManager.scaleSize;
        if (!isInRequiredDistanceRange)
        {
            return false;
        }

        // Calculate angle between target forward and target-player vector
        float targetAngle = Vector3.Angle(coreForward, corePlayerVector);
        // Calcualte angle between player forward and player-target vector
        float playerAngle = Vector3.Angle(playerForward, playerCoreVector);

        // Check if target in in required FOV of player and vice versa
        bool isInRequiredAngularField
            = Mathf.Abs(targetAngle * 2) <= context.AngularFieldToOpenTarget
            && Mathf.Abs(playerAngle * 2) <= context.AngularFieldToOpenTarget;
        if (!isInRequiredAngularField)
        {
            return false;
        }

        // All conditions are satisfied
        return true;
    }

}
