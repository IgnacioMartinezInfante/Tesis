using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SplashManager : MonoBehaviour
{
    public Button botonComenzar;

    void Start()
    {
        // Si hay progreso guardado, ir directo al juego
        if (PlayerPrefs.HasKey("estadoJuego"))
        {
            SceneManager.LoadScene("Juego");
            return;
        }

        botonComenzar.onClick.AddListener(() => {
            SceneManager.LoadScene("SeleccionHistorias");
        });
    }
}