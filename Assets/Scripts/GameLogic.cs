using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class GameLogic : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] private NetworkObject playerPrefab;
    [Networked, Capacity(12)] private NetworkDictionary<PlayerRef, Player> Players => default;

    public override void Spawned()
    {
        // When entering the gameplay scene via a network scene transition, the
        // existing players are already part of Runner.ActivePlayers, so the
        // IPlayerJoined callback will not be invoked for them. Ensure they all
        // receive their NetworkObjects (and thus InputAuthority) as soon as the
        // GameLogic spawns.
        if (!HasStateAuthority)
            return;

        foreach (var player in Runner.ActivePlayers)
        {
            SpawnPlayerFor(player);
        }
    }

    public void PlayerJoined(PlayerRef player){
        if(!HasStateAuthority){return;}

        SpawnPlayerFor(player);

    }

    public void PlayerLeft(PlayerRef player){
        if(!HasStateAuthority){return;}
        if(Players.TryGet(player, out Player playerBehaviour)){
            Players.Remove(player);
            Runner.Despawn(playerBehaviour.Object);
        }
    }

    private void SpawnPlayerFor(PlayerRef player)
    {
        if (Players.ContainsKey(player))
            return;

        // Spawn the player prefab and cache the networked behaviour for quick access
        NetworkObject playerObject = Runner.Spawn(playerPrefab, Vector3.up, Quaternion.identity, player);
        Players.Add(player, playerObject.GetComponent<Player>());
    }
}
