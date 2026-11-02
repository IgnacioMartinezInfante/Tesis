using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class TesisSetupEditor : Editor
{
    [MenuItem("Tesis/1. Configurar UI de Tester en Splash")]
    public static void SetupSplashUI()
    {
        // Abrir escena Splash si no es la activa
        if (EditorSceneManager.GetActiveScene().name != "Splash")
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene("Assets/Scenes/Splash.unity");
            }
            else
            {
                return;
            }
        }

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Error", "No se encontró un Canvas en la escena Splash.", "OK");
            return;
        }

        SplashManager splashManager = Object.FindFirstObjectByType<SplashManager>();
        if (splashManager == null)
        {
            EditorUtility.DisplayDialog("Error", "No se encontró SplashManager en la escena Splash.", "OK");
            return;
        }

        Button botonComenzar = splashManager.botonComenzar;
        if (botonComenzar == null)
        {
            EditorUtility.DisplayDialog("Error", "Boton Comenzar no asignado en SplashManager.", "OK");
            return;
        }

        Vector2 botonPos = botonComenzar.GetComponent<RectTransform>().anchoredPosition;

        // Obtener font existente
        TMP_FontAsset font = null;
        var existingTmp = canvas.GetComponentInChildren<TextMeshProUGUI>();
        if (existingTmp != null) font = existingTmp.font;

        // 1. Label Explicativo
        Transform labelTrans = canvas.transform.Find("LabelNombreTester");
        GameObject labelObj;
        if (labelTrans == null)
        {
            labelObj = new GameObject("LabelNombreTester", typeof(RectTransform), typeof(TextMeshProUGUI));
            Undo.RegisterCreatedObjectUndo(labelObj, "Crear Label Nombre");
            labelObj.transform.SetParent(canvas.transform, false);
        }
        else
        {
            labelObj = labelTrans.gameObject;
        }

        RectTransform labelRt = labelObj.GetComponent<RectTransform>();
        labelRt.anchorMin = new Vector2(0.5f, 0.5f);
        labelRt.anchorMax = new Vector2(0.5f, 0.5f);
        labelRt.sizeDelta = new Vector2(600, 50);
        labelRt.anchoredPosition = new Vector2(0, botonPos.y + 220);

        TextMeshProUGUI labelComp = labelObj.GetComponent<TextMeshProUGUI>();
        if (font != null) labelComp.font = font;
        labelComp.fontSize = 28;
        labelComp.color = Color.white;
        labelComp.alignment = TextAlignmentOptions.Center;
        labelComp.text = "Ingresa tu Nombre para la Tesis:";

        // 2. InputField
        Transform inputTrans = canvas.transform.Find("InputNombreTester");
        GameObject inputObj;
        if (inputTrans == null)
        {
            inputObj = new GameObject("InputNombreTester", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(TMP_InputField));
            Undo.RegisterCreatedObjectUndo(inputObj, "Crear Input Nombre");
            inputObj.transform.SetParent(canvas.transform, false);
        }
        else
        {
            inputObj = inputTrans.gameObject;
        }

        RectTransform inputRt = inputObj.GetComponent<RectTransform>();
        inputRt.anchorMin = new Vector2(0.5f, 0.5f);
        inputRt.anchorMax = new Vector2(0.5f, 0.5f);
        inputRt.sizeDelta = new Vector2(500, 80);
        inputRt.anchoredPosition = new Vector2(0, botonPos.y + 140);

        Image img = inputObj.GetComponent<Image>();
        img.color = new Color(0.12f, 0.12f, 0.16f, 0.95f);

        // Text child
        Transform textChild = inputObj.transform.Find("Text");
        GameObject textObj = textChild != null ? textChild.gameObject : new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        if (textChild == null)
        {
            Undo.RegisterCreatedObjectUndo(textObj, "Crear Text Input");
            textObj.transform.SetParent(inputObj.transform, false);
        }
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(20, 10);
        textRt.offsetMax = new Vector2(-20, -10);

        TextMeshProUGUI textComp = textObj.GetComponent<TextMeshProUGUI>();
        if (font != null) textComp.font = font;
        textComp.fontSize = 26;
        textComp.color = Color.white;
        textComp.alignment = TextAlignmentOptions.MidlineLeft;

        // Placeholder child
        Transform phChild = inputObj.transform.Find("Placeholder");
        GameObject phObj = phChild != null ? phChild.gameObject : new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
        if (phChild == null)
        {
            Undo.RegisterCreatedObjectUndo(phObj, "Crear Placeholder Input");
            phObj.transform.SetParent(inputObj.transform, false);
        }
        RectTransform phRt = phObj.GetComponent<RectTransform>();
        phRt.anchorMin = Vector2.zero;
        phRt.anchorMax = Vector2.one;
        phRt.offsetMin = new Vector2(20, 10);
        phRt.offsetMax = new Vector2(-20, -10);

        TextMeshProUGUI phComp = phObj.GetComponent<TextMeshProUGUI>();
        if (font != null) phComp.font = font;
        phComp.fontSize = 24;
        phComp.fontStyle = FontStyles.Italic;
        phComp.color = new Color(0.7f, 0.7f, 0.7f, 0.7f);
        phComp.text = "Nombre y Apellido...";
        phComp.alignment = TextAlignmentOptions.MidlineLeft;

        // Conectar TMP_InputField
        TMP_InputField inputField = inputObj.GetComponent<TMP_InputField>();
        inputField.textComponent = textComp;
        inputField.placeholder = phComp;
        inputField.characterLimit = 35;

        // 3. TextoEstado
        Transform estadoTrans = canvas.transform.Find("TextoEstado");
        GameObject estadoObj = estadoTrans != null ? estadoTrans.gameObject : new GameObject("TextoEstado", typeof(RectTransform), typeof(TextMeshProUGUI));
        if (estadoTrans == null)
        {
            Undo.RegisterCreatedObjectUndo(estadoObj, "Crear Texto Estado");
            estadoObj.transform.SetParent(canvas.transform, false);
        }
        RectTransform estadoRt = estadoObj.GetComponent<RectTransform>();
        estadoRt.anchorMin = new Vector2(0.5f, 0.5f);
        estadoRt.anchorMax = new Vector2(0.5f, 0.5f);
        estadoRt.sizeDelta = new Vector2(650, 60);
        estadoRt.anchoredPosition = new Vector2(0, botonPos.y - 100);

        TextMeshProUGUI estadoComp = estadoObj.GetComponent<TextMeshProUGUI>();
        if (font != null) estadoComp.font = font;
        estadoComp.fontSize = 22;
        estadoComp.color = new Color(0.85f, 0.85f, 0.85f, 1f);
        estadoComp.alignment = TextAlignmentOptions.Center;
        estadoComp.text = "Conectando con Firebase...";

        // Asignar referencias en SplashManager
        Undo.RecordObject(splashManager, "Asignar UI SplashManager");
        splashManager.inputNombreTester = inputField;
        splashManager.textoEstado = estadoComp;

        EditorUtility.SetDirty(splashManager);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Éxito", "UI de Tester configurada y guardada correctamente en Splash.unity.", "OK");
    }

    [MenuItem("Tesis/2. Limpiar Datos de Prueba Locales (PlayerPrefs)")]
    public static void LimpiarPlayerPrefs()
    {
        PlayerPrefs.DeleteKey("tester_nombre");
        PlayerPrefs.DeleteKey("fallback_local_uid");
        PlayerPrefs.DeleteKey("sesionActiva");
        PlayerPrefs.DeleteKey("estadoJuego");
        PlayerPrefs.DeleteKey("distanciaAcumulada");
        PlayerPrefs.DeleteKey("game_save_data_json");
        PlayerPrefs.Save();
        EditorUtility.DisplayDialog("Tesis", "Datos locales limpiados. Ahora la app pedirá el nombre de nuevo como tester fresco.", "OK");
    }
}
