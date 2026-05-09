using UnityEngine;
using UnityEngine.InputSystem;

public class VesselController : MonoBehaviour
{
    public VesselGenerator vesselGenerator;

    private void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            vesselGenerator.GenerateNew();
        }
    }
}
