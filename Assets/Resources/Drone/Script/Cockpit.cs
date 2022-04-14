using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cockpit : MonoBehaviour {
    private GameObject drone;
	private Vector3 correction;
	// Use this for initialization
	void Start () 
	{
		drone = GameObject.FindGameObjectWithTag("Drone");
		GameManager gameManager = GameObject.FindObjectOfType<GameManager>();
		if (gameManager.scaleSize == 1)
		{
			correction = new Vector3(0,-.4f,0);
		}
		else
		{
			correction = new Vector3 (0,-1f,0);
		}
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		transform.position = drone.transform.position + correction;		
	}
}
