using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CheatCodeSimple : MonoBehaviour
{
    private string cheatCode = "mesela";
    private string runtimeCheatCode = "";

    private void OnEnable()
    {
        // Klavye olayını dinlemeye başla
        if (Keyboard.current != null)
            Keyboard.current.onTextInput += OnTextInput;
    }

    private void OnDisable()
    {
        // Dinlemeyi bırak (Memory leak olmaması için şart)
        if (Keyboard.current != null)
            Keyboard.current.onTextInput -= OnTextInput;
    }

    private void OnTextInput(char character)
    {
        // Gelen karakteri string'e çevir
        string pressedChar = character.ToString().ToLower();

        // Buradan sonrası senin algoritmanla aynı mantığa bağlanabilir
        runtimeCheatCode += pressedChar;

        // Basit bir "Contains" veya "EndsWith" kontrolü daha kolaydır:
        if (runtimeCheatCode.EndsWith(cheatCode))
        {
            MenuController.instance.OpenCheat();
            runtimeCheatCode = "";
        }
        else if (runtimeCheatCode.Length > cheatCode.Length + 5)
        {
            // String sonsuza kadar uzamasın diye belli bir uzunlukta kes
            runtimeCheatCode = runtimeCheatCode.Substring(runtimeCheatCode.Length - cheatCode.Length);
        }
    }
}
