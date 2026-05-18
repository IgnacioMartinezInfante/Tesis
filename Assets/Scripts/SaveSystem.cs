using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    private const string KEY_NODO = "nodoActualId";
    private const string KEY_DISTANCIA = "distanciaAcumulada";

    public void GuardarProgreso(int nodoId, float distancia)
    {
        PlayerPrefs.SetInt(KEY_NODO, nodoId);
        PlayerPrefs.SetFloat(KEY_DISTANCIA, distancia);
        PlayerPrefs.Save();
        Debug.Log($"Progreso guardado - Nodo: {nodoId} | Distancia: {distancia}");
    }

    public int CargarNodoId()
    {
        return PlayerPrefs.GetInt(KEY_NODO, 0);
    }

    public float CargarDistancia()
    {
        return PlayerPrefs.GetFloat(KEY_DISTANCIA, 0f);
    }

    public bool TieneProgreso()
    {
        return PlayerPrefs.HasKey(KEY_NODO);
    }

    public void BorrarProgreso()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("Progreso borrado");
    }
}