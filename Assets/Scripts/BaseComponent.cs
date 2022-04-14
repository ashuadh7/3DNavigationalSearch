using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// State of components (camera, locomotion, interaction)
public enum ComponentState
{
    Initial,           // Initial state - nothing to do
    Setup,          // Setup state - do setup of component
    Pending,      // Setup done state - inform that setup has been done, wait for playing
    Play,           // Play state - run in Play State of Game Manager
    Finished        // Finsihed state - run from the end of Play State of Game Manager
}

// Base class of all components
public abstract class BaseComponent : MonoBehaviour
{

    // Internal variable
    protected ComponentState state = ComponentState.Initial;

    // Get current state
    public ComponentState State { get { return state; } }

    // Go to setup state 
    // Usually called automatically by SetupState of GameManager
    // Usually NECESSARY to override to start additional setup
    public virtual void BeginSetupState()
    {
        state = ComponentState.Setup;
    }

    // Go to play state - should be called at the beginning of Play State of Game Manager
    // Usually called automatically at the beginning of PlayState of GameManager
    // Usually NECESSARY to override to initialize playing parameters 
    public virtual void BeginPlayState()
    {
        state = ComponentState.Play;
    }

    // Go to setup done state - must be called when setup is done
    // NEED to called MANUALLY when setup has been done during OnSetup()
    // Usually NOT necessary to override
    public virtual void BeginPendingState()
    {
        state = ComponentState.Pending;
    }

    // Go to Finished state - should be called at the end of Play State of Game Manager
    // Usually called automatically at the end of PlayState of GameManager
    // Usually NOT necessary to override
    public virtual void BeginFinishedState()
    {
        state = ComponentState.Pending;
    }

    // Update - called once per frame
    // Usually NOT neccessary to override
    protected virtual void Update()
    {
        // Choose what to do based on current state
        switch (state)
        {
            case ComponentState.Setup:
                OnSetupState();
                break;
            case ComponentState.Pending:
                OnPendingState();
                break;
            case ComponentState.Play:
                OnPlayState();
                break;
            case ComponentState.Finished:
                OnFinishedState();
                break;
            default:
                break;
        }
    }

    // Overridable methods in update 
    // The following methods do NOT need to called manually because they are called in Update already

    // Update during Pending state - usually NOT necessary to override
    protected virtual void OnPendingState() { }

    // Update during Setup state - MUST BE override
    protected virtual void OnSetupState() { }

    // Update during Play state - usually NECESSARY to override
    protected virtual void OnPlayState() { }

    // Update during Finished State - usually NOT necessary to override
    protected virtual void OnFinishedState() { }

}
