using UnityEngine;
using System.Collections;

public class ChildSpawner : MonoBehaviour
{
    [Header("Spawn Ayarları")]
    public Transform spawnPoint;        // Buradan spawnlanacak
    public GameObject prefabToSpawn;    // Spawn edilecek prefab
    public float spawnInterval = 2f;    // Kaç saniyede bir spawn olacak

    [Header("Rastgele Materyaller & Tagler")]
    public Material[] materials;        // 4 materyal buraya
    public string[] tags;               // Her materyale karşılık gelen tag

    private Coroutine spawnRoutine;

    // --- İlk seçilen değerler ---
    private Material chosenMat;
    private string chosenTag;
    private bool initialized = false;

    private void Start()
    {
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            Spawn();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    [ContextMenu("Spawn Now")]
    public void Spawn()
    {
        if (prefabToSpawn == null || spawnPoint == null || materials.Length == 0 || tags.Length == 0)
        {
            Debug.LogWarning("[ChildSpawner] Eksik ayar var!");
            return;
        }

        if (materials.Length != tags.Length)
        {
            Debug.LogWarning("[ChildSpawner] Materyal ve tag sayısı eşit olmalı!");
            return;
        }

        // İlk spawn’da rastgele seç, sonra sabit kalsın
        if (!initialized)
        {
            int randIndex = Random.Range(0, materials.Length);
            chosenMat = materials[randIndex];
            chosenTag = tags[randIndex];
            initialized = true;
        }

        // Spawn işlemi
        GameObject obj = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);

        // Materyali uygula
        Renderer rend = obj.GetComponent<Renderer>();
        if (rend != null)
            rend.material = chosenMat;

        // Tag ata
        obj.tag = chosenTag;

        Debug.Log($"Spawn: {obj.name} → Material: {chosenMat.name}, Tag: {chosenTag}");
    }
}
