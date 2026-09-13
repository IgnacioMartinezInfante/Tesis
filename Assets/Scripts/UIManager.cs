using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Pantallas")]
    public GameObject pantallaHistoria;
    public GameObject pantallaFinal;
    public Button botonMenu;

    [Header("Historia")]
    public TextMeshProUGUI textoNodo;
    public TextMeshProUGUI textoOpcion1;
    public TextMeshProUGUI textoOpcion2;
    public Button botonOpcion1;
    public Button botonOpcion2;

    [Header("Billetera de Metros")]
    public TextMeshProUGUI textoBilleteraMetros;
    public Button botonSimularMetros; // Botón opcional para sumar metros (+50m) en pruebas
    public Button botonReset;

    [Header("Alerta Metros Insuficientes")]
    public GameObject panelAlertaMetros;
    public TextMeshProUGUI textoAlertaMetros;
    public Button botonCerrarAlerta;

    [Header("Final")]
    public TextMeshProUGUI textoFinal;
    public Button botonReiniciar;

    private SaveSystem saveSystem;
    private DistanceTracker distanceTracker;
    private StoryManager storyManager;

    void Start()
    {
        storyManager = GetComponent<StoryManager>();
        saveSystem = GetComponent<SaveSystem>();
        distanceTracker = GetComponent<DistanceTracker>();

        if (botonMenu != null) botonMenu.onClick.AddListener(IrAMenuHistorias);
        if (botonOpcion1 != null) botonOpcion1.onClick.AddListener(() => storyManager.ElegirOpcion(0));
        if (botonOpcion2 != null) botonOpcion2.onClick.AddListener(() => storyManager.ElegirOpcion(1));
        if (botonReiniciar != null) botonReiniciar.onClick.AddListener(() => storyManager.ReiniciarJuego());
        if (botonCerrarAlerta != null) botonCerrarAlerta.onClick.AddListener(OcultarAlertaMetros);

        if (botonSimularMetros != null)
        {
            botonSimularMetros.onClick.AddListener(() => distanceTracker.AgregarMetros(50f));
        }

        if (botonReset != null)
        {
            botonReset.onClick.AddListener(() => {
                storyManager.ReiniciarJuego();
            });
        }

        if (distanceTracker != null)
        {
            distanceTracker.OnDistanciaActualizada += ActualizarBilletera;
            ActualizarBilletera(distanceTracker.distanciaAcumulada);
        }

        // Estado inicial de pantallas
        if (pantallaHistoria != null) pantallaHistoria.SetActive(false);
        if (pantallaFinal != null) pantallaFinal.SetActive(false);
        if (panelAlertaMetros != null) panelAlertaMetros.SetActive(false);
    }

    void OnDestroy()
    {
        if (distanceTracker != null)
        {
            distanceTracker.OnDistanciaActualizada -= ActualizarBilletera;
        }
    }

    public void MostrarNodo(StoryNode nodo)
    {
        if (nodo == null) return;
        if (textoNodo != null) textoNodo.text = nodo.texto;
        if (textoOpcion1 != null && nodo.opciones != null && nodo.opciones.Length > 0)
            textoOpcion1.text = $"{nodo.opciones[0].texto}\n{nodo.opciones[0].costo}m";
        if (textoOpcion2 != null && nodo.opciones != null && nodo.opciones.Length > 1)
            textoOpcion2.text = $"{nodo.opciones[1].texto}\n{nodo.opciones[1].costo}m";
    }

    public void MostrarPantallaHistoria()
    {
        StartCoroutine(MostrarPantallaHistoriaCoroutine());
    }

    private IEnumerator MostrarPantallaHistoriaCoroutine()
    {
        yield return null;
        if (pantallaFinal != null) pantallaFinal.SetActive(false);
        if (panelAlertaMetros != null) panelAlertaMetros.SetActive(false);
        yield return null;
        if (pantallaHistoria != null) pantallaHistoria.SetActive(true);
    }

    public void MostrarAlertaMetrosInsuficientes(int costoRequerido, float metrosActuales)
    {
        if (textoAlertaMetros != null)
        {
            textoAlertaMetros.text = "No posees los metros recorridos suficientes. Sal a caminar para desbloquear esta parte de la historia.";
        }
        if (panelAlertaMetros != null)
        {
            panelAlertaMetros.SetActive(true);
        }
        else
        {
            Debug.LogWarning("No posees los metros recorridos suficientes. Sal a caminar para desbloquear esta parte de la historia.");
        }
    }

    public void OcultarAlertaMetros()
    {
        if (panelAlertaMetros != null)
        {
            panelAlertaMetros.SetActive(false);
        }
    }

    public void MostrarPantallaFinal(string texto)
    {
        if (textoFinal != null) textoFinal.text = texto;
        StartCoroutine(MostrarPantallaFinalCoroutine());
    }

    private IEnumerator MostrarPantallaFinalCoroutine()
    {
        yield return null;
        if (pantallaHistoria != null) pantallaHistoria.SetActive(false);
        if (panelAlertaMetros != null) panelAlertaMetros.SetActive(false);
        yield return null;
        if (pantallaFinal != null) pantallaFinal.SetActive(true);
    }

    public void ActualizarBilletera(float metros)
    {
        if (textoBilleteraMetros != null)
        {
            textoBilleteraMetros.text = $"Metros: {metros:F0}m";
        }
    }

    void IrAMenuHistorias()
    {
        PlayerPrefs.SetInt("sesionActiva", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene("SeleccionHistorias");
    }
}