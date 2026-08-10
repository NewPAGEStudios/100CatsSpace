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
        // Klavye olayýný dinlemeye baþla
        Keyboard.current.onTextInput += OnTextInput;
    }

    private void OnDisable()
    {
        // Dinlemeyi býrak (Memory leak olmamasý için þart)
        if (Keyboard.current != null)
            Keyboard.current.onTextInput -= OnTextInput;
    }

    private void OnTextInput(char character)
    {
        // Gelen karakteri string'e çevir
        string pressedChar = character.ToString().ToLower();

        // Buradan sonrasý senin algoritmanla ayný mantýða baðlanabilir
        runtimeCheatCode += pressedChar;

        // Basit bir "Contains" veya "EndsWith" kontrolü daha kolaydýr:
        if (runtimeCheatCode.EndsWith(cheatCode))
        {
            MenuController.instance.OpenCheat();
            runtimeCheatCode = "";
        }
        else if (runtimeCheatCode.Length > cheatCode.Length + 5)
        {
            // String sonsuza kadar uzamasýn diye belli bir uzunlukta kes
            runtimeCheatCode = runtimeCheatCode.Substring(runtimeCheatCode.Length - cheatCode.Length);
        }
    }
}
