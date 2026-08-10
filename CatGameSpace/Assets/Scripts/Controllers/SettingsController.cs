using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Localization.Settings;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{

    [Header("PanelReferances")]
    public GameObject panelGeneral;
    public GameObject panelKeys;
    public Button generalBut;
    public Button keyButton;


    public AudioMixer audioMixer;

    [Header("GeneralSettingsReferances")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider SFXSlider;

    public Slider BrightnessSlider;

    public Volume GlobalVolume;
    private ColorAdjustments colorAdjustments;



    public void StartThis()
    {
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", 4f);
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 4f);
        float sfxVol = PlayerPrefs.GetFloat("SfxVolume", 4f);

        masterSlider.value = masterVol;
        musicSlider.value = musicVol;
        SFXSlider.value = sfxVol;

        audioMixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Clamp(masterSlider.value / 10f, 0.0001f, 1f)) * 20f);
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Clamp(musicSlider.value / 10f, 0.0001f, 1f)) * 20f);
        audioMixer.SetFloat("SfxVolume", Mathf.Log10(Mathf.Clamp(SFXSlider.value / 10f, 0.0001f, 1f)) * 20f);

        GlobalVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments);
        BrightnessSlider.value = PlayerPrefs.GetFloat("Brightness", 10f);
        colorAdjustments.postExposure.value = Mathf.Lerp(-2, 0, BrightnessSlider.value / 10f);

        PlayerPrefs.GetInt("ThemeColorIndex", 0);

        masterSlider.fillRect.GetComponent<Image>().color = GameDataTracker.themeColor;
        musicSlider.fillRect.GetComponent<Image>().color = GameDataTracker.themeColor;
        SFXSlider.fillRect.GetComponent<Image>().color = GameDataTracker.themeColor;
        BrightnessSlider.fillRect.GetComponent<Image>().color = GameDataTracker.themeColor;


        gameObject.SetActive(false);

    }

    public void UpdateThemeColor()
    {
        masterSlider.fillRect.GetComponent<Image>().color = GameDataTracker.themeColor;
        musicSlider.fillRect.GetComponent<Image>().color = GameDataTracker.themeColor;
        SFXSlider.fillRect.GetComponent<Image>().color = GameDataTracker.themeColor;
        BrightnessSlider.fillRect.GetComponent<Image>().color = GameDataTracker.themeColor;
    }


    public void ChangeMasterVol()
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Clamp(masterSlider.value / 10f, 0.0001f, 1f)) * 20f);
        PlayerPrefs.SetFloat("MasterVolume", masterSlider.value);
    }
    public void ChangeMasterVolWBut(float value)
    {
        masterSlider.value += value;
        ChangeMasterVol();
    }

    public void ChangeMusicVol()
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Clamp(musicSlider.value / 10f, 0.0001f, 1f)) * 20f);
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
    }
    public void ChangeMusicVolWBut(float value)
    {
        musicSlider.value += value;
        ChangeMasterVol();
    }


    public void ChangeSFXVol()
    {
        audioMixer.SetFloat("SfxVolume", Mathf.Log10(Mathf.Clamp(SFXSlider.value / 10f, 0.0001f, 1f)) * 20f);
        PlayerPrefs.SetFloat("SfxVolume", SFXSlider.value);
    }
    public void ChangeSFXVolWBut(float value)
    {
        SFXSlider.value += value;
        ChangeMasterVol();
    }


    public void SetBrightness()
    {
        PlayerPrefs.SetFloat("Brightness", BrightnessSlider.value);
        colorAdjustments.postExposure.value = Mathf.Lerp(-2, 0, BrightnessSlider.value / 10f);
    }
    public void ChangeBrightnessVolWBut(float value)
    {
        BrightnessSlider.value += value;
        ChangeMasterVol();
    }


    bool interrupt;
    public void SettingPartChange(int partID)
    {
        if (partID == 0)
        {
            panelGeneral.gameObject.SetActive(true);
            panelKeys.gameObject.SetActive(false);

            GetComponent<SettingsController>().generalBut.interactable = false;
            GetComponent<SettingsController>().keyButton.interactable = true;

            interrupt = true;
            StartCoroutine(GeneralButtonPanelButtonAnim());
        }
        else if (partID == 1)
        {
            panelKeys.gameObject.SetActive(true);
            panelGeneral.gameObject.SetActive(false);

            GetComponent<SettingsController>().generalBut.interactable = true;
            GetComponent<SettingsController>().keyButton.interactable = false;


            interrupt = true;
            StartCoroutine(KeyButtonPanelButtonAnim());
        }
    }

    IEnumerator KeyButtonPanelButtonAnim()
    {
        yield return null;
        yield return null;
        interrupt = false;
        while (true)
        {
            keyButton.GetComponent<Animator>().SetFloat("Blend", Mathf.Lerp(keyButton.GetComponent<Animator>().GetFloat("Blend"), 1, Time.deltaTime * 10f));
            generalBut.GetComponent<Animator>().SetFloat("Blend", Mathf.Lerp(generalBut.GetComponent<Animator>().GetFloat("Blend"), 0, Time.deltaTime * 10f));

            if (keyButton.GetComponent<Animator>().GetFloat("Blend") >= .999f) break;
            else if (interrupt)
            {
                yield break;
            }
            yield return null;
        }
        keyButton.GetComponent<Animator>().SetFloat("Blend", 1);
        generalBut.GetComponent<Animator>().SetFloat("Blend", 0);
    }

    IEnumerator GeneralButtonPanelButtonAnim()
    {
        yield return null;
        yield return null;
        interrupt = false;
        while (true)
        {
            generalBut.GetComponent<Animator>().SetFloat("Blend", Mathf.Lerp(generalBut.GetComponent<Animator>().GetFloat("Blend"), 1, Time.deltaTime * 10f));
            keyButton.GetComponent<Animator>().SetFloat("Blend", Mathf.Lerp(keyButton.GetComponent<Animator>().GetFloat("Blend"), 0, Time.deltaTime * 10f));

            if (generalBut.GetComponent<Animator>().GetFloat("Blend") >= .999f) break;
            else if (interrupt)
            {
                yield break;
            }
            yield return null;
        }
        generalBut.GetComponent<Animator>().SetFloat("Blend", 1);
        keyButton.GetComponent<Animator>().SetFloat("Blend", 0);
    }


}
