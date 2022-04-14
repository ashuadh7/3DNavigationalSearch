using System;

public abstract class StateGroup<T> : IBaseState where T : StateGroup<T>
{
	// Current status of the state
    private bool isInitialized;

	// Check if whether this state is initialized or not
	public bool IsInitialize { get { return this.isInitialized; } }

	// Default constructor
	public StateGroup() {
        isInitialized = false;
    }

	// This method should be invoked every frame by the state's owner
    public void Update() {
		// Invoke initialization for the first time
        if (!isInitialized) {
            Init();
            isInitialized = true;
        }

		// Execute main job for every invocation of this method
        Execute();

		// Check if ended condition is satisfied or not
        if (ShouldBeEnded())
        {
			// Invoke deinitialization once
            Deinit();
            isInitialized = false;
			// Switch to next state
            SwitchToNextState(GetNextState());
        }
    }

	// Force stop the state
	public void ForceStop() {
		if (isInitialized) {
			Deinit ();
			isInitialized = false;
			SwitchToNextState (GetNextState ());
		}
	}

	// Initialize the state, called once
    protected virtual void Init() { }

	// Deinitialize the state, called once
    protected virtual void Deinit() { }

	// Execute the main job, called every frame
    protected virtual void Execute() { }

	// The condition to finish the state
    protected abstract bool ShouldBeEnded();

	// Generate the next state
    protected abstract T GetNextState();

	// Switch to next state
    protected abstract void SwitchToNextState(T nextState);
}
