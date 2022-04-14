using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandFreeCollector : BaseCollector
{
    // Minimum looking time required to collect (in seconds)
    public float requiredTimeToCollect = 1f;

    // Starting time of effective phase
    private float startingEffectiveTime = 0.0f;

    // Waiting audio source
    private AudioSource waitingSound;

    // Update during Setup State
    protected override void OnSetupState()
    {
        base.OnSetupState();
        // Get waiting sound
        waitingSound = GetComponent<AudioSource>();
        // Done setup
        BeginPendingState();
    }

    // Set starting effective time
    public override void OnApproaching(BaseTarget target)
    {
        base.OnApproaching(target);
        startingEffectiveTime = Time.time;
        // Play waiting sound
        if (!target.IsEmpty)
        {
            waitingSound.Play();
        }
    }

    // Stop sound on collecting
    public override void OnCollecting(BaseTarget target)
    {
        base.OnCollecting(target);
        StopWaitingSound();
    }

    // Stop sound on leaving
    public override void OnLeaving(BaseTarget target)
    {
        base.OnLeaving(target);
        StopWaitingSound();
    }

    // Use to check if player is ready to collect a core (if it exists)
    public override bool IsReadyToCollect(BaseTarget target)
    {
        return base.IsReadyToCollect(target)
            && (Time.time - startingEffectiveTime) >= requiredTimeToCollect;
    }

    // Stop waiting sound
    private void StopWaitingSound()
    {
        if (waitingSound.isPlaying)
        {
            waitingSound.Stop();
        }
    }
}
