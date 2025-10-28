using UnityEngine;

public class HaySpawner : MonoBehaviour
{
    [Header("Arena")]
    public Transform arenaCenter;         
    public float arenaRadius = 7.5f;

    [Header("Spawning")]
    public GameObject hayPrefab;
    public float firstDelay = 1.5f;
    public float spawnEvery = 2.0f;

    [Header("Bale Motion")]
    public float baseSpeed = 4.0f;
    public float speedJitter = 1.5f;      // +/- random

    void Start()
    {
        InvokeRepeating(nameof(SpawnOne), firstDelay, spawnEvery);
    }

    void SpawnOne()
    {
        Vector3 c = arenaCenter ? arenaCenter.position : Vector3.zero;

        // pick random point on the circle edge
        float ang = Random.Range(0f, Mathf.PI * 2f);
        Vector3 pos = c + new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * arenaRadius;

        Vector3 dir = (c - pos).normalized;

        GameObject go = Instantiate(hayPrefab, pos, Quaternion.identity);
        float spd = baseSpeed + Random.Range(-speedJitter, speedJitter);

        var hay = go.GetComponent<Hay>();
        if (hay == null) hay = go.AddComponent<Hay>();
        hay.Launch(dir, spd, arenaRadius, arenaCenter);
    }
}
