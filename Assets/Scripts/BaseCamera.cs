using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCamera : BaseComponent {

    // Get player camera (usually the camera main if 1st person camera, otherwise, another character if 3rd person camera)
    public abstract GameObject GetPlayer();

    // Get play area (the virtual frame travelling with player)
    public abstract GameObject GetPlayArea();

    // Player system (usually, the parent of play area)
    public abstract GameObject GetPlayerSystem();
    
}
