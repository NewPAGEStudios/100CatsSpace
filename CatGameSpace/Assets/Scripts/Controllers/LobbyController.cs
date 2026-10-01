using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyController : MonoBehaviour
{
    public static LobbyController instance;


    public PlayerController LocalPlayer;
    [Header("Panels")]
    public GameObject lobby_panel;
    public GameObject playerlist_panel;
    public GameObject LoadingPanel;
    public GameObject MapPanel;

    [Header("LobbyProperties")]
    public TextMeshProUGUI lobbyNumber_IF;
    public GameObject chatPanel;
    public GameObject ColorButParent;
    public Button ReadyBut;
    public Button startGameBut;
    public List<Color32> Colors;
    public Sprite tick;
    public Sprite cross;
    public Button Copybut;

    [Header("PlayerListProperties")]
    public GameObject playerListViewContent;
    public GameObject PlayerListItemPrefab;
    public bool PlayerItemCreated = false;
    public List<PlayerListItem> playerListItems = new();

    [Header("Refs")]
    public Image InterractionImageRef;
    public AudioSource hoverSfx;
    public AudioSource btnSfx;
    public AudioSource mapSfx;
    public AudioSource mapErrorSfx;
    public AudioSource backBtnSfx;

    private StateManager.LobbyState lobbyState = StateManager.LobbyState.None;
    public List<MapParent> mapParents;

    [Header("MenuKeysElement")]
    public GameObject HoveredElement;


    private CustomNetworkManager manager;
    private CustomNetworkManager Manager
    {
        get
        {
            if (manager != null) return manager;
            return manager = CustomNetworkManager.singleton as CustomNetworkManager;
        }
    }


    InputManager inputManager;


    private void OnDestroy()
    {
        if (instance == this)
            instance = null;

        playerListItems.Clear();
        PlayerItemCreated = false;
    }



    private void Awake()
    {
        if (instance == null) instance = this;

    }
    private void Start()
    {
        inputManager = InputManager.Instance;
        LoadingPanel.SetActive(true);

    }

    private void Update()
    {
        if(LoadingPanel.activeSelf)
        {
            if(LocalPlayer == null)
            {
                LoadingPanel.SetActive(true);
                return;
            }
            else
            {
                LoadingPanel.SetActive(false);
                RadioController.instance.startOnLobby();
                GetMapsDatas();
                StartColor();
            }
        }
        if (chatPanel.GetComponent<ChatManager>().inputSelected)
        {
            if (inputManager.ChatEnterPressed())
            {
                chatPanel.GetComponent<ChatManager>().ChatButtonPressed();
            }
            //iff inputPlayer.MenuReadyButon;
        }
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
    public void ActivateMapSfx()
    {
        mapSfx.Play();
    }
    public void ActivateMapErrorSfx()
    {
        mapErrorSfx.Play();
    }

    #endregion
    public void BackBtn()
    {
        //        backBtnSfx.Play();
        if (EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.TryGetComponent<InputFieldBtnAction>(out InputFieldBtnAction ifba))
        {
            EventSystem.current.SetSelectedGameObject(EventSystem.current.GetComponent<EventSystemController>().GetLastSelected());
            return;
        }

        if (lobbyState == StateManager.LobbyState.mapSelect) toLobby();
    }

    public void FindLocalPlayer()
    {
        LocalPlayer = GameObject.Find("LocalGamePlayer").GetComponent<PlayerController>();
    }

    #region InspectorReach
    public void ReadyPlayer()
    {
        LocalPlayer.ChangeReady();
    }
    public void StartColor()
    {
        if (Colors.Count <= Manager.GamePlayers.Count) return;


        if (Colors.Contains(LocalPlayer.PlayerColor))
        {
            int i = Colors.IndexOf(LocalPlayer.PlayerColor);
            while (true)
            {
                i++;
                if (i >= Colors.Count)
                {
                    i = 0;
                }
                if (i == Colors.IndexOf(LocalPlayer.PlayerColor))
                {
                    Debug.Log("All Colors in usage");
                    break;
                }
                bool colorUsed = false;
                foreach (PlayerController player in Manager.GamePlayers)
                {
                    if (player.PlayerColor.r == Colors[i].r && player.PlayerColor.g == Colors[i].g && player.PlayerColor.b == Colors[i].b)
                    {
                        colorUsed = true;
                        break;
                    }
                }
                if (!colorUsed)
                {
                    LocalPlayer.ChangeColor(Colors[i]);
                    break;
                }
            }
        }
        else
        {
            int i;
            for (i = 0; i < Colors.Count; i++)
            {
                bool colorUsed = false;
                foreach (PlayerController player in Manager.GamePlayers)
                {
                    if (player.PlayerColor.r == Colors[i].r && player.PlayerColor.g == Colors[i].g && player.PlayerColor.b == Colors[i].b)
                    {
                        colorUsed = true;
                        break;
                    }
                }
                if (!colorUsed)
                {
                    LocalPlayer.ChangeColor(Colors[i]);
                    break;
                }

            }
        }

    }


    public void ChangeColor(GameObject colorOBJ)
    {
        if (colorOBJ.transform.GetChild(0).gameObject.activeSelf) return;

        LocalPlayer.ChangeColor(Colors[colorOBJ.transform.GetSiblingIndex()]);

    }

    public void SyncColorBtns(int closedChid,int childID)
    {
        if (closedChid >= 0 && closedChid < Colors.Count) ColorButParent.transform.GetChild(closedChid).GetChild(0).gameObject.SetActive(false);
        ColorButParent.transform.GetChild(childID).GetChild(0).gameObject.SetActive(true);
    }

    public void LeaveLobby()
    {
        SteamLobby.instance.LeaveCurrentLobby();
        if (LocalPlayer.connectionID == 0)
        {
            Manager.StopHost();
        }
        else
        {
            Manager.StopClient();
        }
    }

    bool copyTxtButAnimStarted;



    public void CopyTxt()
    {
        copyTxtButAnimStarted = false;
        StartCoroutine(CopyTxtAnimAction());

        GUIUtility.systemCopyBuffer = SteamLobby.instance.currentCode;
    }

    IEnumerator CopyTxtAnimAction()
    {
        Copybut.interactable = false;
        Copybut.GetComponent<Image>().color = new Color(Copybut.GetComponent<Image>().color.r, Copybut.GetComponent<Image>().color.g, Copybut.GetComponent<Image>().color.b, 0); 

        Image copyTMP = Copybut.transform.GetChild(0).GetComponent<Image>();
        Image copiedTMP = Copybut.transform.GetChild(1).GetComponent<Image>();
        copiedTMP.color = new Color(copiedTMP.color.r, copiedTMP.color.g, copiedTMP.color.b, 1);
        copyTMP.color = new Color(copyTMP.color.r, copyTMP.color.g, copyTMP.color.b, 0);
        yield return new WaitForSeconds(2f);

        copyTxtButAnimStarted = true;

        while (true)
        {
            copiedTMP.color = new Color(copiedTMP.color.r, copiedTMP.color.g, copiedTMP.color.b, Mathf.MoveTowards(copiedTMP.color.a, 0, Time.deltaTime * .5f));
            copyTMP.color = new Color(copyTMP.color.r, copyTMP.color.g, copyTMP.color.b, Mathf.MoveTowards(copyTMP.color.a, 1, Time.deltaTime * .5f));
            if (!copyTxtButAnimStarted || (copiedTMP.color.a <= 0 && copyTMP.color.a >= 1)) 
            {
                Copybut.interactable = true;
                Copybut.GetComponent<Image>().color = new Color(Copybut.GetComponent<Image>().color.r, Copybut.GetComponent<Image>().color.g, Copybut.GetComponent<Image>().color.b, 1);
                copiedTMP.color = new Color(copiedTMP.color.r, copiedTMP.color.g, copiedTMP.color.b, 0);
                copyTMP.color = new Color(copyTMP.color.r, copyTMP.color.g, copyTMP.color.b, 0);
                yield break;
            }
            yield return null;
        }
    }


    public void ReadyBtn()
    {
        //if (LocalPlayer.PlayerReady)
        //{
        //    ReadyBut.transform.GetChild(0).GetComponent<Image>().sprite = cross;
        //}
        //else
        //{
        //    ReadyBut.transform.GetChild(0).GetComponent<Image>().sprite = tick;
        //}
    }


    public void SelectPaintID(string id)
    {
        if(LocalPlayer.connectionID == 0)
        {
            SteamLobby.instance.SetLobbyData("selectedID", id);
        }
    }

    //public void SelectPaintIDFeedback(string id,int conID)
    //{
    //    for (int i = 0; i < mapBtnsNormal.Count; i++) 
    //    {
    //        for(int j = 0;j < mapBtnsNormal[i].transform.GetChild(1).childCount; j++)
    //        {
    //            if(conID == mapBtnsNormal[i].transform.GetChild(1).GetChild(j).GetComponent<MapSelectFeedBackImage>().opennedBy)
    //            {
    //                mapBtnsNormal[i].transform.GetChild(1).GetChild(j).GetComponent<MapSelectFeedBackImage>().opennedBy = -1;
    //                mapBtnsNormal[i].transform.GetChild(1).GetChild(j).gameObject.SetActive(false);
    //                break;
    //            }
    //        }
    //    }
    //    for (int i = 0; i < mapBtnsHard.Count; i++)
    //    {
    //        for (int j = 0; j < mapBtnsHard[i].transform.GetChild(1).childCount; j++)
    //        {
    //            if (conID == mapBtnsHard[i].transform.GetChild(1).GetChild(j).GetComponent<MapSelectFeedBackImage>().opennedBy)
    //            {
    //                mapBtnsHard[i].transform.GetChild(1).GetChild(j).GetComponent<MapSelectFeedBackImage>().opennedBy = -1;
    //                mapBtnsHard[i].transform.GetChild(1).GetChild(j).gameObject.SetActive(false);
    //                break;
    //            }
    //        }
    //    }
    //    var mathcedItem0 = mapBtnsNormal.Find(item => item.GetComponent<MapButtons>().id == id);
    //    if (mathcedItem0 == null)
    //    {
    //        mathcedItem0 = mapBtnsHard.Find(item => item.GetComponent<MapButtons>().id == id);
    //    }
    //    for (int i = 0; i < mathcedItem0.transform.GetChild(1).childCount; i++)
    //    {
    //        if (mathcedItem0.transform.GetChild(1).GetChild(i).GetComponent<MapSelectFeedBackImage>().opennedBy == -1)
    //        {
    //            Debug.Log("Aha");
    //            mathcedItem0.transform.GetChild(1).GetChild(i).GetComponent<Image>().color = manager.GamePlayers.Find(item => item.connectionID == conID).PlayerColor;
    //            mathcedItem0.transform.GetChild(1).GetChild(i).GetComponent<MapSelectFeedBackImage>().opennedBy = conID;
    //            mathcedItem0.transform.GetChild(1).GetChild(i).gameObject.SetActive(true);
    //            break;
    //        }
    //    }
    //}
    //public void SelectPaintIDFeedBackColorUpdate(Color col, int conID)
    //{
    //    for (int i = 0; i < mapBtnsNormal.Count; i++)
    //    {
    //        for (int j = 0; j < mapBtnsNormal[i].transform.GetChild(1).childCount; j++)
    //        {
    //            if (conID == mapBtnsNormal[i].transform.GetChild(1).GetChild(j).GetComponent<MapSelectFeedBackImage>().opennedBy)
    //            {
    //                mapBtnsNormal[i].transform.GetChild(1).GetChild(j).GetComponent<Image>().color = col;
    //                break;
    //            }
    //        }
    //    }
    //    for (int i = 0; i < mapBtnsHard.Count; i++)
    //    {
    //        for (int j = 0; j < mapBtnsHard[i].transform.GetChild(1).childCount; j++)
    //        {
    //            if (conID == mapBtnsHard[i].transform.GetChild(1).GetChild(j).GetComponent<MapSelectFeedBackImage>().opennedBy)
    //            {
    //                mapBtnsHard[i].transform.GetChild(1).GetChild(j).GetComponent<Image>().color = col;
    //                break;
    //            }
    //        }
    //    }

    //}

    #endregion


    #region DataDisplayFunctions
    public void SetDisplayLobbyNumber()
    {
        lobbyNumber_IF.text = "******";
    }

    public void ChangeHideStateLobbyNumber()
    {
        if(lobbyNumber_IF.text == "******")
        {
            lobbyNumber_IF.text = SteamLobby.instance.currentCode;
        }
        else
        {
            lobbyNumber_IF.text = "******";
        }
    }

    public void CheckIfCanStartGame()
    {
        if (LocalPlayer == null) return;

        bool allready = false;

        foreach (PlayerController player in Manager.GamePlayers)
        {
            if (player.PlayerReady)
            {
                allready = true;
            }
            else
            {
                allready = false;
                break;
            }
        }
        if (LocalPlayer.connectionID == 0 && allready && !string.IsNullOrEmpty(SteamLobby.instance.GetLobbyData("selectedID")))
        {
            startGameBut.interactable = true;
        }
        else
        {
            startGameBut.interactable = false;
        }
    }
    public void StartGame()
    {
        if (LocalPlayer.connectionID != 0) return;


        GameDataTracker.skipSave = false;

        GameDataTracker.selectedPaintID = SteamLobby.instance.GetLobbyData("selectedID");
        var Result = SaveSystem.LoadData(GameDataTracker.selectedPaintID);

        GameDataTracker.paint_findCatData = Result.Item1.paint_catFindData;
        GameDataTracker.paint_regionFillData = Result.Item1.paint_regionColorData;
        GameDataTracker.paint_timer = Result.Item1.paintTimer;
        GameDataTracker.paint_finished = Result.Item1.paint_finished;
        GameDataTracker.paint_hint = Result.Item1.paint_hint;
        GameDataTracker.paint_SaveSoloCorrupted = Result.Item1.solo_Corrupted;

        //Array.Fill(GameDataTracker.paint_findCatData, true);
        //Array.Fill(GameDataTracker.paint_regionFillData, true);
        //GameDataTracker.paint_regionFillData[GameDataTracker.paint_regionFillData.Length - 1] = false;


        var Sonuc = SaveSystem.LoadReplay(GameDataTracker.selectedPaintID);

        if (Sonuc.Item1 == null)
        {
            Debug.Log("LobbyStart: SaveHasSomeProblems");

            Application.Quit();
            return;
        }

        GameDataTracker.replay_itemID.Clear();
        GameDataTracker.replay_paintPos.Clear();

        foreach (int i in Sonuc.Item1.ItemID)
        {
            GameDataTracker.replay_itemID.Add(i);
        }
        for (int i = 0; i < Sonuc.Item1.xs.Length; i++)
        {
            GameDataTracker.replay_paintPos.Add(new Vector2(Sonuc.Item1.xs[i], Sonuc.Item1.ys[i]));
        }

        //GameDataTracker.replay_itemID.Clear();
        //GameDataTracker.replay_paintPos.Clear();

        //for (int i = 0; i < GameDataTracker.paint_regionFillData.Length; i++)
        //{
        //    GameDataTracker.replay_itemID.Add(i);
        //}

        //for (int i = 0; i < GameDataTracker.paint_regionFillData.Length; i++)
        //{
        //    GameDataTracker.replay_paintPos.Add(new Vector2(0,0));
        //}

        LocalPlayer.CanStartGame("OnlineGamePlay");
    }

    public void GetMapsDatas()
    {
        if (LocalPlayer.connectionID != 0)
        {
            LocalPlayer.CmdFetchMapInfos();
            return;
        }
        //else playerController fetch datas

        var Result = SaveSystem.LoadAllData();

        PaintObject[] paintsArray = Resources.LoadAll<PaintObject>("Paints");

        if (Result.Item1 == null)
        {
            Application.Quit();
            return;
        }
        for (int c = 0; c < paintsArray.Length; c++)
        {
            var matchedItem = Result.Item1.Find(item => item.paintID == paintsArray[c].id);


            int countCat = 0;
            int countRegion = 0;
            for (int i = 0; i < matchedItem.paint_catFindData.Length; i++)
            {
                if (matchedItem.paint_catFindData[i])
                {
                    countCat++;
                }
            }
            for (int i = 0; i < matchedItem.paint_regionColorData.Length; i++)
            {
                if (matchedItem.paint_regionColorData[i])
                {
                    countRegion++;
                }
            }

            var mathcedItem0 = mapParents.Find(item => item.NormalID == paintsArray[c].id);
            if (mathcedItem0 != null)
            {
                InfoPopUpItem infoPopUpItem = mathcedItem0.PopUpItem;

                infoPopUpItem.mapName.text = paintsArray[c].PaintName;

                if (matchedItem.paint_regionColorData.Length <= 0 || matchedItem.paint_catFindData.Length <= 0)
                {
                    mathcedItem0.GetComponent<Button>().interactable = false;

                    infoPopUpItem.NormalSection.SetActive(false);
                    infoPopUpItem.comingSoonTxt.gameObject.SetActive(true);

                    continue;
                }

                infoPopUpItem.CorruptedInformer.gameObject.SetActive(true);

                infoPopUpItem.statistic_normal_catfound.text = countCat + "/" + matchedItem.paint_catFindData.Length;

                float percentage = (float)countRegion / (float)matchedItem.paint_regionColorData.Length;
                percentage *= 100;

                infoPopUpItem.statistic_normal_regionpainted.text = Mathf.FloorToInt(percentage).ToString() + "%";
                infoPopUpItem.mapName.text = paintsArray[c].PaintName;

                infoPopUpItem.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("Error on MapSpawns: " + paintsArray[c].id);
            }

        }
    }

    public void AssignFetchedDatas(List<string> ids,List<string> catCount, List<string> regionCount , List<string> times)
    {
        PaintObject[] paintsArray = Resources.LoadAll<PaintObject>("Paints");

        List<PaintObject> paintsList = paintsArray.ToList<PaintObject>();

        for (int i = 0; i< ids.Count; i++)
        {
            var mathcedItem0 = mapParents.Find(item => item.NormalID == ids[i]);
            if(mathcedItem0 != null)
            {
                var paintItem = paintsList.Find(item => item.id == ids[i]);

                if (paintItem == null) continue;
                mathcedItem0.PopUpItem.mapName.text = paintItem.PaintName;

                mathcedItem0.PopUpItem.CorruptedInformer.gameObject.SetActive(true);
                mathcedItem0.PopUpItem.statistic_normal_catfound.text = catCount[i];
                mathcedItem0.PopUpItem.statistic_normal_regionpainted.text = regionCount[i];
                mathcedItem0.PopUpItem.mapName.text = paintItem.PaintName;
            }
        }
    }

    #endregion



    #region panelChanges
    public void toLobby()
    {
        lobby_panel.SetActive(true);

        lobbyState = StateManager.LobbyState.main;


        StartCoroutine(MapAnim(0));

    }
    public void toMap()
    {

        lobbyState = StateManager.LobbyState.mapSelect;


        StartCoroutine(MapAnim(1));
    }

    #endregion

    IEnumerator MapAnim(float target)
    {
        MapPanel.SetActive(true);
        lobby_panel.SetActive(true);
        inputManager.stopInput();


        Animator anim = MapPanel.GetComponent<Animator>();
        while (true)
        {
            anim.SetFloat("CloseOpenRate", Mathf.Lerp(anim.GetFloat("CloseOpenRate"), target, Time.deltaTime * 25f));
            yield return null;
            if (anim.GetFloat("CloseOpenRate") == target) break;
            else if (target == 0 && lobbyState != StateManager.LobbyState.main) break;
            else if(target == 1 && lobbyState != StateManager.LobbyState.mapSelect) break;
        }

        if (lobbyState == StateManager.LobbyState.mapSelect)
        {
            MapPanel.SetActive(true);
            EventSystem.current.GetComponent<EventSystemController>().SetPanel("MapSelectPanel");
            lobby_panel.SetActive(false);
            inputManager.ContInput();
        }
        else
        {
            lobby_panel.SetActive(true);
            EventSystem.current.GetComponent<EventSystemController>().SetPanel("LobbyPanel");
            MapPanel.SetActive(false);
            inputManager.ContInput();
        }
    }
    #region playerList



    public void UpdatePlayerList()
    {

        if (!PlayerItemCreated) CreateLocalPlayerItem();
        if (playerListItems.Count < Manager.GamePlayers.Count) CreateClientPlayerItem();
        if (playerListItems.Count > Manager.GamePlayers.Count) RemovePlayerItem();
        if (playerListItems.Count == Manager.GamePlayers.Count) UpdatePlayerItem();
    }

    private void UpdatePlayerItem()
    {
        foreach (PlayerController player in Manager.GamePlayers)
        {
            foreach (PlayerListItem PlayerListItemScript in playerListItems)
            {
                if (PlayerListItemScript.ConnectionID == player.connectionID)
                {
                    PlayerListItemScript.PlayerName = player.PlayerName;

                    PlayerListItemScript.Ready = player.PlayerReady;

                    PlayerListItemScript.SelectedColor = player.PlayerColor;


                    PlayerListItemScript.SetPlayerValues();
                }
            }
        }
        CheckIfCanStartGame();
    }

    private void RemovePlayerItem()
    {
        List<PlayerListItem> playerListItemToRemove = new();

        foreach (PlayerListItem playerListItem in playerListItems)
        {
            if (!Manager.GamePlayers.Any(b => b.connectionID == playerListItem.ConnectionID))
            {
                playerListItemToRemove.Add(playerListItem);
            }
        }
        if (playerListItemToRemove.Count > 0)
        {
            foreach (PlayerListItem playerlistItemToRemove in playerListItemToRemove)
            {
                GameObject ObjectToRemove = playerlistItemToRemove.gameObject;
                playerListItems.Remove(playerlistItemToRemove);
                Destroy(ObjectToRemove);
                ObjectToRemove = null;
            }
        }
    }

    private void CreateClientPlayerItem()
    {
        foreach (PlayerController player in Manager.GamePlayers)
        {
            if (!playerListItems.Any(b => b.ConnectionID == player.connectionID))
            {
                GameObject NewPlayerItem = Instantiate(PlayerListItemPrefab) as GameObject;
                PlayerListItem NewPlayerItemScript = NewPlayerItem.GetComponent<PlayerListItem>();

                NewPlayerItemScript.PlayerName = player.PlayerName;
                NewPlayerItemScript.ConnectionID = player.connectionID;
                NewPlayerItemScript.PlayersSteamID = player.PlayerSteamID;
                NewPlayerItemScript.Ready = player.PlayerReady;
                NewPlayerItemScript.SelectedColor = player.PlayerColor;

                NewPlayerItemScript.SetPlayerValues();

                NewPlayerItem.transform.SetParent(playerListViewContent.transform);
                NewPlayerItem.transform.localScale = Vector3.one;

                playerListItems.Add(NewPlayerItemScript);
            }
        }
    }

    private void CreateLocalPlayerItem()
    {
        foreach (PlayerController player in Manager.GamePlayers)
        {
            GameObject NewPlayerItem = Instantiate(PlayerListItemPrefab) as GameObject;
            PlayerListItem NewPlayerItemScript = NewPlayerItem.GetComponent<PlayerListItem>();

            NewPlayerItemScript.PlayerName = player.PlayerName;
            NewPlayerItemScript.ConnectionID = player.connectionID;
            NewPlayerItemScript.PlayersSteamID = player.PlayerSteamID;
            NewPlayerItemScript.Ready = player.PlayerReady;
            NewPlayerItemScript.SelectedColor = player.PlayerColor;


            NewPlayerItemScript.SetPlayerValues();

            NewPlayerItem.transform.SetParent(playerListViewContent.transform);
            NewPlayerItem.transform.localScale = Vector3.one;

            playerListItems.Add(NewPlayerItemScript);
        }
        PlayerItemCreated = true;
    }

    #endregion
}
