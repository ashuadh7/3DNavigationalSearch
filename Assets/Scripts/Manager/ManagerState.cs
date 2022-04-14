using System;

public abstract class ManagerState : StateGroup<ManagerState>
{
	// The context provides access to some restricted  data of a Central Manager.
	protected GameManager.Context context;

	// Initializes a new instance of the <see cref="ManagerState"/> class.
	public ManagerState (GameManager.Context context) : base()
    {
		this.context = context;
	}

	// Gets the next state of the Central Manager.
	protected override ManagerState GetNextState ()
    {
		return context.GetNextState (this);
	}

	// Switchs to the new generated state of the Central Manager.
	protected override void SwitchToNextState (ManagerState nextState)
    {
		context.SwitchToNextState (nextState);
	}

}

