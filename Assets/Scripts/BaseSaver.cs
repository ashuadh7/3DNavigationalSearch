using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseSaver : MonoBehaviour
{

    // Coroutine to save
    public abstract IEnumerator Save();

}
