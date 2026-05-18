using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public StoryNode nodoInicial;

    private StoryNode nodoActual;

    private UIManager uiManager;

    void Start()
    {
        uiManager = GetComponent<UIManager>();
        nodoActual = nodoInicial;
        MostrarNodoActual();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ElegirOpcion(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ElegirOpcion(1);
        }
    }

    void MostrarNodoActual()
    {
        uiManager.MostrarNodo(nodoActual);
    }

    public void ElegirOpcion(int indice)
    {
        if (indice < 0 || indice >= nodoActual.opciones.Length)
        {
            Debug.Log("Opción inválida");
            return;
        }

        Opcion opcionElegida = nodoActual.opciones[indice];

        Debug.Log($"Elegiste: {opcionElegida.texto}");
        Debug.Log($"Necesitás caminar: {opcionElegida.costo}m");

        nodoActual = opcionElegida.siguienteNodo;
        MostrarNodoActual();
    }
}