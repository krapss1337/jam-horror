using UnityEngine;

public abstract class AInteractable : MonoBehaviour
{
    public virtual void Awake()
    {
        gameObject.layer = 6;
    }
    public abstract void OnInteract();
    public abstract void OnFocus();
    public abstract void OnLoseFocus();
}
