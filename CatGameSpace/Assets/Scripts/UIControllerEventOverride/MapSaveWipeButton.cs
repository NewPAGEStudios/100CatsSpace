using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapSaveWipeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler ,ISelectHandler,IDeselectHandler
{
    public InfoPopUpItem popUpParent;


    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            GetComponent<Button>().onClick.AddListener(() => MenuController.instance.saveWipeBtnAYSopen(popUpParent.mapParent.NormalID));
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {



    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            popUpParent.gameObject.SetActive(true);

            MenuController.instance.HoveredElement = gameObject;
            MenuController.instance.ActivateHoverSfx();
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {

    }
}