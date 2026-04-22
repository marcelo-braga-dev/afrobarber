using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ClientRequestData", menuName = "AfroBarber/Client/Request Data")]
public class ClientRequestData : ScriptableObject
{
    [Header("Identificação")]
    public string id;
    public string requestId;
    public string requestName;

    [Header("Apresentação")]
    public Sprite icon;

    [TextArea(2, 5)]
    public string description;

    [Header("Tipo de Corte / Serviço")]
    public HaircutType haircutType;
    public ServiceType serviceType = ServiceType.CorteDeCabelo;
    public string customServiceId;
    public List<ServiceType> additionalServiceTypes = new List<ServiceType>();

    [Header("Preço / Tempo / Dificuldade")]
    public int servicePrice = 30;
    public float serviceTime = 10f;
    public int difficulty = 1;

    [Header("Tabela de Preços")]
    public ServicePriceTable priceTable;

    [Header("Recompensas")]
    public int xpReward = 10;

    [Header("Itens necessários")]
    public List<ServiceRequirementData> requiredItems = new List<ServiceRequirementData>();

    [Header("Visual do Cliente")]
    public string beforeHairId;
    public string afterHairId;

    [Header("História")]
    public string afroCutId;

    [Tooltip("Título da história exibida na UI.")]
    public string historyTitle;

    [TextArea(4, 10)]
    [Tooltip("Resumo/história do serviço ou corte exibido na UI.")]
    public string historySummary;

    public string RequestId
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(requestId))
                return requestId;

            if (!string.IsNullOrWhiteSpace(id))
                return id;

            return name;
        }
    }

    public string RequestName
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(requestName))
                return requestName;

            if (!string.IsNullOrWhiteSpace(name))
                return name;

            return "Pedido";
        }
    }

    public int ServicePrice
    {
        get
        {
            if (GlobalGameplayManagement.Instance != null)
                return GlobalGameplayManagement.Instance.CalculateFinalPriceForRequest(this);

            if (priceTable != null)
                return priceTable.GetPrice(serviceType, customServiceId);

            return Mathf.Max(0, servicePrice);
        }
    }

    public float ServiceTime
    {
        get
        {
            return Mathf.Max(1f, serviceTime);
        }
    }

    public int ServiceTimeRoundedMinutes
    {
        get
        {
            return Mathf.RoundToInt(ServiceTime);
        }
    }

    public int XPReward
    {
        get
        {
            return Mathf.Max(0, xpReward);
        }
    }

    public string HistoryTitle
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(historyTitle))
                return historyTitle;

            return "História do serviço";
        }
    }

    public string HistorySummary
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(historySummary))
                return historySummary;

            return string.Empty;
        }
    }

    public string GetDescription()
    {
        if (!string.IsNullOrWhiteSpace(description))
            return description;

        return "Cliente solicitou um atendimento.";
    }

    public bool HasEducationalContent()
    {
        return !string.IsNullOrWhiteSpace(historyTitle) ||
               !string.IsNullOrWhiteSpace(historySummary) ||
               !string.IsNullOrWhiteSpace(afroCutId);
    }

    public int GetBaseTablePriceWithoutGlobalManagement()
    {
        int total = 0;

        if (priceTable != null)
        {
            total += priceTable.GetPrice(serviceType, customServiceId);

            if (additionalServiceTypes != null)
            {
                for (int i = 0; i < additionalServiceTypes.Count; i++)
                {
                    ServiceType extraType = additionalServiceTypes[i];
                    total += priceTable.GetPrice(extraType);
                }
            }

            return Mathf.Max(0, total);
        }

        total += Mathf.Max(0, servicePrice);
        return Mathf.Max(0, total);
    }

    public void SyncCompatibilityFields()
    {
        if (string.IsNullOrWhiteSpace(requestId) && !string.IsNullOrWhiteSpace(id))
            requestId = id;

        if (string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(requestId))
            id = requestId;

        if (string.IsNullOrWhiteSpace(requestName))
            requestName = name;

        if (servicePrice < 0)
            servicePrice = 0;

        if (serviceTime < 1f)
            serviceTime = 1f;

        if (xpReward < 0)
            xpReward = 0;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        SyncCompatibilityFields();

        if (difficulty < 1)
            difficulty = 1;
    }
#endif
}