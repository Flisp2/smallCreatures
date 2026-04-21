using UnityEngine;
using UnityEngine.InputSystem;

public class lookatMouse : MonoBehaviour
{
    private void Update()
    {
        var ms = Mouse.current;
        if (ms == null) return;
        Vector3 mousePos = ms.position.ReadValue();
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0f));
        Vector3 direction = worldMousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

    }
}
