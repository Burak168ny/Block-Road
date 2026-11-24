using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockRoller : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float rotateTimeHorizontal = 0.3f;
    public float rotateTimeVertical = 0.3f;
    public float holdIntervalHorizontal = 0.3f;
    public float holdIntervalVertical = 0.3f;

    [Header("Triggers")]
    public List<BlockTrigger> triggers = new List<BlockTrigger>();
    private bool isRotating = false;

    [Header("Joystick Ayarları")]
    public FloatingJoystick joystick;
    public float joystickDeadZone = 0.2f;

    [Header("Basılı Tutma Sistemi")]
    private bool isHolding = false;
    private Vector3 holdDir = Vector3.zero;
    private float holdTimer = 0f;

    [Header("Vertical Ayarları")]
    public int verticalRequiredTriggers = 4;

    private Dictionary<GameObject, HashSet<BlockTrigger>> verticalContacts = new Dictionary<GameObject, HashSet<BlockTrigger>>();

    void Awake()
    {
        if (triggers == null || triggers.Count == 0)
        {
            triggers = new List<BlockTrigger>(GetComponentsInChildren<BlockTrigger>());
        }

        foreach (var t in triggers)
        {
            if (t.parentRoller == null) t.parentRoller = this;
        }
    }

    void Update()
    {
        if (isRotating) return;

        HandleJoystick();
        HandleKeyboard();

        if (isHolding && holdDir != Vector3.zero)
        {
            holdTimer += Time.deltaTime;

            float interval = (holdDir == Vector3.left || holdDir == Vector3.right)
                ? holdIntervalHorizontal
                : holdIntervalVertical;

            if (holdTimer >= interval)
            {
                holdTimer = 0f;
                TryRotate(holdDir);
            }
        }
    }

    void HandleJoystick()
    {
        if (joystick == null) return;

        Vector2 input = new Vector2(joystick.Horizontal, joystick.Vertical);

        if (input.magnitude < joystickDeadZone)
        {
            StopHold();
            return;
        }

        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            StartHold((input.x > 0) ? Vector3.right : Vector3.left);
        else
            StartHold((input.y > 0) ? Vector3.forward : Vector3.back);
    }

    void HandleKeyboard()
    {
        if (Input.GetKey(KeyCode.W)) { StartHold(Vector3.forward); }
        else if (Input.GetKey(KeyCode.S)) { StartHold(Vector3.back); }
        else if (Input.GetKey(KeyCode.A)) { StartHold(Vector3.left); }
        else if (Input.GetKey(KeyCode.D)) { StartHold(Vector3.right); }
        else if (!Input.anyKey)
        {
            StopHold();
        }
    }

    void StartHold(Vector3 dir)
    {
        isHolding = true;
        holdDir = dir;

        holdTimer = (dir == Vector3.left || dir == Vector3.right)
            ? holdIntervalHorizontal
            : holdIntervalVertical;
    }

    void StopHold()
    {
        isHolding = false;
        holdDir = Vector3.zero;
    }

    void TryRotate(Vector3 dir)
    {
        List<BlockTrigger> grounded = new List<BlockTrigger>();
        foreach (var t in triggers)
        {
            if (t.IsOnGrid) grounded.Add(t);
        }

        if (grounded.Count < 4) return;

        BlockTrigger pivotTrigger = null;
        float maxDot = -Mathf.Infinity;
        foreach (var t in grounded)
        {
            Vector3 toTrigger = (t.transform.position - transform.position).normalized;
            float d = Vector3.Dot(dir.normalized, toTrigger);
            if (d > maxDot)
            {
                maxDot = d;
                pivotTrigger = t;
            }
        }

        if (pivotTrigger != null)
        {
            float time = (dir == Vector3.left || dir == Vector3.right)
                ? rotateTimeHorizontal
                : rotateTimeVertical;

            StartCoroutine(RollAround(pivotTrigger.transform.position, dir, time));
        }
    }

    IEnumerator RollAround(Vector3 pivot, Vector3 dir, float rotateTime)
    {
        isRotating = true;

        Vector3 axis = Vector3.Cross(Vector3.up, dir);
        float angle = 0f;

        while (angle < 90f)
        {
            float step = (Time.deltaTime / rotateTime) * 90f;
            if (angle + step > 90f) step = 90f - angle;

            transform.RotateAround(pivot, axis, step);
            angle += step;
            yield return null;
        }

        yield return new WaitForSeconds(rotateTime * 0.05f);
        yield return new WaitForFixedUpdate();
        Physics.SyncTransforms();

        RebuildVerticalCounts();

        isRotating = false;

        int groundedCount = 0;
        foreach (var t in triggers)
        {
            if (t.IsOnGrid) groundedCount++;
        }

        if (groundedCount < 4)
        {
            Vector3? altDir = GetFallDirectionIfHalfSupported(dir);
            if (altDir.HasValue)
            {
                isRotating = true;
                StartCoroutine(FallDown(altDir.Value, rotateTime));
            }
            else
            {
                isRotating = true;
                StartCoroutine(FallDown(dir, rotateTime));
            }
        }
    }

    IEnumerator VerticalFall()
    {
        isRotating = true;

        while (true)
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            yield return null;

            if (transform.position.y < -10f)
            {
                FindObjectOfType<MenuManager>()?.RestartScene();
                Destroy(gameObject);
                yield break;
            }
        }
    }

    [Header("Fall Ayarları")]
    public float fallSpeed = 8f;
    public float fallSpinSpeed = 90f;
    public float forwardFallForce = 3f;
    public bool invertFallSpin = false;
    public float initialTumbleTime = 0.2f;

    IEnumerator FallDown(Vector3 dir, float rotateTime)
    {
        isRotating = true;

        float contactHeight = GetGroundContactHeight();
        Vector3 axisPivot = Vector3.Cross(Vector3.up, dir).normalized;
        Vector3 pivot = transform.position + (dir * 0.5f) + (Vector3.down * contactHeight);

        float t = 0f;
        float prevAngle = 0f;
        float targetAngleTotal = 60f;

        while (t < 1f)
        {
            t += Time.deltaTime / initialTumbleTime;
            float eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
            float targetAngle = eased * targetAngleTotal;
            float delta = targetAngle - prevAngle;

            transform.RotateAround(pivot, axisPivot, delta);
            transform.position += Vector3.down * delta * 0.005f;

            prevAngle = targetAngle;
            yield return null;
        }

        Vector3 spinAxis = Vector3.Cross(Vector3.up, dir).normalized;
        if (invertFallSpin) spinAxis = -spinAxis;

        Vector3 extraPush = dir.normalized * forwardFallForce;
        float spinSpeed = fallSpinSpeed;

        while (true)
        {
            transform.position += (Vector3.down * fallSpeed + extraPush) * Time.deltaTime;
            transform.Rotate(spinAxis, spinSpeed * Time.deltaTime, Space.World);

            yield return null;

            if (transform.position.y < -10f)
            {
                FindObjectOfType<MenuManager>()?.RestartScene();
                Destroy(gameObject);
                yield break;
            }
        }
    }

    float GetGroundContactHeight()
    {
        int groundCount = 0;
        foreach (var t in triggers)
        {
            if (t.IsOnGrid) groundCount++;
        }

        return (groundCount >= 2) ? 1f : 0.5f;
    }

    Vector3? GetFallDirectionIfHalfSupported(Vector3 intendedDir)
    {
        List<BlockTrigger> grounded = new List<BlockTrigger>();
        List<BlockTrigger> ungrounded = new List<BlockTrigger>();

        foreach (var t in triggers)
        {
            if (t.IsOnGrid) grounded.Add(t);
            else ungrounded.Add(t);
        }

        if (grounded.Count > 0 && ungrounded.Count > 0)
        {
            Vector3 avgEmpty = Vector3.zero;
            foreach (var t in ungrounded)
                avgEmpty += t.transform.position;
            avgEmpty /= ungrounded.Count;

            Vector3 dir = (avgEmpty - transform.position).normalized;
            dir.y = 0;
            return dir;
        }

        return null;
    }

    public void RegisterTriggerOnVertical(BlockTrigger trigger, GameObject vertical)
    {
        if (vertical == null || trigger == null) return;

        if (!verticalContacts.TryGetValue(vertical, out var set))
        {
            set = new HashSet<BlockTrigger>();
            verticalContacts[vertical] = set;
        }

        set.Add(trigger);

        if (set.Count >= verticalRequiredTriggers)
        {
            OnVerticalReachedThreshold(vertical);
        }
    }

    public void UnregisterTriggerOnVertical(BlockTrigger trigger, GameObject vertical)
    {
        if (vertical == null || trigger == null) return;

        if (verticalContacts.TryGetValue(vertical, out var set))
        {
            set.Remove(trigger);

            if (set.Count == 0)
            {
                verticalContacts.Remove(vertical);
            }
        }
    }

    void RebuildVerticalCounts()
    {
        verticalContacts.Clear();

        foreach (var t in triggers)
        {
            foreach (var v in t.GetVerticals())
            {
                if (v == null) continue;
                if (!verticalContacts.TryGetValue(v, out var set))
                {
                    set = new HashSet<BlockTrigger>();
                    verticalContacts[v] = set;
                }
                set.Add(t);
            }
        }

        var toCheck = new List<GameObject>(verticalContacts.Keys);
        foreach (var v in toCheck)
        {
            if (verticalContacts.TryGetValue(v, out var set) && set.Count >= verticalRequiredTriggers)
            {
                OnVerticalReachedThreshold(v);
            }
        }
    }

    void OnVerticalReachedThreshold(GameObject vertical)
    {
        if (vertical == null) return;

        Destroy(vertical);
        verticalContacts.Remove(vertical);
        StartCoroutine(VerticalFall());
    }
}
