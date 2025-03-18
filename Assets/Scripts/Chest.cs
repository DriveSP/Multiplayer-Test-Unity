using UnityEngine;

public class Chest : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Colisión");
            GetComponent<LootBag>().InstantiateLoot(transform.position);
        }
    }
}
