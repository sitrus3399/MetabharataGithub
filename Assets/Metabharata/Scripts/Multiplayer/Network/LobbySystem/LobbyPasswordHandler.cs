using System;
using UnityEngine;

public class LobbyPasswordHandler : MonoBehaviour
{
    private string _userInputPassword;

    public void SetUserInputPassword(string password)
    {
        _userInputPassword = password;
    }

    public string GetUserInputPassword()
    {
        return _userInputPassword;
    }

    public void ClearUserInputPassword()
    {
        _userInputPassword = null;
    }

    public bool IsPasswordRequired(LobbySetting setting)
    {
        return !string.IsNullOrEmpty(setting?.Password);
    }

    public bool ValidatePassword(string input, string expected)
    {
        return string.Equals(input, expected, StringComparison.Ordinal);
    }
}
