using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

/// <summary>
/// Handles Unity network service initialization and user authentication.
/// </summary>
public class NetworkServiceSystem : MonoBehaviour
{
    /// <summary>
    /// Represents the authentication status of the user.
    /// </summary>
    public enum AuthenticationStatus
    {
        Unauthenticated = 0,
        Authenticating = 1,
        Authenticated = 2
    }

    /// <summary>
    /// Singleton instance of the NetworkServiceSystem.
    /// </summary>
    public static NetworkServiceSystem Instance { get; private set; }

    /// <summary>
    /// Indicates if Unity services have been initialized.
    /// </summary>
    public bool IsInitialized { get; private set; }

    /// <summary>
    /// Current authentication status of the user.
    /// </summary>
    public AuthenticationStatus CurrentAuthStatus { get; private set; } = AuthenticationStatus.Unauthenticated;

    /// <summary>
    /// Event triggered when the authentication status changes.
    /// </summary>
    public event Action<AuthenticationStatus> AuthenticationStatusChanged;

    /// <summary>
    /// Event triggered when Unity services are initialized.
    /// </summary>
    public event Action ServicesInitialized;

    [SerializeField] private NetworkManager networkManagerPrefab;
    private NetworkManager _networkManager;

    private bool _isInitializing;

    private void Awake()
    {
        // Ensure only one instance exists (Singleton pattern)
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        _ = InitializeSystemAsync();
    }

    /// <summary>
    /// Initializes Unity services and sets up authentication event handlers.
    /// </summary>
    public async Task InitializeSystemAsync()
    {
        // Prevent multiple simultaneous initializations
        if (_isInitializing || IsInitialized)
        {
            return;
        }
        _isInitializing = true;

        // Prevent multiple simultaneous sign-in attempts
        if (CurrentAuthStatus == AuthenticationStatus.Authenticating)
        {
            _isInitializing = false;
            return;
        }

        // Initialize Unity services if not already initialized
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            await UnityServices.InitializeAsync();
        }

        IsInitialized = true;
        ServicesInitialized?.Invoke();

        // Register authentication event handlers
        AuthenticationService.Instance.SignedIn += OnSignedIn;
        AuthenticationService.Instance.SignedOut += OnSignedOut;
        AuthenticationService.Instance.SignInFailed += OnSignInFailed;

        // Start authentication process
        SetAuthenticationStatus(AuthenticationStatus.Authenticating);

        // Only sign in if not already signed in or signing in
        if (!AuthenticationService.Instance.IsAuthorized && AuthenticationService.Instance.AccessToken == null)
        {
            await TryLoginGuest();
        }

        // Set authentication status to authenticated if sign-in was successful
        if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
        {
            SetAuthenticationStatus(AuthenticationStatus.Authenticated);
        }
        else
        {
            SetAuthenticationStatus(AuthenticationStatus.Unauthenticated);
        }

        if (_networkManager == null)
        {
            _networkManager = Instantiate(networkManagerPrefab);
            _networkManager.SetSingleton();
            DontDestroyOnLoad(_networkManager);
        }

        _isInitializing = false;
    }

    private static async Task TryLoginGuest()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (AuthenticationException e)
        {
            if (e.ErrorCode == 10000)
            {
                Debug.Log("Player Already logged in...");
            }
            else
            {
                Debug.LogError($"Authentication failed: {e}");
            }
        }
        catch (RequestFailedException e)
        {
            Debug.LogError($"Request failed: {e}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    /// <summary>
    /// Handler for successful sign-in.
    /// </summary>
    private void OnSignedIn()
    {
        Debug.Log($"Signed in: {AuthenticationService.Instance.PlayerId}");
        SetAuthenticationStatus(AuthenticationStatus.Authenticated);
    }

    /// <summary>
    /// Handler for sign-out.
    /// </summary>
    private void OnSignedOut()
    {
        Debug.Log("Signed out");
        SetAuthenticationStatus(AuthenticationStatus.Unauthenticated);
    }

    /// <summary>
    /// Handler for failed sign-in.
    /// </summary>
    private void OnSignInFailed(RequestFailedException error)
    {
        Debug.LogError($"Sign in failed: {error}");
        SetAuthenticationStatus(AuthenticationStatus.Unauthenticated);
    }

    /// <summary>
    /// Helper to set authentication status and trigger event.
    /// </summary>
    private void SetAuthenticationStatus(AuthenticationStatus status)
    {
        if (CurrentAuthStatus == status) return;
        CurrentAuthStatus = status;
        AuthenticationStatusChanged?.Invoke(status);
    }

    /// <summary>
    /// Returns true if the system is initialized and the user is authenticated.
    /// </summary>
    /// <returns>True if the system is ready, false otherwise.</returns>
    public bool IsSystemReady()
    {
        return IsInitialized && CurrentAuthStatus == AuthenticationStatus.Authenticated;
    }
}
