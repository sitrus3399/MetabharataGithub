using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Core;
using UnityEngine;

namespace Metabharata.Network.Multiplayer.NetworkServiceSystem
{
    /// <summary>
    /// MonoBehaviour initiator for the network service system.
    /// </summary>
    public class NetworkServiceInitiator : MonoBehaviour
    {
        #region Singleton

        /// <summary>
        /// Singleton instance of the NetworkServiceInitiator.
        /// </summary>
        public static NetworkServiceInitiator Instance { get; private set; }

        #endregion

        #region Inspector Fields

        [Header("Prefab Reference")]
        [SerializeField]
        private NetworkManager networkManagerPrefab;

        #endregion

        #region Private Fields

        private NetworkServiceSystem _system;

        #endregion

        #region Properties

        /// <summary>
        /// Provides access to the NetworkServiceSystem instance.
        /// </summary>
        public NetworkServiceSystem System => _system;

        /// <summary>
        /// Indicates whether the network system is initialized.
        /// </summary>
        public bool IsInitialized => _system is { IsInitialized: true };

        /// <summary>
        /// Gets the current authentication status.
        /// </summary>
        public NetworkServiceData.AuthenticationStatus CurrentAuthStatus =>
            _system != null ? _system.CurrentAuthStatus : NetworkServiceData.AuthenticationStatus.Unauthenticated;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            _system = new NetworkServiceSystem(networkManagerPrefab);
            _ = _system.InitializeSystemAsync();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Checks if the network system is ready.
        /// </summary>
        /// <returns>True if the system is ready; otherwise, false.</returns>
        public bool IsSystemReady() => _system != null && _system.IsSystemReady();

        #endregion

        #region Utility Method

        public async Task WaitUntilInitializationFinished()
        {
            if (!Instance.IsInitialized)
            {
                var cancellationToken = new CancellationTokenSource(5000);
                while (!Instance.IsInitialized)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw new Exception()
                        {
                            Source = "Request Timed Out when waiting for initialization..."
                        };
                    }
                    await Task.Yield();
                }
            }
        }

        #endregion
    }
}
