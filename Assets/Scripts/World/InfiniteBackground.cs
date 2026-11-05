
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class InfiniteBackground : MonoBehaviour
{
    public Transform targetCamera;
    [Range(0f, 1f)]
    public float parallaxFactor = 0.3f;

    private float spriteWidth;
    private float lastCameraX;

    private void Start()
    {
        spriteWidth = GetComponent<SpriteRenderer>().bounds.size.x;

        if (targetCamera == null && Camera.main != null)
            targetCamera = Camera.main.transform;

        if (targetCamera != null)
            lastCameraX = targetCamera.position.x;
    }

    private void LateUpdate()
    {
        if (targetCamera == null || spriteWidth <= 0f)
            return;

        float cameraX = targetCamera.position.x;
        float deltaX = cameraX - lastCameraX;

        transform.position += Vector3.right * deltaX * parallaxFactor;
        lastCameraX = cameraX;

        float distance = cameraX - transform.position.x;

        if (distance > spriteWidth)
            transform.position += Vector3.right * spriteWidth * 2f;
        else if (distance < -spriteWidth)
            transform.position -= Vector3.right * spriteWidth * 2f;
    }
}
