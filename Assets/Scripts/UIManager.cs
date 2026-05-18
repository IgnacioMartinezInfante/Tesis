using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    private StoryManager storyManager;

    void Start()
    {
        storyManager = GetComponent<StoryManager>();

        botonOpcion1.onClick.AddListener(() => storyManager.ElegirOpcion(0));
        botonOpcion2.onClick.AddListener(() => storyManager.ElegirOpcion(1));

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

    public void MostrarPantallaDistancia()
    {
        pantallaHistoria.SetActive(false);
        pantallaDistancia.SetActive(true);
    }
}