using UnityEngine;
using UnityEngine.Audio;

public class RaytracedAudioSource : MonoBehaviour
{
    [Header("References")]
    public Transform listener;              // Assign your AudioListener Transform
    public AudioSource audioSource;         // The AudioSource component
    public AudioMixer mixer;                // Your AudioMixer with exposed params

    [Header("Raycast Settings")]
    public int volumeRayCount = 10;
    public int echoRayCount = 20;
    public int reverbRayCount = 30;
    public float maxRayDistance = 15f;
    public LayerMask occlusionMask;

    [Header("Debug Ray Colors")]
    public Color volumeRayColor = Color.red;
    public Color echoRayColor = Color.green;
    public Color reverbRayColor = Color.blue;

    // Exposed parameter names in the mixer (must match your mixer exactly)
    public string volumeParam = "MuffleCutoff";
    public string echoParam = "EchoAmount";
    public string reverbParam = "ReverbAmount";

    void Update()
    {
        if (listener == null || audioSource == null || mixer == null)
        {
            Debug.LogWarning("Assign listener, audioSource, and mixer in inspector.");
            return;
        }

        Vector3 sourcePos = transform.position;
        Vector3 listenerDir = (listener.position - sourcePos).normalized;

        // Cast rays and draw debug rays
        int volumeHits = CastRays(sourcePos, listenerDir, volumeRayCount, 15f, 15f, maxRayDistance, occlusionMask, volumeRayColor);
        int echoHits = CastRays(sourcePos, listenerDir, echoRayCount, 90f, 90f, maxRayDistance, occlusionMask, echoRayColor);
        int reverbHits = CastRays(sourcePos, Vector3.up, reverbRayCount, 180f, 180f, maxRayDistance, occlusionMask, reverbRayColor);

        // Example: Map hits to audio mixer parameters (adjust mapping as needed)
        float volumeCutoff = Mathf.Lerp(22000f, 500f, (float)volumeHits / volumeRayCount); // high freq = no muffling, low freq = muffled
        float echoAmount = Mathf.Lerp(0f, 1f, (float)echoHits / echoRayCount);
        float reverbAmount = Mathf.Lerp(0f, 1f, (float)reverbHits / reverbRayCount);

        mixer.SetFloat(volumeParam, volumeCutoff);
        mixer.SetFloat(echoParam, echoAmount);
        mixer.SetFloat(reverbParam, reverbAmount);

        // Optional: Debug log the values
        // Debug.Log($"VolumeCutoff: {volumeCutoff}, Echo: {echoAmount}, Reverb: {reverbAmount}");
    }

    int CastRays(Vector3 origin, Vector3 forwardDir, int rayCount, float coneAngleHorizontal, float coneAngleVertical, float maxDistance, LayerMask layerMask, Color debugColor)
    {
        int hitCount = 0;
        for (int i = 0; i < rayCount; i++)
        {
            Vector3 rayDir = RandomDirectionInCone(forwardDir, coneAngleHorizontal, coneAngleVertical);
            Ray ray = new Ray(origin, rayDir);

            Debug.DrawRay(origin, rayDir * maxDistance, debugColor);

            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, layerMask))
            {
                hitCount++;
                Debug.DrawRay(hit.point, Vector3.up * 0.2f, Color.yellow);
            }
        }
        return hitCount;
    }

    Vector3 RandomDirectionInCone(Vector3 forwardDir, float coneAngleHorizontal, float coneAngleVertical)
    {
        float horAngle = Random.Range(-coneAngleHorizontal / 2f, coneAngleHorizontal / 2f);
        float vertAngle = Random.Range(-coneAngleVertical / 2f, coneAngleVertical / 2f);
        Quaternion rotation = Quaternion.Euler(vertAngle, horAngle, 0);
        Vector3 dir = rotation * forwardDir;
        return dir.normalized;
    }
}
