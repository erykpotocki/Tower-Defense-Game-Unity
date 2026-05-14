using UnityEngine;

[ExecuteAlways]
public class CameraAnchor2D : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    [Header("Pozycja na ekranie")]
    [Range(0f, 1f)]
    [SerializeField] private float viewportX = 0.12f;

    [Range(0f, 1f)]
    [SerializeField] private float viewportY = 0.5f;

    [Header("Dodatkowe przesunięcie")]
    [SerializeField] private Vector2 worldOffset;

    private void Update()
    {
        AnchorToCamera();
    }

    private void OnValidate()
    {
        AnchorToCamera();
    }

    private void AnchorToCamera()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
            return;

        float distanceFromCamera = transform.position.z - targetCamera.transform.position.z;

        Vector3 worldPosition = targetCamera.ViewportToWorldPoint(
            new Vector3(viewportX, viewportY, distanceFromCamera)
        );

        transform.position = new Vector3(
            worldPosition.x + worldOffset.x,
            worldPosition.y + worldOffset.y,
            transform.position.z
        );
    }
}