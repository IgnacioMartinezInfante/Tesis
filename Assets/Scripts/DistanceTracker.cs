using UnityEngine;
using System.Collections;
using UnityEngine.Android;
using TMPro;

public class DistanceTracker : MonoBehaviour
{
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI buttonText;

    private bool isTracking = false;

    private Vector2 lastPosition;
    private float totalDistance = 0f;

    private LocationService location;

    void Start()
    {
        location = Input.location;
    }

    public void ToggleTracking()
    {
        if (!isTracking)
        {
            StartCoroutine(StartTracking());
        }
        else
        {
            StopTracking();
        }
    }

    IEnumerator StartTracking()
    {
        // Pedir permiso
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            yield return new WaitForSeconds(2);
        }

        if (!location.isEnabledByUser)
        {
            Debug.Log("GPS apagado");
            yield break;
        }

        location.Start();

        int maxWait = 20;
        while (location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (location.status != LocationServiceStatus.Running)
        {
            Debug.Log("Error al iniciar GPS");
            yield break;
        }

        // Resetear datos
        totalDistance = 0f;
        lastPosition = Vector2.zero;

        isTracking = true;
        buttonText.text = "Stop";

        StartCoroutine(UpdateDistance());
    }

    void StopTracking()
    {
        isTracking = false;
        location.Stop();

        buttonText.text = "Start";

        Debug.Log("Distancia final: " + totalDistance + " metros");
    }

    IEnumerator UpdateDistance()
    {
        while (isTracking)
        {
            float lat = location.lastData.latitude;
            float lon = location.lastData.longitude;

            Vector2 currentPosition = new Vector2(lat, lon);

            if (lastPosition != Vector2.zero)
            {
                float distance = CalculateDistanceMeters(
                    lastPosition.x, lastPosition.y,
                    currentPosition.x, currentPosition.y
                );

                //  filtro básico (evita ruido GPS)
                if (distance > 0.5f && distance < 20f)
                {
                    totalDistance += distance;
                }
            }

            lastPosition = currentPosition;

            distanceText.text = "Distancia: " + totalDistance.ToString("F1") + " m";

            yield return new WaitForSeconds(1);
        }
    }

    float CalculateDistanceMeters(float lat1, float lon1, float lat2, float lon2)
    {
        float R = 6371000f;

        float dLat = Mathf.Deg2Rad * (lat2 - lat1);
        float dLon = Mathf.Deg2Rad * (lon2 - lon1);

        float a =
            Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
            Mathf.Cos(Mathf.Deg2Rad * lat1) * Mathf.Cos(Mathf.Deg2Rad * lat2) *
            Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);

        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));

        return R * c;
    }
}