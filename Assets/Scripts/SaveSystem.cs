using UnityEngine;

public enum EstadoJuego
{
    Historia,
    Final
}

[System.Serializable]
public class GameSaveData
{
    public string currentNodeId = "";
    public int currentNodeIntId = 0;
    public float accumulatedDistance = 0f;
    public EstadoJuego gameState = EstadoJuego.Historia;
    public int selectedStoryId = 1;
    public long lastSavedTimestampUtc = 0;
    public bool isSyncedWithCloud = false;

    public GameSaveData() { }

    public GameSaveData(string nodeId, int nodeIntId, float distance, EstadoJuego state, int storyId)
    {
        currentNodeId = nodeId;
        currentNodeIntId = nodeIntId;
        accumulatedDistance = distance;
        gameState = state;
        selectedStoryId = storyId;
        lastSavedTimestampUtc = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        isSyncedWithCloud = false;
    }
}

public class SaveSystem : MonoBehaviour
{
    private const string KEY_SAVE_DATA_JSON = "game_save_data_json";
    private const string KEY_NODO_ACTUAL = "nodoActualId";
    private const string KEY_DISTANCIA = "distanciaAcumulada";
    private const string KEY_ESTADO = "estadoJuego";

    public void GuardarProgreso(GameSaveData data)
    {
        if (data == null) return;

        data.lastSavedTimestampUtc = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(KEY_SAVE_DATA_JSON, json);

        // Claves legacy para compatibilidad con SplashManager y UI
        PlayerPrefs.SetInt(KEY_NODO_ACTUAL, data.currentNodeIntId);
        PlayerPrefs.SetFloat(KEY_DISTANCIA, data.accumulatedDistance);
        PlayerPrefs.SetInt(KEY_ESTADO, (int)data.gameState);
        PlayerPrefs.Save();

        Debug.Log($"Guardado DTO - Nodo: {data.currentNodeId} | Metros: {data.accumulatedDistance} | Estado: {data.gameState}");
    }

    public void GuardarProgreso(int nodoActualId, float distancia, EstadoJuego estado)
    {
        GameSaveData data = new GameSaveData(nodoActualId.ToString(), nodoActualId, distancia, estado, PlayerPrefs.GetInt("historiaSeleccionada", 1));
        GuardarProgreso(data);
    }

    public GameSaveData CargarProgreso()
    {
        if (PlayerPrefs.HasKey(KEY_SAVE_DATA_JSON))
        {
            string json = PlayerPrefs.GetString(KEY_SAVE_DATA_JSON, "");
            if (!string.IsNullOrEmpty(json))
            {
                return JsonUtility.FromJson<GameSaveData>(json);
            }
        }

        // Fallback y migración desde claves legacy
        GameSaveData fallback = new GameSaveData
        {
            currentNodeId = PlayerPrefs.GetInt(KEY_NODO_ACTUAL, 0).ToString(),
            currentNodeIntId = PlayerPrefs.GetInt(KEY_NODO_ACTUAL, 0),
            accumulatedDistance = PlayerPrefs.GetFloat(KEY_DISTANCIA, 0f),
            gameState = (EstadoJuego)PlayerPrefs.GetInt(KEY_ESTADO, 0),
            selectedStoryId = PlayerPrefs.GetInt("historiaSeleccionada", 1)
        };
        return fallback;
    }

    public int CargarNodoActualId() => CargarProgreso().currentNodeIntId;
    public float CargarDistancia() => CargarProgreso().accumulatedDistance;
    public EstadoJuego CargarEstado() => CargarProgreso().gameState;

    public bool TieneProgreso() => PlayerPrefs.HasKey(KEY_SAVE_DATA_JSON) || PlayerPrefs.HasKey(KEY_ESTADO);

    public void BorrarProgreso(bool conservarMetros = true)
    {
        PlayerPrefs.DeleteKey(KEY_SAVE_DATA_JSON);
        PlayerPrefs.DeleteKey(KEY_NODO_ACTUAL);
        PlayerPrefs.DeleteKey("nodoPendienteId");
        PlayerPrefs.DeleteKey("costoPendiente");
        PlayerPrefs.DeleteKey(KEY_ESTADO);
        PlayerPrefs.DeleteKey("sesionActiva");
        if (!conservarMetros)
        {
            PlayerPrefs.DeleteKey(KEY_DISTANCIA);
        }
        PlayerPrefs.Save();
        Debug.Log($"Progreso borrado (conservarMetros: {conservarMetros})");
    }
}