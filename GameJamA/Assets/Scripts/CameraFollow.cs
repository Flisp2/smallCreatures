using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.25f;

    Vector3 _vel;

    void LateUpdate()
    {
        if (target == null) 
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                target = player.transform;
                if (target == null) return; // Still can't find player, skip this frame
        }
        Vector3 goal = new Vector3(target.position.x, target.position.y, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, goal, ref _vel, smoothTime);
    }
}
