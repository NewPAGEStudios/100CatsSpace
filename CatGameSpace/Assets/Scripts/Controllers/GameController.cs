using Mirror;
using Steamworks;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    private static GameController _instance;
    public static GameController Instance
    {
        get { return _instance; }
    }
    public PlayerControllerOffline LocalPlayerOfflinePlayer = null;
    public PlayerController LocalPlayerOnlinePlayer = null;


    public StateManager.GameState gameState { get; private set; }
    public StateManager.GameMode gameMode { get; private set; }

    [Header("SFX Refs")]
    public AudioSource hoverSfx;
    public AudioSource btnSfx;
    public AudioSource CongSfx;
    public AudioSource CongEndSfx;
    public AudioSource BackBtnSfx;

    [Header("Panel")]
    [SerializeField]
    private GameObject mainCanvas;
    [SerializeField]
    private GameObject EndCatFindPanel;
    [SerializeField]
    private GameObject EndGamePanel;
    [SerializeField]
    private GameObject AYSPanel;
    [SerializeField]
    private GameObject LoadingPanel;
    [SerializeField]
    private GameObject UpperPanel;
    [SerializeField]
    private GameObject menuPanel;
    [SerializeField]
    private GameObject chatPanel;

    [Header("Game Referances")]
    [SerializeField]
    private GameObject HintTxtParent;
    [SerializeField]
    private GameObject chatNotf;
    [SerializeField]
    private GameObject GameTimerParent;
    [SerializeField]
    private GameObject FillRegionParentOBJCollection;
    [SerializeField]
    public GameObject FindCatParentOBJCollection;
    [SerializeField]
    private RadioController radController;
    [SerializeField]
    private SettingsController setController;
    public Image InterractionImageRef;


    public float timer = 0;//GameClassicTimer
    public TextMeshProUGUI EndTimer;//GameClassicTimer

    public bool colourControllerReady = false;
    public bool catFinderReady = false;


    [Header("MenuKeysElement")]
    public GameObject HoveredElement;

    public PaintObject selectedPaint;
    public bool isOnline = false;

    public Image UpperStatusChanger;
    public Image DownStatusChanger;

    public Sprite downUpperPanelButton;
    public Sprite upUpperPanelButton;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
//            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        gameState = StateManager.GameState.InGame;
        gameMode = StateManager.GameMode.findCat;


        if (GameObject.FindObjectsOfType<PlayerControllerOffline>().Length <= 0)
        {
            LocalPlayerOnlinePlayer = GameObject.Find("LocalGamePlayer").GetComponent<PlayerController>();//hata verme potansiyelli
            isOnline = true;
        }
    }

    private void Start()
    {
        setController.StartThis();


        PaintObject[] paints = Resources.LoadAll<PaintObject>(GameDataTracker.paintObjectFileKey);

        string selectedPaintID;

        if (isOnline)
        {
            selectedPaintID = SteamMatchmaking.GetLobbyData(new CSteamID(SteamLobby.instance.CurrentLobbyID), "selectedID");//GetSteamData
            Debug.Log(selectedPaintID);
            GameDataTracker.paint_SaveSoloCorrupted = true;
        }
        else
        {
            selectedPaintID = GameDataTracker.selectedPaintID;
        }

        GameDataTracker.selectedPaintID = selectedPaintID;

        foreach (PaintObject pa in paints)
        {
            if(pa.id == selectedPaintID)
            {
                selectedPaint = pa;
            }
        }

        CatFindController.Instance.StartTheGame();
        ColourControll.Instance.StartTheGame();


        if(isOnline && LocalPlayerOnlinePlayer.connectionID == 0)
        {
            timer = GameDataTracker.paint_timer;
        }
        else if(!isOnline)
        {
            timer = GameDataTracker.paint_timer;
        }
        CatFindModeOpen();

        #region loadingData



        if (isOnline && LocalPlayerOnlinePlayer.connectionID != 0)
        {
            GameDataTracker.paint_hint = 2;
            HintTxtUpdate();
            //LocalPlayerOnlinePlayer.CmdHintDataRequest();
        }

        if (GameController.Instance.isOnline && GameController.Instance.LocalPlayerOnlinePlayer.connectionID == 0)
        {
            CatFindController.Instance.LoadData(GameDataTracker.paint_findCatData);
            ColourControll.Instance.LoadData(GameDataTracker.paint_regionFillData);
        }
        else if (!GameController.Instance.isOnline)
        {
            CatFindController.Instance.LoadData(GameDataTracker.paint_findCatData);
            ColourControll.Instance.LoadData(GameDataTracker.paint_regionFillData);
        }

        
        HintTxtUpdate();
        #endregion
    }


    // Update is called once per frame
    void Update()
    {
        if(gameState != StateManager.GameState.EndGame)
        {
            if (!isOnline && gameState == StateManager.GameState.InMenu) return;

            if (!isOnline || (isOnline && LocalPlayerOnlinePlayer.connectionID == 0)) 
                if (GameDataTracker.isCheat && gameState == StateManager.GameState.InGame && InputManager.Instance.CheatPressed())
                {
                    if (gameMode == StateManager.GameMode.fillColor)
                    {
                        GetComponent<ColourControll>().AutoRegion();
                    }
                    else if (gameMode == StateManager.GameMode.findCat)
                    {
                        GetComponent<CatFindController>().AutoCat();
                    }
                }

            timer += Time.deltaTime;
            SaveTimerToDataTracker();
            string timerStr;
            if (timer % 60 < 10)
            {
                timerStr = Mathf.FloorToInt(timer / 60).ToString() + ":0" + Mathf.FloorToInt(timer % 60).ToString();
            }
            else
            {
                timerStr = Mathf.FloorToInt(timer / 60).ToString() + ":" + Mathf.FloorToInt(timer % 60).ToString();
            }
            GameTimerParent.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = timerStr;
        }
        //if (Input.GetKeyDown(KeyCode.F5)) 
        //{ 
        //    GameDataTracker.paint_hint += 5; 

        //    HintTxtUpdate();
        //}
    }


    #region Input

    public void ActivateHoverSfx()
    {
//        hoverSfx.Play();
    }
    public void ActivateBtnSfx()
    {
        btnSfx.Play();
    }
    #endregion
    public void BackBtn()
    {
        BackBtnSfx.Play();

        if (gameMode == StateManager.GameMode.fillColor && colourMenuOpenned)
        {
            LowerCanvasStatusChange();
            CloseColourMenu();
        }

        if (gameState == StateManager.GameState.InReplay)
        {
            ReplayController.Instance.StopReplay();
        }
        if (gameState == StateManager.GameState.EndGame)
        {
            ChangeStateOfEndGame();
        }
        else if (setController.gameObject.activeSelf)
        {
            backBtnFromSet();
        }
        else if (AYSPanel.gameObject.activeSelf)
        {
            AysPanelClose();
        }
        else if (menuPanel.gameObject.activeSelf)
        {
            ContGame();
        }
        else if (!menuPanel.gameObject.activeSelf && !setController.gameObject.activeSelf)
        {
            StopGame();
        }
    }

    public void HintTxtUpdate()
    {
        HintTxtParent.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = GameDataTracker.paint_hint.ToString();
    }


    IEnumerator loadPanelOpen()
    {
        if (isOnline)
        {
            yield return new WaitForSeconds(2f);
        }

        while (true)
        {
            if(colourControllerReady && catFinderReady)
            {
                LoadingPanel.SetActive(false);
                yield break;
            }
            yield return null;
        }
    }

    [HideInInspector]
    public bool upperPanelOppenned = true;
    [HideInInspector]
    public bool colourMenuOpenned = false;
    public void LowerCanvasStatusChange()
    {
        if (GameController.Instance.gameMode == StateManager.GameMode.findCat) return;
        if (!colourMenuOpenned) OpenColourMenu();
        else CloseColourMenu();

    }
    public void OpenColourMenu()
    {
        StartCoroutine(colorMenuOpenAnim(1, colourMenuOpenned));
        colourMenuOpenned = true;

        DownStatusChanger.sprite = upUpperPanelButton;
    }
    public void CloseColourMenu()
    {
        StartCoroutine(colorMenuOpenAnim(0, colourMenuOpenned));
        colourMenuOpenned = false;

        DownStatusChanger.sprite = downUpperPanelButton;
    }
    IEnumerator colorMenuOpenAnim(float target, bool changedfrom)
    {
        yield return null;
        yield return null;
        InputManager.Instance.stopInput();
        Animator targetAnimator = ColourControll.Instance.canvasOBJ.GetComponent<Animator>();
        if (!changedfrom)
        {
            ColourControll.Instance.canvasOBJ.transform.GetChild(0).gameObject.SetActive(true);
        }
        while (true)
        {
            targetAnimator.SetFloat("lowerButonPosNumber", Mathf.Lerp(targetAnimator.GetFloat("lowerButonPosNumber"), target, Time.deltaTime * 5f));
            if (Mathf.Abs(targetAnimator.GetFloat("lowerButonPosNumber") - target) <= 0.01f) break;
            else if (colourMenuOpenned == changedfrom) yield break;
            yield return null;
        }
        if (changedfrom)
        {
            ColourControll.Instance.canvasOBJ.transform.GetChild(0).gameObject.SetActive(false);
        }
        EventSystem.current.GetComponent<EventSystemController>().SetPanel("RegionColorPanel");
        InputManager.Instance.ContInput();
    }




    public void UpperCanvasStatusChange()
    {
        if (!upperPanelOppenned) UpperCanvasOpen();
        else UpperCanvasClose();
    }
    public void UpperCanvasOpen()
    {
        StartCoroutine(UpperCanvasAnim(1, upperPanelOppenned));
        upperPanelOppenned = true;
        UpperStatusChanger.sprite = downUpperPanelButton;
    }
    public void UpperCanvasClose()
    {
        StartCoroutine(UpperCanvasAnim(0, upperPanelOppenned));
        upperPanelOppenned = false;
        UpperStatusChanger.sprite = upUpperPanelButton;
    }

    IEnumerator UpperCanvasAnim(float target,bool changedfrom)
    {
        yield return null;
        yield return null;
        InputManager.Instance.stopInput();
        if (!changedfrom)
        {
            UpperPanel.SetActive(true);
        }
        while (true)
        {
            mainCanvas.GetComponent<Animator>().SetFloat("Blend", Mathf.Lerp(mainCanvas.GetComponent<Animator>().GetFloat("Blend"), target, Time.deltaTime * 5f));
            if (Mathf.Abs(mainCanvas.GetComponent<Animator>().GetFloat("Blend") - target) <= 0.01f) break;
            else if (upperPanelOppenned == changedfrom) yield break;
            yield return null;
        }
        if (changedfrom)
        {
            UpperPanel.SetActive(false);
        }

        EventSystem.current.GetComponent<EventSystemController>().SetPanel("mainCanvas");
        InputManager.Instance.ContInput();
    }

    private void SaveTimerToDataTracker()
    {
        if (isOnline && LocalPlayerOnlinePlayer.connectionID == 0)
        {
            GameDataTracker.paint_timer = timer;
        }
        else if(!isOnline)
        {
            GameDataTracker.paint_timer = timer;
        }
    }



    public void ClickAction(Vector3 whereClicked)
    {
        if(gameMode == StateManager.GameMode.fillColor)
        {
            GetComponent<ColourControll>().PaintRegion(whereClicked);
        }
        else if(gameMode == StateManager.GameMode.findCat)
        {
            GetComponent<CatFindController>().FindCat(whereClicked);
        }
    }
    public void ThirdClickAction(Vector3 whereClicked)
    {
        if(gameMode == StateManager.GameMode.fillColor)
        {
            GetComponent<ColourControll>().OpenRegion(whereClicked);
        }
        else if(gameMode == StateManager.GameMode.findCat)
        {

        }
    }
    public void SecClickAction()
    {
        if (gameMode == StateManager.GameMode.fillColor)
        {
            GetComponent<ColourControll>().HintRegion();
        }
        else if (gameMode == StateManager.GameMode.findCat)
        {
            GetComponent<CatFindController>().HintCat(); 
        }
    }
    #region Console&Chat
    public void ChatReceivedWhileClosed()
    {
        if (gameState == StateManager.GameState.InChat) return;
        chatNotf.SetActive(true);

    }
    public void EnterBtn()
    {
        if (LocalPlayerOnlinePlayer == null) return;
        if (gameState == StateManager.GameState.InChat && chatPanel.GetComponent<ChatManager>().inputSelected)
        {
            chatPanel.GetComponent<ChatManager>().ChatButtonPressed();
        }
    }
    public void OpenChatORConsole()
    {
        if (LocalPlayerOnlinePlayer == null) return;
        if (gameState == StateManager.GameState.InChat)
        {
            if (chatPanel.GetComponent<ChatManager>().inputSelected) return;
            else ChangeChatStatus();
        }

    }

    public void ChangeChatStatus()
    {   
        if (LocalPlayerOnlinePlayer == null) return;

        if(gameState == StateManager.GameState.InChat)
        {
            StartCoroutine(ChatAnimation(0, (int)gameState));
            gameState = StateManager.GameState.InGame;
        }
        else if(gameState == StateManager.GameState.InGame)
        {
            chatNotf.SetActive(false);
            StartCoroutine(ChatAnimation(1, (int)gameState));
            gameState = StateManager.GameState.InChat;
        }
    }
    IEnumerator ChatAnimation(float target,int lastStateID)
    {
        yield return null;
        yield return null;
        InputManager.Instance.stopInput();
        if (lastStateID == 0)
        {
            chatPanel.SetActive(true);
        }
        while (true)
        {
            chatPanel.GetComponent<Animator>().SetFloat("Blend", Mathf.Lerp(chatPanel.GetComponent<Animator>().GetFloat("Blend"), target, Time.deltaTime * 5f));

            if (Mathf.Abs(chatPanel.GetComponent<Animator>().GetFloat("Blend") - target) <= 0.01f)
            {
                break;
            }
            else if (lastStateID == (int)gameState) yield break;
            yield return null;
        }
        if (lastStateID == 2)
        {
            chatPanel.SetActive(false);
        }
        EventSystem.current.GetComponent<EventSystemController>().SetPanel("ChatPanel");
        InputManager.Instance.ContInput();
    }
    #endregion
    #region modeSelection
    public void CallColorFillModeOpen()
    {
        if(isOnline && LocalPlayerOnlinePlayer.connectionID == 0)
        {
            Debug.Log("uanajajn");
            ColorFillModeOpen();
            LocalPlayerOnlinePlayer.CMDColorFillModeOpen();
        }
    }

    public void ColorFillModeOpen()
    {
        gameState = StateManager.GameState.InGame;
        gameMode = StateManager.GameMode.fillColor;



        if (isOnline)
        {
            CursorDisplay[] cd = GameObject.FindObjectsOfType<CursorDisplay>();
            foreach (CursorDisplay c in cd)
            {
                c.ChangeMode(StateManager.GameMode.fillColor);
            }

        }
        else
        {
            CursorDisplayOffline cd = GameObject.FindObjectOfType<CursorDisplayOffline>();
            cd.ChangeMode(StateManager.GameMode.fillColor);

        }

        EndCatFindPanel.SetActive(false);

        GetComponent<ColourControll>().canvasOBJ.SetActive(true);
        GetComponent<CatFindController>().canvasOBJ.SetActive(false);

        GetComponent<ColourControll>().ActivateThis();

        FindCatParentOBJCollection.transform.GetChild(0).gameObject.SetActive(false);

        ColourControll.Instance.isGameEnded();
    }
    public void CallFindCatModeOpen()
    {
        if (isOnline && LocalPlayerOnlinePlayer.connectionID == 0)
        {
            CatFindModeOpen();
            LocalPlayerOnlinePlayer.RpcFindCatModeOpen();
        }
    }

    public void CatFindModeOpen()
    {

        gameMode = StateManager.GameMode.findCat;


        if (isOnline)
        {
            CursorDisplay[] cd = GameObject.FindObjectsOfType<CursorDisplay>();
            foreach (CursorDisplay c in cd)
            {
                c.ChangeMode(StateManager.GameMode.findCat);
            }

        }
        else
        {
            CursorDisplayOffline cd = GameObject.FindObjectOfType<CursorDisplayOffline>();
            cd.ChangeMode(StateManager.GameMode.findCat);

        }


        GetComponent<ColourControll>().canvasOBJ.SetActive(false);
        GetComponent<CatFindController>().canvasOBJ.SetActive(true);
        EventSystem.current.GetComponent<EventSystemController>().SetPanel("mainCanvas");

        GetComponent<CatFindController>().ActivateThis();

        FindCatParentOBJCollection.transform.GetChild(0).gameObject.SetActive(true);

        if (LoadingPanel.activeSelf)
        {
            StartCoroutine(loadPanelOpen());
        }
    }
    #endregion
    #region EndGameMenu

    public void EndCatFindSection()
    {
        CongSfx.Play();
        if (isOnline)
        {
            if (LocalPlayerOnlinePlayer.connectionID == 0)
            {
                gameState = StateManager.GameState.CatEndGame;
                if (!GameDataTracker.paint_finished)
                {
                    GameDataTracker.paint_hint += 18;
                    HintTxtUpdate();
//                    LocalPlayerOnlinePlayer.CmdCallHintDataChange();
                }
                GameDataTracker.paint_finished = true;
                EndCatFindPanel.SetActive(true);
                EventSystem.current.GetComponent<EventSystemController>().SetPanel("EndCatFindPanel");
            }
            else
            {
                GameDataTracker.paint_hint += 18;
                HintTxtUpdate();

                GameDataTracker.paint_finished = true;
                EndCatFindPanel.SetActive(true);
                EventSystem.current.GetComponent<EventSystemController>().SetPanel("EndCatFindPanel");
                EndCatFindPanel.transform.GetChild(0).GetComponent<Button>().interactable = false;
            }

        }
        else
        {
            gameState = StateManager.GameState.CatEndGame;
            if(!GameDataTracker.paint_finished)
            {
                GameDataTracker.paint_hint += 18;
                HintTxtUpdate();
            }

            GameDataTracker.paint_finished = true;
            EndCatFindPanel.SetActive(true);
            EventSystem.current.GetComponent<EventSystemController>().SetPanel("EndCatFindPanel");
        }
    }

    public void EndGame()
    {
        if (colourMenuOpenned)
        {
            CloseColourMenu();
        }
        CongEndSfx.Play();

        SteamAchivementController.Instance.TryUnlockAchivement(selectedPaint.id);

        string timerStr;
        if (timer % 60 < 10)
        {
            timerStr = Mathf.FloorToInt(timer / 60).ToString() + ":0" + Mathf.FloorToInt(timer % 60).ToString();
        }
        else
        {
            timerStr = Mathf.FloorToInt(timer / 60).ToString() + ":" + Mathf.FloorToInt(timer % 60).ToString();
        }

        ColourControll.Instance.uniqueColorSelectionPanel.SetActive(false);

        EndTimer.text = timerStr;

        CameraZoom.Instance.defaultCam();

        EndGamePanel.SetActive(true);
        EventSystem.current.GetComponent<EventSystemController>().SetPanel("EndGamePanel");
        gameState = StateManager.GameState.EndGame;
        mainCanvas.SetActive(false);

        if (isOnline && LocalPlayerOnlinePlayer.connectionID == 0)
        {
            LocalPlayerOnlinePlayer.RpcGiveEveryoneReplayInfo(GameDataTracker.replay_paintPos, GameDataTracker.replay_itemID);
        }

        ChangeStateOfEndGame();
    }

    public void ChangeStateOfEndGame()
    {
        if (EndGamePanel.GetComponent<Image>().enabled)
        {
            EndGamePanel.GetComponent<Image>().enabled = false;

            EndGamePanel.transform.GetChild(0).GetComponent<Button>().interactable = false;
            EndGamePanel.transform.GetChild(1).GetComponent<Button>().interactable = false;

            EndGamePanel.transform.GetChild(0).gameObject.SetActive(false);
            EndGamePanel.transform.GetChild(1).gameObject.SetActive(false);
            EndGamePanel.transform.GetChild(3).gameObject.SetActive(false);

        }
        else if (!EndGamePanel.GetComponent<Image>().enabled)
        {
            EndGamePanel.GetComponent<Image>().enabled = true;

            EndGamePanel.transform.GetChild(0).GetComponent<Button>().interactable = true;
            EndGamePanel.transform.GetChild(1).GetComponent<Button>().interactable = true;

            EndGamePanel.transform.GetChild(0).gameObject.SetActive(true);
            EndGamePanel.transform.GetChild(1).gameObject.SetActive(true);
            EndGamePanel.transform.GetChild(3).gameObject.SetActive(true);

        }
    }


    public void toSettings()
    {
        GetComponent<ColourControll>().canvasOBJ.SetActive(false);
        GetComponent<CatFindController>().canvasOBJ.SetActive(false);
        mainCanvas.SetActive(false);

        setController.gameObject.SetActive(true);

        EventSystem.current.GetComponent<EventSystemController>().SetPanel("mainCanvas");


        setController.GetComponent<SettingsController>().SettingPartChange(0);
//        SettingPartChange(0);
    }

    public void SettingPartChange(int partID)
    {
        if (partID == 0)
        {
            setController.transform.GetChild(0).gameObject.SetActive(true);
            setController.transform.GetChild(1).gameObject.SetActive(false);

            setController.generalBut.interactable = false;
            setController.keyButton.interactable = true;

        }
        else if (partID == 1)
        {
            setController.transform.GetChild(0).gameObject.SetActive(false);
            setController.transform.GetChild(1).gameObject.SetActive(true);

            setController.generalBut.interactable = true;
            setController.keyButton.interactable = false;
        }
    }

    public void AysPanelOpen()
    {
        AYSPanel.SetActive(true);
        EventSystem.current.GetComponent<EventSystemController>().SetPanel("AYSPanel");
    }
    public void AysPanelClose()
    {
        AYSPanel.SetActive(false);
        EventSystem.current.GetComponent<EventSystemController>().SetPanel("MenuPanel");
    }

    public void backBtnFromSet()
    {
        GetComponent<ColourControll>().canvasOBJ.SetActive(true);
        GetComponent<CatFindController>().canvasOBJ.SetActive(true);
        mainCanvas.SetActive(true);


        setController.gameObject.SetActive(false);
        EventSystem.current.GetComponent<EventSystemController>().SetPanel("MenuPanel");

    }

    public void toMainMenu()
    {
        if (LocalPlayerOnlinePlayer != null)
        {
            if (LocalPlayerOnlinePlayer.connectionID == 0)
            {
                radController.SaveRadioPrefs();
                GameDataTracker.paint_timer = timer;
                CustomNetworkManager.singleton.GetComponent<CustomNetworkManager>().StopHostAndGo("Menu");
            }
            else
            {
                radController.SaveRadioPrefs();
                CustomNetworkManager.singleton.GetComponent<CustomNetworkManager>().StopClientAndGo("Menu");
            }
            return;
        }
        else
        {
            radController.SaveRadioPrefs();
            GameDataTracker.paint_timer = timer;
        }
        CustomSceneChanger.Instance.ChangeScene("Menu");
    }




    public void toReplay()
    {
        FindCatParentOBJCollection.SetActive(true);
        ColourControll.Instance.canvasOBJ.SetActive(false);
        mainCanvas.transform.gameObject.SetActive(false);
        LoadingPanel.transform.gameObject.SetActive(false);
        EndGamePanel.SetActive(false);

        ReplayController.Instance.CanvasObj.SetActive(true);
        EventSystem.current.GetComponent<EventSystemController>().SetPanel("ReplayPanel");

        gameState = StateManager.GameState.InReplay;

        CameraZoom.Instance.defaultCam();

        Debug.Log(GameDataTracker.replay_itemID.Count);

        if (isOnline && LocalPlayerOnlinePlayer.connectionID == 0)
        {
            ReplayController.Instance.StartReplay(GameDataTracker.replay_paintPos.ToArray(),GameDataTracker.replay_itemID.ToArray());
        }
        else if(isOnline && LocalPlayerOnlinePlayer.connectionID != 0)
        {
            ReplayController.Instance.StartReplay(GameDataTracker.replay_paintPos.ToArray(), GameDataTracker.replay_itemID.ToArray());
        }
        else if (!isOnline)
        {
            ReplayController.Instance.StartReplay(GameDataTracker.replay_paintPos.ToArray(), GameDataTracker.replay_itemID.ToArray());
        }

    }

    public void fromReplay()
    {
        gameState = StateManager.GameState.EndGame;

        FindCatParentOBJCollection.SetActive(false);
        LoadingPanel.transform.parent.gameObject.SetActive(true);
        ReplayController.Instance.CanvasObj.SetActive(false);
        EndGamePanel.SetActive(true);
    }

    #endregion
    StateManager.GameState lastStateBeforeStopGame;

    public void StopGameBtn()
    {
        if (colourMenuOpenned)
        {
            CloseColourMenu();
        }

        BackBtn();

    }

    public void StopGame()
    {
        if(gameState== StateManager.GameState.CatEndGame)
        {
            return;
        }

        lastStateBeforeStopGame = gameState;
        gameState = StateManager.GameState.InMenu;
        menuPanel.SetActive(true);
        EventSystem.current.GetComponent<EventSystemController>().SetPanel("MenuPanel");
    }
    public void ContGame()
    {
        gameState = lastStateBeforeStopGame;
        menuPanel.SetActive(false);
        if(gameMode == StateManager.GameMode.findCat)
        {
            EventSystem.current.GetComponent<EventSystemController>().SetPanel("mainCanvas");
        }
        else if(gameMode == StateManager.GameMode.fillColor)
        {
            EventSystem.current.GetComponent<EventSystemController>().SetPanel("mainCanvas");
        }

    }

}
