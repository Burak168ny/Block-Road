using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject targetObject;

    // Normal restart (ölüm olduğunda çağır)
    public void RestartScene()
    {
        // restart yapılırken "LastSection" silinmez → aynı bölümden devam eder
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Toggle butonu için
    public void ToggleTargetObject()
    {
        if (targetObject != null)
            targetObject.SetActive(!targetObject.activeSelf);
    }
}
