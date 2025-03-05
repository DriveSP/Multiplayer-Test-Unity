using ED.SC;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using UnityEngine;
using Unity.Services.Authentication;
using TMPro;

public class UIScript : MonoBehaviour
{
    public string playerName { get; private set; }
    public GameObject lobby, playerPanel, lobbyPanel, hostPanel;
    public TextMeshProUGUI title, textField;

    private Lobby hostLobby;
    private Lobby joinedLobby;

    public TextMeshProUGUI playerTestText;

    public void PlayerName(string nPlayerName)
    {
        ChangePlayerName(nPlayerName);
    }

    public void ChangePlayerName(string nPlayerName)
    {
        this.playerName = nPlayerName;
        Debug.Log(nPlayerName);
    }

    public void ChangeName()
    {
        textField.text = "";
        Debug.Log("Name changed");
    }

    public void EnterLobby()
    {
        playerPanel.SetActive(false);
        lobby.SetActive(true);
        lobbyPanel.SetActive(true);
        title.text = "Lobbies List";
    }

    public async void CreateLobby()
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

            lobbyPanel.SetActive(false);
            hostPanel.SetActive(true);
            title.text = lobbyName;
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
                playerTestText.text = player.Data["PlayerName"].Value;
            }


        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
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

    public async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            hostPanel.SetActive(false);
            lobbyPanel.SetActive(true);
            title.text = "Lobbies List";
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
}
