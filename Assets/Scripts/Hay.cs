using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Hay : MonoBehaviour
{
    [Header("Spin")]
    public float spinSpeed = 720f;

    Vector3 _dir;
    float _speed;
    float _arenaRadius;
    Vector3 _center;

    public void Launch(Vector3 dir, float speed, float arenaRadius, Transform center)
    {
        _dir = dir.normalized;
        _speed = speed;
        _arenaRadius = arenaRadius;
        _center = center ? center.position : Vector3.zero;
    }

    [Header("Audio")]
    [SerializeField] private AudioClip explosionSound;
    [SerializeField, Range(0f, 1f)] private float explosionVolume = 1f;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D so it’s always audible
    }

    void Update()
    {
        transform.position += _dir * _speed * Time.deltaTime;
        transform.Rotate(Vector3.right * spinSpeed * Time.deltaTime, Space.Self);

        float d = Vector3.Distance(transform.position, _center);
        if (d > _arenaRadius + 1.5f) Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Horse"))
        {
            GameManager.Instance.LoseLife();

            if (explosionSound)
            {
                var at = Camera.main ? Camera.main.transform.position : transform.position;
                AudioSource.PlayClipAtPoint(explosionSound, at, explosionVolume);
            }

            Destroy(gameObject);
        }
    }
}