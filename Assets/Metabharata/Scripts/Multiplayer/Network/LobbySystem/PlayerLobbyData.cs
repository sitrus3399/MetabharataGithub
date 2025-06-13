using UnityEngine;

public class PlayerLobbyData
{
    public static readonly string PlayerReadyKey = "PlayerReadyState";
    public static readonly string PlayerHostKey = "PlayerHostState";

    public string PlayerId { get; set; }
    public string PlayerName { get; set; }
    public string PlayerAvatar { get; set; }
    public bool IsReady { get; set; }
    public bool IsHost { get; set; }
}
