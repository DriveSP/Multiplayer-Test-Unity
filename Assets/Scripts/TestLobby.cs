using ED.SC;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class TestLobby : MonoBehaviour
{
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartBeatTimer;
    private float lobbyUpdateTimer;
    public UIScript uiScript;
    private string playerName;

    private async void Start()
    {
        playerName = uiScript.playerName;

        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            SmartConsole.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        SmartConsole.Log("Your player name is: "+playerName);
    }

    private void Update()
    {
        HandleHeartBeatLobby();
        HandleUpdateLobby();
    }

    private async void HandleHeartBeatLobby()
    {
        if (hostLobby != null)
        {
            heartBeatTimer -= Time.deltaTime;
            if (heartBeatTimer < 0f)
            {
                float heartBeatTimerMax = 15;
                heartBeatTimer = heartBeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
                Debug.Log("HeartBeat");
            }
        }

    }

    private async void HandleUpdateLobby()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                float lobbyUpdateTimerMax = 1.1f;
                lobbyUpdateTimer = lobbyUpdateTimerMax;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;
                Debug.Log("Lobby updated");
            }
        }

    }

    [Command]
    private async void CreateLobby()
    {
        try
        {
            string lobbyName = "LobbyName";
            int maxPlayers = 4;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayers(),
                Data = new Dictionary<string, DataObject>
                {
                    {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, "CaptureTheFlag") },
                    {"Map", new DataObject(DataObject.VisibilityOptions.Public, "Valhalla") }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            hostLobby = lobby;
            joinedLobby = lobby;
            SmartConsole.Log("Create Lobby! " + lobbyName + " " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode + " " + lobby.Data["GameMode"].Value + " " + lobby.Data["Map"].Value);
            PrintPlayersLobby(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private async void ListLobbies()
    {
        try
        {

            /*QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 15,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots,"0",QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };*/

            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();

            SmartConsole.Log("Lobbies found: " + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                SmartConsole.Log(lobby.Name + " " + lobby.MaxPlayers);
            }
        }
        catch (LobbyServiceException e)
        {  
            Debug.Log(e); 
        } 
    }

    [Command]
    private async void JoinServer()
    {
        try
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
            
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id);
            SmartConsole.Log("Lobby joined");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private async void JoinServerCode(string lobbyCode)
    {
        try
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();

            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);

            SmartConsole.Log("Lobby joined with code: " + lobbyCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private async void QuickJoinServer()
    {
        try
        {
            QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions
            {
                Player = GetPlayers()
            };
            Lobby lobbyJoined = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
            joinedLobby = lobbyJoined;
            SmartConsole.Log("Joined quickly");

            PrintPlayersLobby(lobbyJoined);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private void PrintPlayersLobby(Lobby lobby)
    {
        try
        {
            SmartConsole.Log("Players in lobby: " + lobby.Name + " " + lobby.Data["GameMode"].Value + " " + lobby.Data["Map"].Value);
            foreach (Player player in lobby.Players)
            {
                SmartConsole.Log("Player: " + player.Id + " Name: " + player.Data["PlayerName"].Value);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private void PrintPlayers()
    {
        PrintPlayersLobby(joinedLobby);
    }

    private Player GetPlayers()
    {
        return new Player(AuthenticationService.Instance.PlayerId)
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName", new PlayerDataObject (PlayerDataObject.VisibilityOptions.Member, playerName) }
                    }
        };
    }

    [Command]
    private async void UpdateLobbyGameMode(string gameMode)
    {
        try
        {
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions {
                Data = new Dictionary<string, DataObject>
                {
                    {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode) }
                }
            });
            joinedLobby = hostLobby;
            PrintPlayersLobby(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    [Command]
    private async void UpdatePlayerName(string playerName)
    {
        try
        {
            hostLobby = await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    {"PlayerName", new PlayerDataObject (PlayerDataObject.VisibilityOptions.Member, playerName)}
                }
            });
            joinedLobby = hostLobby;
            PrintPlayersLobby(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private async void UpdateHost()
    {
        try
        {
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = joinedLobby.Players[1].Id,
            });
            joinedLobby = hostLobby;
            PrintPlayersLobby(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private async void KickLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[1].Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

}