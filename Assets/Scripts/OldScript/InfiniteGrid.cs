using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ObstacleSpawnData
{
    public GameObject prefab; // hangi prefab
    public int count;         // kaç tane çıkacak
    public Vector3 offset;    // ekstra konum offseti (örn: (0,1,0))
}

[System.Serializable]
public class LevelData
{
    public string levelName;
    public int totalBlocks = 50;
    public float blockInterval = 1f;
    public int highlightCount = 10;

    [Header("Boşluk Ayarları")]
    public int emptyBlockCount = 0;  // toplam boşluk adedi
    public List<ObstacleSpawnData> obstaclePrefabs = new List<ObstacleSpawnData>();
}

public class InfiniteGrid : MonoBehaviour
{
    [Header("Grid Ayarları")]
    public GameObject gridPrefab;
    public int gridSizeX = 10;
    public int gridSizeZ = 100;
    public float cellSize = 1f;

    [Header("Level Ayarları")]
    public List<LevelData> levels = new List<LevelData>();
    private int currentLevelIndex = 0;
    private int movedBlockCount = 0;
    private float timer = 0f;

    private GameObject[,] grid;
    private int frontRow = 0;

    private float currentInterval;
    private int currentLevelTotalBlocks;
    private int highlightCount;

    private Dictionary<Vector2Int, ObstacleSpawnData> prefabTypeLookup = new Dictionary<Vector2Int, ObstacleSpawnData>();
    private HashSet<Vector2Int> emptyBlocks = new HashSet<Vector2Int>();

    void Start()
    {
        grid = new GameObject[gridSizeX, gridSizeZ];

        for (int z = 0; z < gridSizeZ; z++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                Vector3 pos = new Vector3(x * cellSize, 0, z * cellSize);
                GameObject cell = Instantiate(gridPrefab, pos, Quaternion.identity, transform);
                grid[x, z] = cell;
            }
        }

        LoadLevel(0);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= currentInterval)
        {
            MoveRowToFront();
            timer = 0f;
        }
    }

    void LoadLevel(int index)
    {
        if (index >= levels.Count)
        {
            Debug.Log("Tüm leveller bitti!");
            return;
        }

        currentLevelIndex = index;
        movedBlockCount = 0;

        LevelData level = levels[index];
        currentInterval = level.blockInterval;
        currentLevelTotalBlocks = level.totalBlocks;
        highlightCount = level.highlightCount;

        // --- Boş blokları seç ---
        emptyBlocks.Clear();
        prefabTypeLookup.Clear();

        List<ObstacleSpawnData> prefabPool = new List<ObstacleSpawnData>();
        foreach (var data in level.obstaclePrefabs)
        {
            for (int i = 0; i < data.count; i++)
                prefabPool.Add(data);
        }

        int attempts = 0;
        while (emptyBlocks.Count < level.emptyBlockCount && attempts < level.emptyBlockCount * 10 && prefabPool.Count > 0)
        {
            int rndRow = Random.Range(0, currentLevelTotalBlocks - highlightCount);
            int rndCol = Random.Range(0, gridSizeX);
            Vector2Int id = new Vector2Int(rndCol, rndRow);

            if (!emptyBlocks.Contains(id))
            {
                emptyBlocks.Add(id);

                // Prefab havuzundan rastgele seç
                int rndIndex = Random.Range(0, prefabPool.Count);
                prefabTypeLookup[id] = prefabPool[rndIndex];
                prefabPool.RemoveAt(rndIndex);
            }
            attempts++;
        }

        Debug.Log($"Level başladı: {level.levelName} | Boşluk sayısı: {emptyBlocks.Count}");
    }

    void MoveRowToFront()
    {
        int backRow = frontRow % gridSizeZ;
        float newZ = (frontRow + gridSizeZ) * cellSize;

        bool isHighlight = (currentLevelTotalBlocks - movedBlockCount) <= highlightCount;

        for (int x = 0; x < gridSizeX; x++)
        {
            GameObject cell = grid[x, backRow];
            Vector3 pos = cell.transform.position;
            pos.z = newZ;
            cell.transform.position = pos;

            Renderer rend = cell.GetComponent<Renderer>();
            Vector2Int blockID = new Vector2Int(x, movedBlockCount);

            if (rend != null)
            {
                if (!isHighlight && emptyBlocks.Contains(blockID))
                {
                    // Hücreyi gizle
                    cell.SetActive(false);

                    // Prefab spawnla ama bir daha yok etme
                    if (prefabTypeLookup.ContainsKey(blockID))
                    {
                        var data = prefabTypeLookup[blockID];
                        if (data.prefab != null)
                        {
                            Vector3 spawnPos = pos + data.offset; // offset uygulandı
                            GameObject obj = Instantiate(data.prefab, spawnPos, Quaternion.identity);
                            Destroy(obj, 480f); // 120 saniye = 2 dakika sonra yok et
                        }
                    }
                }
                else
                {
                    cell.SetActive(true);
                    if (isHighlight)
                        rend.material.color = Color.blue;
                }
            }
        }

        movedBlockCount++;
        frontRow++;

        if (movedBlockCount >= currentLevelTotalBlocks)
        {
            Debug.Log("Level bitti: " + levels[currentLevelIndex].levelName);
            LoadLevel(currentLevelIndex + 1);
        }
    }
}
