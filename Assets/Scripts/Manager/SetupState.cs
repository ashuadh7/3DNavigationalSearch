using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupState : ManagerState {

    // Setup Phase
    private enum Phase { Initial, Camera, Tracker, Locomotion, Collector, Extras, Field, Pending, Finished }

    // Confiuration variable
    private BaseCamera baseCamera;
    private BaseLocomotion baseLocomotion;
    private BaseCollector baseCollector;
    private BaseField baseField;

    // Phase
    private Phase phase;

    // Constructor with context of central manager
    public SetupState(GameManager.Context context) : base(context) { }

    // Initializer for the state
    protected override void Init()
    {
        base.Init();
        Debug.Log("Entering state 1 - Setup ...");

        // Get components
        baseCamera = GameObject.FindObjectOfType<BaseCamera>();
        baseLocomotion = GameObject.FindObjectOfType<BaseLocomotion>();
        baseCollector = GameObject.FindObjectOfType<BaseCollector>();
        baseField = GameObject.FindObjectOfType<BaseField>();

        // Set initial phase
        phase = Phase.Initial;

        // Show notification
        BaseNotifier.ShowInAnyNotifier("Setting up ...");
    }

    // Internal execution called once per frame
    protected override void Execute()
    {
        base.Execute();
        // Setup camera, locomotion, then interaction
        switch (phase)
        {
            case Phase.Initial:
                phase = Phase.Camera;
                baseCamera.BeginSetupState();
                break;
            case Phase.Camera:
                if (baseCamera.State == ComponentState.Pending)
                {
                    phase = Phase.Tracker;
                    // Enable tracker if needed
                    BaseNotifier.ShowInAnyNotifier("Turn on tracker");
                }
                break;
            case Phase.Tracker:
                ViveTracker tracker = GameObject.FindObjectOfType<ViveTracker>();
                if (tracker != null)
                {
                    // Setup locomotion
                    phase = Phase.Locomotion;
                    baseLocomotion.isStanding = context.IsStanding;
                    baseLocomotion.BeginSetupState();

                    if (!System.IO.File.Exists("center.txt"))
                    {
                        // Centralize player at (0,0)
                        Vector3 playerPos = baseCamera.GetPlayer().transform.position;
                        playerPos.y = 0;
                        baseCamera.GetPlayerSystem().transform.position -= playerPos;
                    }
                    else
                    {
                        using (System.IO.StreamReader reader = new System.IO.StreamReader("center.txt"))
                        {
                            float x = System.Convert.ToSingle(reader.ReadLine());
                            float y = System.Convert.ToSingle(reader.ReadLine());
                            float z = System.Convert.ToSingle(reader.ReadLine());
                            Vector3 trackerZero = new Vector3(x, 0, z);
                            reader.Close();
                            baseCamera.GetPlayerSystem().transform.position -= trackerZero;
                        }
                    }
                }
                break;
            case Phase.Locomotion:
                if (baseLocomotion.State == ComponentState.Pending)
                {
                    phase = Phase.Collector;
                    baseCollector.BeginSetupState();
                }
                break;
            case Phase.Collector:
                if (baseCollector.State == ComponentState.Pending)
                {
                    phase = Phase.Extras;
                    SetupExtras();
                }
                break;
            case Phase.Extras:
                if (AreExtrasSetupDone())
                {
                    phase = Phase.Field;
                    // Hide field from player
                    baseField.transform.position += new Vector3(0, 5, 0);
                    // Setup field
                    baseField.BeginSetupState();
                }
                break;
            case Phase.Field:
                if (baseField.State == ComponentState.Pending)
                {
                    phase = Phase.Pending;
                    BaseNotifier.ShowInAnyNotifier("Are you ready?");
                }
                break;
            case Phase.Pending:
                if (Input.GetKeyUp(KeyCode.Space))
                {
                    // Show target field
                    baseField.transform.position -= new Vector3(0, 5, 0);
                    // Set target height
                    // baseField.ResetTargetHeight(baseCamera.GetPlayer().transform.position.y - 0.10f);
                    // Finish
                    phase = Phase.Finished;
                }
                break;
            default:
                break;
        }
    }

    // Deinitializer for the state
    protected override void Deinit()
    {
        base.Deinit();
    }

    // Condition to stop the state
    protected override bool ShouldBeEnded()
    {
        return phase == Phase.Finished;
    }

    // Setup all savers and extras
    private void SetupExtras()
    {
        foreach (BaseComponent extra in context.Extras)
        {
            extra.BeginSetupState();
        }
    }

    // Check if all savers and extras are setup done
    private bool AreExtrasSetupDone()
    {
        foreach (BaseComponent extra in context.Extras)
        {
            if (extra.State != ComponentState.Pending)
            {
                return false;
            }
        }
        return true;
    }
}
