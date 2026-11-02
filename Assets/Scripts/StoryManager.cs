using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public StoryNode nodoInicial;
    public StoryNode[] todosLosNodos;

    public event System.Action<StoryNode> OnNodeReached;
    public event System.Action<StoryNode, Opcion, int> OnOptionSelected;
    public event System.Action<StoryNode> OnStoryCompleted;
    public event System.Action<Opcion, float> OnInsufficientMetersAlert;

    private StoryNode nodoActual;
    private bool inicializado = false;
    private bool historiaTerminada = false;

    private UIManager uiManager;
    private DistanceTracker distanceTracker;
    private SaveSystem saveSystem;

    void Start()
    {
        uiManager = GetComponent<UIManager>();
        distanceTracker = GetComponent<DistanceTracker>();
        saveSystem = GetComponent<SaveSystem>();

        TelemetryManager.Instance?.RegistrarStoryManager(this);

        if (saveSystem.TieneProgreso())
        {
            CargarPartida();
        }
        else
        {
            NuevaPartida();
        }

        PlayerPrefs.SetInt("sesionActiva", 1);
        PlayerPrefs.Save();

        inicializado = true;
    }

    void NuevaPartida()
    {
        historiaTerminada = false;
        nodoActual = nodoInicial;

        if (nodoActual.EsFinal)
        {
            historiaTerminada = true;
            OnStoryCompleted?.Invoke(nodoActual);
            uiManager.MostrarPantallaFinal(nodoActual.texto);
        }
        else
        {
            uiManager.MostrarNodo(nodoActual);
            uiManager.MostrarPantallaHistoria();
        }

        OnNodeReached?.Invoke(nodoActual);
        GuardarEstadoActual();
    }

    public void ElegirOpcion(int indice)
    {
        if (nodoActual == null || nodoActual.opciones == null || indice < 0 || indice >= nodoActual.opciones.Length)
            return;

        Opcion opcionElegida = nodoActual.opciones[indice];

        // Telemetría: evento explícito de selección de opción
        OnOptionSelected?.Invoke(nodoActual, opcionElegida, indice);

        // Verificamos si posee los metros necesarios en la billetera
        if (distanceTracker.TieneMetrosSuficientes(opcionElegida.costo))
        {
            distanceTracker.ConsumirMetros(opcionElegida.costo);
            AvanzarANodo(opcionElegida.siguienteNodo);
        }
        else
        {
            // Saldo insuficiente: telemetría de fricción y visualización en UI
            float metrosFaltantes = opcionElegida.costo - distanceTracker.distanciaAcumulada;
            OnInsufficientMetersAlert?.Invoke(opcionElegida, metrosFaltantes);
            uiManager.MostrarAlertaMetrosInsuficientes(opcionElegida.costo, distanceTracker.distanciaAcumulada);
        }
    }

    private void AvanzarANodo(StoryNode siguienteNodo)
    {
        if (siguienteNodo == null)
        {
            Debug.LogError("El siguiente nodo no está configurado.");
            return;
        }

        nodoActual = siguienteNodo;

        if (nodoActual.EsFinal)
        {
            historiaTerminada = true;
            OnStoryCompleted?.Invoke(nodoActual);
            uiManager.MostrarPantallaFinal(nodoActual.texto);
        }
        else
        {
            uiManager.MostrarNodo(nodoActual);
            uiManager.MostrarPantallaHistoria();
        }

        OnNodeReached?.Invoke(nodoActual);
        GuardarEstadoActual();
    }

    void GuardarEstadoActual()
    {
        EstadoJuego estado = historiaTerminada ? EstadoJuego.Final : EstadoJuego.Historia;
        string nodeId = nodoActual != null ? nodoActual.NodeId : "";
        int nodeIntId = nodoActual != null ? nodoActual.id : 0;
        int historiaId = PlayerPrefs.GetInt("historiaSeleccionada", 1);

        GameSaveData data = new GameSaveData(
            nodeId,
            nodeIntId,
            distanceTracker.distanciaAcumulada,
            estado,
            historiaId
        );

        saveSystem.GuardarProgreso(data);
    }

    void CargarPartida()
    {
        GameSaveData data = saveSystem.CargarProgreso();

        distanceTracker.distanciaAcumulada = data.accumulatedDistance;

        // Búsqueda por NodeId (string) con fallback a id (int) para compatibilidad
        foreach (StoryNode nodo in todosLosNodos)
        {
            if (nodo != null)
            {
                bool coincideString = !string.IsNullOrEmpty(data.currentNodeId) && nodo.NodeId == data.currentNodeId;
                bool coincideInt = nodo.id == data.currentNodeIntId;
                if (coincideString || coincideInt)
                {
                    nodoActual = nodo;
                    break;
                }
            }
        }

        if (nodoActual == null)
        {
            Debug.Log("Nodo no encontrado, iniciando nueva partida");
            saveSystem.BorrarProgreso();
            NuevaPartida();
            return;
        }

        if (data.gameState == EstadoJuego.Final || nodoActual.EsFinal)
        {
            historiaTerminada = true;
            OnStoryCompleted?.Invoke(nodoActual);
            uiManager.MostrarPantallaFinal(nodoActual.texto);
        }
        else
        {
            historiaTerminada = false;
            uiManager.MostrarNodo(nodoActual);
            uiManager.MostrarPantallaHistoria();
        }

        OnNodeReached?.Invoke(nodoActual);
    }

    void OnApplicationPause(bool pausado)
    {
        if (!inicializado) return;

        if (pausado)
        {
            GuardarEstadoActual();
            Debug.Log("PAUSANDO - Estado guardado.");
        }
    }

    void OnApplicationQuit()
    {
        if (!inicializado) return;
        GuardarEstadoActual();
        Debug.Log("SALIENDO - Estado guardado.");
    }

    public void ReiniciarJuego()
    {
        historiaTerminada = false;
        nodoActual = nodoInicial;
        // No se resetea distanceTracker para conservar el banco global de metros
        uiManager.MostrarNodo(nodoActual);
        uiManager.MostrarPantallaHistoria();
        OnNodeReached?.Invoke(nodoActual);
        GuardarEstadoActual();
    }

    void OnDestroy()
    {
        TelemetryManager.Instance?.DesvincularStoryManager(this);
    }
}