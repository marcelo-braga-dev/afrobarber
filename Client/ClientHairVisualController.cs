using System.Collections.Generic;
using UnityEngine;

public class ClientHairVisualController : MonoBehaviour
{
    [System.Serializable]
    public class HairVariantEntry
    {
        public string hairId;
        public GameObject hairObject;
    }

    [Header("Lista de cabelos disponíveis neste cliente")]
    [SerializeField] private List<HairVariantEntry> hairVariants = new List<HairVariantEntry>();

    [Header("Estado atual")]
    [SerializeField] private string currentHairId;

    public string CurrentHairId => currentHairId;

    public void ApplyHairById(string hairId)
    {
        if (string.IsNullOrWhiteSpace(hairId))
        {
            Debug.LogWarning($"[{name}] hairId inválido.");
            return;
        }

        bool found = false;

        foreach (var entry in hairVariants)
        {
            if (entry == null || entry.hairObject == null)
                continue;

            bool shouldBeActive = entry.hairId == hairId;
            entry.hairObject.SetActive(shouldBeActive);

            if (shouldBeActive)
                found = true;
        }

        if (!found)
        {
            Debug.LogWarning($"[{name}] Nenhum cabelo encontrado com hairId = {hairId}");
            return;
        }

        currentHairId = hairId;
    }

    public void ApplyBeforeHair(ClientServiceProfile profile)
    {
        if (profile == null)
            return;

        ApplyHairById(profile.beforeHairId);
    }

    public void ApplyAfterHair(ClientServiceProfile profile)
    {
        if (profile == null)
            return;

        ApplyHairById(profile.afterHairId);
    }

    [ContextMenu("Desativar Todos os Cabelos")]
    public void DisableAllHairs()
    {
        foreach (var entry in hairVariants)
        {
            if (entry == null || entry.hairObject == null)
                continue;

            entry.hairObject.SetActive(false);
        }

        currentHairId = string.Empty;
    }
}