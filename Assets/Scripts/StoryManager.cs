using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public StoryNode nodoInicial;

    private StoryNode nodoActual;

    void Start()
    {
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
        Debug.Log("=== NODO ACTUAL ===");
        Debug.Log(nodoActual.texto);

        for (int i = 0; i < nodoActual.opciones.Length; i++)
        {
            Debug.Log($"Opción {i + 1}: {nodoActual.opciones[i].texto} - Costo: {nodoActual.opciones[i].costo}m");
        }
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