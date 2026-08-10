using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using System;
public class SteamAchivementController : MonoBehaviour
{
    private static SteamAchivementController _instance;
    public static SteamAchivementController Instance
    {
        get { return _instance; }
    }


    protected Callback<UserStatsReceived_t> UserStatReceived;


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
    }
    private void OnEnable()
    {
        UserStatReceived = Callback<UserStatsReceived_t>.Create(OnUserStat);
    }
    private void OnDisable()
    {
        UserStatReceived.Dispose();
    }

    private void OnUserStat(UserStatsReceived_t param)
    {
        Debug.Log(param.m_eResult);

        Debug.Log("GameId: " + param.m_nGameID);
        Debug.Log("UserId: " + param.m_steamIDUser);

    }
    private void Start()
    {
        SteamUserStats.RequestUserStats(SteamUser.GetSteamID());
//        Delete125Achivement();
    }

    public void Delete125Achivement()
    {
        bool success = SteamUserStats.ClearAchievement("FindAllCats");
        if (success)
        {
            SteamUserStats.StoreStats(); // Deðiþikliði Steam'e gönder
        }
    }

    public void TryUnlockAchivement(string id)
    {
        if (!IsThisAchivementUnlocked(id)) 
        {
            UnlockAchivement(id);
        }
    }

    public bool IsThisAchivementUnlocked(string id)
    {
        SteamUserStats.GetAchievement(id, out bool achived);

        return achived;
    }
    public void UnlockAchivement(string id)
    {
        if (SteamUserStats.SetAchievement(id))
        {
            Debug.Log("id " + id + ": achived");
            SteamUserStats.StoreStats();
        }

    }
    public string GetAchivementDsipaly(string id)
    {
        string name = SteamUserStats.GetAchievementDisplayAttribute(id, "name");
        string desc = SteamUserStats.GetAchievementDisplayAttribute(id, "desc");

        return name + "/" + desc;
    }


}
