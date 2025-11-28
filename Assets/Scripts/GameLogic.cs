using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class GameLogic : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] private NetworkObject playerPrefab;
    [Networked, Capacity(12)] private NetworkDictionary<PlayerRef, Player> Players => default;

    public void PlayerJoined(PlayerRef player){
        if(!HasStateAuthority){return;}

        // Spawn the player prefab and cache the networked behaviour for quick access
        NetworkObject playerObject = Runner.Spawn(playerPrefab, Vector3.up, Quaternion.identity, player);
        Players.Add(player, playerObject.GetComponent<Player>());
        
    }

    public void PlayerLeft(PlayerRef player){
        if(!HasStateAuthority){return;}
        if(Players.TryGet(player, out Player playerBehaviour)){
            Players.Remove(player);
            Runner.Despawn(playerBehaviour.Object);
        }
    }
}
