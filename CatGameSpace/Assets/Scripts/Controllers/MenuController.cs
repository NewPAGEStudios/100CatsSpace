using Mirror;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public static MenuController instance;

    public Color32[] themeColors;

    public string cheatCode;
    private string runtimeCheatCode;

    [Header("Panels")]
    public GameObject CheatingTxt;
    public GameObject AdPanel;
    public GameObject LeaderBoardPanel;
    public GameObject AYSPanel;
    public GameObject AYSaveWipePanel;
    public GameObject TutorialPanel;
    public GameObject CreditPanel;
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject Host_OfflinePanel;
    public GameObject MapPanel;
    public GameObject WaitPanel;
    public GameObject ColorPickBtn;

    public GameObject[] tutorialPages;
    public Button leftTutorialBtn;
    public Button rightTutorialBtn;
    public Button backTutorialBtn;

    [Header("Referances")]
    public AudioSource hoverSfx;
    public AudioSource btnSfx;
    public AudioSource mapSfx;
    public AudioSource mapErrorSfx;
    public AudioSource backBtnSfx;
    public TMP_InputField lobbyCodeIF;
    public SteamLobbyMenu steamLobbyMenu;
    public Image InterractionImageRef;
    public Button ContinueBtn;

    public List<MapParent> mapParents;


    public TextMeshProUGUI versionTxt;

    public StateManager.MainMenuState MenuState {  get; private set; }

    public CursorDisplayOffline cursorDisplayOffline;


    [Header("MenuKeysElement")]
    public GameObject HoveredElement;

    public string selectedPaintId = "";

    private void Awake()
    {
        if(instance == null) instance = this;
        Cursor.visible = true;
    }
    private void Start()
    {
        if (!string.IsNullOrEmpty(GameDataTracker.ErrorMessage))
        {
            GameDataTracker.ErrorMessage = null;
        }


        cursorDisplayOffline.changeColor(themeColors[PlayerPrefs.GetInt("ThemeColorIndex", 2)]);
        ColorPickBtn.GetComponent<Image>().color = themeColors[PlayerPrefs.GetInt("ThemeColorIndex", 2)];
        GameDataTracker.themeColor = themeColors[PlayerPrefs.GetInt("ThemeColorIndex", 2)];


        StartLanguage();


        versionTxt.text = Application.version;

        MenuState = StateManager.MainMenuState.mainMenu;

        AssignButtons();

        settingsPanel.GetComponent<SettingsController>().StartThis();

        Cursor.visible = false;

        if (GameDataTracker.isCheat) CheatingTxt.SetActive(true);
        else CheatingTxt.SetActive(false);

    }
    private void Update()
    {
        cursorDisplayOffline.CursorMovement(getMousePosOnUI());
        if (MenuState != StateManager.MainMenuState.mainMenu) return;

        //foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        //{
        //    if (Input.GetKeyDown(key) && IsKeyboardKey(key))
        //    {
        //        runtimeCheatCode += key.ToString().ToLower();
        //        if (cheatCode[runtimeCheatCode.Length - 1] == runtimeCheatCode[runtimeCheatCode.Length - 1])
        //        {
        //            Debug.Log($"Basýlana tuþ: {key} ** " + cheatCode + " ** " + runtimeCheatCode);
        //            if (cheatCode == runtimeCheatCode)
        //            {
        //                OpenCheat();
        //            }
        //        }
        //        else
        //        {
        //            Debug.Log($"Patlanan tuþ: {key} ** " + cheatCode + " ** " + runtimeCheatCode);
        //            runtimeCheatCode = "";
        //        }

        //    }
        //}

    }
    private Vector2 getMousePosOnUI()
    {
        // Ekran pozisyonunu al
        Vector2 localPoint;
        Vector2 vec = InputManager.Instance.cursorPosition();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(cursorDisplayOffline.transform.parent.GetComponent<RectTransform>(), vec, Camera.main, out localPoint);
        return localPoint;
    }

    public void AssignButtons()
    {
        var Result = SaveSystem.LoadAllData();

        if(Result.Item2 != null)
        {
            Debug.LogWarning("Menu AssignButtons() save problem");
            Application.Quit();
            return;
        }
        if(Result.Item1 == null)
        {
            return;
        }

        PaintObject[] paintsArray = Resources.LoadAll<PaintObject>("Paints");


        for (int c = 0; c < paintsArray.Length; c++) 
        {
            var matchedItem = Result.Item1.Find(item => item.paintID == paintsArray[c].id);

            if(matchedItem == null)
            {
                Debug.Log(paintsArray[c].id + "  couldn't find in save");
                continue;
            }

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
            if(mathcedItem0 != null)
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


                infoPopUpItem.statistic_normal_catfound.text = countCat + "/" + matchedItem.paint_catFindData.Length;

                float percentage = (float)countRegion / (float)matchedItem.paint_regionColorData.Length;
                percentage *= 100;

                infoPopUpItem.statistic_normal_regionpainted.text = Mathf.FloorToInt(percentage).ToString() + "%";
                infoPopUpItem.mapName.text = paintsArray[c].PaintName;
                infoPopUpItem.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log("MapElementCouldn'tFind: " + paintsArray[c].id);
            }

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

    #region panelChange
    public void BackBtn()
    {
        if (MenuState == StateManager.MainMenuState.Wait) return;

        if (EventSystem.current.currentSelectedGameObject.TryGetComponent<InputFieldBtnAction>(out InputFieldBtnAction ifba))
        {
            EventSystem.current.SetSelectedGameObject(EventSystem.current.GetComponent<EventSystemController>().GetLastSelected());
            return;
        }

        Debug.Log(MenuState);
        backBtnSfx.Play();

        if (MenuState == StateManager.MainMenuState.mainMenu)
        {
            AreYouSureButton();
        }
        else if (MenuState == StateManager.MainMenuState.setting)
        {
            BacktoMenuButton();
        }
        else if (MenuState == StateManager.MainMenuState.hostOnlineLobby)
        {
            BacktoMenuButton();
        }
        else if (MenuState == StateManager.MainMenuState.Map)
        {
            PlayButton(null);
        }
        else if (MenuState == StateManager.MainMenuState.AYS)
        {
            BacktoMenuButton();
        }
        else if(MenuState == StateManager.MainMenuState.colorPick)
        {
            ColorPickingStatusChange();
        }
        else if (MenuState == StateManager.MainMenuState.AYSaveWipe)
        {
            saveWipeBtnAYSClose();
        }
        else if(MenuState == StateManager.MainMenuState.CreditSection)
        {
            BacktoMenuButton();
        }
        else if(MenuState == StateManager.MainMenuState.LeaderBoard)
        {
            BacktoMenuButton();
        }
    }
    
    public void AreYouSureButton()
    {
        MenuState = StateManager.MainMenuState.AYS;


        AYSPanel.SetActive(true);
        EventSystem.current.GetComponent<EventSystemController>().SetPanel("AYSPanel");

    }

    public void ColorPickingStatusChange()
    {
        if (MenuState != StateManager.MainMenuState.colorPick)
        {
            MenuState = StateManager.MainMenuState.colorPick;
        }
        else
        {
            MenuState = StateManager.MainMenuState.setting;
        }
    }
    public void SettingsButton()
    {
        MenuState = StateManager.MainMenuState.setting;


        mainMenuPanel.SetActive(false);
        AYSaveWipePanel.SetActive(false);

        settingsPanel.SetActive(true);
        EventSystem.current.GetComponent<EventSystemController>().SetPanel("SettingPanel");

        settingsPanel.GetComponent<SettingsController>().SettingPartChange(0);
    }
    public void BacktoMenuButton()
    {
        MenuState = StateManager.MainMenuState.mainMenu;

        mainMenuPanel.SetActive(true);
        EventSystem.current.GetComponent<EventSystemController>().SetPanel("MainMenuPanel");

        CreditPanel.SetActive(false);
        AYSPanel.SetActive(false);
        LeaderBoardPanel.SetActive(false);
        Host_OfflinePanel.SetActive(false);
        TutorialPanel.SetActive(false);
        settingsPanel.SetActive(false);

    }
    public void PlayButton(string selectedID)
    {
        MenuState = StateManager.MainMenuState.hostOnlineLobby;

        if (PlayerPrefs.GetInt("FirstTime", 1) == 1)
        {
            StartTutorial();
            PlayerPrefs.SetInt("FirstTime", 0);
            return;
        }

        MapPanel.SetActive(false);
        mainMenuPanel.SetActive(true);


        Host_OfflinePanel.SetActive(true);
        EventSystem.current.GetComponent<EventSystemController>().SetPanel("Host_OfflinePanel");

        foreach (MapParent go in mapParents)
        {
            go.PopUpItem.gameObject.SetActive(false);
        }

        selectedPaintId = "";
        if (!string.IsNullOrWhiteSpace(selectedID))
        {
            selectedPaintId = selectedID;
        }
        ContinueBtn.gameObject.SetActive(false);

    }

    public void LeaderBoardBtn()
    {
        MenuState = StateManager.MainMenuState.LeaderBoard;


        LeaderBoardPanel.SetActive(true);
        EventSystem.current.GetComponent<EventSystemController>().SetPanel("LeaderBoardPanel");
    }

    public void MapButton()
    {
        MenuState = StateManager.MainMenuState.Map;

        MapPanel.SetActive(true);
        EventSystem.current.GetComponent<EventSystemController>().SetPanel("MapSelectPanel");
    }
    public void CreditButton()
    {
        MenuState = StateManager.MainMenuState.CreditSection;

        Debug.Log("Trace");
        mainMenuPanel.SetActive(false);

        CreditPanel.SetActive(true);
        EventSystem.current.GetComponent<EventSystemController>().SetPanel("CreditPanel");
    }

    public void OfflineButton()
    {
        GameDataTracker.isHost = false;

        if (!string.IsNullOrWhiteSpace(selectedPaintId))
        {
            ContinueButton();
            return;
        }

        MenuState = StateManager.MainMenuState.Map;

        mainMenuPanel.SetActive(false);
        Host_OfflinePanel.SetActive(false);
        MapPanel.SetActive(true);
        EventSystem.current.GetComponent<EventSystemController>().SetPanel("MapSelectPanel");


        foreach (MapParent item in mapParents)
        {
            if (!item.PopUpItem.NormalSection.activeSelf)
            {
                item.PopUpItem.CorruptedInformer.gameObject.SetActive(false);
            }
            else
            {
                item.PopUpItem.CorruptedInformer.gameObject.SetActive(false);
            }
        }

    }
    public void HostButton()
    {
        GameDataTracker.isHost = true;
        if (!string.IsNullOrWhiteSpace(selectedPaintId))
        {
            ContinueButton();
            return;
        }

        MenuState = StateManager.MainMenuState.Map;

        Host_OfflinePanel.SetActive(false);
        mainMenuPanel.SetActive(false);
        MapPanel.SetActive(true);
        EventSystem.current.GetComponent<EventSystemController>().SetPanel("MapSelectPanel");




        foreach (MapParent item in mapParents)
        {
            if (!item.PopUpItem.NormalSection.activeSelf)
            {
                item.PopUpItem.CorruptedInformer.gameObject.SetActive(false);
            }
            else
            {
                item.PopUpItem.CorruptedInformer.gameObject.SetActive(true);
            }
        }


    }


    StateManager.MainMenuState lastBeforeWait;
    public void WaitPanelOpen()
    {
        lastBeforeWait = MenuState;
        MenuState = StateManager.MainMenuState.Wait;
        WaitPanel.SetActive(true);
    }
    public void WaitPanelClose()
    {
        MenuState = lastBeforeWait;
        WaitPanel.SetActive(false);
    }

    #endregion

    #region buttonFunc




    public void ChangeColorThemeColor(int themeColorIndex)
    {
        GameDataTracker.themeColor = themeColors[themeColorIndex];
        ColorPickBtn.GetComponent<Image>().color = themeColors[themeColorIndex];
        cursorDisplayOffline.changeColor(themeColors[themeColorIndex]);

        settingsPanel.GetComponent<SettingsController>().UpdateThemeColor();





        PlayerPrefs.SetInt("ThemeColorIndex", themeColorIndex);
    }


    public void OpenLink(string link)
    {
        Application.OpenURL(link);
    }

    public void SelectAPaint(string id)
    {

        PaintObject[] paintsArray = Resources.LoadAll<PaintObject>("Paints");

        PaintObject paint = null;

        ActivateBtnSfx();

        foreach(MapParent mp in mapParents)
        {
            if (id == mp.NormalID)
            {
                mp.transform.parent.SetAsLastSibling();
                continue;
            }
            mp.PopUpItem.gameObject.SetActive(false);
        }


        foreach (PaintObject pa in paintsArray)
        {
            if (pa.id == id)
            {
                paint = pa;
            }
        }

        if(paint == null)
        {
            Debug.LogWarning("Painting couldn't find while searching for Menu Map");
            return;
        }

        selectedPaintId = paint.id;

        ContinueBtn.gameObject.SetActive(true);
    }

    public void ContinueButton()
    {
        ActivateBtnSfx();

        if (GameDataTracker.isHost)
        {
            HostGame();
        }
        else
        {
            OfflineGame();
        }
    }

    StateManager.MainMenuState lastStateBeforeAYSSave;
    public void saveWipeBtnAYSopen(string id)
    {
        selectedPaintId = id;
        lastStateBeforeAYSSave = MenuState;
        MenuState = StateManager.MainMenuState.AYSaveWipe;
        AYSaveWipePanel.SetActive(true);
        EventSystem.current.GetComponent<EventSystemController>().SetPanel("AYSaveWipePanel");
    }

    public void saveWipeBtnAYSClose()
    {
        AYSaveWipePanel.SetActive(false);
        selectedPaintId = null;

        if(lastStateBeforeAYSSave == StateManager.MainMenuState.mainMenu)
        {
            BacktoMenuButton();
            return;
        }

        if (GameDataTracker.isHost)
        {
            HostButton();
        }
        else
        {
            OfflineButton();
        }
    }


    public void saveWipe()
    {
        AYSaveWipePanel.SetActive(false);
        if (string.IsNullOrEmpty(selectedPaintId))
        {
            return;
        }

        PaintObject[] paints = Resources.LoadAll<PaintObject>("Paints");

        foreach (PaintObject pa in paints)
        {
            if(pa.id == selectedPaintId)
            {
                bool[] paint_findCatData = new bool[pa.catsSprites.Length];
                bool[] paint_regionFillData = new bool[pa.regionSprites.Count];
                Array.Fill(paint_findCatData, false);
                Array.Fill(paint_regionFillData, false);
                float paint_timer = 0;
                bool paint_finished = false;
                int paint_hint = 2;
                bool paint_SaveSoloCorrupted = false;

                DataElement data = new(selectedPaintId, paint_regionFillData, paint_findCatData, paint_finished, paint_timer, paint_hint, paint_SaveSoloCorrupted);

                ReplayElement dataReplay = new(selectedPaintId, new Vector2[0], new int[0]);

                SaveSystem.SaveReplayElement(dataReplay);
                SaveSystem.SaveDataElement(data);
                break;
            }
        }

        StartCoroutine(saveWipeRoutine());
    }
    public void defaultSettings()
    {
        PlayerPrefs.DeleteAll();
        settingsPanel.GetComponent<SettingsController>().StartThis();

        cursorDisplayOffline.changeColor(themeColors[PlayerPrefs.GetInt("ThemeColorIndex", 2)]);
        ColorPickBtn.GetComponent<Image>().color = themeColors[PlayerPrefs.GetInt("ThemeColorIndex", 2)];
        GameDataTracker.themeColor = themeColors[PlayerPrefs.GetInt("ThemeColorIndex", 2)];

        settingsPanel.GetComponent<SettingsController>().UpdateThemeColor();
    }

    IEnumerator saveWipeRoutine()
    {
        MenuState = StateManager.MainMenuState.Wait;
        WaitPanel.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        WaitPanel.SetActive(false);
        MenuState = StateManager.MainMenuState.Map;
        AssignButtons();
    }


    public void quitButton()
    {
        SteamLeaderboard sl = FindAnyObjectByType<SteamLeaderboard>();
        Application.Quit();
    }
    private int current_tutorial;
    public void StartTutorial()
    {
        MenuState = StateManager.MainMenuState.Tutorial;


        EventSystem.current.GetComponent<EventSystemController>().SetPanel("AYSPanel");

        TutorialPanel.SetActive(true);
        current_tutorial = 0;

        for (int c = 0; c < tutorialPages.Length; c++)
        {
            tutorialPages[c].SetActive(false);
        }

        tutorialPages[0].SetActive(true);

        if (current_tutorial == tutorialPages.Length - 1)
        {
            backTutorialBtn.interactable = true;
            backTutorialBtn.gameObject.SetActive(true);

            rightTutorialBtn.interactable = false;
            rightTutorialBtn.gameObject.SetActive(false);

            leftTutorialBtn.interactable = true;
            leftTutorialBtn.gameObject.SetActive(true);
        }
        else if (current_tutorial == 0)
        {
            backTutorialBtn.interactable = false;
            backTutorialBtn.gameObject.SetActive(false);

            rightTutorialBtn.interactable = true;
            rightTutorialBtn.gameObject.SetActive(true);

            leftTutorialBtn.interactable = false;
            leftTutorialBtn.gameObject.SetActive(false);
        }
        else
        {
            backTutorialBtn.interactable = false;
            backTutorialBtn.gameObject.SetActive(false);

            rightTutorialBtn.interactable = true;
            rightTutorialBtn.gameObject.SetActive(true);

            leftTutorialBtn.interactable = true;
            leftTutorialBtn.gameObject.SetActive(true);
        }
    }
    public void TutorialRight()
    {
        current_tutorial += 1;

        tutorialPages[current_tutorial - 1].gameObject.SetActive(false);
        tutorialPages[current_tutorial].gameObject.SetActive(true);

        if(current_tutorial == tutorialPages.Length - 1)
        {
            backTutorialBtn.interactable = true;
            backTutorialBtn.gameObject.SetActive(true);

            rightTutorialBtn.interactable = false;
            rightTutorialBtn.gameObject.SetActive(false);

            leftTutorialBtn.interactable = true;
            leftTutorialBtn.gameObject.SetActive(true);
        }
        else if(current_tutorial == 0)
        {
            backTutorialBtn.interactable = false;
            backTutorialBtn.gameObject.SetActive(false);

            rightTutorialBtn.interactable = true;
            rightTutorialBtn.gameObject.SetActive(true);

            leftTutorialBtn.interactable = false;
            leftTutorialBtn.gameObject.SetActive(false);
        }
        else
        {
            backTutorialBtn.interactable = false;
            backTutorialBtn.gameObject.SetActive(false);

            rightTutorialBtn.interactable = true;
            rightTutorialBtn.gameObject.SetActive(true);

            leftTutorialBtn.interactable = true;
            leftTutorialBtn.gameObject.SetActive(true);
        }

    }
    public void TutorialLeft()
    {
        if (TutorialPanel.transform.childCount <= current_tutorial)
        {
            TutorialEnd();
        }
        current_tutorial -= 1;
        tutorialPages[current_tutorial + 1].gameObject.SetActive(false);
        tutorialPages[current_tutorial].gameObject.SetActive(true);

        if (current_tutorial == tutorialPages.Length - 1)
        {
            backTutorialBtn.interactable = true;
            backTutorialBtn.gameObject.SetActive(true);

            rightTutorialBtn.interactable = false;
            rightTutorialBtn.gameObject.SetActive(false);

            leftTutorialBtn.interactable = true;
            leftTutorialBtn.gameObject.SetActive(true);
        }
        else if (current_tutorial == 0)
        {
            backTutorialBtn.interactable = false;
            backTutorialBtn.gameObject.SetActive(false);

            rightTutorialBtn.interactable = true;
            rightTutorialBtn.gameObject.SetActive(true);

            leftTutorialBtn.interactable = false;
            leftTutorialBtn.gameObject.SetActive(false);
        }
        else
        {
            backTutorialBtn.interactable = false;
            backTutorialBtn.gameObject.SetActive(false);

            rightTutorialBtn.interactable = true;
            rightTutorialBtn.gameObject.SetActive(true);

            leftTutorialBtn.interactable = true;
            leftTutorialBtn.gameObject.SetActive(true);
        }
    }
    public void TutorialEnd()
    {
        BacktoMenuButton();

    }

    public void JoinGameBtn()
    {
        if (string.IsNullOrEmpty(lobbyCodeIF.text)) return;
        GameDataTracker.isHost = false;
        steamLobbyMenu.searchForLobby(lobbyCodeIF.text);
    }

    public void OfflineGame()
    {
        GameDataTracker.skipSave = false;
        GameDataTracker.selectedPaintID = selectedPaintId;

        GameDataTracker.isOnline = false;

        var Result = SaveSystem.LoadData(selectedPaintId);

        if(Result.Item1 == null)
        {
            Debug.LogWarning("Save Prob.");
            return;
        }

        GameDataTracker.paint_findCatData = Result.Item1.paint_catFindData;
        GameDataTracker.paint_regionFillData = Result.Item1.paint_regionColorData;
        GameDataTracker.paint_timer = Result.Item1.paintTimer;
        GameDataTracker.paint_finished = Result.Item1.paint_finished;
        GameDataTracker.paint_hint = Result.Item1.paint_hint;
        GameDataTracker.paint_SaveSoloCorrupted = Result.Item1.solo_Corrupted;



        var Sonuc = SaveSystem.LoadReplay(selectedPaintId);

        if (Sonuc.Item1 == null) return;
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

        CustomSceneChanger.Instance.ChangeScene("OfflineGamePlay");

    }
    public void HostGame()
    {
        GameDataTracker.skipSave = true;
        GameDataTracker.selectedPaintID = selectedPaintId;
        GameDataTracker.isHost = true;
        GameDataTracker.isOnline = true;


        CustomSceneChanger.Instance.ChangeScene("LobbyScene");
    }

    public void JoinGameAccepted(ulong lobbyID)
    {
        GameDataTracker.skipSave = true;
        GameDataTracker.isHost = false;
        GameDataTracker.LobbyId = lobbyID;
        GameDataTracker.isOnline = true;


        CustomSceneChanger.Instance.ChangeScene("LobbyScene");
    }
    #endregion

    public void OpenCheat()
    {
        GameDataTracker.isCheat = true;
        CheatingTxt.SetActive(true);
    }

    #region language

    public void StartLanguage()
    {

        if (PlayerPrefs.GetInt("Lan", -1) == -1)
        {
            ChangeLocale(5);
        }
        else
        {
            ChangeLocale(PlayerPrefs.GetInt("Lan", -1));
        }
    }

    public void ChangeLocale(int localeID)
    {
        StartCoroutine(SetLocale(localeID));
        Debug.Log(LocalizationSettings.AvailableLocales.Locales[localeID]);
        PlayerPrefs.SetInt("Lan", localeID);
    }
    public void LeftLocale()
    {
        int a = PlayerPrefs.GetInt("Lan");

        a -= 1;

        if (a < 0)
        {
            a = LocalizationSettings.AvailableLocales.Locales.Count - 1;
        }

        StartCoroutine(SetLocale(a));

        PlayerPrefs.SetInt("Lan", a);
    }
    public void RightLocale()
    {
        int a = PlayerPrefs.GetInt("Lan");

        a += 1;

        if (a > LocalizationSettings.AvailableLocales.Locales.Count - 1)
        {
            a = 0;
        }

        StartCoroutine(SetLocale(a));

        PlayerPrefs.SetInt("Lan", a);
    }

    IEnumerator SetLocale(int _localeID)
    {
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[_localeID];
    }
    #endregion


}
