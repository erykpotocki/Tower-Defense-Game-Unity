using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class FitBackgroundToCamera : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    private void Start()
    {
        FitToCamera();
    }

    private void Update()
    {
        FitToCamera();
    }

    private void OnValidate()
    {
        FitToCamera();
    }

    private void FitToCamera()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null || !targetCamera.orthographic)
            return;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer.sprite == null)
            return;

        float cameraHeight = targetCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * targetCamera.aspect;

        float spriteWidth = spriteRenderer.sprite.bounds.size.x;
        float spriteHeight = spriteRenderer.sprite.bounds.size.y;

        float scaleX = cameraWidth / spriteWidth;
        float scaleY = cameraHeight / spriteHeight;

        transform.localScale = new Vector3(scaleX, scaleY, 1f);

        transform.position = new Vector3(
            targetCamera.transform.position.x,
            targetCamera.transform.position.y,
            transform.position.z
        );
    }
}