using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public StoryNode nodoInicial;
    public StoryNode[] todosLosNodos;

    private StoryNode nodoActual;
    private StoryNode nodoPendiente;
    private int costoPendiente;
    private bool esperandoDistancia = false;
    private bool objetivoCompletado = false;
    private bool inicializado = false;
    private bool historiaTerminada = false;

    private UIManager uiManager;
    private DistanceTracker distanceTracker;
    private SaveSystem saveSystem;

    private float tiempoGuardado = 0f;

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
        nodoActual = nodoInicial;
        if (nodoActual.EsFinal)
        {
            uiManager.MostrarPantallaFinal(nodoActual.texto);
        }
        else
        {
            uiManager.MostrarNodo(nodoActual);
            uiManager.MostrarPantallaHistoria();
        }
        GuardarEstadoActual();
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
                GuardarEstadoActual();
            }

            if (distanceTracker.distanciaAcumulada >= costoPendiente)
            {
                esperandoDistancia = false;
                objetivoCompletado = true;
                saveSystem.GuardarProgreso(
                    nodoActual.id,
                    nodoPendiente.id,
                    distanceTracker.distanciaAcumulada,
                    costoPendiente,
                    EstadoJuego.ObjetivoCompletado
                );
                uiManager.MostrarPantallaObjetivo(distanceTracker.distanciaAcumulada);
            }
        }
    }

    public void ElegirOpcion(int indice)
    {
        if (indice < 0 || indice >= nodoActual.opciones.Length) return;

        Opcion opcionElegida = nodoActual.opciones[indice];
        nodoPendiente = opcionElegida.siguienteNodo;
        costoPendiente = opcionElegida.costo;

        distanceTracker.totalDistance = 0f;
        distanceTracker.distanciaAcumulada = 0f;

        esperandoDistancia = true;
        objetivoCompletado = false;
        GuardarEstadoActual();
        uiManager.MostrarPantallaDistancia(costoPendiente);
    }

    public void CompletarObjetivo()
    {
        esperandoDistancia = false;
        objetivoCompletado = true;
        saveSystem.GuardarProgreso(
            nodoActual.id,
            nodoPendiente.id,
            distanceTracker.distanciaAcumulada,
            costoPendiente,
            EstadoJuego.ObjetivoCompletado
        );
        uiManager.MostrarPantallaObjetivo(costoPendiente);
    }

    public void CompletarDistancia()
    {
        objetivoCompletado = false;
        nodoActual = nodoPendiente;
        nodoPendiente = null;

        if (nodoActual.EsFinal)
        {
            historiaTerminada = true;
            objetivoCompletado = false;
            saveSystem.GuardarProgreso(
                nodoActual.id,
                0,
                0f,
                0,
                EstadoJuego.Final
            );
            uiManager.MostrarPantallaFinal(nodoActual.texto);
        }
        else
        {
            uiManager.MostrarNodo(nodoActual);
            uiManager.MostrarPantallaHistoria();
            GuardarEstadoActual();
        }
    }

    void GuardarEstadoActual()
    {
        EstadoJuego estado;
        int nodoActualId = nodoActual != null ? nodoActual.id : 0;
        int nodoPendienteId = nodoPendiente != null ? nodoPendiente.id : 0;

        if (historiaTerminada)
            estado = EstadoJuego.Final;
        if (esperandoDistancia)
            estado = EstadoJuego.Caminando;
        else if (objetivoCompletado)
            estado = EstadoJuego.ObjetivoCompletado;
        else
            estado = EstadoJuego.Historia;

        saveSystem.GuardarProgreso(
            nodoActualId,
            nodoPendienteId,
            distanceTracker.distanciaAcumulada,
            costoPendiente,
            estado
        );
    }

    void CargarPartida()
    {
        Debug.Log($"RAW PlayerPrefs - estado: {PlayerPrefs.GetInt("estadoJuego", -1)} | nodoActual: {PlayerPrefs.GetInt("nodoActualId", -1)} | nodoPendiente: {PlayerPrefs.GetInt("nodoPendienteId", -1)}");
        EstadoJuego estado = saveSystem.CargarEstado();
        int nodoActualId = saveSystem.CargarNodoActualId();
        int nodoPendienteId = saveSystem.CargarNodoPendienteId();
        float distanciaGuardada = saveSystem.CargarDistancia();
        costoPendiente = saveSystem.CargarCosto();

        Debug.Log($"CARGANDO - Estado: {estado} | NodoActual: {nodoActualId} | NodoPendiente: {nodoPendienteId} | Distancia: {distanciaGuardada} | Costo: {costoPendiente}");

        foreach (StoryNode nodo in todosLosNodos)
        {
            if (nodo.id == nodoActualId) nodoActual = nodo;
            if (nodo.id == nodoPendienteId) nodoPendiente = nodo;
        }

        if (nodoActual == null)
        {
            Debug.Log("Nodo no encontrado, nueva partida");
            saveSystem.BorrarProgreso();
            NuevaPartida();
            return;
        }

        distanceTracker.distanciaAcumulada = distanciaGuardada;
        distanceTracker.totalDistance = 0f;

        esperandoDistancia = false;
        objetivoCompletado = false;

        switch (estado)
        {
            case EstadoJuego.Historia:
                if (nodoActual.EsFinal)
                {
                    uiManager.MostrarPantallaFinal(nodoActual.texto);
                }
                else
                {
                    uiManager.MostrarNodo(nodoActual);
                    uiManager.MostrarPantallaHistoria();
                }
                break;

            case EstadoJuego.Caminando:
                esperandoDistancia = true;
                objetivoCompletado = false;
                uiManager.MostrarPantallaDistancia(costoPendiente);
                break;

            case EstadoJuego.ObjetivoCompletado:
                esperandoDistancia = false;
                objetivoCompletado = true;
                uiManager.MostrarPantallaObjetivo(costoPendiente);
                GuardarEstadoActual();
                break;

            case EstadoJuego.Final:
                uiManager.MostrarPantallaFinal(nodoActual.texto);
                break;
        }
    }

    void OnApplicationPause(bool pausado)
    {
        if (!inicializado) return;

        if (pausado)
        {
            GuardarEstadoActual();
            Debug.Log($"PAUSANDO - Estado guardado | esperandoDistancia: {esperandoDistancia} | objetivoCompletado: {objetivoCompletado}");
        }
    }

    public void ReiniciarJuego()
    {
        historiaTerminada = false;
        esperandoDistancia = false;
        objetivoCompletado = false;
        nodoPendiente = null;
        costoPendiente = 0;
        nodoActual = nodoInicial;
        distanceTracker.totalDistance = 0f;
        distanceTracker.distanciaAcumulada = 0f;
        saveSystem.BorrarProgreso();
        uiManager.MostrarNodo(nodoActual);
        uiManager.MostrarPantallaHistoria();
        GuardarEstadoActual();
    }
}