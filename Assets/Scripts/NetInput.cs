using Fusion;
using UnityEngine;

public struct NetInput : INetworkInput
{
    public NetworkButtons Buttons;
    public Vector2 MoveDirection; // WASD
    public float VerticalInput;   // Space = 1, Ctrl = -1
    public float CameraPitch;     // Up/Down angle
    public float CameraYaw;       // Left/Right angle
}