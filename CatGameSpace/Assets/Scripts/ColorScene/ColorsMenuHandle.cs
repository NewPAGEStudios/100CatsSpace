using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorsMenuHandle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    private int onePageElementNumber;
    public int currentPage = 0;

    public int opennedColor = -1;

    public bool isHovered { get; private set; }

    public void OnPointerEnter(PointerEventData ped)
    {
        isHovered = true;
    }
    public void OnPointerExit(PointerEventData ped)
    {
        isHovered = false;
    }



    public void SetColorMenuProperties()
    {
        float x =  transform.GetChild(0).GetComponent<GridLayoutGroup>().cellSize.x;
        float width = transform.GetComponent<RectTransform>().sizeDelta.x - 30;

        Debug.Log(x + "/" + width);

        onePageElementNumber = Mathf.FloorToInt(width / x);


        float remainings = width - onePageElementNumber * x;
        remainings /= onePageElementNumber - 1;
        transform.GetChild(0).GetComponent<GridLayoutGroup>().spacing = new Vector2(remainings, 0);

        for (int i = onePageElementNumber; i < transform.GetChild(0).childCount; i++)
        {
            transform.GetChild(0).GetChild(i).gameObject.SetActive(false);
        }

    }


    public void PageUp()
    {
        int allChilds = transform.GetChild(0).childCount;
        int topPages = Mathf.FloorToInt((float)allChilds / (float)onePageElementNumber);

        currentPage += 1;

        if (currentPage > topPages)
        {
            currentPage = 0;
        }

        for (int i = 0; i < allChilds; i++)
        {
            transform.GetChild(0).GetChild(i).gameObject.SetActive(false);
        }

        int startID = onePageElementNumber * currentPage;

        for (int c = 0; c < onePageElementNumber; c++)
        {
            if (startID >= allChilds)
            {
                break;
            }
            transform.GetChild(0).GetChild(startID).gameObject.SetActive(true);
            startID++;
        }
    }
    public void PageDown()
    {
        int allChilds = transform.GetChild(0).childCount;
        int topPages = Mathf.FloorToInt((float)allChilds / (float)onePageElementNumber);

        currentPage -= 1;

        if (currentPage < 0)
        {
            currentPage = topPages;
        }

        for (int i = 0; i < allChilds; i++)
        {
            transform.GetChild(0).GetChild(i).gameObject.SetActive(false);
        }

        int startID = onePageElementNumber * currentPage;

        for (int c = 0; c < onePageElementNumber; c++)
        {
            if (startID >= allChilds)
            {
                break;
            }
            transform.GetChild(0).GetChild(startID).gameObject.SetActive(true);
            startID++;
        }
    }

    public void UpdatePage()
    {
        int allChilds = transform.GetChild(0).childCount;
        for (int i = 0; i < allChilds; i++)
        {
            transform.GetChild(0).GetChild(i).gameObject.SetActive(false);
        }


        if (opennedColor >= 0)
        {
            currentPage = opennedColor / onePageElementNumber;
        }

        int startID = onePageElementNumber * currentPage;

        for (int c = 0; c < onePageElementNumber; c++)
        {
            if (startID >= allChilds)
            {
                break;
            }
            transform.GetChild(0).GetChild(startID).gameObject.SetActive(true);
            startID++;
        }
    }

    public void UpdateColors(int id,string txt)
    {
        for (int c = 0; c < transform.GetChild(0).childCount; c++)
        {
            int colonIndex = transform.GetChild(0).GetChild(c).gameObject.name.LastIndexOf(':');
            if (colonIndex >= 0 && colonIndex < transform.GetChild(0).GetChild(c).gameObject.name.Length - 1)
            {
                string numberStr = transform.GetChild(0).GetChild(c).gameObject.name.Substring(colonIndex + 1);
                if (int.TryParse(numberStr, out int number))
                {
                    if (number == id)
                    {
                        if (txt == "0")
                        {
                            DestroyImmediate(transform.GetChild(0).GetChild(c).gameObject);
                            opennedColor = -1;
                            UpdatePage();
                        }
                        else
                        {
                            transform.GetChild(0).GetChild(c).GetChild(1).GetComponent<TextMeshProUGUI>().text = txt;
                        }
                        break;
                    }
                }
            }
        }
    }

    public GameObject GetColorElement(int id)
    {
        for (int c = 0; c < transform.GetChild(0).childCount; c++)
        {
            int colonIndex = transform.GetChild(0).GetChild(c).gameObject.name.LastIndexOf(':');
            if (colonIndex >= 0 && colonIndex < transform.GetChild(0).GetChild(c).gameObject.name.Length - 1)
            {
                string numberStr = transform.GetChild(0).GetChild(c).gameObject.name.Substring(colonIndex + 1);
                if (int.TryParse(numberStr, out int number))
                {
                    if (number == id)
                    {
                        return transform.GetChild(0).GetChild(c).gameObject;
                    }
                }
            }
        }
        return null;
    }

}
