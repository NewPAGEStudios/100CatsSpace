using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.SceneManagement;
using System;
using Unity.VisualScripting;
public class CustomNetworkManager : NetworkManager
{
    public static CustomNetworkManager instance;

    [SerializeField] private PlayerController GamePlayerPrefab;

    public List<PlayerController> GamePlayers { get; } = new List<PlayerController>();


    [HideInInspector] public string nextOfflineScene = "MainMenu";
    public override void Awake()
    {
        base.Awake();

        if (instance != null && instance != this)
        {
            Debug.LogWarning("Ýkinci bir NetworkManager bulundu ve yok edildi.");
            Destroy(this.gameObject);
            return;
        }


        instance = this;
    }


    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {

        PlayerController GamePlayerInstance = Instantiate(GamePlayerPrefab);



        GamePlayerInstance.connectionID = conn.connectionId;
        GamePlayerInstance.playerID = GamePlayers.Count + 1;
        GamePlayerInstance.PlayerSteamID = (ulong)SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)SteamLobby.instance.CurrentLobbyID, GamePlayers.Count);


        NetworkServer.AddPlayerForConnection(conn, GamePlayerInstance.gameObject);
        
    }
    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        // Eðer host þu anda LobbyScene’de deðilse, yeni baðlantýyý kes
        if (SceneManager.GetActiveScene().name != "LobbyScene")
        {
            Debug.Log($"Reddedilen baðlantý {conn.connectionId}: Host lobbyde deðil.");
            conn.Disconnect();
            return;
        }

        // Aksi halde normal iþlemleri devam ettir
        base.OnServerConnect(conn);
    }
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        PlayerController player = conn.identity.GetComponent<PlayerController>();
        if (GamePlayers.Contains(player))
        {
            GamePlayers.Remove(player);
        }

        base.OnServerDisconnect(conn);
    }

    // Host için çaðrý (1. oyuncu)
    public void StopHostAndGo(string sceneName)
    {
        nextOfflineScene = sceneName;
        StopHost();
    }
    // Client için çaðrý (2. oyuncu)
    public void StopClientAndGo(string sceneName)
    {
        nextOfflineScene = sceneName;
        StopClient();
    }

    public override void OnStopHost()
    {
        base.OnStopHost();
        GamePlayers.Clear();

    }
    public override void OnStopClient()
    {
        base.OnStopClient();
    }


    public void StartGame(string SceneName)
    {
        ServerChangeScene(SceneName);
    }

}
