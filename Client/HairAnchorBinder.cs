using UnityEngine;

public class HairAnchorBinder : MonoBehaviour
{
    [SerializeField] private Transform hairAnchor;
    public Transform HairAnchor => hairAnchor;

    private void Reset()
    {
        if (hairAnchor == null)
            hairAnchor = transform;
    }
}