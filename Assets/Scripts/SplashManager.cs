using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SplashManager : MonoBehaviour
{
    public Button botonComenzar;

    void Start()
    {
        if (PlayerPrefs.HasKey("estadoJuego") && PlayerPrefs.GetInt("sesionActiva", 0) == 1)
        {
            botonComenzar.GetComponentInChildren<TextMeshProUGUI>().text = "Continuar";
            botonComenzar.onClick.AddListener(() => {
                SceneManager.LoadScene("Juego");
            });
        }
        else
        {
            botonComenzar.GetComponentInChildren<TextMeshProUGUI>().text = "Comenzar";
            botonComenzar.onClick.AddListener(() => {
                SceneManager.LoadScene("SeleccionHistorias");
            });
        }
    }
}