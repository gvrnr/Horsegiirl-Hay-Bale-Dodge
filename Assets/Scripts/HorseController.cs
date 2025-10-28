using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HorseController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Arena (circle)")]
    [SerializeField] private Transform arenaCenter;   // leave null to use (0,0,0)
    [SerializeField] private float arenaRadius = 7.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip gallopClip; 
    [SerializeField, Range(0f, 1f)] private float gallopVolume = 0.35f;

    [Header("Boost")]
    [SerializeField] private float boostMultiplierDefault = 2f;  // used if caller passes <= 0
    private float baseMoveSpeed;
    private Coroutine boostCo;

    private Animator anim;   
    private AudioSource audioSrc;   
    private Rigidbody rb;        

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        if (!anim) Debug.LogError("Animator not found on Horse (or its Sprite child).");

        rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        audioSrc = GetComponent<AudioSource>();
        if (!audioSrc) audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.loop = true;
        audioSrc.playOnAwake = false;
        audioSrc.clip = gallopClip;
        audioSrc.volume = gallopVolume;
        audioSrc.spatialBlend = 0f; // 2D sound; set to 1 for 3D if you prefer
    }

    void Start()
    {
        baseMoveSpeed = moveSpeed; // cache starting speed for boosts
    }

    void Update()
    {
        // read input (Arrow/WASD0
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(h, 0f, v);
        bool isMoving = input.sqrMagnitude > 0.001f;
        if (isMoving) input.Normalize();

        // animator
        if (anim)
        {
            anim.SetBool("IsMoving", isMoving);
            anim.SetInteger("Direction", ComputeDirection(h, v)); // 0 D, 1 L, 2 R, 3 U
        }

        // move & clamp to circle
        Vector3 target = transform.position + input * moveSpeed * Time.deltaTime;
        Vector3 c = arenaCenter ? arenaCenter.position : Vector3.zero;
        Vector3 flat = new Vector3(target.x - c.x, 0f, target.z - c.z);
        if (flat.magnitude > arenaRadius) target = c + flat.normalized * arenaRadius;

        target.y = 1f;
        transform.position = target;

        // gallop audio (only while moving)
        if (isMoving)
        {
            if (gallopClip && !audioSrc.isPlaying) audioSrc.Play();
        }
        else
        {
            if (audioSrc.isPlaying) audioSrc.Stop();
        }
    }

    /// 0=Down, 1=Left, 2=Right, 3=Up (matching Animator transitions)
    private int ComputeDirection(float h, float v)
    {
        if (Mathf.Abs(h) > Mathf.Abs(v))
            return (h < 0f) ? 1 : (h > 0f ? 2 : 0); // Left or Right
        else
            return (v > 0f) ? 3 : (v < 0f ? 0 : 0); // Up or Down
    }

    public void ApplySpeedBoost(float multiplier, float duration)
    {
        if (multiplier <= 0f) multiplier = boostMultiplierDefault;
        if (boostCo != null) StopCoroutine(boostCo);
        boostCo = StartCoroutine(BoostRoutine(multiplier, duration));
    }

    private System.Collections.IEnumerator BoostRoutine(float multiplier, float duration)
    {
        moveSpeed = baseMoveSpeed * multiplier;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            yield return null;
        }
        moveSpeed = baseMoveSpeed;
        boostCo = null;
    }
}