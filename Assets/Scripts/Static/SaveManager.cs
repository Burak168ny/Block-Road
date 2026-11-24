using UnityEngine;

public static class SaveManager
{
    private const string SaveKey = "GameSave";

    [System.Serializable]
    class SaveData
    {
        public float[] position; // x,y,z
        public float[] rotation; // x,y,z,w
        public float[] scale;    // x,y,z
        public int currentZ;
        public int nextSectionIndex;
    }

    public static void Save(GameObject player, int currentZ, int nextSectionIndex)
    {
        var t = player.transform;

        SaveData data = new SaveData
        {
            position = new float[]
            {
                t.position.x,
                t.position.y,
                Mathf.FloorToInt(t.position.z / 100f) * 100 // Z yuvarlanıyor
            },
            rotation = new float[]
            {
                t.rotation.x, t.rotation.y, t.rotation.z, t.rotation.w
            },
            scale = new float[]
            {
                t.localScale.x, t.localScale.y, t.localScale.z
            },
            currentZ = Mathf.FloorToInt(currentZ / 100f) * 100,
            nextSectionIndex = nextSectionIndex
        };

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
        Debug.Log("Game Saved: " + json);
    }

    public static bool Load(out Vector3 pos, out Quaternion rot, out Vector3 scale, out int currentZ, out int nextSectionIndex)
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            pos = Vector3.zero; rot = Quaternion.identity; scale = Vector3.one;
            currentZ = 0; nextSectionIndex = 0;
            return false;
        }

        string json = PlayerPrefs.GetString(SaveKey);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        pos = new Vector3(data.position[0], data.position[1], data.position[2]);
        rot = new Quaternion(data.rotation[0], data.rotation[1], data.rotation[2], data.rotation[3]);
        scale = new Vector3(data.scale[0], data.scale[1], data.scale[2]);
        currentZ = data.currentZ;
        nextSectionIndex = data.nextSectionIndex;

        Debug.Log("Game Loaded: " + json);
        return true;
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(SaveKey);
    }
}
