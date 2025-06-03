using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using System.Collections.Generic;
using System;
using System.Text;
using GooglePlayGames.BasicApi.SavedGame;
using Newtonsoft.Json;
using UnityEngine.SocialPlatforms.Impl;

public class GooglePlayManager : MonoBehaviour
{
    public static GooglePlayManager Manager;

    [SerializeField] private string saveName = "MySavedGame"; // Nama file save
    [SerializeField] private string leaderboardID = "Cfji293fjsie_QA"; // Ganti dengan ID leaderboard Anda
    private string playerName;
    private int playerMoney;
    private int playerRanking;
    private List<int> characterOwned = new List<int>();
    private List<int> characterUpgrade = new List<int>();
    private List<int> dailyOpened = new List<int>();
    private List<int> dailyClaimed = new List<int>();

    public delegate void AfterSignIn();
    public static event AfterSignIn afterSignIn;

    private void Awake()
    {
        if (Manager == null)
        {
            Manager = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        PlayGamesPlatform.Activate();

        SignIn();
        LoadLeaderboardScores();

        // Show leaderboard UI
        PlayGamesPlatform.Instance.ShowLeaderboardUI("Cfji293fjsie_QA");

        //Mengakses data Papan Peringkat
        string leaderboardID = "GPGSIds.leaderboard_leaders_in_smoketesting";
        PlayGamesPlatform.Instance.LoadScores(
            leaderboardID,
            LeaderboardStart.PlayerCentered,
            100,
            LeaderboardCollection.Public,
            LeaderboardTimeSpan.AllTime,
            (data) =>
            {
                if (data.Valid)
                {
                    Debug.Log("Leaderboard data valid!");
                    Debug.Log($"Total Approximate: {data.ApproximateCount}");
                    Debug.Log($"Total Scores Fetched: {data.Scores.Length}");

                    foreach (var score in data.Scores)
                    {
                        Debug.Log($"{score.rank}. {score.userID} - {score.value}");
                    }
                }
                else
                {
                    Debug.Log("Leaderboard data is not valid.");
                }
            });
    }

    public void SignIn()
    {
        SaveGame();
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
        LoadLeaderboardScores();
    }

    public void LoadLeaderboardScores()
    {
        PlayGamesPlatform.Instance.LoadScores(
            leaderboardID,
            LeaderboardStart.TopScores, // Mengambil dari skor teratas
            10, // Ambil 10 skor pertama
            LeaderboardCollection.Public,
            LeaderboardTimeSpan.AllTime,
            (data) =>
            {
                if (data.Valid)
                {
                    Debug.Log("Leaderboard Loaded Successfully!");
                    //DisplayLeaderboard(data);
                }
                else
                {
                    Debug.Log("Failed to load leaderboard.");
                }
            });
    }

    //void DisplayLeaderboard(LeaderboardScoreData data)
    //{
    //    // Hapus entri sebelumnya
    //    foreach (Transform child in leaderboardContainer)
    //    {
    //        Destroy(child.gameObject);
    //    }

    //    userIds.Clear();

    //    foreach (var score in data.Scores)
    //    {
    //        userIds.Add(score.userID);
    //    }

    //    // Load user profiles
    //    Social.LoadUsers(userIds.ToArray(), (users) =>
    //    {
    //        foreach (var score in data.Scores)
    //        {
    //            IUserProfile user = FindUser(users, score.userID);
    //            string playerName = user != null ? user.userName : "**Unknown**";

    //            GameObject entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
    //            TMP_Text nameText = entryObj.transform.Find("playerNameTMP").GetComponent<TMP_Text>();
    //            TMP_Text scoreText = entryObj.transform.Find("playerScoreTMP").GetComponent<TMP_Text>();

    //            nameText.text = playerName;
    //            scoreText.text = score.formattedValue;

    //            // Jika pemain saat ini, ubah warna teks menjadi merah
    //            if (score.userID == Social.localUser.id)
    //            {
    //                nameText.color = Color.red;
    //                scoreText.color = Color.red;
    //            }
    //        }
    //    });
    //}

    IUserProfile FindUser(IUserProfile[] users, string userID)
    {
        foreach (var user in users)
        {
            if (user.id == userID)
                return user;
        }
        return null;
    }

    public void GetNextPage(LeaderboardScoreData data)
    {
        if (data.NextPageToken != null)
        {
            PlayGamesPlatform.Instance.LoadMoreScores(data.NextPageToken, 10,
                (results) =>
                {
                    if (results.Valid)
                    {
                        //DisplayLeaderboard(results);
                    }
                });
        }
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

            afterSignIn.Invoke();
        }
        else
        {
            Debug.Log("Login Gagal");
            // Disable your integration with Play Games Services or show a login button
            // to ask users to sign-in. Clicking it should call
            // PlayGamesPlatform.Instance.ManuallyAuthenticate(ProcessAuthentication);
        }
    }

    void SaveGame()
    {
        //Post score to leaderboard
        // Post score 12345 to leaderboard ID "Cfji293fjsie_QA" and tag "FirstDaily")
        PlayGamesPlatform.Instance.ReportScore(12345, "Cfji293fjsie_QA", "FirstDaily", (bool success) => {
            // Handle success or failure
        });

        if (!PlayGamesPlatform.Instance.IsAuthenticated())
        {
            Debug.LogWarning("User is not authenticated to Google Play Services");
            return;
        }

        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;

        // Buka file save terlebih dahulu
        savedGameClient.OpenWithAutomaticConflictResolution(
            saveName, 
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime,
            OnSavedGameOpened
        );
    }

    void OnSavedGameOpened(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            // data yang akan disimpan
            string tmpPlayerName = playerName;
            int tmpPlayerMoney = playerMoney;
            int tmpPlayerRanking = playerRanking;
            List<int> tmpCharacterOwned = characterOwned;
            List<int> tmpCharacterUpgrade = characterUpgrade;
            List<int> tmpDailyOpened = dailyOpened;
            List<int> tmpDailyClaimed = dailyClaimed;

            // Waktu bermain total (hanya contoh)
            TimeSpan totalPlaytime = TimeSpan.FromHours(5);

            // Panggil fungsi SaveGame yang telah dibuat
            SavePlayerProgress(game, tmpPlayerName, tmpPlayerMoney, tmpPlayerRanking, tmpCharacterOwned, tmpCharacterUpgrade, tmpDailyOpened, tmpDailyClaimed, totalPlaytime);
        }
        else
        {
            Debug.LogError("Gagal membuka save game!");
        }
    }

    void SavePlayerProgress(ISavedGameMetadata game, string tmpPlayerName, int tmpPlayerMoney, int tmpPlayerRanking, List<int> tmpCharacterOwned, List<int> tmpCharacterUpgrade, List<int> tmpDailyOpened, List<int> tmpDailyClaimed, TimeSpan totalPlaytime)
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;

        // Buat objek data untuk disimpan
        PlayerData data = new PlayerData
        (
            tmpPlayerName,
            tmpPlayerMoney,
            tmpPlayerRanking,
            tmpCharacterOwned,
            tmpCharacterUpgrade,
            tmpDailyOpened,
            tmpDailyClaimed
        );

        // Konversi objek ke JSON
        string jsonData = JsonUtility.ToJson(data);
        byte[] savedData = Encoding.UTF8.GetBytes(jsonData);

        SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder();
        builder = builder
            .WithUpdatedPlayedTime(totalPlaytime)
            .WithUpdatedDescription("Saved game at " + DateTime.Now.ToString());

        SavedGameMetadataUpdate updatedMetadata = builder.Build();
        savedGameClient.CommitUpdate(game, updatedMetadata, savedData, OnSavedGameWritten);
    }

    public void OnSavedGameWritten(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            Debug.Log("Game successfully saved.");
        }
        else
        {
            Debug.LogError("Failed to save game.");
        }
    }

    void LoadPlayerData()
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.OpenWithAutomaticConflictResolution(saveName, DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime, OnSavedGameDataRead);
        //savedGameClient.ReadBinaryData(game, OnSavedGameDataRead);
    }

    public void OnSavedGameDataRead(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            savedGameClient.ReadBinaryData(game, OnSavedGameDataRead);
            // handle processing the byte array data
        }
        else
        {
            // handle error
        }
    }

    public void OnSavedGameDataRead(SavedGameRequestStatus status, byte[] data)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            if (data != null && data.Length > 0)
            {
                string json = Encoding.UTF8.GetString(data);
                PlayerData loadedData = JsonConvert.DeserializeObject<PlayerData>(json);

                if (loadedData != null)
                {
                    Debug.Log($"Player Name: {loadedData.playerName}");
                    Debug.Log($"Player Money: {loadedData.playerMoney}");
                    Debug.Log($"Player Ranking: {loadedData.playerRanking}");
                    Debug.Log($"List1: {string.Join(",", loadedData.list1)}");
                    Debug.Log($"List2: {string.Join(",", loadedData.list2)}");
                    Debug.Log($"List3: {string.Join(",", loadedData.list3)}");
                    Debug.Log($"List4: {string.Join(",", loadedData.list4)}");
                }
            }
            else
            {
                Debug.Log("No saved data found.");
            }
        }
        else
        {
            Debug.LogError("Failed to load game data.");
        }
    }

    public void GetLeaderboardData()
    {

    }
}

[Serializable]
public class PlayerData
{
    public string playerName;
    public int playerMoney;
    public int playerRanking;
    public List<int> list1;
    public List<int> list2;
    public List<int> list3;
    public List<int> list4;

    public PlayerData(string newPlayerName, int newPlayerMoney, int newPlayerRanking, List<int> items, List<int> skills, List<int> achievements, List<int> upgrades)
    {
        this.playerName = newPlayerName;
        this.playerMoney = newPlayerMoney;
        this.playerRanking = newPlayerRanking;
        this.list1 = items;
        this.list2 = skills;
        this.list3 = achievements;
        this.list4 = upgrades;
    }
}
