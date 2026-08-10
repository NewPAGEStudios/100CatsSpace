using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputFieldBtnAction : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,ISelectHandler,IDeselectHandler,ISubmitHandler
{
    public bool isActionShrinking = true;


    private float sizeDuplicator = .8f;

    private RectTransform targetRect;

    private Vector2 sizeDown;
    private Vector2 sizeUp;

    void Start()
    {
        targetRect = GetComponent<RectTransform>();

        sizeUp = targetRect.localScale;
        sizeDown = targetRect.localScale * sizeDuplicator;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }

    public void OnSelect(BaseEventData eventData)
    {
        Debug.Log(gameObject.name + " SELECTED");
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if(EventSystem.current.currentSelectedGameObject == this) EventSystem.current.SetSelectedGameObject(EventSystem.current.GetComponent<EventSystemController>().GetLastSelected());

    }

    public void OnSubmit(BaseEventData eventData)
    {
    }

    public void OnEndEdit()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            Debug.Log("Tried to Submit");
            GetComponent<InputfieldSelectBtnHolder>().enterBtn.onClick.Invoke();
        }
    }
}
