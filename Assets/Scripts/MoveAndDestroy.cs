using UnityEngine;

public class MoveAndDestroy : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float speed = 5f;          // -Z yönünde hız
    public float lifeTime = 30f;      // Kaç saniye sonra yok olacak

    private void Start()
    {
        // Belirtilen süre sonunda yok et
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        // Global -Z yönünde sabit hızda hareket
        transform.position += Vector3.back * speed * Time.fixedDeltaTime;
    }
}
