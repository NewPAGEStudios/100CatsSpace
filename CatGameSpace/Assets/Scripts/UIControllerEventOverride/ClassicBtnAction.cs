using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ClassicBtnAction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler,IDeselectHandler
{
    public bool isActionShrinking = true;
    public bool isSceneChange = false;
    public bool isAnimation = true;
    public bool isFullThreshold = false;

    private float sizeDuplicator = .8f;

    private RectTransform targetRect;


    private Vector2 sizeDown;
    private Vector2 sizeUp;

    private void Start()
    {
        if (TryGetComponent<Slider>(out Slider sli))
        {
            targetRect = sli.handleRect;
        }
        else
        {
            targetRect = GetComponent<RectTransform>();
        }
        if (isFullThreshold) ((Image)GetComponent<Button>().targetGraphic).alphaHitTestMinimumThreshold = .1f;
        sizeUp = targetRect.localScale;
        sizeDown = targetRect.localScale * sizeDuplicator;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            MenuController.instance.ActivateBtnSfx();
        }
        else if (SceneManager.GetActiveScene().name == "LobbyScene")
        {
            LobbyController.instance.ActivateBtnSfx();
        }
        else if (SceneManager.GetActiveScene().name == "OfflineGamePlay" || SceneManager.GetActiveScene().name == "OnlineGamePlay")
        {
            GameController.Instance.ActivateBtnSfx();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (TryGetComponent<Button>(out Button but) && !but.interactable) return;

        if (isActionShrinking)
        {
            targetRect.localScale = sizeDown;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //EventSystem.current.SetSelectedGameObject(gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {

        if (isSceneChange) return;
        if (EventSystem.current != null)
        {
            //            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isActionShrinking)
        {
            targetRect.localScale = sizeUp;
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        Debug.Log("OnSelectÇalýþtý : " + gameObject.name);

        if (isAnimation)
        {
            transform.GetChild(0).gameObject.GetComponent<Animator>().SetBool("Hovering", true);
        }

        if (SceneManager.GetActiveScene().name == "Menu")
        {
            MenuController.instance.ActivateHoverSfx();
        }
        else if (SceneManager.GetActiveScene().name == "LobbyScene")
        {
            LobbyController.instance.ActivateHoverSfx();
        }
        else if (SceneManager.GetActiveScene().name == "OfflineGamePlay" || SceneManager.GetActiveScene().name == "OnlineGamePlay")
        {
            GameController.Instance.ActivateHoverSfx();
        }

    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (isAnimation)
        {
            transform.GetChild(0).gameObject.GetComponent<Animator>().SetBool("Hovering", false);
        }

    }
}
