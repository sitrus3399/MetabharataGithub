using UnityEngine;
using System;
using System.Collections;
using System.Net;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

public class DailyCheckInManager : MonoBehaviour
{
    private const string lastLoginKey = "LastLoginDate";
    [HideInInspector] public string rewardOpenKey = "DailyLoginOpen";
    [HideInInspector] public string rewardClaimedKey = "DailyLoginClaim";
    private const string missedDaysKey = "MissedLoginDays";
    [SerializeField] private int maxLoginDays = 30;
    [SerializeField] private bool[] rewardOpen;
    [SerializeField] private bool[] rewardClaimed;

    void Start()
    {
        StartCoroutine(CheckDailyLogin());
    }

    IEnumerator CheckDailyLogin()
    {
        DateTime serverTime = GetServerTimeFromGoogle();
        if (serverTime == DateTime.MinValue)
        {
            Debug.LogError("Gagal mendapatkan waktu server.");
            yield break;
        }

        serverTime = serverTime.AddHours(7);// Konversi ke GMT+7

        DateTime lastLogin = GetLastLogin(); //Ambil data last login yang tersimpan

        // Hitung waktu berlalu
        TimeSpan timeElapsed = serverTime - lastLogin;
        //DisplayTimeElapsed(timeElapsed);

        // Hitung waktu hingga hari berikutnya
        TimeSpan timeUntilNextDay = GetTimeUntilNextDay(serverTime);
        Debug.Log($"⏳ Waktu hingga hari berikutnya: {timeUntilNextDay.Hours} jam {timeUntilNextDay.Minutes} menit {timeUntilNextDay.Seconds} detik");

        int missedDays = (serverTime.Date - lastLogin.Date).Days - 1;
        if (missedDays > 0)
        {
            int totalMissedDays = PlayerPrefs.GetInt(missedDaysKey, 0) + missedDays;
            PlayerPrefs.SetInt(missedDaysKey, totalMissedDays);
            PlayerPrefs.Save();
            Debug.Log($"⚠️ Anda melewatkan {missedDays} hari login.");
        }

        LoadDataReward();

        if (serverTime.Date > lastLogin.Date)
        {
            OpenReward();
            SaveLastLogin(serverTime);
            Debug.Log("🎉 Login pertama hari ini! Reward diberikan.");
        }
        else
        {
            Debug.Log("✅ Sudah login hari ini. Coba lagi besok!");
        }

        // Tampilkan statistik
        DisplayLoginStatistics();
    }

    DateTime GetServerTimeFromGoogle() //Ambil data waktu dari google
    {
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.google.com");
            request.Method = "HEAD"; // Hanya mengambil header
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                string dateHeader = response.Headers["date"]; //Mengambil data date dari header
                return DateTime.Parse(dateHeader).ToUniversalTime(); // Konversi ke UTC
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Gagal mendapatkan waktu dari Google: " + e.Message);
            return DateTime.MinValue;
        }
    }

    DateTime GetLastLogin()
    {
        string lastLoginStr = PlayerPrefs.GetString(lastLoginKey, "");
        if (DateTime.TryParse(lastLoginStr, out DateTime lastLogin))
        {
            return lastLogin;
        }
        return DateTime.MinValue;
    }

    void SaveLastLogin(DateTime date)
    {
        PlayerPrefs.SetString(lastLoginKey, date.ToString("yyyy-MM-dd HH:mm:ss"));
        PlayerPrefs.Save();
    }

    void LoadDataReward()
    {
        for (int i = 0; i < rewardOpen.Length; i++)
        {
            if (PlayerPrefs.GetInt($"{rewardOpenKey}{i}") == 1)
            {
                rewardOpen[i] = true;
            }
            else
            {
                rewardOpen[i] = false;
            }
        }

        for (int i = 0; i < rewardClaimed.Length; i++)
        {
            if (PlayerPrefs.GetInt($"{rewardClaimedKey}{i}") == 1)
            {
                rewardClaimed[i] = true;
            }
            else
            {
                rewardClaimed[i] = false;
            }
        }
    }

    void OpenReward()
    {
        int currentLoginDay = PlayerPrefs.GetInt(rewardOpenKey, 1);

        if (currentLoginDay < maxLoginDays)
        {
            currentLoginDay++;
        }

        CheckAllRewardClaimed();

        rewardOpen[currentLoginDay - 1] = true;

        for (int i = 0;i < currentLoginDay;i++)
        {
            rewardOpen[i] = true;
            PlayerPrefs.SetInt($"{rewardOpenKey}{i}", 1);
        }

        PlayerPrefs.SetInt(rewardOpenKey, currentLoginDay);
        PlayerPrefs.Save();
    }

    void CheckAllRewardClaimed()
    {
        bool allOpen = rewardOpen.Contains(false);
        bool allClaim = rewardClaimed.Contains(false);

        if (!allOpen && !allClaim)
        {
            ResetDayReward();
        }
    }

    public void ClaimDailyReward(int indexReward)
    {
        rewardClaimed[indexReward] = true;

        //PlayerPrefs.SetInt(rewardOpenKey, indexReward);

        PlayerPrefs.SetInt($"{rewardClaimedKey}{indexReward}", 1);
        PlayerPrefs.Save();

        PlayerManager.Manager.AddCoin(indexReward);

        Debug.Log($"🎁 Daily Reward #{indexReward} diberikan!");
    }

    void DisplayTimeElapsed(TimeSpan timeElapsed)
    {
        int years = (int)(timeElapsed.Days / 365);
        int months = (int)(timeElapsed.Days / 30);
        int days = timeElapsed.Days;
        int hours = timeElapsed.Hours;
        int minutes = timeElapsed.Minutes;
        int seconds = timeElapsed.Seconds;

        Debug.Log($"⏳ Waktu berlalu sejak login terakhir:");
        Debug.Log($"📅 Tahun: {years}");
        Debug.Log($"📆 Bulan: {months}");
        Debug.Log($"📌 Hari: {days}");
        Debug.Log($"🕒 Jam: {hours}");
        Debug.Log($"⏳ Menit: {minutes}");
        Debug.Log($"⌛ Detik: {seconds}");
    }

    TimeSpan GetTimeUntilNextDay(DateTime currentTime)
    {
        DateTime nextDay = currentTime.Date.AddDays(1);
        return nextDay - currentTime;
    }

    void DisplayLoginStatistics()
    {
        int totalRewards = PlayerPrefs.GetInt(rewardOpenKey, 0);
        int totalMissedDays = PlayerPrefs.GetInt(missedDaysKey, 0);

        Debug.Log("📊 Statistik Daily Login:");
        Debug.Log($"✅ Total login: {totalRewards}");
        Debug.Log($"⚠️ Total hari yang terlewat: {totalMissedDays}");

        // Hitung hari yang belum diklaim dalam siklus ini
        List<int> unclaimedDays = new List<int>();
        for (int i = 1; i <= maxLoginDays; i++)
        {
            //if (!claimedDays.Contains(i))
            //{
            //    unclaimedDays.Add(i);
            //}
        }

        if (unclaimedDays.Count > 0)
        {
            Debug.Log($"❌ Hari login yang belum diklaim: {string.Join(", ", unclaimedDays)}");
        }
    }

    public bool CheckRewardOpen(int index)
    {
        return rewardOpen[index];
    }

    public bool CheckRewardClaimed(int index)
    {
        return rewardClaimed[index];
    }

    void ResetDayReward()
    {
        for (int i = 0; i < rewardClaimed.Length; i++)
        {
            rewardClaimed[0] = false;
            PlayerPrefs.SetInt($"{rewardClaimed}{i}", 0);
        }

        for (int i = 0; i < rewardOpen.Length; i++)
        {
            rewardOpen[0] = false;
            PlayerPrefs.SetInt($"{rewardOpenKey}{i}", 0);
        }
    }
}