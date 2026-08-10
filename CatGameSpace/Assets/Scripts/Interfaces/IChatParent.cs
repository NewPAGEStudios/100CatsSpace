using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public interface IChatParent
{
    public void AddChat(string sender,string text, bool isLocal);
    public void Focus();
    public void UnFocus();
    public void ChatButtonPressed();

}
