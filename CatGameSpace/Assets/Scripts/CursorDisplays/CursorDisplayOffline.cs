using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CursorDisplayOffline : MonoBehaviour
{
    public RectTransform fakeCursor;

    public Image fillColor;
    public Image catFindDefault;

    private void Start()
    {
        fillColor.gameObject.SetActive(false);
        // Mobilde imleç yok; sadece basılı tutma göstergesi parmağın altında görünür
        if (InputManager.IsTouchMode) catFindDefault.gameObject.SetActive(false);
    }

    public void CursorMovement(Vector3 mousePos)
    {
        fakeCursor.anchoredPosition = mousePos;
    }
    public void changeColor(Color color)
    {
        fillColor.color = color;
        catFindDefault.color = color;
        transform.GetChild(1).GetComponent<Image>().color = color;
    }

    public void DisplayTimer(float ratio)
    {
        if (!transform.GetChild(1).gameObject.activeSelf)
        {
            transform.GetChild(1).gameObject.SetActive(true);
        }
        transform.GetChild(1).GetComponent<Image>().fillAmount = ratio;
    }
    public void CloseTimer()
    {
        transform.GetChild(1).gameObject.SetActive(false);
    }
    public void ChangeMode(StateManager.GameMode gMode)
    {
        if (InputManager.IsTouchMode)
        {
            catFindDefault.gameObject.SetActive(false);
            fillColor.gameObject.SetActive(false);
            return;
        }

        if (gMode == StateManager.GameMode.fillColor)
        {
            catFindDefault.gameObject.SetActive(false);
            fillColor.gameObject.SetActive(true);
        }
        else if(gMode == StateManager.GameMode.findCat)
        {
            catFindDefault.gameObject.SetActive(true);
            fillColor.gameObject.SetActive(false);
        }
    }

}
