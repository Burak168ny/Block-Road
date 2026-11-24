using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [Header("Takip Ayarları")]
    public Transform target;   // Takip edilecek obje
    public float followSpeed = 5f; 
    public Vector3 offset = new Vector3(0, 10, -10); // Kameranın hedefe göre konumu

    [Header("Sabit Değerler")]
    public float fixedY = 10f;        // Kameranın sabit Y yüksekliği
    public Vector3 fixedRotation = new Vector3(45, 0, 0); // Sabit rotasyon (örn. eğik bakış)

    void LateUpdate()
    {
        if (target == null) return;

        // Hedefin X ve Z'sini takip et, Y sabit kalsın
        Vector3 desiredPosition = new Vector3(
            target.position.x + offset.x,
            fixedY,
            target.position.z + offset.z
        );

        // Yumuşak takip
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Kameranın rotasyonu sabit kalsın
        transform.rotation = Quaternion.Euler(fixedRotation);
    }
}
