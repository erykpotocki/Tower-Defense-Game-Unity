using UnityEngine;

public class DamageStartLineGizmo : MonoBehaviour
{
    [Header("Damage Start Line")]
    [SerializeField] private float xPosition = 8.8f;
    [SerializeField] private float yMin = -5f;
    [SerializeField] private float yMax = 5f;

    [Header("Visual")]
    [SerializeField] private Color lineColor = Color.yellow;
    [SerializeField] private float sphereSize = 0.15f;

    private void OnDrawGizmos()
    {
        Gizmos.color = lineColor;

        Vector3 bottom = new Vector3(xPosition, yMin, 0f);
        Vector3 top = new Vector3(xPosition, yMax, 0f);

        Gizmos.DrawLine(bottom, top);
        Gizmos.DrawSphere(bottom, sphereSize);
        Gizmos.DrawSphere(top, sphereSize);
    }
}