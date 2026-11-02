using UnityEngine;

[CreateAssetMenu(fileName = "NuevoNodo", menuName = "Historia/Nodo")]
public class StoryNode : ScriptableObject
{
    public int id;

    [Tooltip("Identificador único string para telemetría y Firestore.")]
    public string nodeId;

    [TextArea(3, 6)]
    public string texto;

    public Sprite imagen;

    public Opcion[] opciones;

    public bool EsFinal => opciones == null || opciones.Length == 0;

    public string NodeId => !string.IsNullOrEmpty(nodeId) ? nodeId : (!string.IsNullOrEmpty(name) ? name : id.ToString());
}

[System.Serializable]
public class Opcion
{
    [Tooltip("Identificador único de la opción para Analytics.")]
    public string opcionId;

    public string texto;
    public int costo;
    public StoryNode siguienteNodo;

    public string OpcionId => !string.IsNullOrEmpty(opcionId) ? opcionId : texto;
}