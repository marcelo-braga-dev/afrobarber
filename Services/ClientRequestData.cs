using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ClientRequestData", menuName = "AfroBarber/Client/Request Data")]
public class ClientRequestData : ScriptableObject
{
    [Header("Compatibilidade / Identificação")]
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

    [Header("Preço / Tempo / Dificuldade")]
    public int price = 30;
    public int servicePrice = 30;
    public float time = 10f;
    public float serviceTime = 10f;
    public float serviceTimeMinutes = 10f;
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

    [Header("Educação / História")]
    public string afroCutId;

    [TextArea(2, 5)]
    public string educationalTitle;

    [TextArea(4, 10)]
    public string educationalSummary;

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
            if (priceTable != null)
                return priceTable.GetPrice(serviceType, customServiceId);

            if (servicePrice > 0)
                return servicePrice;

            return Mathf.Max(0, price);
        }
    }

    public float ServiceTime
    {
        get
        {
            if (serviceTimeMinutes > 0f)
                return serviceTimeMinutes;

            if (serviceTime > 0f)
                return serviceTime;

            return Mathf.Max(1f, time);
        }
    }

    public int ServiceTimeRoundedMinutes
    {
        get
        {
            return Mathf.RoundToInt(ServiceTime);
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
        return !string.IsNullOrWhiteSpace(educationalTitle) ||
               !string.IsNullOrWhiteSpace(educationalSummary) ||
               !string.IsNullOrWhiteSpace(afroCutId);
    }

    public void SyncCompatibilityFields()
    {
        if (string.IsNullOrWhiteSpace(requestId) && !string.IsNullOrWhiteSpace(id))
            requestId = id;

        if (string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(requestId))
            id = requestId;

        if (string.IsNullOrWhiteSpace(requestName))
            requestName = name;

        if (serviceTimeMinutes <= 0f && serviceTime > 0f)
            serviceTimeMinutes = serviceTime;

        if (serviceTime <= 0f && serviceTimeMinutes > 0f)
            serviceTime = serviceTimeMinutes;

        if (time <= 0f && serviceTimeMinutes > 0f)
            time = serviceTimeMinutes;

        if (servicePrice <= 0 && price > 0)
            servicePrice = price;

        if (price <= 0 && servicePrice > 0)
            price = servicePrice;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        SyncCompatibilityFields();

        if (difficulty < 1)
            difficulty = 1;

        if (price < 0)
            price = 0;

        if (servicePrice < 0)
            servicePrice = 0;

        if (xpReward < 0)
            xpReward = 0;

        if (serviceTimeMinutes < 1f)
            serviceTimeMinutes = 1f;

        if (serviceTime < 1f)
            serviceTime = serviceTimeMinutes;

        if (time < 1f)
            time = serviceTimeMinutes;
    }
#endif
}