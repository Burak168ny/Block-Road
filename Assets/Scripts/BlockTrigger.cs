using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BlockTrigger : MonoBehaviour
{
    [HideInInspector] public BlockRoller parentRoller;
    [HideInInspector] public int gridTouchCount = 0;

    private HashSet<GameObject> verticals = new HashSet<GameObject>();

    public bool IsOnGrid => gridTouchCount > 0;
    public bool IsOnVertical => verticals.Count > 0;

    void Awake()
    {
        if (parentRoller == null)
            parentRoller = GetComponentInParent<BlockRoller>();
    }

    public IEnumerable<GameObject> GetVerticals()
    {
        return verticals;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Grid"))
        {
            gridTouchCount++;
        }
        else if (other.CompareTag("Vertical"))
        {
            GameObject v = other.gameObject;
            if (verticals.Add(v))
            {
                parentRoller?.RegisterTriggerOnVertical(this, v);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Grid"))
        {
            gridTouchCount = Mathf.Max(0, gridTouchCount - 1);
        }
        else if (other.CompareTag("Vertical"))
        {
            GameObject v = other.gameObject;
            if (verticals.Remove(v))
            {
                parentRoller?.UnregisterTriggerOnVertical(this, v);
            }
        }
    }
}
