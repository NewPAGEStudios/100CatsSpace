using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class InputManager : MonoBehaviour
{
    private static InputManager _instance;
    public static InputManager Instance
    {
        get { return _instance; }
    }

    private InputActionMain _action;

    public bool mainInputTriggered;

    //Controlls
    public bool MoveMenu { get; private set; }
    public bool panning{ get; private set; }

    public float holdTime = 0.4f;
    public float holdTimeWait = 0.1f;

    public float GamePadRightStickSens;
    public float GamePadLeftStickSens;
    public float GamePadZommSens;

    private float ThirdInteractionHoldTimer;
    private bool ThirdInteractionStarted;

    Vector2 currentMousePos = Vector2.zero;

    public Sprite Keyboard_MouseIcon;
    public Sprite GamePadIcon;

    private void Awake()
    {
        _action = new InputActionMain();
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            _instance = this;
        }

        DontDestroyOnLoad(this);
        currentMousePos = Mouse.current.position.ReadValue();

        SceneManager.sceneLoaded += OnSceneLoad;
    }
    public void OnSceneLoad(Scene activeScene, LoadSceneMode loadSceneMode)
    {

    }


    #region Event Activaiton
    public void Activate()
    {
        #region KeyboardMouseEvents
        _action.GeneralMap.PrimaryInteract.performed += MainInteractPerformed;
        _action.GeneralMap.ThirdInteract.started += ThirdInteractStarted;
        _action.GeneralMap.ThirdInteract.performed += ThirdInteractPerformed;
        _action.GeneralMap.ThirdInteractPress.performed += ThirdInteractPerformedPress;
        _action.GeneralMap.SecondaryInteract.performed += ColorModeOpenningPerformed;
        _action.GeneralMap.Pan.performed += panPerformed;
        _action.GeneralMap.Pan.canceled += panCanceled;
        _action.GeneralMap.BackBtn.started += BackBtnPerformed;

        ThirdInteractionHoldTimer = 0;
        #endregion
    }
    public void Deactivate()
    {
        #region KeyboardMouseEvents
        _action.GeneralMap.PrimaryInteract.performed -= MainInteractPerformed;
        _action.GeneralMap.ThirdInteract.started -= ThirdInteractStarted;
        _action.GeneralMap.ThirdInteract.performed -= ThirdInteractPerformed;
        _action.GeneralMap.ThirdInteractPress.performed -= ThirdInteractPerformedPress;
        _action.GeneralMap.SecondaryInteract.performed -= ColorModeOpenningPerformed;
        _action.GeneralMap.Pan.performed -= panPerformed;
        _action.GeneralMap.Pan.canceled -= panCanceled;
        _action.GeneralMap.BackBtn.started -= BackBtnPerformed;

        ThirdInteractionHoldTimer = 0;
        #endregion
    }

    #endregion


    private void OnEnable()
    {
        _action.Enable();
        Activate();
    }
    private void OnDisable()
    {
        Deactivate();
        _action.Disable();
    }

    private void Update()
    {
        //HoldTimeCheck
        if (ThirdInteractionStarted)
        {
            if(ThirdInteractionHoldTimer < holdTimeWait)
            {
                ThirdInteractionHoldTimer += Time.deltaTime;
            }
            else if (ThirdInteractionHoldTimer < holdTime + holdTimeWait)
            {
                ThirdInteractionHoldTimer += Time.deltaTime;

                if (SceneManager.GetActiveScene().name == "OfflineGamePlay") GameController.Instance.LocalPlayerOfflinePlayer.DisplayHoldOnDisplay(ThirdInteractionHoldTimer, holdTime + holdTimeWait);
                else if (SceneManager.GetActiveScene().name == "OnlineGamePlay") GameController.Instance.LocalPlayerOnlinePlayer.DisplayHoldOnDisplay(ThirdInteractionHoldTimer, holdTime + holdTimeWait);
            }
            else
            {
                ThirdInteractionStarted = false;
                ThirdInteractionHoldTimer = 0;

                if (SceneManager.GetActiveScene().name == "OfflineGamePlay") GameController.Instance.LocalPlayerOfflinePlayer.CloseHoldOnDisplay();
                else if (SceneManager.GetActiveScene().name == "OnlineGamePlay") GameController.Instance.LocalPlayerOnlinePlayer.CloseHoldOnDisplay();

                if (SceneManager.GetActiveScene().name == "OfflineGamePlay") GameController.Instance.LocalPlayerOfflinePlayer.ThirdInterract();
                else if (SceneManager.GetActiveScene().name == "OnlineGamePlay") GameController.Instance.LocalPlayerOnlinePlayer.ThirdInterract();

            }
        }


        //GamePad
        if (Application.isFocused)
        {
            if (_action.GeneralMap.CursorMove.ReadValue<Vector2>() == Vector2.zero)
            {
                currentMousePos = Mouse.current.position.ReadValue();
                return;
            }

            currentMousePos += (_action.GeneralMap.CursorMove.ReadValue<Vector2>() * GamePadRightStickSens);

            currentMousePos.x = Mathf.Clamp(currentMousePos.x, 0, Screen.width);
            currentMousePos.y = Mathf.Clamp(currentMousePos.y, 0, Screen.height);

            Mouse.current.WarpCursorPosition(currentMousePos);
            InputState.Change(Mouse.current.position, currentMousePos);
        }

    }

    #region Events
    private void MainInteractPerformed(InputAction.CallbackContext context)
    {
        mainInputTriggered = true;
    }
    private void ThirdInteractStarted(InputAction.CallbackContext context)
    {
        ThirdInteractionStarted = true;
    }
    private void ThirdInteractPerformed(InputAction.CallbackContext context)
    {
        //if (!ThirdInteractionStarted) return;

        Debug.Log("ThirdInteraction");

        ThirdInteractionStarted = false;
        ThirdInteractionHoldTimer = 0;

        if (SceneManager.GetActiveScene().name == "OfflineGamePlay") GameController.Instance.LocalPlayerOfflinePlayer.CloseHoldOnDisplay();
        else if (SceneManager.GetActiveScene().name == "OnlineGamePlay") GameController.Instance.LocalPlayerOnlinePlayer.CloseHoldOnDisplay();

    }
    private void panCanceled(InputAction.CallbackContext context)
    {
        panning = false;
    }
    private void panPerformed(InputAction.CallbackContext context)
    {
        panning = true;
    }
    private void ColorModeOpenningPerformed(InputAction.CallbackContext context)
    {
        if (SceneManager.GetActiveScene().name == "OfflineGamePlay") GameController.Instance.LocalPlayerOfflinePlayer.SecondaryInterract();
        else if (SceneManager.GetActiveScene().name == "OnlineGamePlay") GameController.Instance.LocalPlayerOnlinePlayer.SecondaryInterract();



    }
    private void ThirdInteractPerformedPress(InputAction.CallbackContext context)
    {
        if (SceneManager.GetActiveScene().name == "OfflineGamePlay") GameController.Instance.LocalPlayerOfflinePlayer.ThirdInterract();
        else if (SceneManager.GetActiveScene().name == "OnlineGamePlay") GameController.Instance.LocalPlayerOnlinePlayer.ThirdInterract();
    }

    private void BackBtnPerformed(InputAction.CallbackContext context)
    {
        if (SceneManager.GetActiveScene().name == "Menu") MenuController.instance.BackBtn();
        else if (SceneManager.GetActiveScene().name == "OfflineGamePlay" || SceneManager.GetActiveScene().name == "OnlineGamePlay") GameController.Instance.BackBtn();
        else if (SceneManager.GetActiveScene().name == "LobbyScene") LobbyController.instance.BackBtn();
    }

    #endregion

    public bool getPan()
    {
        return panning;
    }

    public bool ChatOpenBtnPressed()
    {
        return _action.GeneralMap.ChatOpen.triggered;
    }
    public bool ChatEnterPressed()
    {
        return _action.GeneralMap.ChatConfirmIF.triggered;
    }

    public Vector2 cursorPosition()
    {
        return currentMousePos;
    }
    public float zoomAction()
    {
        return _action.GeneralMap.Zoom.ReadValue<float>();
    }
    public bool CheatPressed()
    {
        return _action.GeneralMap.Cheat.IsPressed();
    }

    public Vector2 CamMovement()
    {
        return _action.GeneralMap.MoveCam.ReadValue<Vector2>() * GamePadLeftStickSens;
    }

    #region Settings

    public void stopInput()
    {
        Debug.Log("InputStopped");
        _action.Disable();
        EventSystem.current.GetComponent<EventSystemController>().DisableUIInput();
    }
    public void ContInput()
    {
        Debug.Log("InputContinued");
        _action.Enable();
        EventSystem.current.GetComponent<EventSystemController>().EnableUIInput();
    }

    #endregion
}
