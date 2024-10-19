using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInteractable : AInteractable
{
    public override void OnFocus()
    {
        Debug.Log("Looking At: " + gameObject.name);
    }

    public override void OnInteract()
    {
        Debug.Log("Interacted With: " + gameObject.name);
    }

    public override void OnLoseFocus()
    {
        Debug.Log("Stopped Looking At: " + gameObject.name);
    }
}
