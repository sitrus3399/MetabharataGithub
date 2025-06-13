using Metabharata.Network.Multiplayer.NetworkServiceSystem;
using NyxMachina.Shared.EventFramework;
using NyxMachina.Shared.EventFramework.Core.Payloads;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public static class NetworkServiceEvents
    {
        #region Events

        public class AuthenticationStatusChanged : IPayload
        {
            public NetworkServiceData.AuthenticationStatus Status;

            public AuthenticationStatusChanged(NetworkServiceData.AuthenticationStatus status)
            {
                Status = status;
            }

            public static void Publish(AuthenticationStatusChanged payload)
            {
                EventMessenger.Main.Publish(payload);
            }

            public static AuthenticationStatusChanged GetState()
            {
                return EventMessenger.Main.GetState<AuthenticationStatusChanged>();
            }
        }

        public class ServiceInitializedEvent : IPayload
        {
            public bool IsInitialized;
            public NetworkServiceSystem System;

            public ServiceInitializedEvent(bool isInitialized, NetworkServiceSystem system)
            {
                IsInitialized = isInitialized;
                System = system;
            }

            public static void Publish(ServiceInitializedEvent payload)
            {
                EventMessenger.Main.Publish(payload);
            }

            public static ServiceInitializedEvent GetState()
            {
                return EventMessenger.Main.GetState<ServiceInitializedEvent>();
            }
        }

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

        #endregion

        #region Private Event Handlers

        private static void OnSignedIn()
        {
            Debug.Log($"Signed in: {AuthenticationService.Instance.PlayerId}");
            var authenticationState = NetworkServiceData.AuthenticationStatus.Authenticated;
            AuthenticationStatusChanged.Publish(new AuthenticationStatusChanged(authenticationState));
        }

        private static void OnSignedOut()
        {
            Debug.Log("Signed out");
            var authenticationState = NetworkServiceData.AuthenticationStatus.Unauthenticated;
            AuthenticationStatusChanged.Publish(new AuthenticationStatusChanged(authenticationState));
        }

        private static void OnSignInFailed(RequestFailedException error)
        {
            Debug.LogError($"Sign in failed: {error}");
            var authenticationState = NetworkServiceData.AuthenticationStatus.Unauthenticated;
            AuthenticationStatusChanged.Publish(new AuthenticationStatusChanged(authenticationState));
        }

        #endregion
    }