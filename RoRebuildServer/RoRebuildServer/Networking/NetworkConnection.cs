﻿using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using RebuildSharedData.Enum;
using RoRebuildServer.Database.Requests;
using RoRebuildServer.EntityComponents;
using RoRebuildServer.EntitySystem;

namespace RoRebuildServer.Networking;

public enum ConnectionStatus
{
    PendingAuthentication,
    Connected,
    Disconnected,
}

public enum ActiveDbAction
{
    None,
    CreateParty
}

public class NetworkConnection
{
    public WebSocket Socket { get; set; }
    public ConnectionStatus Status { get; set; }
    public Entity Entity;
    public WorldObject? Character { get; set; }
    public Player? Player { get; set; }
    public int AccountId { get; set; }
    public string AccountName { get; set; }
    public float LoginTime { get; set; }
    public double LastKeepAlive { get; set; }
    public bool Confirmed { get; set; } = false;
    public CancellationToken Cancellation { get; set; }
    public CancellationTokenSource CancellationSource { get; set; }
    public LoadCharacterRequest? LoadCharacterRequest { get; set; }
    public StorageLoadRequest? LoadStorageRequest { get; set; }
    public CreatePartyRequest? CreatePartyRequest { get; set; }
    public ActiveDbAction ActiveDbAction { get; set; }

    //when this connection has its entity removed from the world it is no longer alive. Used to prevent queueing removal while the entity is awaiting recycling.
    //this happens because the server may remove the player AND the connection might also queue the removal of the player at the same time.
    public bool IsAlive; 

    public NetworkConnection(WebSocket socket)
    {
        Socket = socket;
        AccountName = "[Account Not Loaded]";
        CancellationSource = new CancellationTokenSource();
        Cancellation = CancellationSource.Token;
        ActiveDbAction = ActiveDbAction.None;
        IsAlive = true;
    }

    public bool IsConnected => Status == ConnectionStatus.Connected;
    public bool IsConnectedAndInGame => IsConnected && Character?.IsActive == true && Character?.Map != null;
    public bool IsAdmin => Player?.IsAdmin == true;
    public bool IsOnlineAdmin => IsAdmin && IsConnectedAndInGame;
    public bool IsPlayerAlive => IsConnectedAndInGame && Character!.State != CharacterState.Dead;
}