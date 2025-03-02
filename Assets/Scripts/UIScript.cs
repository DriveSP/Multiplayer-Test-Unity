using UnityEngine;

public class UIScript : MonoBehaviour
{
    public string playerName { get; private set; }
    public GameObject lobby, uiPanel;

    public void PlayerName(string nPlayerName)
    {
        this.playerName = nPlayerName;
        Debug.Log(nPlayerName);
    }

    public void EnterLobby()
    {
        uiPanel.SetActive(false);
        lobby.SetActive(true);
    }
}
