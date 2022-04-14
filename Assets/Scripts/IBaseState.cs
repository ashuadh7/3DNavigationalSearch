using System;

// The base interface of all state
public interface IBaseState
{
	// This update method should be invoked every frame by the state's owner
    void Update();

	// Force stop 
	void ForceStop();
}
