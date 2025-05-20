using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RaytracedAudioSource : MonoBehaviour
{
    public float maxDistance = 50f;
    public float reflectionStrength = 0.5f;
    public float reverbFactor = 0.5f;

    [HideInInspector] public AudioSource source;
    [HideInInspector] public Vector3 lastKnownPosition;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        lastKnownPosition = transform.position;
    }
}
