using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
public class CursorDisplay : MonoBehaviour
{

    public RectTransform fakeCursor;
    public PlayerController ownedPlayer;


    public Image fillColor;
    public Image catFindDefault;

    private void Start()
    {
        fillColor.gameObject.SetActive(false);
    }



    private void Update()
    {
        if(ownedPlayer == null)
        {
            closeCursor();
            return;
        }
    }


    public void CursorMovement(Vector3 mousePos)
    {
        fakeCursor.anchoredPosition = mousePos;
    }
    public void CursorMovementNonLocal(Vector3 mousePos)
    {
//        Debug.Log(ownedPlayer.gameObject.name + ": " +  mousePos);
        fakeCursor.position = new Vector3(mousePos.x, mousePos.y, 0);
    }
    public void changeColor(Color color)
    {
        fillColor.color = color;
        catFindDefault.color = color;
        transform.GetChild(1).GetComponent<Image>().color = color;
    }

    public void closeCursor()
    {
        Cursor.visible = true;
        Destroy(this.gameObject);
    }
    public void DisplayTimer(float ratio)
    {
        transform.GetChild(1).gameObject.SetActive(true);
        transform.GetChild(1).GetComponent<Image>().fillAmount = ratio;
    }
    public void CloseTimer()
    {
        transform.GetChild(1).gameObject.SetActive(false);
    }
    public void ChangeMode(StateManager.GameMode gMode)
    {
        if (gMode == StateManager.GameMode.fillColor)
        {
            catFindDefault.gameObject.SetActive(false);
            fillColor.gameObject.SetActive(true);
        }
        else if (gMode == StateManager.GameMode.findCat)
        {
            catFindDefault.gameObject.SetActive(true);
            fillColor.gameObject.SetActive(false);
        }
    }

}
