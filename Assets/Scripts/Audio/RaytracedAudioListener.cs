using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioListener))]
public class RaytracedAudioListener : MonoBehaviour
{
    [Header("Audio Mixer Settings")]
    public AudioMixer mixer;                  // Assign your main audio mixer here
    public string reflectionParam = "ReflectionStrength"; // exposed param in mixer
    public string reverbParam = "ReverbFactor";           // exposed param in mixer

    [Header("Environment Sampling")]
    public LayerMask environmentMask;         // layers for walls/floors/ceilings
    public int rayCount = 64;                 // number of rays for sampling
    public float maxDistance = 20f;           // max raycast distance

    [Header("Update Settings")]
    public float updateInterval = 0.2f;       // seconds between samples
    private float timeSinceLastUpdate = 0f;

    void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate >= updateInterval)
        {
            timeSinceLastUpdate = 0f;
            UpdateEnvironmentAcoustics();
        }
    }

    void UpdateEnvironmentAcoustics()
    {
        Vector3 origin = transform.position;
        float totalDistance = 0f;
        int hits = 0;

        for (int i = 0; i < rayCount; i++)
        {
            Vector3 dir = Random.onUnitSphere;

            if (Physics.Raycast(origin, dir, out RaycastHit hit, maxDistance, environmentMask))
            {
                totalDistance += hit.distance;
                hits++;
            }
            else
            {
                totalDistance += maxDistance;
                hits++;
            }
        }

        float avgDistance = hits > 0 ? totalDistance / hits : maxDistance;

        float reflectionStrength = Mathf.InverseLerp(0f, maxDistance, avgDistance);
        float reverbFactor = Mathf.InverseLerp(0f, maxDistance, avgDistance);

        reflectionStrength = Mathf.Clamp(reflectionStrength, 0.1f, 1f);
        reverbFactor = Mathf.Clamp(reverbFactor, 0f, 1f);

        mixer.SetFloat(reflectionParam, reflectionStrength);
        mixer.SetFloat(reverbParam, reverbFactor);

        // Optional: debug log to check values in real time
        // Debug.Log($"Reflection: {reflectionStrength:F2}, Reverb: {reverbFactor:F2}");
    }
}
