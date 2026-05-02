using UnityEngine;
using UnityEngine.InputSystem;

public class WorldData : MonoBehaviour
{
    public static int currentLevel = 1;
    public static GameObject Death;
    public static GameObject Pause;

    private VesselGenerator vesselGen;
    void Awake()
    {
        Death = GameObject.Find("Death");
        Pause = GameObject.Find("Pause");
        vesselGen = GetComponent<VesselGenerator>();
        if (vesselGen == null)
        {
            return;
        }
        if (Death != null && Pause != null)
        {
            Death.SetActive(false);
            Pause.SetActive(false);
        }
        SetLevelGen();
    }

    public static void DeathScreen()
    {
        Debug.Log("Player has died. Showing death screen.");
        if (Death != null && Pause != null)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Death.SetActive(true);
            Pause.SetActive(true);
        } else
        {
            Debug.LogWarning("Death or Pause GameObject is not assigned in WorldData.");
        }
    }

    public void SetLevelGen()
    {
        if (vesselGen == null) return; // Don't overwrite if already set in editor

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        vesselGen.seed = System.DateTime.Now.Millisecond + currentLevel * 1000;
        vesselGen.maxDepth = 3 + currentLevel;
        vesselGen.minSegmentLength = 10f * (1f - currentLevel * 0.05f);
        vesselGen.maxSegmentLength = 50f * (1f - currentLevel * 0.05f);
        vesselGen.branchAngle = 30f - currentLevel * 2f;
        vesselGen.branchAngleVariance = 25f - currentLevel * 1f;
        vesselGen.maxWBCs = 0 + currentLevel * 2;
        vesselGen.GenerateNew();
    }

    public void ResetLevel()
    {
        currentLevel = 1;
        Time.timeScale = 1f;
        if (GameObject.FindWithTag("Player") != null)
        {
            GameObject.FindWithTag("Player").GetComponent<PlayerCode>().enabled = true;
        }
        if (Death != null && Pause != null)
        {
            Death.SetActive(false);
            Pause.SetActive(false);
        }
        SetLevelGen();
    }
}


