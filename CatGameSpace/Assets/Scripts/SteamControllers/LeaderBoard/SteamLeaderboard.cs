using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class SteamLeaderboard : MonoBehaviour
{
    public static SteamLeaderboard Instance;

    public Button[] Buts;
    public Button[] Buts2;
    public GameObject[] panels;

    [Header("Ayarlar")]
    public GameObject leaderListItemPrefab;
    public List<LeaderboardConfig> leaderboards = new List<LeaderboardConfig>();

    private Dictionary<string, SteamLeaderboardInstance> _instances = new Dictionary<string, SteamLeaderboardInstance>();

    // Callback'ler artýk sadece Avatar için gerekli, isim data'dan geliyor.
    private Callback<AvatarImageLoaded_t> _avatarLoadedCallback;

    bool isReady = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("SteamManager sahnede yok!");
            return;
        }

        _avatarLoadedCallback = Callback<AvatarImageLoaded_t>.Create(OnAvatarLoaded);

        foreach (var config in leaderboards)
        {
            if (string.IsNullOrEmpty(config.id)) continue;
            var instance = new SteamLeaderboardInstance(config, leaderListItemPrefab);
            _instances.Add(config.id, instance);
        }

        LeaderBoardButton(0);
        LeaderBoardButton2(0);
        isReady = true;
    }

    private void OnAvatarLoaded(AvatarImageLoaded_t param)
    {
        foreach (var instance in _instances.Values)
        {
            instance.OnAvatarLoadedReceived(param);
        }
    }

    public void UpdateScore(string leaderboardID, int score)
    {
        StartCoroutine(routineWaitStart(leaderboardID, score));
    }

    public void RefreshLeaderboard(string leaderboardID)
    {
        if (_instances.TryGetValue(leaderboardID, out var instance))
        {
            instance.DownloadScores(false); // Global
            instance.DownloadScores(true);  // Friend
        }
    }

    IEnumerator routineWaitStart(string id, int score)
    {
        yield return new WaitUntil(() => isReady);

        if (_instances.TryGetValue(id, out var instance))
            instance.UploadScore(score);
        else
            Debug.LogError($"Leaderboard bulunamadý: {id}");


    }


    public void RefreshLeaderboardByIndex(int index)
    {
        if (index >= 0 && index < leaderboards.Count)
            RefreshLeaderboard(leaderboards[index].id);
    }

    // UI Butonlarý
    public void LeaderBoardButton(int i)
    {
        for (int c = 0; c < Buts.Length; c++)
            Buts[c].GetComponent<ButtonAnimation>().CloseAnim();
        Buts[i].GetComponent<ButtonAnimation>().OpenAnim();
    }

    public void LeaderBoardButton2(int i)
    {
        for (int c = 0; c < Buts2.Length; c++)
            Buts2[c].GetComponent<ButtonAnimation>().CloseAnim();
        Buts2[i].GetComponent<ButtonAnimation>().OpenAnim();

        for (int c = 0; c < leaderboards.Count; c++)
        {
            bool showGlobal = (i == 0);
            leaderboards[c].globalContentParent.gameObject.SetActive(showGlobal);
            leaderboards[c].friendContentParent.gameObject.SetActive(!showGlobal);
        }
    }
    public void ChangePanel(GameObject target)
    {
        foreach (GameObject panel in panels) panel.SetActive(false);
        target.SetActive(true);
    }
}
public static class SteamHelper
{
    // Upload ederken: String Ýsmi -> Int Array'e çevirir
    public static int[] EncodeNameToDetails(string name)
    {
        if (string.IsNullOrEmpty(name)) return new int[0];

        // Ýsim çok uzunsa keselim (Güvenlik önlemi)
        if (name.Length > 200) name = name.Substring(0, 200);

        byte[] bytes = Encoding.UTF8.GetBytes(name);
        List<int> details = new List<int>();

        for (int i = 0; i < bytes.Length; i += 4)
        {
            int val = 0;
            for (int j = 0; j < 4 && (i + j) < bytes.Length; j++)
            {
                val |= bytes[i + j] << (j * 8);
            }
            details.Add(val);
        }
        return details.ToArray();
    }

    // Download ederken: Int Array'i -> String Ýsme çevirir
    public static string DecodeNameFromDetails(int[] details)
    {
        if (details == null || details.Length == 0) return "Unknown";

        List<byte> bytes = new List<byte>();
        foreach (int val in details)
        {
            for (int j = 0; j < 4; j++)
            {
                byte b = (byte)((val >> (j * 8)) & 0xFF);
                if (b == 0) break; // String bitiþi
                bytes.Add(b);
            }
        }
        return Encoding.UTF8.GetString(bytes.ToArray());
    }
}