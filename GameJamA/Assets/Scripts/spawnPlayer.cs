using UnityEngine;

public class spawnPlayer : MonoBehaviour
{
    public GameObject playerPrefab;
    public Vector2 spawnPosition = new Vector2(0f, 0f);

    private void Start()
    {
        Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
    }
}
