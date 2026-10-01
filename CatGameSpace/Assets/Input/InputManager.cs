using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
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

    // Mobil: dokun = tıkla, basılı tut = ThirdInteract, tek parmak sürükle = kaydır, iki parmak = zoom
    public static bool IsTouchMode => Application.isMobilePlatform;

    [Header("Touch")]
    [Tooltip("Dokunuşun sürükleme sayılması için gereken hareket (dp)")]
    public float touchDragThreshold = 12f;

    private bool touchGestureActive;
    private bool touchStartedOverUI;
    private bool touchDragging;
    private Vector2 touchStartPos;
    private Vector2 touchPanDelta;
    private float touchPinchRatio = 1f;
    private bool pinchActive;
    private float lastPinchDistance;
    private Vector2 lastPinchMid;
    private readonly List<RaycastResult> uiRaycastResults = new List<RaycastResult>();

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
        if (Mouse.current != null) currentMousePos = Mouse.current.position.ReadValue();

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
        if (IsTouchMode) EnhancedTouchSupport.Enable();
    }
    private void OnDisable()
    {
        Deactivate();
        _action.Disable();
        if (IsTouchMode) EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        if (IsTouchMode) UpdateTouch();

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
        if (Application.isFocused && !IsTouchMode && Mouse.current != null)
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

    #region Touch
    private void UpdateTouch()
    {
        touchPanDelta = Vector2.zero;
        touchPinchRatio = 1f;

        var touches = Touch.activeTouches;
        if (touches.Count == 0)
        {
            pinchActive = false;
            if (touchGestureActive) CancelTouchHold();
            touchGestureActive = false;
            return;
        }

        if (touches.Count >= 2)
        {
            // İki parmak: pinch zoom + kaydırma, tap/hold iptal
            touchDragging = true;
            CancelTouchHold();

            Vector2 p0 = touches[0].screenPosition;
            Vector2 p1 = touches[1].screenPosition;
            float dist = Vector2.Distance(p0, p1);
            Vector2 mid = (p0 + p1) * 0.5f;

            if (pinchActive && dist > 0f && lastPinchDistance > 0f && !touchStartedOverUI)
            {
                touchPinchRatio = lastPinchDistance / dist;
                touchPanDelta = mid - lastPinchMid;
            }
            pinchActive = true;
            lastPinchDistance = dist;
            lastPinchMid = mid;
            return;
        }
        pinchActive = false;

        Touch t = touches[0];
        currentMousePos = t.screenPosition;

        switch (t.phase)
        {
            case TouchPhase.Began:
                touchGestureActive = true;
                touchDragging = false;
                touchStartPos = t.screenPosition;
                touchStartedOverUI = IsScreenPointOverUI(t.screenPosition);
                if (!touchStartedOverUI)
                {
                    ThirdInteractionStarted = true;
                    ThirdInteractionHoldTimer = 0;
                }
                break;

            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                if (!touchGestureActive || touchStartedOverUI) break;

                float threshold = touchDragThreshold * (Screen.dpi > 0 ? Screen.dpi / 160f : 1f);
                if (!touchDragging && Vector2.Distance(t.screenPosition, touchStartPos) > threshold)
                {
                    touchDragging = true;
                    CancelTouchHold();
                }
                if (touchDragging) touchPanDelta = t.delta;
                break;

            case TouchPhase.Ended:
                // Basılı tutma tamamlanmadıysa ve sürüklenmediyse: tap
                if (touchGestureActive && !touchStartedOverUI && !touchDragging && ThirdInteractionStarted)
                {
                    mainInputTriggered = true;
                }
                CancelTouchHold();
                touchGestureActive = false;
                break;

            case TouchPhase.Canceled:
                CancelTouchHold();
                touchGestureActive = false;
                break;
        }
    }

    private void CancelTouchHold()
    {
        if (!ThirdInteractionStarted) return;

        ThirdInteractionStarted = false;
        ThirdInteractionHoldTimer = 0;

        if (SceneManager.GetActiveScene().name == "OfflineGamePlay") GameController.Instance.LocalPlayerOfflinePlayer.CloseHoldOnDisplay();
        else if (SceneManager.GetActiveScene().name == "OnlineGamePlay") GameController.Instance.LocalPlayerOnlinePlayer.CloseHoldOnDisplay();
    }

    private bool IsScreenPointOverUI(Vector2 screenPos)
    {
        if (EventSystem.current == null) return false;

        PointerEventData data = new PointerEventData(EventSystem.current) { position = screenPos };
        uiRaycastResults.Clear();
        EventSystem.current.RaycastAll(data, uiRaycastResults);
        return uiRaycastResults.Count > 0;
    }

    // Tek parmak (veya iki parmak ortası) sürükleme miktarı, piksel
    public Vector2 touchPan()
    {
        return touchPanDelta;
    }
    // >1 uzaklaş, <1 yakınlaş (kamera boyutu çarpanı)
    public float touchPinch()
    {
        return touchPinchRatio;
    }
    #endregion

    #region Events
    private void MainInteractPerformed(InputAction.CallbackContext context)
    {
        if (IsTouchMode) return;
        mainInputTriggered = true;
    }
    private void ThirdInteractStarted(InputAction.CallbackContext context)
    {
        if (IsTouchMode) return;
        ThirdInteractionStarted = true;
    }
    private void ThirdInteractPerformed(InputAction.CallbackContext context)
    {
        if (IsTouchMode) return;
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
        if (IsTouchMode) return;
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
