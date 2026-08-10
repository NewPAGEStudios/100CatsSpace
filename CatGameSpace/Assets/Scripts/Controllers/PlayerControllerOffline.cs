using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerControllerOffline : MonoBehaviour
{
    GameController gameController;
    ColourControll colourControll;
    InputManager inputManager;
    CameraZoom cameraZoom;

    public CursorDisplayOffline CursorDisplayOffline;

    public string playerName;
    public Color color;



    // Start is called before the first frame update
    void Start()
    {
        color = GameDataTracker.themeColor;

        Cursor.visible = false;

        playerName = SteamFriends.GetPersonaName();
        CursorDisplayOffline.changeColor(color);

        gameController = GameController.Instance;
        colourControll = ColourControll.Instance;
        inputManager = InputManager.Instance;
        cameraZoom = CameraZoom.Instance;


    }
    

    // Update is called once per frame
    void Update()
    {

        CursorDisplayOffline.CursorMovement(getMousePosOnUI());

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
        else if (gameController.gameState == StateManager.GameState.EndGame)
        {
            if (inputManager.getPan() && !EventSystem.current.IsPointerOverGameObject()) cameraZoom.panCam(inputManager.cursorPosition());
            else cameraZoom.stopPanCam();

            if (inputManager.CamMovement() != Vector2.zero) cameraZoom.MoveCam(inputManager.CamMovement());

            if (inputManager.zoomAction() != 0 && !EventSystem.current.IsPointerOverGameObject()) cameraZoom.zoomCam(inputManager.zoomAction());

            if (inputManager.ChatOpenBtnPressed()) gameController.ChangeChatStatus();
        }


    }

    private Vector2 getMousePosOnUI()
    {
        // Ekran pozisyonunu al
        Vector2 localPoint;
        Vector2 vec = inputManager.cursorPosition();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(CursorDisplayOffline.transform.parent.GetComponent<RectTransform>(), vec, Camera.main, out localPoint);
        return localPoint;
    }

    public void MainInterract()
    {
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
        if (gameController.gameState != StateManager.GameState.InGame) return;

        gameController.SecClickAction();
    }

    public void DisplayHoldOnDisplay(float currentTime,float HoldTime)
    {
        if (gameController.gameState != StateManager.GameState.InGame) return;

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (GameController.Instance.gameMode != StateManager.GameMode.fillColor) return;

        CursorDisplayOffline.DisplayTimer((HoldTime - currentTime) / HoldTime);
    }
    public void CloseHoldOnDisplay()
    {
        if (gameController.gameState != StateManager.GameState.InGame) return;
        CursorDisplayOffline.CloseTimer();
    }
}
