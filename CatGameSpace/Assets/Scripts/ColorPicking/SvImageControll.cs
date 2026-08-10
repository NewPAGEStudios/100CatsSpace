using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SvImageControll : MonoBehaviour, IDragHandler,IPointerClickHandler
{

    [SerializeField]
    private Image pickerImage;

    private RawImage SVImage;

    private ColorPickingSystem colorPickingSystem;

    private RectTransform rectTransform, pickerTransform;

    private void Awake()
    {
        SVImage = GetComponent<RawImage>();
        colorPickingSystem = FindObjectOfType<ColorPickingSystem>();
        rectTransform = GetComponent<RectTransform>();

        pickerTransform=pickerImage.GetComponent<RectTransform>();
        pickerTransform.localPosition = new Vector2(-(rectTransform.sizeDelta.x * .5f), -(rectTransform.sizeDelta.y * .5f));

    }

    void UpdateColor()
    {

        Vector2 pos = getMousePosOnUI();

        float deltaX = rectTransform.sizeDelta.x * .5f;
        float deltaY = rectTransform.sizeDelta.y * .5f;


        if (pos.x < -deltaX)
        {
            pos.x = -deltaX;
        }
        else if (pos.x > deltaX)
        {
            pos.x = deltaX;
        }

        if (pos.y < -deltaY)
        {
            pos.y = -deltaY;
        }
        else if(pos.y > deltaY)
        {
            pos.y = deltaY;
        }
        float x = pos.x + deltaX;
        float y = pos.y + deltaY;

        float xNorm = x / rectTransform.sizeDelta.x;
        float yNorm = y / rectTransform.sizeDelta.y;

        pickerTransform.localPosition = new Vector2(pos.x,pos.y);
        pickerImage.color = Color.HSVToRGB(0, 0, 1 - yNorm);

        colorPickingSystem.SetSV(xNorm, yNorm);
    }

    private Vector2 getMousePosOnUI()
    {
        // Ekran pozisyonunu al
        Vector2 localPoint;
        Vector2 vec = InputManager.Instance.cursorPosition();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.GetComponent<RectTransform>(), vec, Camera.main, out localPoint);
        return localPoint;
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateColor();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UpdateColor();
    }
}
