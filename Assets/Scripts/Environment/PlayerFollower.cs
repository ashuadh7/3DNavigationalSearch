using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollower : MonoBehaviour
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
            // Follow player
            this.transform.position = new Vector3(
            player.transform.position.x,
            this.transform.position.y,
            player.transform.position.z
        );
        }
    }

}
