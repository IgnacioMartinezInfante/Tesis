using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public StoryNode nodoInicial;
    public StoryNode[] todosLosNodos;

    public event System.Action<StoryNode> OnNodeReached;

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

        // Verificamos si posee los metros necesarios en la billetera
        if (distanceTracker.TieneMetrosSuficientes(opcionElegida.costo))
        {
            distanceTracker.ConsumirMetros(opcionElegida.costo);
            AvanzarANodo(opcionElegida.siguienteNodo);
        }
        else
        {
            // Saldo insuficiente: mostrar cartel de alerta sin avanzar de nodo
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
        int nodoActualId = nodoActual != null ? nodoActual.id : 0;

        saveSystem.GuardarProgreso(
            nodoActualId,
            distanceTracker.distanciaAcumulada,
            estado
        );
    }

    void CargarPartida()
    {
        EstadoJuego estado = saveSystem.CargarEstado();
        int nodoActualId = saveSystem.CargarNodoActualId();
        float distanciaGuardada = saveSystem.CargarDistancia();

        distanceTracker.distanciaAcumulada = distanciaGuardada;

        foreach (StoryNode nodo in todosLosNodos)
        {
            if (nodo != null && nodo.id == nodoActualId)
            {
                nodoActual = nodo;
                break;
            }
        }

        if (nodoActual == null)
        {
            Debug.Log("Nodo no encontrado, iniciando nueva partida");
            saveSystem.BorrarProgreso();
            NuevaPartida();
            return;
        }

        if (estado == EstadoJuego.Final || nodoActual.EsFinal)
        {
            historiaTerminada = true;
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
}