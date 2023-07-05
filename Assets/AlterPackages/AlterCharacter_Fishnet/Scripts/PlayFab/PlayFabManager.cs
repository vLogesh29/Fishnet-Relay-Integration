using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System;

public class PlayFabManager : MonoBehaviour
{
    void Awake()
    {
        Login();
    }
    
    void Login()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = "Test-ID:" + SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true,
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccessCallback, OnErrorCallback);
    }

    private void OnLoginSuccessCallback(LoginResult result)
    {
        Debug.Log("Login Successful! ID:" + result.PlayFabId);
        AddLoginCount();
    }
    private void OnErrorCallback(PlayFabError error)
    {
        Debug.Log("ERROR-CODE:"+ error.Error);
        Debug.Log(error.GenerateErrorReport());
    }

    public void AddLoginCount()
    {
        var reqest = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = "Login-Frequency",
                    Value = 1,
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(reqest, OnLoginCountUpdatedCallback, OnErrorCallback);
    }

    private void OnLoginCountUpdatedCallback(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Login Attempt Leaderboard Updated");
        Invoke(nameof(RequestLoginCountLeaderBoard), 1);
    }

    public void RequestLoginCountLeaderBoard()
    {
        var reqest = new GetLeaderboardRequest
        {
            StatisticName = "Login-Frequency",
            StartPosition = 0,
            MaxResultsCount = 10,
        };
        PlayFabClientAPI.GetLeaderboard(reqest, OnLoginCountLeaderboardFetched, OnErrorCallback);
    }

    private void OnLoginCountLeaderboardFetched(GetLeaderboardResult result)
    {
        string output = "===== Login Count Leaderboard =====";
        foreach(var item in result.Leaderboard)
        {
            output += "\n[" + item.Position + ":" + item.PlayFabId + "] => " + item.StatValue;
        }
        output += "===== ===== ===== ===== ===== =====";
        Debug.Log(output);
    }
}
