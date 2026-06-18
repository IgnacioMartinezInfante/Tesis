using UnityEngine;

public enum EstadoJuego
{
    Historia,
    Caminando,
    ObjetivoCompletado,
    Final
}

public class SaveSystem : MonoBehaviour
{
    private const string KEY_NODO_ACTUAL = "nodoActualId";
    private const string KEY_NODO_PENDIENTE = "nodoPendienteId";
    private const string KEY_DISTANCIA = "distanciaAcumulada";
    private const string KEY_COSTO = "costoPendiente";
    private const string KEY_ESTADO = "estadoJuego";

    public void GuardarProgreso(int nodoActualId, int nodoPendienteId, float distancia, int costo, EstadoJuego estado)
    {
        PlayerPrefs.SetInt(KEY_NODO_ACTUAL, nodoActualId);
        PlayerPrefs.SetInt(KEY_NODO_PENDIENTE, nodoPendienteId);
        PlayerPrefs.SetFloat(KEY_DISTANCIA, distancia);
        PlayerPrefs.SetInt(KEY_COSTO, costo);
        PlayerPrefs.SetInt(KEY_ESTADO, (int)estado);
        PlayerPrefs.Save();
        Debug.Log($"Guardado - Estado: {estado} | NodoActual: {nodoActualId} | NodoPendiente: {nodoPendienteId} | Distancia: {distancia} | Costo: {costo}");
    }

    public int CargarNodoActualId() => PlayerPrefs.GetInt(KEY_NODO_ACTUAL, 0);
    public int CargarNodoPendienteId() => PlayerPrefs.GetInt(KEY_NODO_PENDIENTE, 0);
    public float CargarDistancia() => PlayerPrefs.GetFloat(KEY_DISTANCIA, 0f);
    public int CargarCosto() => PlayerPrefs.GetInt(KEY_COSTO, 0);
    public EstadoJuego CargarEstado() => (EstadoJuego)PlayerPrefs.GetInt(KEY_ESTADO, 0);

    public bool TieneProgreso() => PlayerPrefs.HasKey(KEY_ESTADO);

    public void BorrarProgreso()
    {
        PlayerPrefs.DeleteKey(KEY_NODO_ACTUAL);
        PlayerPrefs.DeleteKey(KEY_NODO_PENDIENTE);
        PlayerPrefs.DeleteKey(KEY_DISTANCIA);
        PlayerPrefs.DeleteKey(KEY_COSTO);
        PlayerPrefs.DeleteKey(KEY_ESTADO);
        PlayerPrefs.Save();
        Debug.Log("Progreso borrado");
    }
}