using UnityEngine;

public class ARCollectible : MonoBehaviour
{
    [Header("Collectible Settings")]
    [SerializeField] private int pointValue = 10;
    [SerializeField] private float rotationSpeed = 60f;

    [Header("Optional Audio / FX")]
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private ParticleSystem collectEffect;

    private ARSpawner spawner;

    public void Initialize(ARSpawner parentSpawner)
    {
        spawner = parentSpawner;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }

#if UNITY_EDITOR
    // Native Unity Editor click support on 3D colliders
    private void OnMouseDown()
    {
        Collect();
    }
#endif

    public void Collect()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameActive())
        {
            GameManager.Instance.AddScore(pointValue);
        }

        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        if (collectEffect != null)
        {
            collectEffect.transform.position = transform.position;
            collectEffect.Play();
        }

        if (spawner != null)
        {
            spawner.ReturnToPool(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
