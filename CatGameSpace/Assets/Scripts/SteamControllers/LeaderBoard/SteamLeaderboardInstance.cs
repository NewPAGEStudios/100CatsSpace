using Steamworks;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class SteamLeaderboardInstance
{
    private LeaderboardConfig _config;
    private GameObject _prefab;
    private SteamLeaderboard_t _steamLeaderboard;
    private bool _initialized = false;

    private Queue<int> _pendingScoreUploads = new Queue<int>();

    private CallResult<LeaderboardFindResult_t> _findResult;
    private CallResult<LeaderboardScoreUploaded_t> _uploadResult;
    private CallResult<LeaderboardScoresDownloaded_t> _downloadGlobalResult;
    private CallResult<LeaderboardScoresDownloaded_t> _downloadFriendResult;

    public SteamLeaderboardInstance(LeaderboardConfig config, GameObject prefab)
    {
        _config = config;
        _prefab = prefab;

        _findResult = CallResult<LeaderboardFindResult_t>.Create(OnLeaderboardFound);
        _uploadResult = CallResult<LeaderboardScoreUploaded_t>.Create(OnScoreUploaded);
        _downloadGlobalResult = CallResult<LeaderboardScoresDownloaded_t>.Create(OnGlobalScoresDownloaded);
        _downloadFriendResult = CallResult<LeaderboardScoresDownloaded_t>.Create(OnFriendScoresDownloaded);

        SteamAPICall_t handle = SteamUserStats.FindLeaderboard(_config.id);
        _findResult.Set(handle);
    }

    private void OnLeaderboardFound(LeaderboardFindResult_t param, bool bIOFailure)
    {
        if (bIOFailure || param.m_bLeaderboardFound == 0) return;

        _steamLeaderboard = param.m_hSteamLeaderboard;
        _initialized = true;

        while (_pendingScoreUploads.Count > 0)
        {
            UploadScore(_pendingScoreUploads.Dequeue());
        }

        DownloadScores(false);
        DownloadScores(true);
    }

    // --- ÖNEMLÝ: UPLOAD KISMI (Ýsim Encoding) ---
    public void UploadScore(int score)
    {
        if (!_initialized)
        {
            _pendingScoreUploads.Enqueue(score);
            return;
        }

        // 1. Oyuncunun ismini al
        string myName = SteamFriends.GetPersonaName();
        // 2. Ýsmi sayý dizisine çevir
        int[] detailName = SteamHelper.EncodeNameToDetails(myName);

        // 3. Skorla birlikte ismi de gönder
        SteamAPICall_t handle = SteamUserStats.UploadLeaderboardScore(
            _steamLeaderboard,
            ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest,
            score,
            detailName,       // Detaylarý ekle
            detailName.Length // Uzunluðu ekle
        );
        _uploadResult.Set(handle);
    }

    private void OnScoreUploaded(LeaderboardScoreUploaded_t param, bool bIOFailure)
    {
        if (!bIOFailure && param.m_bSuccess == 1 && param.m_bScoreChanged == 1)
        {
            DownloadScores(false);
            DownloadScores(true);
        }
    }

    public void DownloadScores(bool friendsOnly)
    {
        if (!_initialized) return;

        ELeaderboardDataRequest request = friendsOnly ? ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends : ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal;
        SteamAPICall_t handle = SteamUserStats.DownloadLeaderboardEntries(_steamLeaderboard, request, 0, 10);

        if (friendsOnly) _downloadFriendResult.Set(handle);
        else _downloadGlobalResult.Set(handle);
    }

    private void OnGlobalScoresDownloaded(LeaderboardScoresDownloaded_t param, bool bIOFailure)
    {
        if (!bIOFailure) ProcessDownloadedScores(param, _config.globalContentParent);
    }

    private void OnFriendScoresDownloaded(LeaderboardScoresDownloaded_t param, bool bIOFailure)
    {
        if (!bIOFailure) ProcessDownloadedScores(param, _config.friendContentParent);
    }

    // --- ÖNEMLÝ: DOWNLOAD KISMI (Ýsim Decoding) ---
    private void ProcessDownloadedScores(LeaderboardScoresDownloaded_t param, Transform targetParent)
    {
        if (targetParent == null) return;

        foreach (Transform child in targetParent) GameObject.Destroy(child.gameObject);

        for (int i = 0; i < param.m_cEntryCount; i++)
        {
            LeaderboardEntry_t entry;
            int[] details = new int[64]; // Detaylar için yer aç

            // 1. Veriyi ve DETAYLARI indir
            SteamUserStats.GetDownloadedLeaderboardEntry(param.m_hSteamLeaderboardEntries, i, out entry, details, 64);

            // 2. Detaylarý isme çevir (Artýk bekleme yok, isim burada!)
            string playerName = SteamHelper.DecodeNameFromDetails(details);

            // Eðer detay boþsa (eski skor vs.) Unknown yazar
            if (string.IsNullOrEmpty(playerName)) playerName = "Unknown";

            int ImageID = SteamFriends.GetLargeFriendAvatar(entry.m_steamIDUser);

            GameObject go = GameObject.Instantiate(_prefab, targetParent);
            var itemScript = go.GetComponent<LeaderboardListItem>();

            if (itemScript != null)
            {
                itemScript.SetPlayerInfos(entry.m_nGlobalRank, playerName, ImageID, entry.m_steamIDUser, entry.m_nScore);
            }

            if (entry.m_steamIDUser == SteamUser.GetSteamID() && _config.selfStatsUI != null)
            {
                _config.selfStatsUI.gameObject.SetActive(true);
                _config.selfStatsUI.SetPlayerInfos(entry.m_nGlobalRank, playerName, ImageID, entry.m_steamIDUser, entry.m_nScore);
            }
        }
    }

    // Avatar yüklendiðinde listeyi güncellemek için
    public void OnAvatarLoadedReceived(AvatarImageLoaded_t callback)
    {
        UpdateListAvatar(_config.globalContentParent, callback);
        UpdateListAvatar(_config.friendContentParent, callback);

        if (_config.selfStatsUI != null && _config.selfStatsUI.steamID == callback.m_steamID)
        {
            _config.selfStatsUI.SetSteamImageAsTexture(callback.m_iImage);
        }
    }

    private void UpdateListAvatar(Transform parent, AvatarImageLoaded_t callback)
    {
        if (parent == null) return;
        foreach (Transform t in parent)
        {
            var item = t.GetComponent<LeaderboardListItem>();
            if (item != null && item.steamID == callback.m_steamID)
            {
                item.SetSteamImageAsTexture(callback.m_iImage);
            }
        }
    }
}
[System.Serializable]
public struct LeaderboardConfig
{
    public string id; // Steam Leaderboard ID ("uk_CityName_Normal" vb.)
    public Transform globalContentParent; // Global liste parent'ý
    public Transform friendContentParent; // Arkadaþ liste parent'ý
    public LeaderboardListItem selfStatsUI; // Oyuncunun kendi statlarýný gösteren UI
}