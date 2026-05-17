using UnityEngine;

[CreateAssetMenu(fileName = "NuevoNodo", menuName = "Historia/Nodo")]
public class StoryNode : ScriptableObject
{
    [TextArea(3, 6)]
    public string texto;

    public Sprite imagen;

    public Opcion[] opciones;
}

[System.Serializable]
public class Opcion
{
    public string texto;
    public int costo;
    public StoryNode siguienteNodo;
}