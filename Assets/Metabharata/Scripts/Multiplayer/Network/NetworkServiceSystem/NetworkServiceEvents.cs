using System;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Metabharata.Network.Multiplayer.NetworkServiceSystem
{
    /// <summary>
    /// Handles network service-related events and authentication callbacks.
    /// </summary>
    public static class NetworkServiceEvents
    {
        #region Events

        /// <summary>
        /// Occurs when the authentication status changes.
        /// </summary>
        public static event Action<NetworkServiceData.AuthenticationStatus> AuthenticationStatusChanged;

        /// <summary>
        /// Occurs when Unity services are initialized.
        /// </summary>
        public static event Action ServicesInitialized;

        #endregion

        #region Public Methods

        /// <summary>
        /// Registers Unity Authentication event handlers.
        /// </summary>
        public static void RegisterAuthEventHandlers()
        {
            AuthenticationService.Instance.SignedIn += OnSignedIn;
            AuthenticationService.Instance.SignedOut += OnSignedOut;
            AuthenticationService.Instance.SignInFailed += OnSignInFailed;
        }

        /// <summary>
        /// Unregisters Unity Authentication event handlers.
        /// </summary>
        public static void UnregisterAuthEventHandlers()
        {
            AuthenticationService.Instance.SignedIn -= OnSignedIn;
            AuthenticationService.Instance.SignedOut -= OnSignedOut;
            AuthenticationService.Instance.SignInFailed -= OnSignInFailed;
        }

        /// <summary>
        /// Invokes the <see cref="ServicesInitialized"/> event.
        /// </summary>
        public static void InvokeServicesInitialized() => ServicesInitialized?.Invoke();

        /// <summary>
        /// Invokes the <see cref="AuthenticationStatusChanged"/> event with the given status.
        /// </summary>
        /// <param name="status">The new authentication status.</param>
        public static void InvokeAuthStatusChanged(NetworkServiceData.AuthenticationStatus status) =>
            AuthenticationStatusChanged?.Invoke(status);

        #endregion

        #region Private Event Handlers

        private static void OnSignedIn()
        {
            Debug.Log($"Signed in: {AuthenticationService.Instance.PlayerId}");
            AuthenticationStatusChanged?.Invoke(NetworkServiceData.AuthenticationStatus.Authenticated);
        }

        private static void OnSignedOut()
        {
            Debug.Log("Signed out");
            AuthenticationStatusChanged?.Invoke(NetworkServiceData.AuthenticationStatus.Unauthenticated);
        }

        private static void OnSignInFailed(RequestFailedException error)
        {
            Debug.LogError($"Sign in failed: {error}");
            AuthenticationStatusChanged?.Invoke(NetworkServiceData.AuthenticationStatus.Unauthenticated);
        }

        #endregion
    }
}
