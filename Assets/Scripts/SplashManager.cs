using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Threading.Tasks;

public class SplashManager : MonoBehaviour
{
    [Header("Referencias UI")]
    public Button botonComenzar;
    public TMP_InputField inputNombreTester;
    public TextMeshProUGUI textoEstado;

    private const string PREF_TESTER_NAME = "tester_nombre";
    private bool esContinuar = false;

    void Awake()
    {
        // Asegurar que FirebaseInit exista en la escena
        if (FindFirstObjectByType<FirebaseInit>() == null)
        {
            GameObject fbObj = new GameObject("FirebaseInit");
            fbObj.AddComponent<FirebaseInit>();
        }

        // Asegurar que TelemetryManager exista en la escena
        if (FindFirstObjectByType<TelemetryManager>() == null)
        {
            GameObject telObj = new GameObject("TelemetryManager");
            telObj.AddComponent<TelemetryManager>();
        }
    }

    void Start()
    {
        esContinuar = PlayerPrefs.HasKey("estadoJuego") && PlayerPrefs.GetInt("sesionActiva", 0) == 1;

        // Auto-vincular o generar UI si no está asignada en el inspector
        AsegurarComponentesUI();

        // Cargar nombre previo si existe
        string nombreGuardado = PlayerPrefs.GetString(PREF_TESTER_NAME, "");
        if (inputNombreTester != null && !string.IsNullOrEmpty(nombreGuardado))
        {
            inputNombreTester.text = nombreGuardado;
        }

        if (botonComenzar != null)
        {
            var textoBoton = botonComenzar.GetComponentInChildren<TextMeshProUGUI>();
            if (textoBoton != null)
            {
                textoBoton.text = esContinuar ? "Continuar" : "Comenzar";
            }

            botonComenzar.onClick.RemoveAllListeners();
            botonComenzar.onClick.AddListener(OnBotonComenzarClick);
        }

        // Suscribirse a cambios de estado de Firebase
        if (FirebaseInit.Instance != null)
        {
            FirebaseInit.Instance.OnInitializationStatusChanged += ActualizarTextoEstado;
            if (FirebaseInit.IsInitialized)
            {
                ActualizarTextoEstado("Firebase listo.");
            }
        }
    }

    void OnDestroy()
    {
        if (FirebaseInit.Instance != null)
        {
            FirebaseInit.Instance.OnInitializationStatusChanged -= ActualizarTextoEstado;
        }
    }

    private void ActualizarTextoEstado(string estado)
    {
        if (textoEstado != null)
        {
            textoEstado.text = estado;
        }
    }

    private async void OnBotonComenzarClick()
    {
        string nombreIngresado = inputNombreTester != null ? inputNombreTester.text.Trim() : "";

        if (string.IsNullOrWhiteSpace(nombreIngresado))
        {
            ActualizarTextoEstado("<color=yellow>Por favor, ingresa tu nombre para registrar tu sesión.</color>");
            return;
        }

        if (botonComenzar != null)
        {
            botonComenzar.interactable = false;
        }

        ActualizarTextoEstado("Registrando tester en Firebase...");

        if (FirebaseInit.Instance != null)
        {
            await FirebaseInit.Instance.GuardarNombreTesterAsync(nombreIngresado);
        }

        if (TelemetryManager.Instance != null)
        {
            TelemetryManager.Instance.IniciarNuevaSesion(nombreIngresado);
        }

        ActualizarTextoEstado("¡Todo listo! Iniciando...");

        if (esContinuar)
        {
            SceneManager.LoadScene("Juego");
        }
        else
        {
            SceneManager.LoadScene("SeleccionHistorias");
        }
    }

    private void AsegurarComponentesUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // Si inputNombreTester no fue asignado, intentar buscarlo en la escena
        if (inputNombreTester == null)
        {
            inputNombreTester = canvas.GetComponentInChildren<TMP_InputField>();
        }

        // Si textoEstado no fue asignado, buscar objeto llamado "TextoEstado"
        if (textoEstado == null)
        {
            Transform estadoTransform = canvas.transform.Find("TextoEstado");
            if (estadoTransform != null)
            {
                textoEstado = estadoTransform.GetComponent<TextMeshProUGUI>();
            }
        }

        // Generar dinámicamente si aún no existen para garantizar 100% funcionamiento
        if (inputNombreTester == null && botonComenzar != null)
        {
            CrearInputNombreDinamico(canvas);
        }
    }

    private void CrearInputNombreDinamico(Canvas canvas)
    {
        // Encontrar la fuente existente en el botón o título
        TMP_FontAsset font = null;
        var tmpExistente = canvas.GetComponentInChildren<TextMeshProUGUI>();
        if (tmpExistente != null) font = tmpExistente.font;

        // Contenedor principal de Input
        GameObject inputObj = new GameObject("InputNombreTester", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(TMP_InputField));
        inputObj.transform.SetParent(canvas.transform, false);

        RectTransform rt = inputObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(480, 80);
        // Ubicarlo cómodamente arriba del botón Comenzar
        Vector2 botonPos = botonComenzar.GetComponent<RectTransform>().anchoredPosition;
        rt.anchoredPosition = new Vector2(0, botonPos.y + 160);

        Image img = inputObj.GetComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.18f, 0.95f);

        // Área de texto
        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(inputObj.transform, false);
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(20, 10);
        textRt.offsetMax = new Vector2(-20, -10);

        TextMeshProUGUI textComp = textObj.GetComponent<TextMeshProUGUI>();
        if (font != null) textComp.font = font;
        textComp.fontSize = 28;
        textComp.color = Color.white;
        textComp.alignment = TextAlignmentOptions.MidlineLeft;

        // Placeholder
        GameObject placeholderObj = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
        placeholderObj.transform.SetParent(inputObj.transform, false);
        RectTransform placeholderRt = placeholderObj.GetComponent<RectTransform>();
        placeholderRt.anchorMin = Vector2.zero;
        placeholderRt.anchorMax = Vector2.one;
        placeholderRt.offsetMin = new Vector2(20, 10);
        placeholderRt.offsetMax = new Vector2(-20, -10);

        TextMeshProUGUI placeholderComp = placeholderObj.GetComponent<TextMeshProUGUI>();
        if (font != null) placeholderComp.font = font;
        placeholderComp.fontSize = 26;
        placeholderComp.fontStyle = FontStyles.Italic;
        placeholderComp.color = new Color(0.7f, 0.7f, 0.7f, 0.7f);
        placeholderComp.text = "Ingresa tu nombre aquí...";
        placeholderComp.alignment = TextAlignmentOptions.MidlineLeft;

        // Configurar TMP_InputField
        TMP_InputField inputField = inputObj.GetComponent<TMP_InputField>();
        inputField.textComponent = textComp;
        inputField.placeholder = placeholderComp;
        inputField.lineType = TMP_InputField.LineType.SingleLine;
        inputField.characterLimit = 30;

        inputNombreTester = inputField;

        // Label explicativo arriba del input
        GameObject labelObj = new GameObject("LabelNombre", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObj.transform.SetParent(canvas.transform, false);
        RectTransform labelRt = labelObj.GetComponent<RectTransform>();
        labelRt.anchorMin = new Vector2(0.5f, 0.5f);
        labelRt.anchorMax = new Vector2(0.5f, 0.5f);
        labelRt.sizeDelta = new Vector2(500, 40);
        labelRt.anchoredPosition = new Vector2(0, rt.anchoredPosition.y + 65);

        TextMeshProUGUI labelComp = labelObj.GetComponent<TextMeshProUGUI>();
        if (font != null) labelComp.font = font;
        labelComp.fontSize = 26;
        labelComp.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        labelComp.alignment = TextAlignmentOptions.Center;
        labelComp.text = "Identificación de Tester:";

        // Texto de Estado abajo del botón
        if (textoEstado == null)
        {
            GameObject estadoObj = new GameObject("TextoEstado", typeof(RectTransform), typeof(TextMeshProUGUI));
            estadoObj.transform.SetParent(canvas.transform, false);
            RectTransform estadoRt = estadoObj.GetComponent<RectTransform>();
            estadoRt.anchorMin = new Vector2(0.5f, 0.5f);
            estadoRt.anchorMax = new Vector2(0.5f, 0.5f);
            estadoRt.sizeDelta = new Vector2(600, 60);
            estadoRt.anchoredPosition = new Vector2(0, botonPos.y - 100);

            textoEstado = estadoObj.GetComponent<TextMeshProUGUI>();
            if (font != null) textoEstado.font = font;
            textoEstado.fontSize = 20;
            textoEstado.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            textoEstado.alignment = TextAlignmentOptions.Center;
            textoEstado.text = "Inicializando Firebase...";
        }
    }
}