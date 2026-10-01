using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if STEAMWORKS_NET
using Steamworks;
#endif
using System;
public class SteamAchivementController : MonoBehaviour
{
    private static SteamAchivementController _instance;
    public static SteamAchivementController Instance
    {
        get { return _instance; }
    }

#if STEAMWORKS_NET
    protected Callback<UserStatsReceived_t> UserStatReceived;
#endif


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

#if STEAMWORKS_NET
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
            SteamUserStats.StoreStats(); // Değişikliği Steam'e gönder
        }
    }
#endif

    public void TryUnlockAchivement(string id)
    {
        if (!IsThisAchivementUnlocked(id))
        {
            UnlockAchivement(id);
        }
    }

    public bool IsThisAchivementUnlocked(string id)
    {
#if STEAMWORKS_NET
        SteamUserStats.GetAchievement(id, out bool achived);

        return achived;
#else
        return false;
#endif
    }
    public void UnlockAchivement(string id)
    {
#if STEAMWORKS_NET
        if (SteamUserStats.SetAchievement(id))
        {
            Debug.Log("id " + id + ": achived");
            SteamUserStats.StoreStats();
        }
#endif

    }
    public string GetAchivementDsipaly(string id)
    {
#if STEAMWORKS_NET
        string name = SteamUserStats.GetAchievementDisplayAttribute(id, "name");
        string desc = SteamUserStats.GetAchievementDisplayAttribute(id, "desc");

        return name + "/" + desc;
#else
        return "";
#endif
    }


}
