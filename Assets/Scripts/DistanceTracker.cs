using UnityEngine;
using System.Collections;
using UnityEngine.Android;
using TMPro;

public class DistanceTracker : MonoBehaviour
{
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI buttonText;

    private int warmupSamples = 5;
    private int currentSamples = 0;

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
        currentSamples = 0;

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

        location.Start(1f, 0.5f);

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
            var data = location.lastData;

            float lat = data.latitude;
            float lon = data.longitude;
            float accuracy = data.horizontalAccuracy;

            Vector2 currentPosition = new Vector2(lat, lon);

            //  WARMUP GPS (clave)
            if (currentSamples < warmupSamples)
            {
                lastPosition = currentPosition;
                currentSamples++;

                Debug.Log("Calentando GPS... " + currentSamples);

                yield return new WaitForSeconds(1);
                continue;
            }

            if (lastPosition != Vector2.zero)
            {
                float distance = CalculateDistanceMeters(
                    lastPosition.x, lastPosition.y,
                    currentPosition.x, currentPosition.y
                );

                float minDistance = 2f;
                float maxDistance = 15f;
                float maxAccuracy = 8f;

                if (accuracy <= maxAccuracy)
                {
                    if (distance > minDistance && distance < maxDistance)
                    {
                        totalDistance += distance;
                    }
                }

                Debug.Log($"Dist: {distance:F2} | Acc: {accuracy:F2}");
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