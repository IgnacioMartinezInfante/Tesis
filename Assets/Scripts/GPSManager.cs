using UnityEngine;
using System.Collections;
using UnityEngine.Android;

public class GPSManager : MonoBehaviour
{
    public float latitude;
    public float longitude;

    private Vector2 lastPosition;
    public float totalDistanceMeters = 0f;

    IEnumerator Start()
    {
        //  Pedir permiso de ubicación
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            yield return new WaitForSeconds(2);
        }

        //  Verificar si el GPS está activado
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("GPS no activado");
            yield break;
        }

        //  Iniciar GPS
        Input.location.Start();

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            Debug.Log("Inicializando GPS...");
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1)
        {
            Debug.Log("Tiempo de espera agotado");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("No se pudo obtener ubicación");
            yield break;
        }

        Debug.Log("GPS iniciado correctamente");

        //  Loop principal
        while (true)
        {
            latitude = Input.location.lastData.latitude;
            longitude = Input.location.lastData.longitude;

            Vector2 currentPosition = new Vector2(latitude, longitude);

            if (lastPosition != Vector2.zero)
            {
                float distance = CalculateDistanceMeters(
                    lastPosition.x, lastPosition.y,
                    currentPosition.x, currentPosition.y
                );

                totalDistanceMeters += distance;

                Debug.Log("Distancia total (m): " + totalDistanceMeters);
            }

            lastPosition = currentPosition;

            yield return new WaitForSeconds(1);
        }
    }

    //  Fórmula real (Haversine)
    float CalculateDistanceMeters(float lat1, float lon1, float lat2, float lon2)
    {
        float R = 6371000f; // Radio de la Tierra en metros

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