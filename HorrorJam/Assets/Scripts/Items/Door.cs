using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : AInteractable
{
    private bool isOpen = false;
    private bool interactPossible = true;
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public override void OnFocus()
    {
        
    }

    public override void OnInteract()
    {
        if (interactPossible) { 
            isOpen = !isOpen;

            Vector3 doorTransformDirection = transform.TransformDirection(Vector3.forward);
            Vector3 playerTransformationDirection = PlayerController._instance.transform.position - transform.position;
            float dot = Vector3.Dot(doorTransformDirection, playerTransformationDirection);

            anim.SetFloat("dot", dot);
            anim.SetBool("isOpen", isOpen);
            StartCoroutine(AutoClose());
        }
    }

    public override void OnLoseFocus()
    {
        
    }

    private IEnumerator AutoClose()
    {
        if (isOpen)
        {
            yield return new WaitForSeconds(3);

            if(Vector3.Distance(transform.position, PlayerController._instance.transform.position) > 3){
                isOpen = false;
                anim.SetFloat("dot", 0);
                anim.SetBool("isOpen", isOpen);
            }
        }
    }

    private void Animator_LockInteraction()
    {
        interactPossible = false;
    }
    private void Animator_UnlockInteraction()
    {
        interactPossible = true;
    }
}
