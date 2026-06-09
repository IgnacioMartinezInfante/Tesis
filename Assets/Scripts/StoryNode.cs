using UnityEngine;

[CreateAssetMenu(fileName = "NuevoNodo", menuName = "Historia/Nodo")]
public class StoryNode : ScriptableObject
{
    public int id;

    [TextArea(3, 6)]
    public string texto;

    public Sprite imagen;

    public Opcion[] opciones;

    public bool EsFinal => opciones == null || opciones.Length == 0;
}

[System.Serializable]
public class Opcion
{
    public string texto;
    public int costo;
    public StoryNode siguienteNodo;
}