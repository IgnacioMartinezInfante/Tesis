using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Analytics;
using Firebase.Firestore;
using Firebase.Extensions;

public class TelemetryManager : MonoBehaviour
{
    private static TelemetryManager _instance;
    public static TelemetryManager Instance => _instance;

    [Header("Configuración")]
    public bool sincronizacionAutomaticaPeriodica = true;
    public float intervaloSyncSegundos = 30f;

    [Header("Estado de Sesión Actual")]
    public string sessionId;
    public string testerName;
    public string uid;
    public int storyId = 1;
    public float totalMetersWalked = 0f;
    public float realGpsMeters = 0f;
    public float simulatedMeters = 0f;
    public float walletBalance = 0f;
    public string currentNodeId = "";
    public bool isStoryCompleted = false;

    private DateTime sessionStartTimeUtc;
    private float sessionStartRealtime;
    private float lastSyncTime;
    private bool isSessionActive = false;
    private Coroutine syncWaitCoroutine = null;

    // Historial cronológico de nodos recorridos
    private readonly List<Dictionary<string, object>> nodeHistory = new List<Dictionary<string, object>>();

    // Registro crudo de eventos de tesis
    private readonly List<Dictionary<string, object>> rawEvents = new List<Dictionary<string, object>>();

    private bool isFirebaseReady = false;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        storyId = PlayerPrefs.GetInt("historiaSeleccionada", 1);
        testerName = PlayerPrefs.GetString("tester_nombre", "Tester_Anonimo");

        if (FirebaseInit.IsInitialized)
        {
            OnFirebaseReady();
        }
        else
        {
            FirebaseInit.OnFirebaseInitialized += OnFirebaseReady;
        }

        IniciarNuevaSesion(testerName);
    }

    void OnDestroy()
    {
        FirebaseInit.OnFirebaseInitialized -= OnFirebaseReady;
    }

    private void OnFirebaseReady()
    {
        isFirebaseReady = true;
        uid = FirebaseInit.CurrentUserId;

        // Actualizar nombres y propiedades en Analytics
        if (!string.IsNullOrEmpty(uid))
        {
            FirebaseAnalytics.SetUserId(uid);
        }
        if (!string.IsNullOrEmpty(testerName))
        {
            FirebaseAnalytics.SetUserProperty("tester_name", testerName);
        }

        Debug.Log($"[TelemetryManager] Firebase enlazado con éxito. UID: {uid}");

        // Sincronizar estado inicial de la sesión
        SincronizarConFirestoreAsync();
    }

    public void IniciarNuevaSesion(string nombre)
    {
        if (!string.IsNullOrWhiteSpace(nombre))
        {
            testerName = nombre.Trim();
        }

        sessionStartTimeUtc = DateTime.UtcNow;
        sessionStartRealtime = Time.realtimeSinceStartup;
        lastSyncTime = sessionStartRealtime;

        string timestampSuffix = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        string cleanUid = !string.IsNullOrEmpty(FirebaseInit.CurrentUserId) ? FirebaseInit.CurrentUserId : (!string.IsNullOrEmpty(uid) ? uid : "local");
        sessionId = $"sesion_{cleanUid}_{timestampSuffix}";

        nodeHistory.Clear();
        rawEvents.Clear();
        totalMetersWalked = 0f;
        realGpsMeters = 0f;
        simulatedMeters = 0f;
        isStoryCompleted = false;
        isSessionActive = true;

        RegistrarEvento("SESSION_START", new Dictionary<string, object>
        {
            { "testerName", testerName },
            { "storyId", storyId },
            { "timestampUtc", sessionStartTimeUtc.ToString("o") }
        });

        if (isFirebaseReady)
        {
            FirebaseAnalytics.LogEvent("session_start_thesis",
                new Parameter("tester_name", testerName),
                new Parameter("story_id", storyId)
            );
        }

        Debug.Log($"[TelemetryManager] Nueva sesión de tesis iniciada: {sessionId} para '{testerName}'");
        SincronizarConFirestoreAsync();
    }

    void Update()
    {
        if (!isSessionActive) return;

        if (sincronizacionAutomaticaPeriodica && (Time.realtimeSinceStartup - lastSyncTime >= intervaloSyncSegundos))
        {
            lastSyncTime = Time.realtimeSinceStartup;
            SincronizarConFirestoreAsync();
        }
    }

    // ---------------------------------------------------------
    // VINCULACIÓN CON DISTANCE TRACKER (GPS y Simulación)
    // ---------------------------------------------------------
    public void RegistrarDistanceTracker(DistanceTracker tracker)
    {
        if (tracker == null) return;

        tracker.OnDistanceRecorded -= OnDistanceRecorded;
        tracker.OnMetersConsumed -= OnMetersConsumed;

        tracker.OnDistanceRecorded += OnDistanceRecorded;
        tracker.OnMetersConsumed += OnMetersConsumed;

        walletBalance = tracker.distanciaAcumulada;
        Debug.Log("[TelemetryManager] DistanceTracker vinculado.");
    }

    public void DesvincularDistanceTracker(DistanceTracker tracker)
    {
        if (tracker == null) return;
        tracker.OnDistanceRecorded -= OnDistanceRecorded;
        tracker.OnMetersConsumed -= OnMetersConsumed;
    }

    private void OnDistanceRecorded(float deltaMeters, float totalWallet, bool isSimulated)
    {
        if (deltaMeters <= 0f) return;

        totalMetersWalked += deltaMeters;
        walletBalance = totalWallet;

        if (isSimulated)
        {
            simulatedMeters += deltaMeters;
        }
        else
        {
            realGpsMeters += deltaMeters;
        }

        // Registro de telemetría uniforme
        RegistrarEvento("DISTANCE_WALKED", new Dictionary<string, object>
        {
            { "deltaMeters", Math.Round(deltaMeters, 2) },
            { "walletBalance", Math.Round(totalWallet, 2) },
            { "isSimulated", isSimulated },
            { "totalMetersSession", Math.Round(totalMetersWalked, 2) }
        });

        if (isFirebaseReady)
        {
            FirebaseAnalytics.LogEvent("walk_meters_recorded",
                new Parameter("delta_meters", Math.Round(deltaMeters, 2)),
                new Parameter("total_wallet", Math.Round(totalWallet, 2)),
                new Parameter("is_simulated", isSimulated ? 1 : 0)
            );
        }

        Debug.Log($"[TelemetryManager] Distancia registrada: +{deltaMeters:F1}m (Simulado: {isSimulated}) | Total sesión: {totalMetersWalked:F1}m");

        // En desarrollo, el botón simulador dispara sincronización inmediata para retroalimentación instantánea
        if (isSimulated)
        {
            SincronizarConFirestoreAsync();
        }
    }

    private void OnMetersConsumed(float metrosConsumidos, float saldoRestante)
    {
        walletBalance = saldoRestante;

        RegistrarEvento("METERS_CONSUMED", new Dictionary<string, object>
        {
            { "consumedMeters", Math.Round(metrosConsumidos, 2) },
            { "walletBalance", Math.Round(saldoRestante, 2) }
        });

        if (isFirebaseReady)
        {
            FirebaseAnalytics.LogEvent("wallet_meters_consumed",
                new Parameter("consumed", Math.Round(metrosConsumidos, 2)),
                new Parameter("remaining", Math.Round(saldoRestante, 2))
            );
        }
    }

    // ---------------------------------------------------------
    // VINCULACIÓN CON STORY MANAGER (Nodos y Decisiones)
    // ---------------------------------------------------------
    public void RegistrarStoryManager(StoryManager story)
    {
        if (story == null) return;

        story.OnNodeReached -= OnNodeReached;
        story.OnOptionSelected -= OnOptionSelected;
        story.OnStoryCompleted -= OnStoryCompleted;
        story.OnInsufficientMetersAlert -= OnInsufficientMetersAlert;

        story.OnNodeReached += OnNodeReached;
        story.OnOptionSelected += OnOptionSelected;
        story.OnStoryCompleted += OnStoryCompleted;
        story.OnInsufficientMetersAlert += OnInsufficientMetersAlert;

        storyId = PlayerPrefs.GetInt("historiaSeleccionada", 1);
        Debug.Log($"[TelemetryManager] StoryManager vinculado para historia #{storyId}.");
    }

    public void DesvincularStoryManager(StoryManager story)
    {
        if (story == null) return;
        story.OnNodeReached -= OnNodeReached;
        story.OnOptionSelected -= OnOptionSelected;
        story.OnStoryCompleted -= OnStoryCompleted;
        story.OnInsufficientMetersAlert -= OnInsufficientMetersAlert;
    }

    private void OnNodeReached(StoryNode node)
    {
        if (node == null) return;

        currentNodeId = node.NodeId;

        var nodeEntry = new Dictionary<string, object>
        {
            { "nodeId", node.NodeId },
            { "nodeIntId", node.id },
            { "nodeTextPreview", node.texto != null && node.texto.Length > 60 ? node.texto.Substring(0, 60) + "..." : node.texto },
            { "timestampUtc", DateTime.UtcNow.ToString("o") },
            { "sessionDurationSeconds", Math.Round(Time.realtimeSinceStartup - sessionStartRealtime, 1) },
            { "totalWalkedSoFar", Math.Round(totalMetersWalked, 2) },
            { "isFinal", node.EsFinal }
        };

        nodeHistory.Add(nodeEntry);

        RegistrarEvento("NODE_REACHED", new Dictionary<string, object>
        {
            { "nodeId", node.NodeId },
            { "isFinal", node.EsFinal }
        });

        if (isFirebaseReady)
        {
            FirebaseAnalytics.LogEvent("story_node_reached",
                new Parameter("node_id", node.NodeId),
                new Parameter("is_final", node.EsFinal ? 1 : 0)
            );
        }

        SincronizarConFirestoreAsync();
    }

    private void OnOptionSelected(StoryNode sourceNode, Opcion opcion, int optionIndex)
    {
        string fromNodeId = sourceNode != null ? sourceNode.NodeId : "unknown";
        string targetNodeId = (opcion != null && opcion.siguienteNodo != null) ? opcion.siguienteNodo.NodeId : "none";
        string optionId = opcion != null ? opcion.OpcionId : "none";
        int cost = opcion != null ? opcion.costo : 0;

        RegistrarEvento("OPTION_SELECTED", new Dictionary<string, object>
        {
            { "fromNodeId", fromNodeId },
            { "optionId", optionId },
            { "optionIndex", optionIndex },
            { "targetNodeId", targetNodeId },
            { "costMeters", cost },
            { "walletBeforeChoice", Math.Round(walletBalance, 2) }
        });

        if (isFirebaseReady)
        {
            FirebaseAnalytics.LogEvent("story_option_selected",
                new Parameter("from_node", fromNodeId),
                new Parameter("option_id", optionId),
                new Parameter("cost", cost)
            );
        }
    }

    private void OnInsufficientMetersAlert(Opcion opcion, float missingMeters)
    {
        string optionId = opcion != null ? opcion.OpcionId : "unknown";
        int cost = opcion != null ? opcion.costo : 0;

        RegistrarEvento("INSUFFICIENT_METERS_ALERT", new Dictionary<string, object>
        {
            { "optionId", optionId },
            { "costRequired", cost },
            { "currentWallet", Math.Round(walletBalance, 2) },
            { "missingMeters", Math.Round(missingMeters, 2) }
        });

        if (isFirebaseReady)
        {
            FirebaseAnalytics.LogEvent("insufficient_meters_alert",
                new Parameter("option_id", optionId),
                new Parameter("missing_meters", Math.Round(missingMeters, 2))
            );
        }

        SincronizarConFirestoreAsync();
    }

    private void OnStoryCompleted(StoryNode finalNode)
    {
        isStoryCompleted = true;
        float duracionTotal = Time.realtimeSinceStartup - sessionStartRealtime;

        RegistrarEvento("STORY_COMPLETED", new Dictionary<string, object>
        {
            { "finalNodeId", finalNode != null ? finalNode.NodeId : "unknown" },
            { "totalSessionSeconds", Math.Round(duracionTotal, 1) },
            { "totalMetersWalked", Math.Round(totalMetersWalked, 2) }
        });

        if (isFirebaseReady)
        {
            FirebaseAnalytics.LogEvent("story_completed",
                new Parameter("final_node", finalNode != null ? finalNode.NodeId : "unknown"),
                new Parameter("duration_seconds", Math.Round(duracionTotal, 1)),
                new Parameter("total_meters", Math.Round(totalMetersWalked, 2))
            );
        }

        SincronizarConFirestoreAsync();
    }

    // ---------------------------------------------------------
    // REGISTRO DE EVENTO Y SINCRONIZACIÓN FIRESTORE
    // ---------------------------------------------------------
    private void RegistrarEvento(string tipo, Dictionary<string, object> detalle)
    {
        var eventoItem = new Dictionary<string, object>
        {
            { "tipo", tipo },
            { "timestampUtc", DateTime.UtcNow.ToString("o") },
            { "segundosDesdeInicio", Math.Round(Time.realtimeSinceStartup - sessionStartRealtime, 1) },
            { "detalle", detalle }
        };

        rawEvents.Add(eventoItem);
    }

    public void SincronizarConFirestoreAsync()
    {
        if (!FirebaseInit.IsInitialized)
        {
            if (syncWaitCoroutine == null && gameObject.activeInHierarchy)
            {
                syncWaitCoroutine = StartCoroutine(EsperarYSincronizar());
            }
            return;
        }

        EjecutarSincronizacionFirestore();
    }

    private IEnumerator EsperarYSincronizar()
    {
        Debug.Log("[TelemetryManager] Esperando a que FirebaseInit complete la inicialización antes de sincronizar con Firestore...");
        yield return new WaitUntil(() => FirebaseInit.IsInitialized);
        syncWaitCoroutine = null;
        EjecutarSincronizacionFirestore();
    }

    private void EjecutarSincronizacionFirestore()
    {
        var firestore = FirebaseInit.Db ?? FirebaseFirestore.DefaultInstance;
        if (firestore == null || string.IsNullOrEmpty(sessionId)) return;

        float sessionDuration = Time.realtimeSinceStartup - sessionStartRealtime;
        string activeUid = !string.IsNullOrEmpty(FirebaseInit.CurrentUserId) ? FirebaseInit.CurrentUserId : uid;
        if (string.IsNullOrEmpty(activeUid)) activeUid = "local";

        const string coleccion = "sesiones_tesis";

        var sessionDocument = new Dictionary<string, object>
        {
            { "sessionId", sessionId },
            { "uid", activeUid },
            { "testerName", testerName },
            { "historiaSeleccionada", storyId },
            { "inicioSesionUtc", sessionStartTimeUtc.ToString("o") },
            { "ultimaActualizacionUtc", DateTime.UtcNow.ToString("o") },
            { "duracionSegundos", Math.Round(sessionDuration, 1) },
            { "metrosCaminadosTotales", Math.Round(totalMetersWalked, 2) },
            { "metrosRealesGps", Math.Round(realGpsMeters, 2) },
            { "metrosSimulados", Math.Round(simulatedMeters, 2) },
            { "billeteraMetrosActual", Math.Round(walletBalance, 2) },
            { "nodoActualId", currentNodeId },
            { "historiaCompletada", isStoryCompleted },
            { "cantidadNodosVisitados", nodeHistory.Count },
            { "historialNodos", new List<Dictionary<string, object>>(nodeHistory) },
            { "eventosLog", new List<Dictionary<string, object>>(rawEvents) },
            { "dispositivo", SystemInfo.deviceModel },
            { "sistemaOperativo", SystemInfo.operatingSystem }
        };

        firestore.Collection(coleccion).Document(sessionId).SetAsync(sessionDocument, SetOptions.MergeAll)
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

    void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            RegistrarEvento("APP_PAUSE", new Dictionary<string, object>());
            SincronizarConFirestoreAsync();
        }
        else
        {
            RegistrarEvento("APP_RESUME", new Dictionary<string, object>());
        }
    }

    void OnApplicationQuit()
    {
        RegistrarEvento("APP_QUIT", new Dictionary<string, object>());
        SincronizarConFirestoreAsync();
    }
}
