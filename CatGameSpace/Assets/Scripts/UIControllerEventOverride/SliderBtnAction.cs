using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderBtnAction : MonoBehaviour, IPointerDownHandler , IPointerUpHandler
{
    public bool isActionShrinking = true;


    private float sizeDuplicator = .8f;

    private RectTransform targetRect;

    private Vector2 sizeDown;
    private Vector2 sizeUp;

    void Start()
    {
        targetRect = GetComponent<Slider>().handleRect;

        sizeUp = targetRect.localScale;
        sizeDown = targetRect.localScale * sizeDuplicator;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (TryGetComponent<Button>(out Button but) && !but.interactable) return;

        if (isActionShrinking)
        {
            targetRect.localScale = sizeDown;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isActionShrinking)
        {
            targetRect.localScale = sizeUp;
        }
        EventSystem.current.SetSelectedGameObject(EventSystem.current.GetComponent<EventSystemController>().GetLastSelected());
    }
}
