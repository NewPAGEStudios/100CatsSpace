using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapButtons : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public MapParent mapParent;

    public InfoPopUpItem popUpParent;

    public string id;

    private void Start()
    {
        mapParent = popUpParent.mapParent;

        if (transform.parent.gameObject.name == "Normal") id = mapParent.NormalID;

        //if (id == "Sahte")
        //{
        //    GetComponent<Button>().interactable = false;
        //}
        //else if (SceneManager.GetActiveScene().name == "Menu")
        //{
        //    GetComponent<Button>().onClick.AddListener(() => MenuController.instance.SelectAPaint(id));
        //}
        //else if (SceneManager.GetActiveScene().name == "LobbyScene") 
        //{
        //    GetComponent<Button>().onClick.AddListener(() => LobbyController.instance.SelectPaintID(id));
        //}

    }
    public void OnPointerClick(PointerEventData eventData)
    {
        
        if(id == "Sahte")
        {
            if (SceneManager.GetActiveScene().name == "Menu") MenuController.instance.ActivateMapErrorSfx();
            else if (SceneManager.GetActiveScene().name == "LobbyScene") LobbyController.instance.ActivateMapErrorSfx();
        }
        else
        {
            if (SceneManager.GetActiveScene().name == "Menu") MenuController.instance.ActivateMapSfx();
            else if (SceneManager.GetActiveScene().name == "LobbyScene") LobbyController.instance.ActivateMapSfx();
        }


    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

        if (SceneManager.GetActiveScene().name == "Menu")
        {
            //if (MenuController.instance.HoveredElement != gameObject)
            //{
            //    ExecuteEvents.Execute<IPointerExitHandler>(
            //        MenuController.instance.HoveredElement,
            //        new PointerEventData(EventSystem.current),
            //        ExecuteEvents.pointerExitHandler
            //    );
            //}

            //ExecuteEvents.Execute<IPointerEnterHandler>(mapParent.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerEnterHandler);

            //MenuController.instance.HoveredElement = gameObject;
            //MenuController.instance.ActivateHoverSfx();
        }
        else if(SceneManager.GetActiveScene().name == "LobbyScene")
        {
            //if (LobbyController.instance.HoveredElement != gameObject)
            //{
            //    ExecuteEvents.Execute<IPointerExitHandler>(
            //        LobbyController.instance.HoveredElement,
            //        new PointerEventData(EventSystem.current),
            //        ExecuteEvents.pointerExitHandler
            //    );
            //}

            //ExecuteEvents.Execute<IPointerEnterHandler>(mapParent.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerEnterHandler);

            //LobbyController.instance.HoveredElement = gameObject;
            //LobbyController.instance.ActivateHoverSfx();
        }

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(id)) return;

    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }
}
