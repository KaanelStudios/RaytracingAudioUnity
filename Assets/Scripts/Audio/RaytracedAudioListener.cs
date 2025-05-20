using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioListener))]
public class RaytracedAudioListener : MonoBehaviour
{
    public AudioMixer mixer;
    public LayerMask environmentMask;

    [Range(0.5f, 10f)] public float updateInterval = 1f;
    [Range(5, 100)] public int rayCount = 32;
    [Range(1f, 50f)] public float maxDistance = 20f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            UpdateEnvironmentAcoustics();
        }
    }

    void UpdateEnvironmentAcoustics()
    {
        Vector3 origin = transform.position;
        float totalDistance = 0f;
        int hits = 0;
        int blockedCount = 0;

        for (int i = 0; i < rayCount; i++)
        {

            Vector3 dir = Random.onUnitSphere;
            if (Physics.Raycast(origin, dir, out RaycastHit hit, maxDistance, environmentMask))
            {
                totalDistance += hit.distance;
                blockedCount++;
                hits++;
            }
            else
            {
                totalDistance += maxDistance;
                hits++;
            }
            Debug.DrawRay(origin, dir * hit.distance, Color.red, 1f);

        }

        float avgDistance = hits > 0 ? totalDistance / hits : maxDistance;

        // Reflection strength
        float reflectionStrength = Mathf.Clamp01(avgDistance / maxDistance);
        reflectionStrength = Mathf.Clamp(reflectionStrength, 0.1f, 1f);

        // Reverb factor (same as reflection for simplicity)
        float reverbFactor = reflectionStrength;

        // Muffle amount (based on how many rays hit something)
        float muffleFactor = Mathf.InverseLerp(0f, rayCount, blockedCount);
        float cutoffFreq = Mathf.Lerp(22000f, 80, muffleFactor); // Hz

        // Send values to mixer
        mixer.SetFloat("ReflectionStrength", Mathf.Lerp(-80f, 0f, reflectionStrength)); // dB
        mixer.SetFloat("ReverbFactor", Mathf.Lerp(-80f, 0f, reverbFactor));             // dB
        mixer.SetFloat("MuffleAmount", cutoffFreq);                                     // Hz
    }
}
