using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpotlight : MonoBehaviour
{
    // Player
    private GameObject player;

    // Use this for initialization
    void Start()
    {
        player = null;
    }

    void Update()
    {
        if (player == null)
        {
            // Get camera
            BaseCamera baseCamera = GameObject.FindObjectOfType<BaseCamera>();
            // Get player
            player = baseCamera == null ? null : baseCamera.GetPlayer();
        }
        if (player != null)
        {
            // Attach to player
            this.transform.position = player.transform.position;
            this.transform.forward = player.transform.forward;
        }
    }

}
