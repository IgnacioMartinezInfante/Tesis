using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SplashManager : MonoBehaviour
{
    public Button botonComenzar;

    void Start()
    {
        botonComenzar.onClick.AddListener(() => {
            SceneManager.LoadScene("SeleccionHistorias");
        });
    }
}