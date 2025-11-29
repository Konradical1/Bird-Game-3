using System.Collections;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tracks lobby ready states and triggers the network-wide transition to the gameplay scene once everyone is ready.
/// Attach this to a NetworkObject that exists in the lobby scene.
/// </summary>
public class LobbyManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] private string gameplayScenePath = "Assets/Scenes/Game.unity";
    [SerializeField] private float startDelaySeconds = 1.5f;

    [Networked, Capacity(12)] private NetworkDictionary<PlayerRef, NetworkBool> ReadyPlayers => default;

    private Coroutine _startRoutine;

    public override void Spawned()
    {
        base.Spawned();

        if (!HasStateAuthority)
            return;

        foreach (var player in Runner.ActivePlayers)
        {
            if (!ReadyPlayers.ContainsKey(player))
            {
                ReadyPlayers.Add(player, false);
            }
        }
    }

    public void PlayerJoined(PlayerRef player)
    {
        if (!HasStateAuthority)
            return;

        if (!ReadyPlayers.ContainsKey(player))
        {
            ReadyPlayers.Add(player, false);
        }

        CancelStartRoutine();
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (!HasStateAuthority)
            return;

        if (ReadyPlayers.ContainsKey(player))
        {
            ReadyPlayers.Remove(player);
        }

        CancelStartRoutine();
        EvaluateReadiness();
    }

    /// <summary>
    /// Called by local UI to flip the ready state for the active player.
    /// </summary>
    public void RequestSetReady(bool ready)
    {
        if (!Object || !Runner)
            return;

        RPC_SetReady(ready);
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority)]
    private void RPC_SetReady(NetworkBool ready, RpcInfo info = default)
    {
        if (!ReadyPlayers.ContainsKey(info.Source))
        {
            ReadyPlayers.Add(info.Source, ready);
        }
        else
        {
            ReadyPlayers.Set(info.Source, ready);
        }

        EvaluateReadiness();
    }

    private void EvaluateReadiness()
    {
        if (!HasStateAuthority || Runner == null)
            return;

        foreach (var player in Runner.ActivePlayers)
        {
            if (!ReadyPlayers.TryGet(player, out var isReady) || !isReady)
            {
                CancelStartRoutine();
                return;
            }
        }

        if (_startRoutine == null)
        {
            _startRoutine = StartCoroutine(BeginGameAfterDelay());
        }
    }

    private IEnumerator BeginGameAfterDelay()
    {
        yield return new WaitForSeconds(startDelaySeconds);

        if (!HasStateAuthority || Runner == null)
            yield break;

        var sceneManager = Runner.GetComponent<NetworkSceneManagerDefault>();
        if (sceneManager)
        {
            var sceneRef = sceneManager.GetSceneRef(gameplayScenePath);
            sceneManager.LoadScene(sceneRef, LoadSceneMode.Single);
        }
        else
        {
            Runner.SetActiveScene(gameplayScenePath);
        }
    }

    private void CancelStartRoutine()
    {
        if (_startRoutine == null)
            return;

        StopCoroutine(_startRoutine);
        _startRoutine = null;
    }
}
