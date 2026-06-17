using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SeleccionHistoriasManager : MonoBehaviour
{
    public Button botonHistoria1;
    public Button botonHistoria2;

    void Start()
    {
        botonHistoria1.onClick.AddListener(() => {
            PlayerPrefs.SetInt("historiaSeleccionada", 1);
            PlayerPrefs.Save();
            SceneManager.LoadScene("Juego");
        });

        botonHistoria2.onClick.AddListener(() => {
            PlayerPrefs.SetInt("historiaSeleccionada", 2);
            PlayerPrefs.Save();
            SceneManager.LoadScene("Juego");
        });
    }
}