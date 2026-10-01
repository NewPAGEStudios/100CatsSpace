using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapParent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
    public string NormalID;

    public InfoPopUpItem PopUpItem;

    private void Start()
    {
        if (TryGetComponent<Image>(out Image im))
        {
            im.alphaHitTestMinimumThreshold = .1f;
        }
        if (NormalID == "Sahte")
        {
            GetComponent<Button>().interactable = false;
        }
        else if (SceneManager.GetActiveScene().name == "Menu")
        {
            GetComponent<Button>().onClick.AddListener(() => MenuController.instance.SelectAPaint(NormalID));
        }
        else if (SceneManager.GetActiveScene().name == "LobbyScene")
        {
            GetComponent<Button>().onClick.AddListener(() => LobbyController.instance.SelectPaintID(NormalID));
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    //public void OnPointerEnter(PointerEventData eventData)
    //{
    //    transform.parent.SetAsLastSibling();
    //    if (SceneManager.GetActiveScene().name == "Menu")
    //    {
    //        if (MenuController.instance.HoveredElement != gameObject)
    //        {
    //            ExecuteEvents.Execute<IPointerExitHandler>(
    //                MenuController.instance.HoveredElement,
    //                new PointerEventData(EventSystem.current),
    //                ExecuteEvents.pointerExitHandler
    //            );
    //        }


    //        MenuController.instance.HoveredElement = gameObject;
    //        MenuController.instance.ActivateHoverSfx();

    //        foreach (MapParent q in MenuController.instance.mapParents)
    //        {
    //            if (!string.IsNullOrEmpty(MenuController.instance.selectedPaintId) && MenuController.instance.selectedPaintId == q.NormalID)
    //            {
    //                q.transform.parent.SetAsLastSibling();
    //                continue;
    //            }

    //            if (q == this) continue;
    //            q.PopUpItem.gameObject.SetActive(false);
    //        }
    //        PopUpItem.gameObject.SetActive(true);
    //    }
    //    else if (SceneManager.GetActiveScene().name == "LobbyScene")
    //    {
    //        if (LobbyController.instance.HoveredElement != gameObject)
    //        {
    //            ExecuteEvents.Execute<IPointerExitHandler>(
    //                LobbyController.instance.HoveredElement,
    //                new PointerEventData(EventSystem.current),
    //                ExecuteEvents.pointerExitHandler
    //            );
    //        }


    //        LobbyController.instance.HoveredElement = gameObject;
    //        LobbyController.instance.ActivateHoverSfx();

    //        foreach (MapParent q in LobbyController.instance.mapParents)
    //        {
    //            if (q == this) continue;
    //            q.PopUpItem.gameObject.SetActive(false);
    //        }
    //        PopUpItem.gameObject.SetActive(true);

    //    }



    //}

    public void OnPointerExit(PointerEventData eventData)
    {

    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    public void OnSelect(BaseEventData eventData)
    {
        Debug.Log("OnSelectÇalıştı : " + gameObject.name);

        OpenPopUp();
    }

    public void OnDeselect(BaseEventData eventData)
    {

    }

    public void OpenPopUp()
    {
        transform.parent.SetAsLastSibling();
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            MenuController.instance.ActivateHoverSfx();

            foreach (MapParent q in MenuController.instance.mapParents)
            {
                if (!string.IsNullOrEmpty(MenuController.instance.selectedPaintId) && MenuController.instance.selectedPaintId == q.NormalID)
                {
                    q.transform.parent.SetAsLastSibling();
                    continue;
                }

                if (q == this) continue;
                q.PopUpItem.gameObject.SetActive(false);
            }
            PopUpItem.gameObject.SetActive(true);
        }
        else if (SceneManager.GetActiveScene().name == "LobbyScene")
        {

            LobbyController.instance.HoveredElement = gameObject;
            LobbyController.instance.ActivateHoverSfx();

            foreach (MapParent q in LobbyController.instance.mapParents)
            {
                if (q == this) continue;
                q.PopUpItem.gameObject.SetActive(false);
            }
            PopUpItem.gameObject.SetActive(true);

        }
    }


}
