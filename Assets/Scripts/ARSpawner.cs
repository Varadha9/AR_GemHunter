using System.Collections.Generic;
using UnityEngine;

public class ARSpawner : MonoBehaviour
{
    [Header("AR Camera")]
    [SerializeField] private Camera arCamera;

    [Header("Prefab & Pooling")]
    [SerializeField] private ARCollectible collectiblePrefab;
    [SerializeField] private int poolSize = 10;
    [SerializeField] private int maxActiveObjects = 5;

    [Header("Spawn Area (In Front of Camera)")]
    [SerializeField] private float minSpawnDistance = 1.0f;
    [SerializeField] private float maxSpawnDistance = 2.2f;
    [SerializeField] private float minHeightOffset = -0.2f;
    [SerializeField] private float maxHeightOffset = 0.4f;

    [Header("Spawn Timing")]
    [SerializeField] private float spawnInterval = 1.2f;

    [Header("Raycast Settings")]
    [SerializeField] private LayerMask collectibleLayer;

    private Queue<ARCollectible> objectPool = new Queue<ARCollectible>();
    private List<ARCollectible> activeObjects = new List<ARCollectible>();
    private float spawnTimer = 0f;
    private bool isSpawning = false;

    private void Awake()
    {
        if (arCamera == null)
        {
            arCamera = Camera.main ?? FindObjectOfType<Camera>();
        }

        // Auto-assign layer if unassigned or 0
        if (collectibleLayer.value == 0)
        {
            int layer = LayerMask.NameToLayer("Collectible");
            if (layer >= 0) collectibleLayer = 1 << layer;
            else collectibleLayer = ~0;
        }

        InitializePool();
    }

    private void InitializePool()
    {
        if (collectiblePrefab == null)
        {
            Debug.LogError("ARSpawner: Collectible Prefab is not assigned!");
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            ARCollectible obj = Instantiate(collectiblePrefab, transform);
            obj.Initialize(this);
            obj.gameObject.SetActive(false);
            objectPool.Enqueue(obj);
        }
    }

    public void StartSpawning()
    {
        isSpawning = true;
        spawnTimer = 0f;

        for (int i = 0; i < maxActiveObjects; i++)
        {
            SpawnObject();
        }
    }

    public void StopSpawning()
    {
        isSpawning = false;

        for (int i = activeObjects.Count - 1; i >= 0; i--)
        {
            ReturnToPool(activeObjects[i]);
        }
    }

    private void Update()
    {
        HandleInput();

        if (!isSpawning) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval && activeObjects.Count < maxActiveObjects)
        {
            spawnTimer = 0f;
            SpawnObject();
        }
    }

    private void SpawnObject()
    {
        if (objectPool.Count == 0 || arCamera == null) return;

        ARCollectible obj = objectPool.Dequeue();

        float distance = Random.Range(minSpawnDistance, maxSpawnDistance);
        Vector3 forward = arCamera.transform.forward;
        Vector3 right = arCamera.transform.right;
        Vector3 up = arCamera.transform.up;

        Vector3 spawnOffset = (forward * distance) 
                            + (right * Random.Range(-1.0f, 1.0f))
                            + (up * Random.Range(minHeightOffset, maxHeightOffset));

        obj.transform.position = arCamera.transform.position + spawnOffset;
        obj.gameObject.SetActive(true);
        activeObjects.Add(obj);
    }

    public void ReturnToPool(ARCollectible obj)
    {
        obj.gameObject.SetActive(false);
        activeObjects.Remove(obj);
        objectPool.Enqueue(obj);
    }

    private void HandleInput()
    {
        // 1. Touch Input (Mobile)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                PerformRaycast(touch.position);
            }
        }

        // 2. Mouse Click (Unity Editor Testing)
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            PerformRaycast(Input.mousePosition);
        }
#endif
    }

    private void PerformRaycast(Vector2 screenPosition)
    {
        if (arCamera == null) arCamera = Camera.main;
        if (arCamera == null) return;

        Ray ray = arCamera.ScreenPointToRay(screenPosition);

        // Try hitting via layer mask first
        if (Physics.Raycast(ray, out RaycastHit hit, 50f, collectibleLayer))
        {
            if (hit.collider.TryGetComponent(out ARCollectible collectible))
            {
                collectible.Collect();
                return;
            }
        }

        // Fallback: hit any collider in front
        if (Physics.Raycast(ray, out hit, 50f))
        {
            if (hit.collider.TryGetComponent(out ARCollectible collectible))
            {
                collectible.Collect();
            }
        }
    }
}
