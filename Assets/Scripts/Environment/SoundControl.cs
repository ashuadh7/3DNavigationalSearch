using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundControl : MonoBehaviour {

    // Warning audio source
    private AudioSource warningSound;
    // Collecting audio source
    private AudioSource collectingSound;

    // Use this for initialization
    void Start ()
    {
        // Get game manager
        GameManager gameManager = GameObject.FindObjectOfType<GameManager>();

        // Get audio source
        warningSound = GameObject.Find("Warning Sound").GetComponent<AudioSource>();
        collectingSound = GameObject.Find("Collecting Sound").GetComponent<AudioSource>();

        // Listen to player in unsafe area event
        gameManager.OnPlayerInUnsafeArea += delegate (GameManager.GamePlayEventArgs e) 
        {
            if (!warningSound.isPlaying)
            {
                warningSound.Play();
            }
        };

        // Listen to player in safe area event
        gameManager.OnPlayerInSafeArea += delegate (GameManager.GamePlayEventArgs e)
        {
            if (warningSound.isPlaying)
            {
                warningSound.Stop();
            }
        };

        // Setop warning sound when game stops
        gameManager.OnPlayStateFinishing += delegate (GameManager.GamePlayEventArgs e)
        {
            if (warningSound.isPlaying)
            {
                warningSound.Stop();
            }
        };

        // Listen to collecting core event
        gameManager.OnPlayerCollectingTarget += delegate (GameManager.GamePlayEventArgs e)
        {
            collectingSound.Play();
        };
    }

}
