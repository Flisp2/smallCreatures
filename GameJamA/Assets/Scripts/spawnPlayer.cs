using UnityEngine;

public class spawnPlayer : MonoBehaviour
{
    public GameObject playerPrefab;
    public Vector2 spawnPosition = new Vector2(0f, 0f);

    private void Start()
    {
        // Yield to VesselGenerator when present — it spawns the player at the vessel root
        if (FindFirstObjectByType<VesselGenerator>() != null) return;
        Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
    }
}
