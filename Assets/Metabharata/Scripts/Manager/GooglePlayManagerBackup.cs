using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using System.Text;

public class GooglePlayManagerBackup : MonoBehaviour
{
    private string playerName;
    private int playerMoney;
    private bool isSaving;
    private List<int> playerItems = new List<int>();
    private List<int> playerSkills = new List<int>();
    private List<int> playerAchievements = new List<int>();
    private List<int> playerUpgrades = new List<int>();

    void Start()
    {
        PlayGamesPlatform.Activate();

        SignIn();
    }

    public void SignIn()
    {
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    }

    internal void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            Debug.Log("Login Berhasil: " + PlayGamesPlatform.Instance.GetUserDisplayName());
            string name = PlayGamesPlatform.Instance.GetUserDisplayName();
            string id = PlayGamesPlatform.Instance.GetUserId();
            string ImgUrl = PlayGamesPlatform.Instance.GetUserImageUrl();

            GetLeaderboardData();
            LoadPlayerData();
        }
        else
        {
            Debug.Log("Login Gagal");
            // Disable your integration with Play Games Services or show a login button
            // to ask users to sign-in. Clicking it should call
            // PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication);
        }
    }

    public void GetLeaderboardData()
    {
        //PlayGamesPlatform.Instance.LoadScores(
        //    GPGSIds.leaderboard_highscore,
        //    LeaderboardStart.TopScores,
        //    10,
        //    LeaderboardCollection.Public,
        //    LeaderboardTimeSpan.AllTime,
        //    (data) =>
        //    {
        //        if (data.Valid)
        //        {
        //            Debug.Log("Leaderboard Loaded");
        //            foreach (IScore score in data.Scores)
        //            {
        //                Debug.Log(score.userID + " : " + score.value);
        //            }
        //        }
        //        else
        //        {
        //            Debug.Log("Gagal mengambil leaderboard");
        //        }
        //    });
    }

    public void SaveLeaderboardData()
    {
        int playerScore = 1000; // Contoh skor yang ingin disimpan
        //Social.ReportScore(playerScore, GPGSIds.leaderboard_highscore, (bool success) =>
        //{
        //    if (success)
        //    {
        //        Debug.Log("Data Leaderboard Berhasil Disimpan ke Google Account");
        //    }
        //    else
        //    {
        //        Debug.Log("Gagal Menyimpan Data Leaderboard ke Google Account");
        //    }
        //});
    }

    public void SavePlayerData()
    {
        //string data = JsonUtility.ToJson(new PlayerData(playerName, playerMoney, playerItems, playerSkills, playerAchievements, playerUpgrades));

        //if (!PlayGamesPlatform.Instance.IsAuthenticated())
        //{
        //    Debug.LogWarning("User is not authenticated to Google Play Services");
        //    return;
        //}

        ////if (isSaving)
        ////{
        ////    Debug.LogWarning("Already saving data");
        ////    return;
        ////}

        //isSaving = true;

        //((PlayGamesPlatform)Social.Active).SavedGame.OpenWithAutomaticConflictResolution(
        //    fileName,
        //    DataSource.ReadCacheOrNetwork,
        //    ConflictResolutionStrategy.UseMostRecentlySaved,
        //    (status, metadata) =>
        //    {
        //        if (status != SavedGameRequestStatus.Success)
        //        {
        //            Debug.LogError("Error opening saved game");
        //            isSaving = false;
        //            return;
        //        }

        //        SaveData data = new SaveData
        //        {
        //            playerName = "John",
        //            score = UnityEngine.Random.Range(0, 101)
        //        };

        //        string jsonString = JsonUtility.ToJson(data);
        //        byte[] savedData = Encoding.ASCII.GetBytes(jsonString);

        //        SavedGameMetadataUpdate updatedMetadata = new SavedGameMetadataUpdate.Builder()
        //            .WithUpdatedDescription("My Save File Description")
        //            .Build();

        //        ((PlayGamesPlatform)Social.Active).SavedGame.CommitUpdate(
        //            metadata,
        //            updatedMetadata,
        //            savedData,
        //            (commitStatus, _) =>
        //            {
        //                isSaving = false;
        //                debugText.text = commitStatus == SavedGameRequestStatus.Success
        //                    ? "Data saved successfully"
        //                    : "Error saving data";
        //            });
        //    });


        //PlayGamesPlatform.Instance.SaveGame("player_data", System.Text.Encoding.UTF8.GetBytes(data), (status) =>
        //{
        //    if (status == GooglePlayGames.BasicApi.SavedGame.SavedGameRequestStatus.Success)
        //    {
        //        Debug.Log("Data Player Berhasil Disimpan ke Google Play");
        //    }
        //    else
        //    {
        //        Debug.Log("Gagal Menyimpan Data Player");
        //    }
        //});
    }

    public void LoadPlayerData()
    {
        //PlayGamesPlatform.Instance.LoadGameData("player_data", (status, data) =>
        //{
        //    if (status == GooglePlayGames.BasicApi.SavedGame.SavedGameRequestStatus.Success && data != null)
        //    {
        //        string json = System.Text.Encoding.UTF8.GetString(data);
        //        PlayerData loadedData = JsonUtility.FromJson<PlayerData>(json);
        //        playerName = loadedData.name;
        //        playerMoney = loadedData.money;
        //        playerItems = loadedData.items;
        //        playerSkills = loadedData.skills;
        //        playerAchievements = loadedData.achievements;
        //        playerUpgrades = loadedData.upgrades;
        //        Debug.Log("Data Player Berhasil Dimuat: " + playerName);
        //    }
        //    else
        //    {
        //        Debug.Log("Gagal Memuat Data Player");
        //    }
        //});
    }
}
