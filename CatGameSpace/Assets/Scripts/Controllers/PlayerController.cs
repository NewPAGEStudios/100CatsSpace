using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PlayerController : NetworkBehaviour
{
    // Data
    [SyncVar] public int connectionID;
    [SyncVar] public int playerID;
    [SyncVar] public ulong PlayerSteamID;

    [SyncVar(hook = nameof(PlayerNameUpdate))] public string PlayerName;

    [SyncVar(hook = nameof(PlayerReadyUpdate))] public bool PlayerReady;
    [SyncVar(hook = nameof(PlayerColorUpdate))] public Color32 PlayerColor;

    public GameObject cursorPrefab;


    GameController gameController;
    ColourControll colourControll;
    CatFindController catFindController;
    InputManager inputManager;
    CameraZoom cameraZoom;

    [SyncVar]
    private Vector3 syncedPos = Vector3.zero;



    private bool WorkUpdate = false;
    public CursorDisplay cursorDisplay;

    private CustomNetworkManager manager;

    private CustomNetworkManager Manager
    {
        get
        {
            if (manager != null) return manager;
            return manager = CustomNetworkManager.singleton as CustomNetworkManager;
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
    }

    private void Start()
    {
        OnSceneLoad(SceneManager.GetActiveScene(), LoadSceneMode.Single);

        if (isLocalPlayer)
        {
            inputManager = InputManager.Instance;
        }

    }

    

    private void OnSceneLoad(Scene loadedScene, LoadSceneMode loadSceneMode)
    {
        if (loadedScene.name == "OnlineGamePlay")
        {
            cursorDisplay = Instantiate(cursorPrefab, GameObject.Find("Canvas").transform).GetComponent<CursorDisplay>();
            cursorDisplay.ownedPlayer = this;
            cursorDisplay.changeColor(PlayerColor);


            Cursor.visible = false;

            WorkUpdate = true;

            inputManager = InputManager.Instance;
            gameController = GameController.Instance;
            colourControll = ColourControll.Instance;
            catFindController = CatFindController.Instance;
            cameraZoom = CameraZoom.Instance;


            if (isLocalPlayer && isServer)
            {
                StartCoroutine(loadDataCall());
            }


        }
        else if(loadedScene.name == "LobbyScene")
        {
            cursorDisplay = Instantiate(cursorPrefab, GameObject.Find("Canvas").transform).GetComponent<CursorDisplay>();
            cursorDisplay.ownedPlayer = this;
            cursorDisplay.changeColor(this.PlayerColor);



            Cursor.visible = false;
            WorkUpdate = false;
        }
        else if(loadedScene.name == "Menu")
        {
        }

    }

    private void Update()
    {
        if (!NetworkClient.ready) return;

        if (isLocalPlayer)
        {
            cursorDisplay.CursorMovement(getMousePosOnUI());
            Vector3 vp = Camera.main.ScreenToWorldPoint(inputManager.cursorPosition());
            CmdSetCursorPos(vp);
        }
        else
        {
            cursorDisplay.CursorMovementNonLocal(syncedPos);
            return;
        }
        if (!WorkUpdate)
        {
            return;
        }

        if (gameController.gameState == StateManager.GameState.InGame)
        {

            if (inputManager.mainInputTriggered)
            {
                inputManager.mainInputTriggered = false;
                MainInterract();
            }


            if (inputManager.getPan() && !EventSystem.current.IsPointerOverGameObject()) cameraZoom.panCam(inputManager.cursorPosition());
            else cameraZoom.stopPanCam();

            if (inputManager.CamMovement() != Vector2.zero) cameraZoom.MoveCam(inputManager.CamMovement());

            if (inputManager.zoomAction() != 0 && !EventSystem.current.IsPointerOverGameObject()) cameraZoom.zoomCam(inputManager.zoomAction());


            if (ColourControll.Instance.colourMenuOpenned && EventSystem.current.IsPointerOverGameObject() && ColourControll.Instance.uniqueColorSelectionPanel.GetComponent<ColorsMenuHandle>().isHovered && inputManager.zoomAction() != 0)
            {
                if (inputManager.zoomAction() > 0) ColourControll.Instance.uniqueColorSelectionPanel.GetComponent<ColorsMenuHandle>().PageUp();
                else ColourControll.Instance.uniqueColorSelectionPanel.GetComponent<ColorsMenuHandle>().PageDown();
            }
        }
        else if (gameController.gameState == StateManager.GameState.InChat)
        {

            if (inputManager.ChatEnterPressed())
            {
                gameController.EnterBtn();
            }
            else if (inputManager.ChatOpenBtnPressed())
            {
                gameController.OpenChatORConsole();
            }
        }
        else if(gameController.gameState == StateManager.GameState.EndGame)
        {
            if (inputManager.getPan() && !EventSystem.current.IsPointerOverGameObject())
            {
                cameraZoom.panCam(inputManager.cursorPosition());
            }
            else
            {
                cameraZoom.stopPanCam();
            }
            if (inputManager.CamMovement() != Vector2.zero)
            {
                cameraZoom.MoveCam(inputManager.CamMovement());
            }


            if (inputManager.zoomAction() != 0 && !EventSystem.current.IsPointerOverGameObject())
            {
                cameraZoom.zoomCam(inputManager.zoomAction());
            }

            if (inputManager.ChatOpenBtnPressed()) gameController.ChangeChatStatus();
        }

    }
    #region InputInterractions
    public void MainInterract()
    {
        if (!NetworkClient.ready) return;
        if (!isLocalPlayer) return;
        if (!WorkUpdate) return;
        if (gameController.gameState != StateManager.GameState.InGame) return;

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Vector2 wp = Camera.main.ScreenToWorldPoint(inputManager.cursorPosition());
        gameController.ClickAction(wp);
    }
    public void ThirdInterract()
    {
        if (!NetworkClient.ready) return;
        if (!isLocalPlayer) return;
        if (!WorkUpdate) return;
        if (gameController.gameState != StateManager.GameState.InGame) return;

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        Vector2 wp = Camera.main.ScreenToWorldPoint(inputManager.cursorPosition());
        gameController.ThirdClickAction(wp);

    }
    public void SecondaryInterract()
    {
        if (!NetworkClient.ready) return;
        if (!isLocalPlayer) return;
        if (!WorkUpdate) return;
        if (gameController.gameState != StateManager.GameState.InGame) return;

        gameController.SecClickAction();
    }
    public void DisplayHoldOnDisplay(float currentTime, float HoldTime)
    {
        if (!NetworkClient.ready) return;
        if (!isLocalPlayer) return;
        if (!WorkUpdate) return;
        if (gameController.gameState != StateManager.GameState.InGame) return;

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (GameController.Instance.gameMode != StateManager.GameMode.fillColor) return;

        cursorDisplay.DisplayTimer((HoldTime - currentTime) / HoldTime);
    }
    public void CloseHoldOnDisplay()
    {
        if (!NetworkClient.ready) return;
        if (!isLocalPlayer) return;
        if (!WorkUpdate) return;
        if (gameController.gameState != StateManager.GameState.InGame) return;


        cursorDisplay.CloseTimer();
    }


    #endregion



    #region ColorScene
    IEnumerator loadDataCall()
    {
        yield return new WaitForSeconds(2f);
        while (true)
        {
            if(gameController.catFinderReady && gameController.colourControllerReady)
            {
                break;
            }
            yield return null;
        }
        RpcLoadDataBum(GameDataTracker.paint_regionFillData, GameDataTracker.paint_findCatData, GameDataTracker.paint_finished, GameDataTracker.paint_timer);
    }
    IEnumerator loadingDataFromHost(bool[] dataReg, bool[] dataCat, bool finished, float t)
    {
        while (true)
        {
            if (gameController != null && inputManager != null && colourControll != null && catFindController != null)
            {
                break;
            }
            yield return null;
        }
        GameController.Instance.timer = t;
        ColourControll.Instance.LoadData(dataReg);
        CatFindController.Instance.LoadData(dataCat);
    }

    [Command]
    public void CmdClickAction(int Itemid, int Modeid,Vector2 wp)
    {
        RpcClickAction(Itemid,Modeid,wp);
    }

    [ClientRpc(includeOwner = false)]
    public void RpcClickAction(int Itemid,int Modeid,Vector2 wp)
    {
        switch (Modeid)
        {
            case (int)StateManager.GameMode.fillColor:
                colourControll.ForcePaintRegion(Itemid,wp);
                break;
            case (int)StateManager.GameMode.findCat:
                catFindController.ForceFindCat(Itemid);
                break;
        }
    }


    [ClientRpc(includeOwner = false)]
    public void RpcLoadDataBum(bool[] data00, bool[] data01,bool finished, float timer)
    {
        StartCoroutine(loadingDataFromHost(data00, data01, finished, timer));
    }

    [Command]
    public void CMDColorFillModeOpen()
    {
        Debug.Log("lOG");
        RpcColorFillModeOpen();

    }
    [ClientRpc(includeOwner = false)]
    public void RpcColorFillModeOpen()
    {
        Debug.Log("Üf");
        gameController.ColorFillModeOpen();
    }
    [ClientRpc(includeOwner = false)]
    public void RpcFindCatModeOpen()
    {
        gameController.CatFindModeOpen();
    }

    [Command]
    public void CmdCallHintDataChange()
    {
        RpcHintDataChangeUpdate(GameDataTracker.paint_hint);

    }
    [ClientRpc(includeOwner = false)]
    public void RpcHintDataChangeUpdate(int data)
    {
        GameDataTracker.paint_hint = data;
        gameController.HintTxtUpdate();
    }


    [Command]
    public void CmdHintDataRequest()
    {
        TargetGiveHintData(GameDataTracker.paint_hint);
    }
    [TargetRpc]
    public void TargetGiveHintData(int data)
    {
        GameDataTracker.paint_hint = data;
        gameController.HintTxtUpdate();
    }


    [Command]
    public void CmdHintCall()
    {
        if (GameDataTracker.paint_hint - 1 < 0)
        {
            TargetHintPermissionGive(GameDataTracker.paint_hint, false);
        }
        else
        {
            GameDataTracker.paint_hint -= 1;
            gameController.HintTxtUpdate();
            TargetHintPermissionGive(GameDataTracker.paint_hint, true);
        }
    }
    [TargetRpc]
    public void TargetHintPermissionGive(int data,bool permission)
    {
        if(permission)
        {
            if (gameController.gameMode == StateManager.GameMode.fillColor)
            {
                colourControll.HintPermission = true;
                colourControll.HintDataGot = true;

                GameDataTracker.paint_hint = data;
                gameController.HintTxtUpdate();
            }
            else if (gameController.gameMode == StateManager.GameMode.findCat)
            {
                catFindController.HintPermission = true;
                catFindController.HintDataGot = true;

                GameDataTracker.paint_hint = data;
                gameController.HintTxtUpdate();
            }
        }
        else
        {
            if (gameController.gameMode == StateManager.GameMode.fillColor)
            {
                colourControll.HintPermission = false;
                colourControll.HintDataGot = true;


                GameDataTracker.paint_hint = data;
                gameController.HintTxtUpdate();
            }
            else if(gameController.gameMode == StateManager.GameMode.findCat)
            {
                catFindController.HintPermission = false;
                catFindController.HintDataGot = true;

                GameDataTracker.paint_hint = data;
                gameController.HintTxtUpdate();

            }
        }
    }
    #endregion
    private Vector2 getMousePosOnUI()
    {
        // Ekran pozisyonunu al
        Vector2 localPoint;
        Vector2 vec = inputManager.cursorPosition();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(cursorDisplay.transform.parent.GetComponent<RectTransform>(), vec, Camera.main, out localPoint);
        return localPoint;
    }

    [Command]
    private void CmdSetCursorPos(Vector3 wp)
    {
        syncedPos = wp;
    }
    #region Radio

    [Command]
    public void CmdFetchRadio(NetworkIdentity playerIdentity)
    {
        var Result = RadioController.instance.GetCurrentMusicInfos();
        TargetGiveRadioInfo(playerIdentity.connectionToClient,RadioController.instance.CurrentRadioId,Result.Item1,Result.Item2);
    }
    [TargetRpc]
    public void TargetGiveRadioInfo(NetworkConnection target, int radioNum, string musicName, float sec)
    {
        RadioController.instance.CurrentRadioId = radioNum;
        RadioController.instance.OpenMusic(musicName, sec);
    }
    [ClientRpc(includeOwner = false)]
    public void RpcGiveRadioInfo(int radioNum,string musicName, float sec)
    {
        RadioController.instance.CurrentRadioId = radioNum;
        RadioController.instance.OpenMusic(musicName, sec);
    }

    #endregion

    #region LobbyScene
    public void CanStartGame(string SceneName)
    {
        if (isLocalPlayer)
        {
            CmdCanStartGame(SceneName);
        }
    }
    [Command]
    public void CmdCanStartGame(string SceneName)
    {
        CustomSceneChanger.Instance.ChangeSceneCNM(manager, SceneName);
    }

    [ClientRpc]
    public void RpcGiveEveryoneReplayInfo(List<Vector2> wps,List<int> itemID)
    {
        Debug.Log(wps);
        GameDataTracker.replay_paintPos = wps;
        GameDataTracker.replay_itemID = itemID;
    }
    private void PlayerNameUpdate(string OldValue, string NewValue)
    {
        if (isServer)
        {
            this.PlayerName = NewValue;
        }
        if (isClient)
        {
            LobbyController.instance.UpdatePlayerList();
        }
    }
    private void PlayerReadyUpdate(bool oldValue, bool newValue)
    {
        if (isServer)
        {
            this.PlayerReady = newValue;
        }
        if (isClient)
        {
            if(SceneManager.GetActiveScene().name == "LobbyScene") LobbyController.instance.UpdatePlayerList();
        }
    }

    private void PlayerColorUpdate(Color32 oldValue, Color32 newValue)
    {
        if (isServer)
        {
            this.PlayerColor = newValue;
            cursorDisplay.changeColor(newValue);
            LobbyController.instance.SyncColorBtns(LobbyController.instance.Colors.IndexOf(oldValue), LobbyController.instance.Colors.IndexOf(newValue));
//            LobbyController.instance.SelectPaintIDFeedBackColorUpdate(newValue, this.connectionID);
        }
        if (isClient)
        {
            if (cursorDisplay != null) cursorDisplay.changeColor(newValue);
            LobbyController.instance.UpdatePlayerList();
            LobbyController.instance.SyncColorBtns(LobbyController.instance.Colors.IndexOf(oldValue), LobbyController.instance.Colors.IndexOf(newValue));
//            LobbyController.instance.SelectPaintIDFeedBackColorUpdate(newValue, this.connectionID);
        }

    }


    [Command]
    public void CmdLobbyMapSelect(string id, int changeBy)
    {
//        RpcLobbyMapSelect(id, changeBy);
    }


    [ClientRpc]
    public void RpcLobbyMapSelect(string id,int changeBy)
    {
//        LobbyController.instance.SelectPaintIDFeedback(id, changeBy);
    }

    [Command]
    public void CmdFetchMapInfos()
    {
        List<string> ids = new List<string>();
        List<string> catStrings = new List<string>();
        List<string> regionStrings = new List<string>();
        List<string> timeStrings = new List<string>();
        foreach (MapParent go in LobbyController.instance.mapParents)
        {
            ids.Add(go.NormalID);
            catStrings.Add(go.PopUpItem.statistic_normal_catfound.text);
            regionStrings.Add(go.PopUpItem.statistic_normal_regionpainted.text);
        }

        RpcGiveMapInfos(ids, catStrings, regionStrings,timeStrings);

    }
    [ClientRpc]
    public void RpcGiveMapInfos(List<string> ids,List<string> data0, List<string> data1, List<string> data2)
    {
        LobbyController.instance.AssignFetchedDatas(ids, data0, data1, data2);

    }

    public void ChangeReady()
    {
        if (isLocalPlayer) CmdSetPlayerReady();
    }
    public void ChangeColor(Color32 col)
    {

        if (isLocalPlayer)
        {
            GameDataTracker.themeColor = col;

            CmdSetPlayerColor(col);
        }
    }


    [Command]
    private void CmdSetPlayerReady()
    {
        this.PlayerReadyUpdate(this.PlayerReady, !this.PlayerReady);
    }
    public override void OnStartAuthority()
    {
        CmdSetPlayerName(PlayerIdentity.GetPlayerName());


        gameObject.name = "LocalGamePlayer";
        LobbyController.instance.FindLocalPlayer();
//        LobbyController.instance.StartColor();
    }
    public override void OnStartClient()
    {
        Manager.GamePlayers.Add(this);
        LobbyController.instance.UpdatePlayerList();
    }
    public override void OnStopClient()
    {
        Manager.GamePlayers.Remove(this);
        if (LobbyController.instance == null) return;
        LobbyController.instance.UpdatePlayerList();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        if (GameController.Instance != null) GameController.Instance.LocalPlayerOnlinePlayer = this;
    }

    [Command]
    private void CmdSetPlayerName(string PlayerName)
    {
        this.PlayerNameUpdate(this.PlayerName, PlayerName);
    }

    [Command]
    private void CmdSetPlayerColor(Color32 selectedColor)
    {
        this.PlayerColorUpdate(this.PlayerColor, selectedColor);
    }




    #endregion
}
