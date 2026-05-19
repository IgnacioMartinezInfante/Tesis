using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [Header("Pantallas")]
    public GameObject pantallaHistoria;
    public GameObject pantallaDistancia;

    [Header("Historia")]
    public TextMeshProUGUI textoNodo;
    public TextMeshProUGUI textoOpcion1;
    public TextMeshProUGUI textoOpcion2;
    public Button botonOpcion1;
    public Button botonOpcion2;

    [Header("Distancia")]
    public TextMeshProUGUI textoDistancia;
    public TextMeshProUGUI textoObjetivo;
    public Button botonSimular;

    private StoryManager storyManager;

    void Start()
    {
        storyManager = GetComponent<StoryManager>();
        botonOpcion1.onClick.AddListener(() => storyManager.ElegirOpcion(0));
        botonOpcion2.onClick.AddListener(() => storyManager.ElegirOpcion(1));
        botonSimular.onClick.AddListener(() => storyManager.CompletarDistancia());
        MostrarPantallaHistoria();
    }

    public void MostrarNodo(StoryNode nodo)
    {
        textoNodo.text = nodo.texto;
        textoOpcion1.text = $"{nodo.opciones[0].texto}\n{nodo.opciones[0].costo}m";
        textoOpcion2.text = $"{nodo.opciones[1].texto}\n{nodo.opciones[1].costo}m";
    }

    public void MostrarPantallaHistoria()
    {
        pantallaHistoria.SetActive(true);
        pantallaDistancia.SetActive(false);
    }

    public void MostrarPantallaDistancia(int costo)
    {
        textoObjetivo.text = "Objetivo: " + costo + "m";
        StartCoroutine(MostrarPantallaDistanciaCoroutine());
    }

    private IEnumerator MostrarPantallaDistanciaCoroutine()
    {
        yield return null;
        pantallaHistoria.SetActive(false);
        yield return null;
        pantallaDistancia.SetActive(true);
    }

    public void ActualizarDistancia(float actual, float objetivo)
    {
        textoObjetivo.text = $"Objetivo: {actual:F0}m / {objetivo}m";
    }
}