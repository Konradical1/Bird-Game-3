using UnityEngine;
using Fusion;

[RequireComponent(typeof(Rigidbody))]
public class Player : NetworkBehaviour
{
    [Header("Settings")]
    public float flySpeed = 20f;
    public float acceleration = 4f; 
    public float deceleration = 2f; 
    public float turnSpeed = 10f;

    private Rigidbody rb;

    public bool IsLocalPlayer => HasInputAuthority && Object.HasStateAuthority == false;

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody>();
        
        rb.useGravity = false;
        rb.drag = 1f; 
        rb.angularDrag = 2f;

        if (HasInputAuthority)
        {
            CameraFollow.Singleton.SetTarget(transform);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetInput input))
        {
            Move(input);
        }
    }

    private void Move(NetInput input)
    {
        // 1. Reconstruct Camera Rotation
        // FIX: Negate CameraPitch to correct the inverted vertical movement logic
        Quaternion cameraRotation = Quaternion.Euler(-input.CameraPitch, input.CameraYaw, 0f);

        // 2. Determine Movement Direction
        Vector3 moveDir = Vector3.zero;

        // W maps to Camera Forward (Vector3.back relative to camera angle)
        moveDir += cameraRotation * Vector3.back * input.MoveDirection.y;

        // A/D maps to Camera Left/Right (Vector3.left relative to camera angle)
        moveDir += cameraRotation * Vector3.left * input.MoveDirection.x;

        // Vertical (Space/Ctrl) - Absolute World Up/Down
        moveDir += Vector3.up * input.VerticalInput;

        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        // 3. Apply Velocity with Drift
        Vector3 targetVelocity = moveDir * flySpeed;
        float currentAccel = (moveDir.sqrMagnitude > 0.01f) ? acceleration : deceleration;
        
        rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, Runner.DeltaTime * currentAccel);

        // 4. Rotate Bird to face movement direction
        if (moveDir.sqrMagnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRot, Runner.DeltaTime * turnSpeed);
        }
    }
}