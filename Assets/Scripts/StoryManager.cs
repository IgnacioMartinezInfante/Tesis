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

    private float tiempoGuardado = 0f;

    void Start()
    {
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
            uiManager.ActualizarDistancia(distanceTracker.distanciaAcumulada, costoPendiente);

            tiempoGuardado += Time.deltaTime;
            if (tiempoGuardado >= 5f)
            {
                tiempoGuardado = 0f;
                saveSystem.GuardarProgreso(nodoPendiente.id, distanceTracker.distanciaAcumulada);
            }

            if (distanceTracker.distanciaAcumulada >= costoPendiente)
            {
                esperandoDistancia = false;
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

        // Reseteamos distanciaAcumulada solo acá, controlado
        distanceTracker.totalDistance = 0f;
        distanceTracker.distanciaAcumulada = 0f;

        esperandoDistancia = true;
        saveSystem.GuardarProgreso(nodoPendiente.id, 0f);
        uiManager.MostrarPantallaDistancia(costoPendiente);
    }

    public void CompletarDistancia()
    {
        nodoActual = nodoPendiente;

        if (nodoActual.EsFinal)
        {
            uiManager.MostrarPantallaFinal(nodoActual.texto);
        }
        else
        {
            uiManager.MostrarNodo(nodoActual);
            uiManager.MostrarPantallaHistoria();
        }
    }

    void CargarPartida()
    {
        Debug.Log("Cargando partida...");
        int nodoId = saveSystem.CargarNodoId();
        float distanciaGuardada = saveSystem.CargarDistancia();
        Debug.Log($"NodoId guardado: {nodoId} | Distancia: {distanciaGuardada}");

        foreach (StoryNode nodo in todosLosNodos)
        {
            Debug.Log($"Comparando nodo id: {nodo.id} con {nodoId}");
            if (nodo.id == nodoId)
            {
                nodoPendiente = nodo;
                Debug.Log($"Nodo encontrado: {nodo.id}");
                break;
            }
        }

        Debug.Log($"nodoPendiente null: {nodoPendiente == null}");

        if (nodoPendiente == null)
        {
            Debug.Log("Nodo no encontrado, arrancando desde el principio");
            saveSystem.BorrarProgreso();
            nodoActual = nodoInicial;
            uiManager.MostrarNodo(nodoActual);
            uiManager.MostrarPantallaHistoria();
            return;
        }

        Debug.Log("Buscando costo...");
        bool encontrado = false;
        foreach (StoryNode nodo in todosLosNodos)
        {
            if (encontrado) break;
            foreach (Opcion opcion in nodo.opciones)
            {
                Debug.Log($"Revisando opcion que va a nodo: {opcion.siguienteNodo?.id}");
                if (opcion.siguienteNodo != null && opcion.siguienteNodo.id == nodoPendiente.id)
                {
                    costoPendiente = opcion.costo;
                    encontrado = true;
                    Debug.Log($"Costo encontrado: {costoPendiente}");
                    break;
                }
            }
        }

        Debug.Log($"Costo pendiente: {costoPendiente} | Encontrado: {encontrado}");

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
        distanceTracker.totalDistance = 0f;
        esperandoDistancia = true;
        Debug.Log("Mostrando pantalla distancia");
        uiManager.MostrarPantallaDistancia(costoPendiente);
        Debug.Log("CargarPartida terminado");
    }
    void OnApplicationPause(bool pausado)
    {
        if (pausado && esperandoDistancia)
        {
            saveSystem.GuardarProgreso(nodoPendiente.id, distanceTracker.distanciaAcumulada);
            Debug.Log("Progreso guardado al pausar");
        }
    }

    public void ReiniciarJuego()
    {
        esperandoDistancia = false;
        nodoPendiente = null;
        costoPendiente = 0;
        nodoActual = nodoInicial;
        uiManager.MostrarNodo(nodoActual);
        uiManager.MostrarPantallaHistoria();
    }
}
