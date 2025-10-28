using UnityEngine;
using System.Collections;

public class Apple : MonoBehaviour
{
    public enum AppleType { RedSpeedBoost, GoldHeart, BadApple }

    [Header("Type")]
    public AppleType type = AppleType.RedSpeedBoost;

    [Header("Lifetime (seconds)")]
    public Vector2 despawnIfUntouched = new Vector2(3f, 4f);  // 3–4s

    [Header("Audio (optional)")]
    public AudioClip pickupSfx;
    [Range(0f, 1f)] public float pickupVolume = 0.8f;

    private void Start()
    {
        float life = Random.Range(despawnIfUntouched.x, despawnIfUntouched.y);
        Destroy(gameObject, life);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Horse")) return;

        if (pickupSfx)
        {
            var at = Camera.main ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(pickupSfx, at, pickupVolume);
        }

        switch (type)
        {
            case AppleType.RedSpeedBoost:
                var horse = other.GetComponent<HorseController>();
                if (horse) horse.ApplySpeedBoost(2f, 5f);  // 2× for 5 sec
                break;

            case AppleType.GoldHeart:
                GameManager.Instance.GainLife(1);
                break;
            case AppleType.BadApple:
                GameManager.Instance.AddScore(-20); // subtract 20 points
                break;
        }

        Destroy(gameObject);
    }
}
