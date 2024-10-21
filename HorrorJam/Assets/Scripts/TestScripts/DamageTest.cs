using UnityEngine;

public class DamageTest : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            PlayerController.OnTakeDamage(15f);
        }
    }
}
