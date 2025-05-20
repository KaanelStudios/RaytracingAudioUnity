using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class RaytracedAudioListener : MonoBehaviour
{
    public LayerMask occlusionMask;
    public int maxReflections = 1;
    public AudioMixer mixer;
    public float reverbMaxDistance = 40f;

    private List<RaytracedAudioSource> audioSources;

    void Start()
    {
        audioSources = new List<RaytracedAudioSource>(FindObjectsOfType<RaytracedAudioSource>());
    }

    void Update()
    {
        foreach (var source in audioSources)
        {
            SimulateAudioRaytracing(source);
        }
    }

    void SimulateAudioRaytracing(RaytracedAudioSource ras)
    {
        Vector3 dir = ras.transform.position - transform.position;
        float distance = dir.magnitude;
        Ray ray = new Ray(transform.position, dir.normalized);

        bool blocked = Physics.Raycast(ray, distance, occlusionMask);

        // Muffle if blocked
        mixer.SetFloat("LowpassCutoff", blocked ? 800f : 22000f);

        // Echo approximation
        if (Physics.Raycast(ras.transform.position, -dir.normalized, out RaycastHit reflectionHit, distance, occlusionMask))
        {
            float echoVolume = ras.reflectionStrength / Mathf.Max(1f, reflectionHit.distance);
            mixer.SetFloat("EchoVolume", echoVolume * 0.5f);
        }
        else
        {
            mixer.SetFloat("EchoVolume", 0f);
        }

        // Reverb approximation based on open space
        float opennessFactor = 1f;
        int hitCount = 0;

        for (int i = 0; i < 4; i++)
        {
            Vector3 randomDir = Random.onUnitSphere;
            if (Physics.Raycast(transform.position, randomDir, out _, reverbMaxDistance, occlusionMask))
                hitCount++;
        }

        opennessFactor = 1f - (hitCount / 4f); // how open the space is
        float reverbLevel = ras.reverbFactor * opennessFactor;
        mixer.SetFloat("ReverbAmount", reverbLevel);
    }
}
