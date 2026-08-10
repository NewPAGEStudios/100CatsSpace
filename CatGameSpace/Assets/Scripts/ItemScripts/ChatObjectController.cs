using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatObjectController : MonoBehaviour
{
    public TextMeshProUGUI m_Header;
    public TextMeshProUGUI m_Body;
    public bool isLocal;

    public Sprite local;
    public Sprite client;


    public void Init(string Head, string Body,bool isLocal)
    {
        m_Header.text = Head;
        m_Body.text = Body;

        RectTransform headerRect = m_Header.transform.parent.GetComponent<RectTransform>();

        this.isLocal = isLocal;
        if (isLocal)
        {
            m_Body.transform.parent.GetComponent<Image>().sprite = local;

            headerRect.pivot = new Vector2(1, 0);
            headerRect.anchorMin = new Vector2(1, 0);
            headerRect.anchorMax = new Vector2(1, 0);
            headerRect.anchoredPosition = new Vector2(0, 0);

            GetComponent<RectTransform>().pivot = new Vector2(1, 0);
            GetComponent<RectTransform>().anchorMin = new Vector2 (1, 0);
            GetComponent<RectTransform>().anchorMax = new Vector2 (1, 0);
            GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

            m_Header.alignment = TextAlignmentOptions.MidlineRight;
            m_Body.alignment = TextAlignmentOptions.MidlineRight;
        }
        else
        {
            m_Body.transform.parent.GetComponent<Image>().sprite = client;

            headerRect.pivot = new Vector2(0, 0);
            headerRect.anchorMin = new Vector2(0, 0);
            headerRect.anchorMax = new Vector2(0, 0);
            headerRect.anchoredPosition = new Vector2(0, 0);

            GetComponent<RectTransform>().pivot = new Vector2(0, 0);
            GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
            GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

            m_Header.alignment = TextAlignmentOptions.MidlineLeft;
            m_Body.alignment = TextAlignmentOptions.MidlineLeft;
        }
    }

    public void GoUp()
    {
        GetComponent<RectTransform>().anchoredPosition = new Vector2(0, GetComponent<RectTransform>().anchoredPosition.y + GetComponent<RectTransform>().sizeDelta.y);

    }



}
