using Mirror.BouncyCastle.Pqc.Crypto.Falcon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class GameDataTracker : MonoBehaviour
{
    private static GameDataTracker _instance;
    public static GameDataTracker Instance
    {
        get { return _instance; }
    }

    public static bool isCheat = false;

    public static string ErrorMessage;

    public static string selectedPaintID;
    public static bool skipSave { get; set; } = false;
    public static bool isOnline = false;
    public static bool isHost = false;
    public static ulong LobbyId;

    public const string paintObjectFileKey = "Paints";

    public static bool[] paint_regionFillData;
    public static bool[] paint_findCatData;
    public static bool paint_finished;
    public static float paint_timer;
    public static int paint_hint;
    public static bool paint_SaveSoloCorrupted;

    public static List<Vector2> replay_paintPos = new();
    public static List<int> replay_itemID = new();

    public static Color32 themeColor;


    public static int radioID;
    public static float[] radiosCurrentFloat;
    public static float[] radiosMaxFloat;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            _instance = this;
        }

        DontDestroyOnLoad(this);
        CheckSaveData();
        InitializationColor();
        InitializationRadio();
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoad;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
    }
    private void Update()
    {
        //Music
        for (int i = 0; i < radiosCurrentFloat.Length; i++)
        {
            radiosCurrentFloat[i] += Time.deltaTime;
            if (radiosCurrentFloat[i] >= radiosMaxFloat[i])
            {
                radiosCurrentFloat[i] = 0f;
            }
        }

    }

    private void OnSceneLoad(Scene activeScene, LoadSceneMode arg1)
    {
        if (activeScene.name == "Menu")
        {
            CheckAchivements();
            SaveData();
            selectedPaintID = null;
            isHost = false;
            LobbyId = 0;
        }
        else if (activeScene.name == "LobbyScene")
        {
            GameObject.Find("Global Volume").GetComponent<Volume>().profile.TryGet<ColorAdjustments>(out ColorAdjustments colorAdjustments);
            colorAdjustments.postExposure.value = Mathf.Lerp(-2, 0, PlayerPrefs.GetFloat("Brightness", 10f) / 10f);
        }
        else if (activeScene.name == "OfflineGamePlay")
        {
            Debug.Log(replay_paintPos.Count);
        }
        else if (activeScene.name == "OnlineGamePlay")
        {
        }
    }
    public void InitializationColor()
    {
        themeColor = new Color(PlayerPrefs.GetFloat("themeColorR", 1), PlayerPrefs.GetFloat("themeColorG", 1), PlayerPrefs.GetFloat("themeColorB", 0), 1);
    }
    public void InitializationRadio()
    {
        RadioObject[] radios = Resources.LoadAll<RadioObject>("RadioObjs");
        foreach (RadioObject radio in radios)
        {
            radio.RandomizeClips();
        }

        radiosCurrentFloat = new float[radios.Length];
        radiosMaxFloat = new float[radios.Length];

        for (int c = 0; c < radiosMaxFloat.Length; c++)
        {
            float maxFloat = 0f;
            foreach (AudioClip clip in radios[c].clips)
            {
                maxFloat += clip.length;
            }
            radiosMaxFloat[c] = maxFloat;
        }

        radioID = 0;

        for (int i = 0; i < radiosCurrentFloat.Length; i++)
        {
            radiosCurrentFloat[i] = UnityEngine.Random.Range(0, radiosMaxFloat[i]);
        }

    }

    private void OnApplicationQuit()
    {

        PlayerPrefs.SetFloat("themeColorR", themeColor.r);
        PlayerPrefs.SetFloat("themeColorG", themeColor.g);
        PlayerPrefs.SetFloat("themeColorB", themeColor.b);

        if (SceneManager.GetActiveScene().name == "Menu")
        {
            return;
        }
        if(LobbyController.instance != null)
        {
            LobbyController.instance = null;
        }
        if (!string.IsNullOrEmpty(selectedPaintID))
        {
            SaveData();
        }
    }

    public void CheckSaveData()
    {
        if(!SaveSystem.SaveExists())SaveSystem.InitSave();
        else
        {
            var Result = SaveSystem.GetSaveVersion();
            if (!string.IsNullOrEmpty(Result.Item2))
            {
                Debug.Log("GameDataTracker" + Result.Item2);
                return;
            }

            if (Application.version != Result.Item1)
            {
                var Result0 = SaveSystem.LoadAllData();
                List<DataElement> datas = Result0.Item1;

                PaintObject[] paintsArray = Resources.LoadAll<PaintObject>("Paints");

                List<PaintObject> paints = paintsArray.ToList<PaintObject>();

                DataElement[] dataArray = datas.ToArray();

                foreach (DataElement data in dataArray)
                {
                    var matchedItem = paints.Find(ite => ite.id == data.paintID);
                    DataElement dataE = data;
                    if (matchedItem == null)
                    {
                        datas.Remove(dataE);
                    }
                    else
                    {
                        bool[] cat;
                        bool[] regions;
                        if (dataE.paint_catFindData.Length != matchedItem.catsSprites.Length)
                        {
                            cat = new bool[matchedItem.catsSprites.Length];

                            for (int i = 0; i < cat.Length; i++)
                            {
                                if (i >= dataE.paint_catFindData.Length)
                                {
                                    cat[i] = false;
                                    continue;
                                }
                                cat[i] = dataE.paint_catFindData[i];
                            }

                        }
                        else
                        {
                            cat = dataE.paint_catFindData;
                        }
                        if (dataE.paint_regionColorData.Length != matchedItem.regionSprites.Count)
                        {
                            regions = new bool[matchedItem.regionSprites.Count];

                            for (int i = 0; i < regions.Length; i++)
                            {
                                if (i >= dataE.paint_regionColorData.Length)
                                {
                                    regions[i] = false;
                                    continue;
                                }
                                regions[i] = dataE.paint_regionColorData[i];
                            }
                        }
                        else
                        {
                            regions = dataE.paint_regionColorData;
                        }

                        datas.Remove(dataE);
                        datas.Add(new(dataE.paintID, regions, cat, dataE.paint_finished, dataE.paintTimer, dataE.paint_hint, dataE.solo_Corrupted));
                    }
                }
                foreach (PaintObject pa in paintsArray)
                {
                    var matchedItem = datas.Find(ite => ite.paintID == pa.id);
                    if (matchedItem == null)
                    {
                        bool[] reg = new bool[pa.regionSprites.Count];
                        bool[] cat = new bool[pa.catsSprites.Length];

                        Array.Fill(reg, false);
                        Array.Fill(cat, false);

                        datas.Add(new(pa.id, reg, cat, false, 0f, 2, false));
                    }
                }


                SaveElement saveElement = new();
                saveElement.GameVersion = Application.version;
                saveElement.dataElements = datas;

                SaveSystem.SavePlayer(saveElement);

            }
        }

        if (!SaveSystem.SaveReplayExits()) SaveSystem.InitReplay();
        else
        {
            var Result = SaveSystem.GetSaveReplayVersion();
            if (!string.IsNullOrEmpty(Result.Item2))
            {
                Debug.Log("GameDataTracker" + Result.Item2);
                return;
            }

            if (Application.version != Result.Item1)
            {
                var Result0 = SaveSystem.LoadAllReplays();
                List<ReplayElement> datas = Result0.Item1;

                PaintObject[] paintsArray = Resources.LoadAll<PaintObject>("Paints");

                List<PaintObject> paints = paintsArray.ToList<PaintObject>();

                ReplayElement[] replayArray = datas.ToArray();

                if (paints.Count > datas.Count)
                {
                    foreach (PaintObject paint in paints)
                    {
                        var matchedItem = datas.Find(item => item.paintID == paint.id);
                        if (matchedItem == null)
                        {
                            int[] mode = new int[0];
                            int[] item = new int[0];

                            Vector2[] poss = new Vector2[0];

                            datas.Add(new(paint.id, poss, item));
                        }

                    }
                }
                else if (paints.Count < datas.Count)
                {
                    foreach (ReplayElement data in replayArray)
                    {
                        var matchedItem = paints.Find(item => item.id == data.paintID);
                        if (matchedItem == null)
                        {
                            datas.Remove(data);
                        }
                    }
                }
                else
                {
                    foreach (PaintObject paint in paints)
                    {
                        var matchedItem = datas.Find(item => item.paintID == paint.id);
                        if (matchedItem == null)
                        {
                            int[] mode = new int[0];
                            int[] item = new int[0];

                            Vector2[] poss = new Vector2[0];

                            datas.Add(new(paint.id, poss, item));
                        }

                    }

                    foreach (ReplayElement data in replayArray)
                    {
                        var matchedItem = paints.Find(item => item.id == data.paintID);
                        if (matchedItem == null)
                        {
                            datas.Remove(data);
                        }
                        else if(data.ItemID.Length > matchedItem.regionSprites.Count)
                        {
                            bool[] paintData = new bool[matchedItem.regionSprites.Count];
                            Array.Fill(paintData, false);
                            bool[] catData = new bool[matchedItem.catsSprites.Length];
                            Array.Fill(catData, false);
                            DataElement d = new(matchedItem.id, paintData, catData, false, 0f, 2, false);
                            SaveSystem.SaveDataElement(d);

                            int indexOnList = datas.IndexOf(data);
                            data.ItemID = new int[0];
                            data.xs = new float[0];
                            data.ys = new float[0];
                            datas[indexOnList] = data;

                        }
                    }
                }

                ReplaySaveElement replaySaveElement = new();
                replaySaveElement.GameVersion = Application.version;
                replaySaveElement.replayElements = datas;

                SaveSystem.SaveReplay(replaySaveElement);

            }

        }

    }

    public void SaveData()
    {
        if (string.IsNullOrEmpty(selectedPaintID)) return;

        if (skipSave) 
        {
            replay_itemID.Clear();
            replay_paintPos.Clear();
            return;
        }
        if (!paint_SaveSoloCorrupted)
        {
            bool addleaderBoard = true;
            foreach (bool region in paint_regionFillData)
            {
                if (!region)
                {
                    addleaderBoard = false;
                }
            }

            if (addleaderBoard && !isCheat)
            {
                SteamLeaderboard instance = FindObjectOfType<SteamLeaderboard>();
                if (instance != null) instance.UpdateScore(selectedPaintID, Mathf.CeilToInt(paint_timer));
            }
        }



        DataElement data = new(selectedPaintID, paint_regionFillData, paint_findCatData, paint_finished, paint_timer, paint_hint, paint_SaveSoloCorrupted);
        SaveSystem.SaveDataElement(data);

        Array.Fill(paint_findCatData, false);
        Array.Fill(paint_regionFillData, false);
        paint_timer = 0;
        paint_finished = false;
        paint_hint = 2;
        paint_SaveSoloCorrupted = false;
        ReplayElement replayData = new(selectedPaintID, replay_paintPos.ToArray(), replay_itemID.ToArray());
        SaveSystem.SaveReplayElement(replayData);

        replay_itemID.Clear();
        replay_paintPos.Clear();
    }

    public void CheckAchivements()
    {

        var Result = SaveSystem.LoadAllData();

        foreach (DataElement item in Result.Item1)
        {
            if (item.paintID == selectedPaintID && paint_finished) continue;
            else if (item.paintID == selectedPaintID && !paint_finished) return;
            if (!item.paint_finished) return;
        }

    }

}