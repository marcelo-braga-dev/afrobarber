using System.Collections.Generic;
using UnityEngine;

public class MobileNpcLimiter : MonoBehaviour
{
    [Header("Alvos")]
    [SerializeField] private List<GameObject> npcRoots = new List<GameObject>();

    [Header("Limites")]
    [SerializeField] private int maxActiveNpcs = 5;

    [Header("Debug")]
    [SerializeField] private bool verboseLogs;

    private void OnEnable()
    {
        ApplyLimit();
    }

    [ContextMenu("ApplyLimit")]
    public void ApplyLimit()
    {
        int activeCount = 0;

        for (int i = 0; i < npcRoots.Count; i++)
        {
            GameObject npc = npcRoots[i];
            if (npc == null)
                continue;

            bool shouldBeActive = activeCount < maxActiveNpcs;
            npc.SetActive(shouldBeActive);

            if (shouldBeActive)
                activeCount++;
        }

        if (verboseLogs)
            Debug.Log($"[MobileNpcLimiter] NPCs ativos após limite: {activeCount}/{npcRoots.Count}");
    }
}
