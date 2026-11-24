using UnityEngine;

public class NextLevelTrigger : MonoBehaviour
{
    private LevelSpawner spawner;

    void Start()
    {
        spawner = FindObjectOfType<LevelSpawner>();
    }

void OnTriggerEnter(Collider other)
{
    Debug.Log("Triggered by: " + other.gameObject.name + " | Tag: " + other.tag);

    if (other.CompareTag("NextLevel"))
    {
        spawner.AdvanceSection();
        Debug.Log("Destroying: " + other.gameObject.name);
        Destroy(other.gameObject);
    }
}

}
