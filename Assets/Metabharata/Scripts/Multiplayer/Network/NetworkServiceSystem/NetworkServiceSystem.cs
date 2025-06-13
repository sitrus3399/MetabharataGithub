using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using AuthenticationStatusChanged = NetworkServiceEvents.AuthenticationStatusChanged;
using ServiceInitializedEvent = NetworkServiceEvents.ServiceInitializedEvent;

// ReSharper disable once CheckNamespace
namespace Metabharata.Network.Multiplayer.NetworkServiceSystem
{
    /// <summary>
    /// Handles Unity network service initialization and user authentication.
    /// </summary>
    public class NetworkServiceSystem
    {
        public static Action OnInitializationFinished;

        #region Fields & Properties

        /// <summary>
        /// Indicates if the system is currently initializing.
        /// </summary>
        public bool IsInitializing { get; set; }

        /// <summary>
        /// Indicates if Unity services have been initialized.
        /// </summary>
        public bool IsInitialized { get; set; }

        /// <summary>
        /// Current authentication status of the user.
        /// </summary>
        public NetworkServiceData.AuthenticationStatus CurrentAuthStatus { get; set; } = NetworkServiceData.AuthenticationStatus.Unauthenticated;

        /// <summary>
        /// The active NetworkManager instance.
        /// </summary>
        public NetworkManager NetworkManagerInstance { get; set; }

        /// <summary>
        /// The prefab to use for instantiating the NetworkManager.
        /// </summary>
        public NetworkManager NetworkManagerPrefab { get; set; }

        #endregion

        #region Constructor

        public NetworkServiceSystem(NetworkManager networkManagerPrefab)
        {
            NetworkManagerPrefab = networkManagerPrefab;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes Unity services and handles authentication.
        /// </summary>
        public async Task InitializeSystemAsync()
        {
            if (IsInitializing || IsInitialized ||
                CurrentAuthStatus == NetworkServiceData.AuthenticationStatus.Authenticating)
                return;

            IsInitializing = true;

            try
            {
                // Initialize Unity Services if needed
                if (UnityServices.State == ServicesInitializationState.Uninitialized)
                    await UnityServices.InitializeAsync();

                ServiceInitializedEvent.Publish(new ServiceInitializedEvent(true, this));

                // Unregister and re-register authentication event handlers to avoid duplicates
                NetworkServiceEvents.UnregisterAuthEventHandlers();
                NetworkServiceEvents.RegisterAuthEventHandlers();

                // Set initial authentication status
                SetAuthenticationStatus(NetworkServiceData.AuthenticationStatus.Authenticating);

                // Attempt guest login if not already authorized
                if (!AuthenticationService.Instance.IsAuthorized && AuthenticationService.Instance.AccessToken == null)
                    await TryLoginGuest();

                // Instantiate and persist the NetworkManager if needed
                if (NetworkManagerInstance == null)
                {
                    NetworkManagerInstance = UnityEngine.Object.Instantiate(NetworkManagerPrefab);
                    NetworkManagerInstance.SetSingleton();
                    UnityEngine.Object.DontDestroyOnLoad(NetworkManagerInstance);
                }

                IsInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"NetworkServiceSystem initialization failed...\n" +
                               $"Log: {ex}");
                IsInitialized = false;
            }
            finally
            {
                IsInitializing = false;
                OnInitializationFinished?.Invoke();
            }
        }

        #endregion

        #region Authentication

        /// <summary>
        /// Attempts to sign in as a guest.
        /// </summary>
        private static async Task TryLoginGuest()
        {
            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            catch (AuthenticationException e)
            {
                if (e.ErrorCode == 10000)
                    Debug.Log("Player already logged in...");
                else
                    Debug.LogError($"Authentication failed: {e}");
            }
            catch (RequestFailedException e)
            {
                Debug.LogError($"Request failed: {e}");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }

        /// <summary>
        /// Updates the current authentication status and notifies listeners.
        /// </summary>
        private void SetAuthenticationStatus(NetworkServiceData.AuthenticationStatus status)
        {
            if (CurrentAuthStatus == status)
                return;

            CurrentAuthStatus = status;
            AuthenticationStatusChanged.Publish(new AuthenticationStatusChanged(status));
        }

        #endregion

        #region Utility

        /// <summary>
        /// Checks if the system is initialized and authenticated.
        /// </summary>
        public bool IsSystemReady()
        {
            return IsInitialized &&
                   CurrentAuthStatus == NetworkServiceData.AuthenticationStatus.Authenticated;
        }

        #endregion
    }
}
