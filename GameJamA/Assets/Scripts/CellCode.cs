using UnityEngine;

public class CellCode : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerCode playerCode = collision.GetComponent<PlayerCode>();
            if (playerCode != null)
            {
                playerCode.TakeDamage(1f);
            }
        }
    }
}
