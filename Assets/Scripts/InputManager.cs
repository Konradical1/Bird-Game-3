using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : SimulationBehaviour, IBeforeUpdate, INetworkRunnerCallbacks
{
    private NetInput accumulatedInput;
    private bool resetInput;

    void IBeforeUpdate.BeforeUpdate()
    {
        if (resetInput)
        {
            resetInput = false;
            accumulatedInput = default;
        }

        // 1. Handle Mouse Locking (Click to lock, Esc to unlock)
        if (Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // 2. Handle Camera Look (Client Side Only)
        if (Cursor.lockState == CursorLockMode.Locked && CameraFollow.Singleton != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            // Send mouse data to CameraFollow to update the camera transform immediately
            CameraFollow.Singleton.AddLookRotation(new Vector2(mouseDelta.y, mouseDelta.x), 0.15f);
        }

        // 3. Accumulate Input for Network
        // Get Camera Angles so the server knows which way is "Forward"
        if (CameraFollow.Singleton != null)
        {
            accumulatedInput.CameraPitch = CameraFollow.Singleton.GetCameraVerticalAngle();
            accumulatedInput.CameraYaw = CameraFollow.Singleton.GetCameraHorizontalAngle();
        }

        // Movement (WASD)
        Vector2 moveInput = Vector2.zero;
        if (Keyboard.current.wKey.isPressed) moveInput += Vector2.up;    // Forward
        if (Keyboard.current.sKey.isPressed) moveInput += Vector2.down;  // Backward
        if (Keyboard.current.aKey.isPressed) moveInput += Vector2.left;  // Left
        if (Keyboard.current.dKey.isPressed) moveInput += Vector2.right; // Right
        
        if (moveInput.sqrMagnitude > 0) 
            accumulatedInput.MoveDirection += moveInput.normalized;

        // Vertical (Space/Ctrl)
        if (Keyboard.current.spaceKey.isPressed) accumulatedInput.VerticalInput = 1f;
        else if (Keyboard.current.ctrlKey.isPressed) accumulatedInput.VerticalInput = -1f;
        else accumulatedInput.VerticalInput = 0f;
    }

    void INetworkRunnerCallbacks.OnInput(NetworkRunner runner, NetworkInput input)
    {
        // Normalize direction so diagonal movement isn't faster
        accumulatedInput.MoveDirection.Normalize();
        
        input.Set(accumulatedInput);
        
        resetInput = true;
    }

    // --- Boilerplate Fusion Callbacks (Unchanged) ---
    void INetworkRunnerCallbacks.OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    void INetworkRunnerCallbacks.OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    void INetworkRunnerCallbacks.OnShutdown(NetworkRunner runner, ShutdownReason reason) { }
    void INetworkRunnerCallbacks.OnConnectedToServer(NetworkRunner runner) { }
    void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    void INetworkRunnerCallbacks.OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    void INetworkRunnerCallbacks.OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    void INetworkRunnerCallbacks.OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    void INetworkRunnerCallbacks.OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    void INetworkRunnerCallbacks.OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    void INetworkRunnerCallbacks.OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    void INetworkRunnerCallbacks.OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey reliableKey, System.ArraySegment<byte> data) { }
    void INetworkRunnerCallbacks.OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey reliableKey, float progress) { }
    void INetworkRunnerCallbacks.OnSceneLoadDone(NetworkRunner runner) { }
    void INetworkRunnerCallbacks.OnSceneLoadStart(NetworkRunner runner) { }
    void INetworkRunnerCallbacks.OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    void INetworkRunnerCallbacks.OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
}