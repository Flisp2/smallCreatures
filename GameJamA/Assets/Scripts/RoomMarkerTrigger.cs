using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomMarkerTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        SceneManager.LoadScene("CellScene");
    }
}
