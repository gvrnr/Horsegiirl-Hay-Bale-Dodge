using UnityEngine;
using System.Collections;

public class AppleSpawner : MonoBehaviour
{
    [Header("Arena")]
    public Transform arenaCenter;
    public float arenaRadius = 7.5f;

    [Header("Prefabs")]
    public GameObject redApplePrefab;    // speed boost
    public GameObject goldApplePrefab;   // +1 life
    public GameObject badApplePrefab;    // -20 points (green)

    [Header("Spawn Timing (seconds)")]
    public Vector2 spawnIntervalRange = new Vector2(3f, 5f);

    [Header("Mix (percent-like; they just need to sum <= 1)")]
    [SerializeField, Range(0f, 1f)] private float redChance = 0.6f;
    [SerializeField, Range(0f, 1f)] private float goldChance = 0.2f;

    [Header("Placement")]
    [SerializeField] private float spawnY = 0.1f; // keep above ground

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            float wait = Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
            yield return new WaitForSecondsRealtime(wait); // unaffected by timeScale

            // if game over, skip this tick
            if (GameManager.Instance != null && GameManager.Instance.CurrentLives <= 0)
                continue;

            SpawnOne();
        }
    }

    private void SpawnOne()
    {
        if (!redApplePrefab || !goldApplePrefab || !badApplePrefab) return;

        // spawn random point inside circle
        Vector3 c = arenaCenter ? arenaCenter.position : Vector3.zero;
        float ang = Random.Range(0f, Mathf.PI * 2f);
        float r = Mathf.Sqrt(Random.value) * (arenaRadius - 0.5f);
        Vector3 pos = c + new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * r;
        pos.y = spawnY;

        // normalized cutoffs
        float cutRed = Mathf.Clamp01(redChance);
        float cutGold = Mathf.Clamp01(redChance + goldChance); // cumulative
        float roll = Random.value;

        GameObject prefab;
        if (roll < cutRed)
            prefab = redApplePrefab;
        else if (roll < cutGold)
            prefab = goldApplePrefab;
        else
            prefab = badApplePrefab;

        Instantiate(prefab, pos, prefab.transform.rotation);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        float sum = redChance + goldChance;
        if (sum > 1f)
        {
            // scale down proportionally so sum==1
            redChance /= sum;
            goldChance /= sum;
        }
    }
#endif
}