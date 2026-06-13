using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform target;
    [SerializeField] private bool useStartingOffset = true;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float smoothTime = 0.12f;

    private Vector3 velocity;

    private void Awake()
    {
        if (target == null)
            target = transform;

        ResolveCamera();

        if (useStartingOffset && targetCamera != null)
            offset = targetCamera.transform.position - target.position;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        ResolveCamera();

        if (targetCamera == null)
            return;

        Vector3 targetPosition = target.position + offset;
        Transform cameraTransform = targetCamera.transform;

        if (smoothTime <= 0f)
        {
            cameraTransform.position = targetPosition;
            return;
        }

        cameraTransform.position = Vector3.SmoothDamp(
            cameraTransform.position,
            targetPosition,
            ref velocity,
            smoothTime);
    }

    private void ResolveCamera()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }
}
