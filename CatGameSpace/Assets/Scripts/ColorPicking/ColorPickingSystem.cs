using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorPickingSystem : MonoBehaviour
{
    public float currentHue;
    public float currentSat;
    public float currentVal;

    [SerializeField]
    private RawImage hueImage;
    [SerializeField]
    private RawImage satVallImage;
    [SerializeField]
    private RawImage outputImage;

    [SerializeField]
    private Slider hueSlider;

    [SerializeField]
    private TMP_InputField hexInputField;

    private Texture2D hueTexture;
    private Texture2D svTexture;
    private Texture2D outputTexture;

    [SerializeField]
    private Image changeThisColor;

    public void StartThis()
    {
        CreateHueImage();
        CreateSVImage();
        CreateOutputImage();
        UpdateOutputImage();

    }

    private void CreateHueImage()
    {
        hueTexture = new Texture2D(1, 16);
        hueTexture.wrapMode = TextureWrapMode.Clamp;
        hueTexture.name = "HueTexture";

        for (int i = 0; i < hueTexture.height; i++) 
        {
            float hue = (float)i / (hueTexture.height - 1);
            hueTexture.SetPixel(0, i, Color.HSVToRGB(hue, 1, 1f));
        }

        hueTexture.Apply();

        currentHue = 0;

        hueImage.texture = hueTexture;
    }

    private void CreateSVImage()
    {
        svTexture = new Texture2D(16, 16);
        svTexture.wrapMode = TextureWrapMode.Clamp;
        svTexture.name = "SatValTexture";

        for (int y = 0; y < svTexture.height; y++) 
        {
            for (int x = 0; x < svTexture.width; x++) 
            {
                svTexture.SetPixel(x, y, Color.HSVToRGB(currentHue, (float)x / svTexture.width, (float)y / svTexture.height));
            }
        }

        svTexture.Apply();
        currentSat = 0;
        currentVal = 0;

        satVallImage.texture = svTexture;
    }

    private void CreateOutputImage()
    {
        outputTexture=new Texture2D(1, 16);
        outputTexture.wrapMode = TextureWrapMode.Clamp;
        outputTexture.name = "OutputTexture";

        Color currentColour = Color.HSVToRGB(currentHue, currentSat, currentVal);
        for(int i=0; i<outputTexture.height; i++)
        {
            outputTexture.SetPixel(0, i, currentColour);
        }
        outputTexture.Apply();

        outputImage.texture = outputTexture;
    }
    private void UpdateOutputImage()
    {
        Color currentColour = Color.HSVToRGB(currentHue, currentSat, currentVal);

        for(int i = 0; i < outputTexture.height; i++)
        {
            outputTexture.SetPixel(0, i, currentColour);
        }
        outputTexture.Apply();

        hexInputField.text = ColorUtility.ToHtmlStringRGB(currentColour);


        changeThisColor.color = currentColour;
        GameDataTracker.themeColor = currentColour;
        MenuController.instance.cursorDisplayOffline.changeColor(currentColour);

        //foreach(GameObject map in MenuController.instance.mapBtns)
        //{
        //    map.transform.GetChild(1).GetComponent<Image>().color = currentColour;
        //}
    }
    public void SetSV(float s, float v)
    {
        currentSat = s;
        currentVal = v;

        UpdateOutputImage();

    }

    public void UpdateSVImage()
    {
        currentHue=hueSlider.value;
        for (int y = 0; y < svTexture.height; y++) 
        {
            for(int x = 0; x < svTexture.width; x++)
            {
                svTexture.SetPixel(x, y, Color.HSVToRGB(currentHue, (float)x / svTexture.width, (float)y / svTexture.height));
            }
        }

        svTexture.Apply();

        UpdateOutputImage();
    }
    public void OnTextInput()
    {
        if (hexInputField.text.Length < 6) return;
        Color newCol;

        if (ColorUtility.TryParseHtmlString("#" + hexInputField.text, out newCol))
            Color.RGBToHSV(newCol,out currentHue, out currentSat, out currentVal);

        hueSlider.value = currentHue;

        hexInputField.text = "";
        UpdateOutputImage();

    }

    public void ClosePickingSystem()
    {
        gameObject.SetActive(false);
    }
    public static bool IsColorDark(Color color)
    {
        // W3C'ye göre algılanan parlaklık (Relative Luminance) hesaplaması
        double luminance = (0.2126 * color.r) + (0.7152 * color.g) + (0.0722 * color.b);

        // Eşik değeri 0.5, daha küçükse koyu, büyükse açık renk olarak kabul edilir
        return luminance <= 0.5;
    }

}
