// CylinderMover.cs
using UnityEngine;

public class CylinderMover : MonoBehaviour
{
    public enum RotationAxis { X, Y, Z }

    [Header("Hareket Seçenekleri")]
    public bool vertical = true;     // Z ekseninde ileri-geri hareket
    public bool horizontal = false;  // X ekseninde ileri-geri hareket
    public bool turn = false;        // Yerinde dönme

    [Header("Hareket Ayarları")]
    public float moveDistance = 10f;   // ileri-geri mesafe
    public float moveSpeed = 2f;       // hareket/dönüş hızı

    [Header("Dönüş Seçenekleri")]
    public RotationAxis rotationAxis = RotationAxis.Y; // Inspector’dan seç

    private Vector3 startPos;  // cylinder'ın başlangıç pozisyonu
    private float lastPos;     // bir önceki frame pozisyonu
    private float radius;      // cylinder radius
    private float startTime;   // sahne yüklenme zamanı

    void Start()
    {
        startPos = transform.position;  // bulunduğu pozisyonu başlangıç al
        radius = transform.localScale.x * 0.5f; // X scale yarısı radius
        startTime = Time.time;

        lastPos = vertical ? startPos.z : startPos.x; // başlangıç pozisyonu
    }

    void Update()
    {
        float elapsed = Time.time - startTime;

        if (turn)
        {
            // Yerinde dönme
            float deltaAngle = moveSpeed * Time.deltaTime * 60f; // hız frame-rate bağımsız
            Rotate(deltaAngle);
            return; // diğer hareketleri yapma
        }

        if (vertical)
        {
            float offset = Mathf.PingPong(elapsed * moveSpeed, moveDistance * 2) - moveDistance;
            Vector3 newPos = startPos + new Vector3(0, 0, offset);

            float delta = newPos.z - lastPos;
            float anglePerUnit = 360f / (2 * Mathf.PI * radius);
            float deltaAngle = -delta * anglePerUnit;

            Rotate(deltaAngle);

            transform.position = newPos;
            lastPos = newPos.z;
        }
        else if (horizontal)
        {
            float offset = Mathf.PingPong(elapsed * moveSpeed, moveDistance * 2) - moveDistance;
            Vector3 newPos = startPos + new Vector3(offset, 0, 0);

            float delta = newPos.x - lastPos;
            float anglePerUnit = 360f / (2 * Mathf.PI * radius);
            float deltaAngle = delta * anglePerUnit;

            Rotate(deltaAngle);

            transform.position = newPos;
            lastPos = newPos.x;
        }
    }

    void Rotate(float deltaAngle)
    {
        // Inspector'dan seçilen eksene göre dön
        switch (rotationAxis)
        {
            case RotationAxis.X:
                transform.Rotate(Vector3.right, deltaAngle, Space.Self);
                break;
            case RotationAxis.Y:
                transform.Rotate(Vector3.up, deltaAngle, Space.Self);
                break;
            case RotationAxis.Z:
                transform.Rotate(Vector3.forward, deltaAngle, Space.Self);
                break;
        }
    }
}
