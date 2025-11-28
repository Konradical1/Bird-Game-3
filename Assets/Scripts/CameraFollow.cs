using UnityEngine;
using Fusion; // Required for Player component access

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Singleton{
        get => _singleton;
        set{
            if(value == null)
                _singleton = null;
            else if(_singleton ==null)
                _singleton = value;
            else if(_singleton != value)
            {
                Destroy(value);
                Debug.LogError($"There should only be one instance of {nameof(CameraFollow)}");
            }
        }
    }
    private static CameraFollow _singleton;

    [SerializeField] private float distance = 5f;
    [SerializeField] private float height = 2f;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 60f;
    // NOTE: smoothSpeed is now used directly in SmoothDamp, not as 1/smoothSpeed
    [SerializeField] private float smoothTime = 0.1f; // Use a smoothTime value for camera dampening

    private Transform target;
    private Player targetPlayer; // NEW: Cache the Player component
    private float currentHorizontalAngle;
    private float currentVerticalAngle;
    private Vector3 currentVelocity; // Used for SmoothDamp

    private void Awake(){
        Singleton = this;
    }
    private void OnDestroy(){
        if(Singleton == this)
            Singleton = null;
    }
    
    public void SetTarget(Transform newTarget){
        target = newTarget;
        targetPlayer = newTarget.GetComponent<Player>(); // NEW: Get Player component
        if(target != null){
            // Initialize camera angles based on target's forward direction
            currentHorizontalAngle = target.rotation.eulerAngles.y;
            currentVerticalAngle = 0f;
            currentVelocity = Vector3.zero; // Reset smooth damp velocity
        }
    }

    public void AddLookRotation(Vector2 lookDelta, float multiplier)
    {
        currentHorizontalAngle += lookDelta.y * rotationSpeed * multiplier;
        currentVerticalAngle -= lookDelta.x * rotationSpeed * multiplier;
        currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);

        if (currentHorizontalAngle > 360) currentHorizontalAngle -= 360;
        if (currentHorizontalAngle < 0) currentHorizontalAngle += 360;
    }

    public float GetCameraVerticalAngle(){
        return currentVerticalAngle;
    }

    public float GetCameraHorizontalAngle(){
        return currentHorizontalAngle;
    }

    private void LateUpdate(){
        if(target != null){
            Vector3 targetPosition;
            float currentSmoothTime;

            if (targetPlayer != null && targetPlayer.IsLocalPlayer)
            {
                // JITTER FIX: For the local player, we follow the current transform position 
                // immediately and use the camera's own smoothTime for a clean follow.
                targetPosition = target.position;
                currentSmoothTime = smoothTime;
            }
            else
            {
                // For remote players, Fusion's internal interpolation handles smoothing, 
                // so we snap the camera to the interpolated position instantly (0 smooth time).
                targetPosition = target.position;
                currentSmoothTime = 0f; 
            }
            
            // Calculate desired position using spherical coordinates
            float horizontalRad = currentHorizontalAngle * Mathf.Deg2Rad;
            float verticalRad = currentVerticalAngle * Mathf.Deg2Rad;

            // Calculate offset from player
            float horizontalDistance = distance * Mathf.Cos(verticalRad);
            Vector3 offset = new Vector3(
                Mathf.Sin(horizontalRad) * horizontalDistance,
                Mathf.Sin(verticalRad) * distance + height,
                Mathf.Cos(horizontalRad) * horizontalDistance
            );

            // Calculate the camera's new desired position
            Vector3 desiredPosition = targetPosition + offset;

            // Smoothly move camera to desired position. If currentSmoothTime is 0, it snaps.
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, currentSmoothTime);

            // Look at target with height offset
            Vector3 lookAtPosition = targetPosition + Vector3.up * height;
            transform.LookAt(lookAtPosition);
        }
    }
}