using UnityEngine;
using UnityEngine.InputSystem;

public class WorldData : MonoBehaviour
{
    public static int currentLevel = 1;

    private VesselGenerator vesselGen;
    void Awake()
    {
        vesselGen = GetComponent<VesselGenerator>();
        SetLevelGen();
    }

    public void SetLevelGen()
    {
        if (vesselGen == null) return; // Don't overwrite if already set in editor

        vesselGen.seed = System.DateTime.Now.Millisecond + currentLevel * 1000;
        vesselGen.maxDepth = 3 + currentLevel;
        vesselGen.minSegmentLength = 10f * (1f - currentLevel * 0.05f);
        vesselGen.maxSegmentLength = 50f * (1f - currentLevel * 0.05f);
        vesselGen.branchAngle = 30f - currentLevel * 2f;
        vesselGen.branchAngleVariance = 25f - currentLevel * 1f;
        vesselGen.maxWBCs = 0 + currentLevel * 2;
        vesselGen.GenerateNew();
    }

    void Update()
    {
        if (Keyboard.current.nKey.wasPressedThisFrame)
        {
            currentLevel++;
            Debug.Log("Advancing to level " + currentLevel);
            SetLevelGen();
        }
    }
}


