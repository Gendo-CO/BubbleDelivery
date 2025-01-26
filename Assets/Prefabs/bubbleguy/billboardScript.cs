using UnityEngine;

public class BillboardSprite : MonoBehaviour
{
    public Camera mainCamera;
    public bool lockX = false;
    public bool lockY = false;
    public bool lockZ = false;

    void Start()
    {
        // If no camera specified, use the main camera
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        if (mainCamera == null) return;

        // Calculate rotation to face the camera
        /*Vector3 directionToCamera = mainCamera.transform.position - transform.position;

        // Create a rotation looking at the camera
        Quaternion targetRotation = Quaternion.LookRotation(directionToCamera);

        // Lock specific axes if needed
        Vector3 eulerAngles = targetRotation.eulerAngles;
        if (lockX) eulerAngles.x = transform.rotation.eulerAngles.x;
        if (lockY) eulerAngles.y = transform.rotation.eulerAngles.y;
        if (lockZ) eulerAngles.z = transform.rotation.eulerAngles.z;*/

        // Apply the rotation
        transform.rotation = mainCamera.transform.rotation;
    }
}