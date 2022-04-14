using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoCanvas : BaseNotifier
{

    // Get text
    private Text text;

    // Use this for initialization
    void Start () {
        text = GetComponentInChildren<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Camera.main != null)
        {
            // Follow main camera
            this.transform.position = Camera.main.transform.position
                + Camera.main.transform.forward.normalized * 0.5f
                - Camera.main.transform.up * 0.12f;
            this.transform.rotation = Camera.main.transform.rotation;
        }
    }

    // Show notification
    public override void Show(string message)
    {
        text.text = message;
    }
}
