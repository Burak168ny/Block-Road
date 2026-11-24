// DeathManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathManager : MonoBehaviour
{
    [Header("Oyuncu Tag Bilgisi")]
    private string currentTag; // Oyuncunun güncel tag'i (TransformManager'dan güncellenecek)

    // TransformManager buradan çağıracak
    public void UpdateTag(string newTag)
    {
        currentTag = newTag;
        Debug.Log($"DeathManager → Güncel Tag: {currentTag}");
    }

    void OnCollisionEnter(Collision collision)
    {
        CheckDeath(collision.gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        CheckDeath(other.gameObject);
    }

    void OnCollisionStay(Collision collision)
    {
        CheckDeath(collision.gameObject);
    }

    void OnTriggerStay(Collider other)
    {
        CheckDeath(other.gameObject);
    }

    void CheckDeath(GameObject otherObj)
    {
        string otherTag = otherObj.tag;

        // Eğer aynı tag → hiçbir şey yapma
        if (otherTag == currentTag) return;

        // Eğer element taglerinden biri veya Parkour ise → öl
        if (IsElementalTag(otherTag) || otherTag == "Parkour")
        {
            Die();
        }
    }

    bool IsElementalTag(string tag)
    {
        return tag == "Fire" || tag == "Water" || tag == "Earth" || tag == "Green";
    }

    void Die()
    {
        Debug.Log("Farklı element veya Parkour objesi ile çarpışıldı! Oyun yeniden başlatılıyor...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
