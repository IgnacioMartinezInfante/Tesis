using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public StoryNode nodoInicial;
    public StoryNode[] todosLosNodos;

    private StoryNode nodoActual;
    private StoryNode nodoPendiente;
    private int costoPendiente;
    private bool esperandoDistancia = false;

    private UIManager uiManager;
    private DistanceTracker distanceTracker;
    private SaveSystem saveSystem;

    void Start()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("1 - StoryManager Start");
        uiManager = GetComponent<UIManager>();
        Debug.Log("2 - UIManager: " + (uiManager != null));
        distanceTracker = GetComponent<DistanceTracker>();
        Debug.Log("3 - DistanceTracker: " + (distanceTracker != null));
        saveSystem = GetComponent<SaveSystem>();
        Debug.Log("4 - SaveSystem: " + (saveSystem != null));
        Debug.Log("5 - NodoInicial: " + (nodoInicial != null));
        Debug.Log("6 - TieneProgreso: " + saveSystem.TieneProgreso());

        if (saveSystem.TieneProgreso())
        {
            Debug.Log("7 - Cargando partida");
            CargarPartida();
        }
        else
        {
            Debug.Log("7 - Nueva partida");
            nodoActual = nodoInicial;
            Debug.Log("8 - MostrarNodo");
            uiManager.MostrarNodo(nodoActual);
            Debug.Log("9 - MostrarPantalla");
            uiManager.MostrarPantallaHistoria();
            Debug.Log("10 - Listo");
        }
    }

    void Update()
    {
        if (esperandoDistancia)
        {
            // Actualizar UI
            uiManager.ActualizarDistancia(distanceTracker.distanciaAcumulada, costoPendiente);

            //  GUARDAR progreso en tiempo real
            saveSystem.GuardarProgreso(nodoPendiente.id, distanceTracker.distanciaAcumulada);

            // Chequear si completó
            if (distanceTracker.distanciaAcumulada >= costoPendiente)
            {
                esperandoDistancia = false;

                //  borrar progreso porque ya terminó este nodo
                saveSystem.BorrarProgreso();

                CompletarDistancia();
            }
        }
    }

    public void ElegirOpcion(int indice)
    {
        if (indice < 0 || indice >= nodoActual.opciones.Length) return;

        Opcion opcionElegida = nodoActual.opciones[indice];
        nodoPendiente = opcionElegida.siguienteNodo;
        costoPendiente = opcionElegida.costo;

        distanceTracker.ResetDistance();
        esperandoDistancia = true;

        saveSystem.GuardarProgreso(nodoPendiente.id, 0f);

        uiManager.MostrarPantallaDistancia(costoPendiente);
    }

    public void CompletarDistancia()
    {
        nodoActual = nodoPendiente;
        uiManager.MostrarNodo(nodoActual);
        uiManager.MostrarPantallaHistoria();
    }

    void CargarPartida()
    {
        Debug.Log("Cargando partida...");
        int nodoId = saveSystem.CargarNodoId();
        float distanciaGuardada = saveSystem.CargarDistancia();
        Debug.Log($"NodoId guardado: {nodoId} | Distancia guardada: {distanciaGuardada}");

        foreach (StoryNode nodo in todosLosNodos)
        {
            if (nodo.id == nodoId)
            {
                nodoPendiente = nodo;
                break;
            }
        }

        if (nodoPendiente == null)
        {
            Debug.Log("Nodo no encontrado, arrancando desde el principio");
            saveSystem.BorrarProgreso();
            nodoActual = nodoInicial;
            uiManager.MostrarNodo(nodoActual);
            uiManager.MostrarPantallaHistoria();
            return;
        }

        // Buscar el costo de la opción que lleva a nodoPendiente
        bool encontrado = false;
        foreach (StoryNode nodo in todosLosNodos)
        {
            if (encontrado) break;
            foreach (Opcion opcion in nodo.opciones)
            {
                if (opcion.siguienteNodo != null && opcion.siguienteNodo.id == nodoPendiente.id)
                {
                    costoPendiente = opcion.costo;
                    encontrado = true;
                    break;
                }
            }
        }

        if (costoPendiente == 0)
        {
            Debug.Log("Costo no encontrado, arrancando desde el principio");
            saveSystem.BorrarProgreso();
            nodoActual = nodoInicial;
            uiManager.MostrarNodo(nodoActual);
            uiManager.MostrarPantallaHistoria();
            return;
        }

        distanceTracker.distanciaAcumulada = distanciaGuardada;
        esperandoDistancia = true;
        Debug.Log($"Partida cargada - Costo pendiente: {costoPendiente}");
        uiManager.MostrarPantallaDistancia(costoPendiente);
    }
}