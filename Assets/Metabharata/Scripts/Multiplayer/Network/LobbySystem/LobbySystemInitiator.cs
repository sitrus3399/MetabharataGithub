using System;
using UnityEngine;

public class LobbySystemInitiator : MonoBehaviour
{
    private LobbySystem _system;

    public static LobbySystemInitiator Instance { get; private set; }

    public LobbySystem System => _system;

    /// <summary>
    /// Indicates whether the network system is initialized.
    /// </summary>
    public bool IsInitialized => _system is { IsInitialized: true };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        _system = new LobbySystem();
        _system.InitializeSystem();
    }

    #region Public Methods

    /// <summary>
    /// Checks if the network system is ready.
    /// </summary>
    /// <returns>True if the system is ready; otherwise, false.</returns>
    public bool IsSystemReady() => _system != null && _system.IsSystemReady();

    #endregion
}
