using UnityEngine;

public enum EstadoJuego
{
    Historia,
    Final
}

public class SaveSystem : MonoBehaviour
{
    private const string KEY_NODO_ACTUAL = "nodoActualId";
    private const string KEY_DISTANCIA = "distanciaAcumulada";
    private const string KEY_ESTADO = "estadoJuego";

    public void GuardarProgreso(int nodoActualId, float distancia, EstadoJuego estado)
    {
        PlayerPrefs.SetInt(KEY_NODO_ACTUAL, nodoActualId);
        PlayerPrefs.SetFloat(KEY_DISTANCIA, distancia);
        PlayerPrefs.SetInt(KEY_ESTADO, (int)estado);
        PlayerPrefs.Save();
        Debug.Log($"Guardado - Estado: {estado} | NodoActual: {nodoActualId} | Distancia: {distancia}");
    }

    public int CargarNodoActualId() => PlayerPrefs.GetInt(KEY_NODO_ACTUAL, 0);
    public float CargarDistancia() => PlayerPrefs.GetFloat(KEY_DISTANCIA, 0f);
    public EstadoJuego CargarEstado() => (EstadoJuego)PlayerPrefs.GetInt(KEY_ESTADO, 0);

    public bool TieneProgreso() => PlayerPrefs.HasKey(KEY_ESTADO);

    public void BorrarProgreso(bool conservarMetros = true)
    {
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