using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseNotifier : MonoBehaviour {

    // Show a text
    public abstract void Show(string message);

    // Static show
    public static void ShowInAnyNotifier(string message)
    {
        BaseNotifier notifier = GameObject.FindObjectOfType<BaseNotifier>();
        if (notifier != null)
        {
            notifier.Show(message);
        }
    }

}
