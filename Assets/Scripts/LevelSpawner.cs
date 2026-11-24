using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[System.Serializable]
public class Section
{
    public int id;
    public List<string> grid;
}

[System.Serializable]
public class SectionData
{
    public List<Section> Sections;
}

[System.Serializable]
public class PrefabOffset
{
    public GameObject prefab;
    public Vector3 offset = Vector3.zero;
    public Vector3 rotationEuler = Vector3.zero;
}

public class LevelSpawner : MonoBehaviour
{
    [Header("Prefab Listeleri")]
    public List<PrefabOffset> groundPrefabs;
    public List<PrefabOffset> enemyPrefabs;
    public List<PrefabOffset> endPrefabs;
    public List<PrefabOffset> specialPrefabs;
    public List<PrefabOffset> parkourPrefabs;

    [Header("Bölüm Ayarları")]
    public int overrideSectionIndex = -1;
    public int startSectionIndex = 0;

    [Header("Kaç bölüm sahnede olacak")]
    public int activeSectionCount = 3;

    private SectionData sectionData;
    private int currentZ = 0;
    private int nextSectionIndex = 0;

    private Queue<GameObject> activeSections = new Queue<GameObject>();

    void Start()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("levels");
        if (jsonFile == null)
        {
            Debug.LogError("JSON dosyası bulunamadı!");
            return;
        }
        sectionData = JsonUtility.FromJson<SectionData>(jsonFile.text);

        // 🔑 PlayerPrefs’ten kaldığı bölüm alınıyor
        if (PlayerPrefs.HasKey("LastSection"))
        {
            nextSectionIndex = PlayerPrefs.GetInt("LastSection");
        }
        else if (overrideSectionIndex != -1)
        {
            nextSectionIndex = overrideSectionIndex;
        }
        else
        {
            nextSectionIndex = startSectionIndex;
        }

        for (int i = 0; i < activeSectionCount; i++)
            SpawnNextSection();
    }

    public void SpawnNextSection()
    {
        if (sectionData == null || nextSectionIndex >= sectionData.Sections.Count)
            return;

        var section = sectionData.Sections[nextSectionIndex];
        GameObject parent = new GameObject("Section_" + section.id);

        for (int z = 0; z < section.grid.Count; z++)
        {
            string[] row = section.grid[z].Split(',');
            for (int x = 0; x < row.Length; x++)
            {
                string cell = row[x];
                if (string.IsNullOrEmpty(cell) || cell == ".") continue;

                string[] codes = cell.Split('|');
                foreach (var code in codes)
                {
                    if (string.IsNullOrEmpty(code) || code == ".") continue;

                    PrefabOffset prefabOffset = GetPrefabOffset(code);
                    if (prefabOffset != null && prefabOffset.prefab != null)
                    {
                        Vector3 basePos = new Vector3(x, 0, currentZ + z);
                        Quaternion rot = Quaternion.Euler(prefabOffset.rotationEuler);
                        Vector3 pos = basePos + rot * prefabOffset.offset;

                        Instantiate(prefabOffset.prefab, pos, rot, parent.transform);
                    }
                }
            }
        }

        activeSections.Enqueue(parent);
        currentZ += section.grid.Count;
        nextSectionIndex++;
    }

    public void AdvanceSection()
    {
        if (activeSections.Count > 0)
        {
            GameObject oldest = activeSections.Dequeue();
            Destroy(oldest);
        }
        SpawnNextSection();
    }

    private PrefabOffset GetPrefabOffset(string code)
    {
        if (code.StartsWith("G")) return groundPrefabs[int.Parse(code.Substring(1))];
        if (code.StartsWith("E")) return enemyPrefabs[int.Parse(code.Substring(1))];
        if (code.StartsWith("B")) return endPrefabs[int.Parse(code.Substring(1))];
        if (code.StartsWith("A")) return specialPrefabs[int.Parse(code.Substring(1))];
        if (code.StartsWith("P")) return parkourPrefabs[int.Parse(code.Substring(1))];
        return null;
    }

    ///////////////////
    // Menüden çağırılacak
    public void LoadSectionsFromButton(int sectionIndex)
    {
        // 🔑 Seçilen bölüm kaydediliyor
        PlayerPrefs.SetInt("LastSection", sectionIndex);
        PlayerPrefs.Save();

        // Sahneyi yeniden yükle
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    ///////////////////
}
