using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RadioController : MonoBehaviour
{
    public static RadioController instance;

    private readonly char[] timeBuffer = new char[5]; // örn: "00:00"

    public GameObject MutedIndicator;
    public Sprite sync;
    public Sprite synced;
    public AudioSource audioSource;

    public TextMeshProUGUI musicTXT;
    public TextMeshProUGUI playbackTXT;
    public Image MuteImage;
    public int CurrentRadioId = 0;
    public int CurrentMusicInRadioID = 0;

    public Button leftBtn;
    public Button rightBtn;
    public Button syncBtn;

    public bool musicIsSynced = false;

    private RadioObject[] rads;

    private bool isWorking = false;
    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        rads = Resources.LoadAll<RadioObject>("RadioObjs");

        if(SceneManager.GetActiveScene().name == "LobbyScene")//LobbyController
        {
            CurrentRadioId = GameDataTracker.radioID;
            audioSource.Stop();

            return;
        }
        else if(SceneManager.GetActiveScene().name == "TutScene")
        {
            isWorking = true;

            CurrentRadioId = GameDataTracker.radioID;

            LoadRadioPrefs();
        }
        else//Gamecontroller
        {
            isWorking = true;

            CurrentRadioId = GameDataTracker.radioID;

            LoadRadioPrefs();

            if (GameController.Instance.isOnline)
            {
                if (GameController.Instance.LocalPlayerOnlinePlayer.connectionID != 0)
                {
                    leftBtn.interactable = false;
                    rightBtn.interactable = false;

                    musicIsSynced = true;
                    syncBtn.transform.GetChild(0).GetComponent<Image>().sprite = synced;

                    GameController.Instance.LocalPlayerOnlinePlayer.CmdFetchRadio(GameController.Instance.LocalPlayerOnlinePlayer.GetComponent<NetworkIdentity>());
                }
                else
                {
                    syncBtn.interactable = false;
                    syncBtn.gameObject.SetActive(false);

                    musicIsSynced = false;
                }
            }



        }


    }

    public void startOnLobby()
    {
        if (LobbyController.instance.LocalPlayer.connectionID != 0)
        {
            leftBtn.interactable = false;
            rightBtn.interactable = false;

            musicIsSynced = true;
            syncBtn.transform.GetChild(0).GetComponent<Image>().sprite = synced;

            LobbyController.instance.LocalPlayer.CmdFetchRadio(LobbyController.instance.LocalPlayer.GetComponent<NetworkIdentity>());

            isWorking = true;

        }
        else 
        {
            syncBtn.gameObject.SetActive(false);

            isWorking = true;

            LoadRadioPrefs();

            musicIsSynced = false;
            syncBtn.GetComponent<Image>().sprite = sync;
        }
    }



    private void Update()
    {
        if (!isWorking) return;

        PlaybackDisplay();

        if (SceneManager.GetActiveScene().name == "LobbyScene")
        {
            if(LobbyController.instance.LocalPlayer.connectionID == 0)
            {
                if (!audioSource.isPlaying)
                {
                    CurrentMusicInRadioID++;
                    if (CurrentMusicInRadioID >= rads[CurrentRadioId].clips.Length)
                    {
                        CurrentMusicInRadioID = 0;
                    }
                    audioSource.clip = rads[CurrentRadioId].clips[CurrentMusicInRadioID];
                    ChangeNameOfDisplay(rads[CurrentRadioId].RadName);
                    LobbyController.instance.LocalPlayer.RpcGiveRadioInfo(CurrentRadioId, audioSource.clip.name, 0f);
                    audioSource.Play();
                }
            }
            else if (!musicIsSynced)
            {
                if (!audioSource.isPlaying)
                {
                    CurrentMusicInRadioID++;
                    if (CurrentMusicInRadioID >= rads[CurrentRadioId].clips.Length)
                    {
                        CurrentMusicInRadioID = 0;
                    }
                    audioSource.clip = rads[CurrentRadioId].clips[CurrentMusicInRadioID];
                    ChangeNameOfDisplay(rads[CurrentRadioId].RadName);
                    audioSource.Play();
                }
            }
        }
        else if (SceneManager.GetActiveScene().name == "TutScene")
        {
            if (!audioSource.isPlaying)
            {
                CurrentMusicInRadioID++;
                if (CurrentMusicInRadioID >= rads[CurrentRadioId].clips.Length)
                {
                    CurrentMusicInRadioID = 0;
                }
                audioSource.clip = rads[CurrentRadioId].clips[CurrentMusicInRadioID];
                ChangeNameOfDisplay(rads[CurrentRadioId].RadName);
                audioSource.Play();
            }
        }
        else if (GameController.Instance.isOnline)
        {
            if (GameController.Instance.LocalPlayerOnlinePlayer.connectionID == 0) // is localplayer a Server ? 
            {
                if (!audioSource.isPlaying)
                {
                    CurrentMusicInRadioID++;
                    if (CurrentMusicInRadioID >= rads[CurrentRadioId].clips.Length)
                    {
                        CurrentMusicInRadioID = 0;
                    }
                    audioSource.clip = rads[CurrentRadioId].clips[CurrentMusicInRadioID];
                    ChangeNameOfDisplay(rads[CurrentRadioId].RadName);
                    GameController.Instance.LocalPlayerOnlinePlayer.RpcGiveRadioInfo(CurrentRadioId, audioSource.clip.name, 0f);
                    audioSource.Play();
                }
            }
            else if (!musicIsSynced)
            {
                if (!audioSource.isPlaying)
                {
                    CurrentMusicInRadioID++;
                    if (CurrentMusicInRadioID >= rads[CurrentRadioId].clips.Length)
                    {
                        CurrentMusicInRadioID = 0;
                    }
                    audioSource.clip = rads[CurrentRadioId].clips[CurrentMusicInRadioID];
                    ChangeNameOfDisplay(rads[CurrentRadioId].RadName);
                    audioSource.Play();
                }
            }
        }
        else
        {
            if (!audioSource.isPlaying)
            {
                CurrentMusicInRadioID++;
                if (CurrentMusicInRadioID >= rads[CurrentRadioId].clips.Length)
                {
                    CurrentMusicInRadioID = 0;
                }
                audioSource.clip = rads[CurrentRadioId].clips[CurrentMusicInRadioID];
                ChangeNameOfDisplay(rads[CurrentRadioId].RadName);
                audioSource.Play();
            }
        }

    }

    public void LoadRadioPrefs()
    {
        float current = GameDataTracker.radiosCurrentFloat[CurrentRadioId];
        foreach (AudioClip clip in rads[CurrentRadioId].clips)
        {
            if (current - clip.length < 0)
            {
                CurrentMusicInRadioID = Array.IndexOf(rads[CurrentRadioId].clips, clip);
                audioSource.clip = clip;
                break;
            }
            else
            {
                current -= clip.length;
            }
        }
        ChangeNameOfDisplay(rads[CurrentRadioId].RadName);
        audioSource.Play();
        audioSource.time = current;
    }

    public void SaveRadioPrefs()
    {
        if (musicIsSynced) return;
        float current = 0;
        foreach (AudioClip clip in rads[CurrentRadioId].clips)
        {
            int i = Array.IndexOf(rads[CurrentRadioId].clips, clip);
            if (i == CurrentMusicInRadioID)
            {
                current += audioSource.time;
                break;
            }
            else
            {
                current += clip.length;
            }
        }
        GameDataTracker.radiosCurrentFloat[CurrentRadioId] = current;
    }


    public void ChangeRadio()
    {
        LoadRadioPrefs();
        if (SceneManager.GetActiveScene().name == "LobbyScene" && LobbyController.instance.LocalPlayer.connectionID == 0)
        {
            LobbyController.instance.LocalPlayer.RpcGiveRadioInfo(CurrentRadioId, audioSource.clip.name, audioSource.time);
        }
        else if(SceneManager.GetActiveScene().name == "TutScene")
        {

        }
        else if(GameController.Instance.LocalPlayerOnlinePlayer != null && GameController.Instance.LocalPlayerOnlinePlayer.connectionID == 0)
        {
            GameController.Instance.LocalPlayerOnlinePlayer.RpcGiveRadioInfo(CurrentRadioId, audioSource.clip.name, audioSource.time);
        }
    }

    public (string,float) GetCurrentMusicInfos()
    {
        return (rads[CurrentRadioId].clips[CurrentMusicInRadioID].name, audioSource.time);
    }

    public void OpenMusic(string clipName, float playTime)
    {
        if (!musicIsSynced)
        {
            return;
        }
        foreach(AudioClip clip in rads[CurrentRadioId].clips)
        {
            if (clip.name == clipName)
            {
                audioSource.clip = clip;
                break;
            }
        }
        audioSource.time = playTime;
        audioSource.Play();
        ChangeNameOfDisplay(rads[CurrentRadioId].RadName);
    }
    public void MuteRadio()
    {
        audioSource.mute = !audioSource.mute;
        if (audioSource.mute)
        {
            MutedIndicator.SetActive(true);
        }
        else
        {
            MutedIndicator.SetActive(false);
        }
    }

    public void LeftBtn()
    {
        SaveRadioPrefs();
        CurrentRadioId--;
        if (CurrentRadioId < 0)
        {
            CurrentRadioId = rads.Length - 1;
        }
        ChangeRadio();
    }

    public void RightBtn()
    {
        SaveRadioPrefs();
        CurrentRadioId++;
        if(CurrentRadioId >= rads.Length)
        {
            CurrentRadioId = 0;
        }
        ChangeRadio();
    }

    public void ChangeNameOfDisplay(string radioName)
    {
        musicTXT.text = radioName;
    }

    public void PlaybackDisplay()
    {
        int totalSeconds = Mathf.FloorToInt(audioSource.time);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timeBuffer[0] = (char)('0' + minutes / 10);
        timeBuffer[1] = (char)('0' + minutes % 10);
        timeBuffer[2] = ':';
        timeBuffer[3] = (char)('0' + seconds / 10);
        timeBuffer[4] = (char)('0' + seconds % 10);

        playbackTXT.text = new string(timeBuffer);
    }

    //public void PlaybackDisplay()
    //{
    //    string minute = "";
    //    string sec = "";
    //    if (audioSource.time / 60 < 10) minute = "0" + Mathf.FloorToInt(audioSource.time / 60);
    //    else minute = Mathf.FloorToInt(audioSource.time / 60).ToString();

    //    if (audioSource.time % 60 < 10) sec = "0" + Mathf.FloorToInt(audioSource.time % 60);
    //    else sec = Mathf.FloorToInt(audioSource.time % 60).ToString();

    //    playbackTXT.text = minute + ":" + sec;
    //}

    public void SyncStatusChange()
    {
        musicIsSynced = !musicIsSynced;
        if (musicIsSynced)
        {
            SaveRadioPrefs();
            leftBtn.interactable = false;
            rightBtn.interactable = false;
            syncBtn.transform.GetComponent<Image>().sprite = synced;
            if (SceneManager.GetActiveScene().name == "LobbyScene")
            {
                LobbyController.instance.LocalPlayer.CmdFetchRadio(LobbyController.instance.LocalPlayer.GetComponent<NetworkIdentity>());
            }
            else
            {
                GameController.Instance.LocalPlayerOnlinePlayer.CmdFetchRadio(GameController.Instance.LocalPlayerOnlinePlayer.GetComponent<NetworkIdentity>());
            }
        }
        else
        {
            leftBtn.interactable = true;
            rightBtn.interactable = true;
            CurrentRadioId = UnityEngine.Random.Range(0,rads.Length);
            syncBtn.transform.GetComponent<Image>().sprite = sync;
            LoadRadioPrefs();
        }
    }
}
