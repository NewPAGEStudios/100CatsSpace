using UnityEngine;
#if STEAMWORKS_NET
using Steamworks;
#endif

public static class PlayerIdentity
{
    // Steam olmayan platformlarda (Android vb.) kullanılan isim
    public const string PlayerNamePrefKey = "PlayerName";
    public const string DefaultPlayerName = "Player";

    public static string GetPlayerName()
    {
#if STEAMWORKS_NET
        if (SteamManager.Initialized) return SteamFriends.GetPersonaName();
#endif
        return PlayerPrefs.GetString(PlayerNamePrefKey, DefaultPlayerName);
    }
}
