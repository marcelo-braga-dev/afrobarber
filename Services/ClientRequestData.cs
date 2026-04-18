using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ClientRequestData
{
    [Header("Tabela de Preços")]
    public ServicePriceTable priceTable;
    public ServiceType serviceType = ServiceType.CorteDeCabelo;
    public string customServiceId;

    [Header("Recompensas")]
    public int xpReward = 10;

    [Header("Tempo")]
    public float serviceTimeMinutes = 10f;

    [Header("Visual do Cliente")]
    public string beforeHairId;
    public string afterHairId;    

    [Header("Identificação")]
    public string id;
    public string requestName;

    [TextArea]
    public string description;

    [Header("Visual")]
    public Sprite icon;

    [Header("Tipo")]
    public HaircutType haircutType;

    [Header("Gameplay")]
    public int servicePrice = 30;
    public int ServicePrice => servicePrice;

    [Tooltip("Duração do atendimento em minutos do relógio global.")]
    [Min(1f)]
    public float serviceTime = 8f;
    public float ServiceTime => serviceTime;

    public int ServiceTimeRoundedMinutes
    {
        get
        {
            return Mathf.Max(1, Mathf.RoundToInt(serviceTime));
        }
    }

    [Header("Dificuldade")]
    [Range(1, 5)]
    public int difficulty = 1;

    [Header("Itens necessários")]
    public List<ServiceRequirementData> requiredItems = new List<ServiceRequirementData>();

    [Header("Sistema educacional")]
    public string afroCutId;
    [TextArea(2, 5)]
    public string educationalTitle;
    [TextArea]
    public string educationalSummary;

    [Header("Compatibilidade com sistema novo")]
    public string requestId;
    public string requestTitle;

    [TextArea]
    public string requestDescription;

    public string RequestId
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(requestId))
                return requestId;

            return id;
        }
    }

    public string RequestName
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(requestTitle))
                return requestTitle;

            if (!string.IsNullOrWhiteSpace(requestName))
                return requestName;

            return "Atendimento";
        }
    }

    public string Description
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(requestDescription))
                return requestDescription;

            if (!string.IsNullOrWhiteSpace(description))
                return description;

            return requestName;
        }
    }

    public string GetDescription()
    {
        if (!string.IsNullOrWhiteSpace(requestDescription))
            return requestDescription;

        if (!string.IsNullOrWhiteSpace(description))
            return description;

        if (!string.IsNullOrWhiteSpace(requestName))
            return requestName;

        return "Atendimento sem descrição.";
    }

    public bool HasRequirements()
    {
        return requiredItems != null && requiredItems.Count > 0;
    }

    public bool HasEducationalContent()
    {
        return !string.IsNullOrWhiteSpace(afroCutId) ||
               !string.IsNullOrWhiteSpace(educationalTitle) ||
               !string.IsNullOrWhiteSpace(educationalSummary);
    }

    public void SyncCompatibilityFields()
    {
        if (string.IsNullOrWhiteSpace(requestId) && !string.IsNullOrWhiteSpace(id))
            requestId = id;

        if (string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(requestId))
            id = requestId;

        if (string.IsNullOrWhiteSpace(requestTitle) && !string.IsNullOrWhiteSpace(requestName))
            requestTitle = requestName;

        if (string.IsNullOrWhiteSpace(requestName) && !string.IsNullOrWhiteSpace(requestTitle))
            requestName = requestTitle;

        if (string.IsNullOrWhiteSpace(requestDescription) && !string.IsNullOrWhiteSpace(description))
            requestDescription = description;

        if (string.IsNullOrWhiteSpace(description) && !string.IsNullOrWhiteSpace(requestDescription))
            description = requestDescription;
    }


}