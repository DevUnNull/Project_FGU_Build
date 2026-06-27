using UnityEngine;

public class DestroyUnitTrigger : MonoBehaviour
{
    public static bool isOverDestroyZone = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isOverDestroyZone = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isOverDestroyZone = false;
        }
    }

}
