using UnityEngine;
using UnityEngine.UI;

public class TransformManager : MonoBehaviour
{
    [Header("Referanslar")]
    public GameObject targetObject;   // Material atanacak obje
    public FixedJoystick joystick;    // Inspector’dan bağla

    [Header("UI Göstergeler")]
    public Image[] fillImages;        // Her formun barı (Inspector’dan bağla)
    public Button revertButton;       // Normale dön butonu (Inspector’dan bağla)

    [Header("Normal Form (Başlangıç Hali)")]
    public Material normalMaterial;   // Normal hal materyali
    public string normalTag = "Normal"; // Normal hal tagi

    [Header("Elementler (Sağ, Sol, Yukarı, Aşağı)")]
    public Material[] materials;      // 4 materyal
    public string[] tags;             // 4 tag

    [Header("Ayarlar")]
    public float elementalDuration = 10f; 
    public float deadZone = 0.2f;     // merkeze yakınsa işlem yapma

    private float[] currentTimes;      
    private bool[] isActive;           
    private int currentIndex = -1;    // aktif form index
    private bool isNormal = true;     // normalde mi?

    // --- JOYSTICK DURUMU ---
    public static bool JoystickInUse = false;

    private void Start()
    {
        currentTimes = new float[materials.Length];
        isActive = new bool[materials.Length];

        for (int i = 0; i < materials.Length; i++)
        {
            currentTimes[i] = elementalDuration;
            if (fillImages[i] != null) fillImages[i].fillAmount = 1f;
        }

        // Başlangıçta normal forma dön
        ApplyNormal();

        // Buton eventi bağla
        if (revertButton != null)
            revertButton.onClick.AddListener(ApplyNormal);
    }

    private void Update()
    {
        // --- Joystick kontrolü ---
        Vector2 input = new Vector2(joystick.Horizontal, joystick.Vertical);

        if (input.magnitude < deadZone)
        {
            JoystickInUse = false; // joystick merkezde → hiçbir şey yapma
        }
        else
        {
            JoystickInUse = true; // joystick hareket halinde

            float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;

            // açı bölgeleri
            if (angle >= 45f && angle < 135f)      ApplyOption(2); // Yukarı
            else if (angle >= 135f && angle < 225f) ApplyOption(3); // Sol
            else if (angle >= 225f && angle < 315f) ApplyOption(1); // Aşağı
            else                                    ApplyOption(0); // Sağ
        }

        // --- Timer güncelle ---
        for (int i = 0; i < materials.Length; i++)
        {
            if (isActive[i])
            {
                currentTimes[i] -= Time.deltaTime;
                if (currentTimes[i] <= 0)
                {
                    // süre bitince otomatik normal forma dön
                    ApplyNormal();
                }
            }
            else
            {
                currentTimes[i] += Time.deltaTime;
                if (currentTimes[i] > elementalDuration)
                    currentTimes[i] = elementalDuration;
            }

            if (fillImages[i] != null)
                fillImages[i].fillAmount = currentTimes[i] / elementalDuration;
        }
    }

    private void ApplyOption(int index)
    {
        if (index == currentIndex && !isNormal) return; // aynı formsa tekrar yapma
        isNormal = false;
        currentIndex = index;

        for (int i = 0; i < isActive.Length; i++)
            isActive[i] = false;
        isActive[index] = true;

        // Material ata
        if (materials != null && index < materials.Length)
        {
            Renderer rend = targetObject.GetComponent<Renderer>();
            if (rend != null) rend.material = materials[index];
        }

        // Tag ata
        if (tags != null && index < tags.Length)
        {
            targetObject.tag = tags[index];

            DeathManager dm = targetObject.GetComponent<DeathManager>();
            if (dm != null) dm.UpdateTag(tags[index]);
        }

        Debug.Log($"Joystick → {index}. form: {materials[index]?.name}, Tag: {tags[index]}");
    }

    private void ApplyNormal()
    {
        isNormal = true;
        currentIndex = -1;

        for (int i = 0; i < isActive.Length; i++)
            isActive[i] = false;

        // Normal materyal ata
        Renderer rend = targetObject.GetComponent<Renderer>();
        if (rend != null && normalMaterial != null)
            rend.material = normalMaterial;

        // Normal tag ata
        targetObject.tag = normalTag;

        DeathManager dm = targetObject.GetComponent<DeathManager>();
        if (dm != null) dm.UpdateTag(normalTag);

        Debug.Log("Normal forma dönüldü.");
    }
}
