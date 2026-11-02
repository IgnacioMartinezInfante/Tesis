using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Analytics;
using Firebase.Extensions;

public class FirebaseInit : MonoBehaviour
{
    private static FirebaseInit _instance;
    public static FirebaseInit Instance => _instance;

    public static bool IsInitialized { get; private set; } = false;
    public static string CurrentUserId { get; private set; } = "";
    public static string TesterName { get; private set; } = "";
    public static FirebaseFirestore Db { get; private set; }

    public static event Action OnFirebaseInitialized;
    public event Action<string> OnInitializationStatusChanged;

    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private FirebaseApp app;

    private const string PREF_TESTER_NAME = "tester_nombre";
    private const string PREF_FALLBACK_UID = "fallback_local_uid";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoBootServices()
    {
        if (_instance == null && FindFirstObjectByType<FirebaseInit>() == null)
        {
            GameObject fb = new GameObject("FirebaseInit");
            fb.AddComponent<FirebaseInit>();
        }

        if (TelemetryManager.Instance == null && FindFirstObjectByType<TelemetryManager>() == null)
        {
            GameObject tm = new GameObject("TelemetryManager");
            tm.AddComponent<TelemetryManager>();
        }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        TesterName = PlayerPrefs.GetString(PREF_TESTER_NAME, "");
    }

    void Start()
    {
        InitializeFirebase();
    }

    public void InitializeFirebase()
    {
        IsInitialized = false;
        OnInitializationStatusChanged?.Invoke("Verificando dependencias de Firebase...");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                app = FirebaseApp.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;
                db = FirebaseFirestore.DefaultInstance;
                Db = db;

                AuthenticateAnonymously();
            }
            else
            {
                Debug.LogError($"[FirebaseInit] No se pudieron resolver las dependencias: {dependencyStatus}");
                OnInitializationStatusChanged?.Invoke($"Error de dependencias: {dependencyStatus}");
                UseFallbackOfflineUid();
            }
        });
    }

    private void AuthenticateAnonymously()
    {
        OnInitializationStatusChanged?.Invoke("Autenticando tester anónimamente...");

        if (auth != null && auth.CurrentUser != null)
        {
            CurrentUserId = auth.CurrentUser.UserId;
            CompleteInitialization();
            return;
        }

        if (auth == null)
        {
            UseFallbackOfflineUid();
            return;
        }

        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Exception baseEx = task.Exception != null ? task.Exception.GetBaseException() : null;
                string exMessage = baseEx != null ? baseEx.Message : (task.Exception != null ? task.Exception.Message : "Error desconocido");

                if (exMessage.Contains("internal error") || exMessage.Contains("CONFIGURATION_NOT_FOUND"))
                {
                    Debug.LogWarning("[FirebaseInit] Autenticación anónima pendiente de activación en consola de Firebase (Authentication > Sign-in method > Anónimo > Habilitar). Usando UID de desarrollo para continuar.");
                }
                else
                {
                    Debug.LogWarning($"[FirebaseInit] Falló autenticación anónima ({exMessage}). Usando UID de desarrollo.");
                }

                UseFallbackOfflineUid();
                return;
            }

            var authResult = task.Result;
            CurrentUserId = authResult.User.UserId;
            CompleteInitialization();
        });
    }

    private void CompleteInitialization()
    {
        if (db == null)
        {
            db = FirebaseFirestore.DefaultInstance;
            Db = db;
        }

        IsInitialized = true;

        // Asegura que Analytics recolecte métricas tanto en nube como en modo Editor/Offline
        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
        FirebaseAnalytics.LogEvent("app_open", new Parameter("debug_mode", 1));
        FirebaseAnalytics.SetUserId(CurrentUserId);

        if (!string.IsNullOrEmpty(TesterName))
        {
            FirebaseAnalytics.SetUserProperty("tester_name", TesterName);
        }

        Debug.Log($"[FirebaseInit] Firebase inicializado con éxito. UID: {CurrentUserId}");
        OnInitializationStatusChanged?.Invoke("Firebase inicializado correctamente.");
        OnFirebaseInitialized?.Invoke();
    }

    private void UseFallbackOfflineUid()
    {
        if (!PlayerPrefs.HasKey(PREF_FALLBACK_UID))
        {
            PlayerPrefs.SetString(PREF_FALLBACK_UID, "dev_" + Guid.NewGuid().ToString("N").Substring(0, 16));
            PlayerPrefs.Save();
        }

        CurrentUserId = PlayerPrefs.GetString(PREF_FALLBACK_UID);

        if (db == null)
        {
            try
            {
                db = FirebaseFirestore.DefaultInstance;
                Db = db;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[FirebaseInit] Firestore no disponible en fallback: {ex.Message}");
            }
        }

        IsInitialized = true;
        Debug.Log($"[FirebaseInit] Usando UID de desarrollo: {CurrentUserId}");
        OnInitializationStatusChanged?.Invoke("Modo de desarrollo listo.");
        OnFirebaseInitialized?.Invoke();
    }

    public async Task GuardarNombreTesterAsync(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) return;

        TesterName = nombre.Trim();
        PlayerPrefs.SetString(PREF_TESTER_NAME, TesterName);
        PlayerPrefs.Save();

        FirebaseAnalytics.SetUserProperty("tester_name", TesterName);

        // Esperar a que la inicialización esté 100% completa
        float timeout = 10f;
        float elapsed = 0f;
        while (!IsInitialized && elapsed < timeout)
        {
            await Task.Delay(100);
            elapsed += 0.1f;
        }

        var firestore = Db ?? db ?? FirebaseFirestore.DefaultInstance;
        if (firestore != null && !string.IsNullOrEmpty(CurrentUserId))
        {
            const string coleccion = "usuarios";
            var usuarioData = new Dictionary<string, object>
            {
                { "uid", CurrentUserId },
                { "nombre", TesterName },
                { "fechaRegistroUtc", DateTime.UtcNow.ToString("o") },
                { "plataforma", Application.platform.ToString() },
                { "versionApp", Application.version }
            };

            await firestore.Collection(coleccion).Document(CurrentUserId).SetAsync(usuarioData, SetOptions.MergeAll)
                .ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        Debug.Log("[FirestoreSuccess] Documento subido correctamente a la colección: " + coleccion);
                    }
                    else if (task.IsFaulted)
                    {
                        Debug.LogError("[FirestoreError] Error al subir a Firestore: " + task.Exception);
                    }
                });
        }
    }
}
