using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ChatManager : MonoBehaviour , IChatParent
{



    [SerializeField]
    private TMP_InputField m_TextMeshProInputField;
    public bool inputSelected;
    public Transform chatParent;
    public GameObject p_chat;

    public List<GameObject> chatList;

    public void AddChat(string sender,string text,bool isLocal)
    {

        for (int i = 0; i < chatParent.childCount; i++)
        {
            chatParent.GetChild(i).GetComponent<ChatObjectController>().GoUp();
        }
        ChatObjectController coc = Instantiate(p_chat, chatParent).GetComponent<ChatObjectController>();

        chatList.Add(coc.gameObject);

        if(chatList.Count > 10)
        {
            GameObject go = chatList[0];
            chatList.RemoveAt(0);
            Destroy(go);
        }

        coc.gameObject.name = sender + chatList.Count;
        coc.Init(sender, text, isLocal);

        if(sender == GameObject.Find("LocalGamePlayer").GetComponent<PlayerController>().PlayerName)
        {
            m_TextMeshProInputField.text = "";
            m_TextMeshProInputField.ActivateInputField();
        }
    }

    public void ChatButtonPressed()
    {
        if (m_TextMeshProInputField.text == "")
        {
            return;
        }
        SteamLobby.instance.SendChatMessage(m_TextMeshProInputField.text);


    }

    public void Focus()
    {
        inputSelected = true;
    }

    public void UnFocus()
    {
        inputSelected = false;
    }

}
