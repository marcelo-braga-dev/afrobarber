using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ServicePriceTable", menuName = "AfroBarber/Services/Price Table")]
public class ServicePriceTable : ScriptableObject
{
    [SerializeField] private List<ServicePriceEntry> prices = new List<ServicePriceEntry>();
    public IReadOnlyList<ServicePriceEntry> Prices => prices;

    public int GetPrice(ServiceType serviceType, string customServiceId = "")
    {
        if (serviceType == ServiceType.Outro && !string.IsNullOrWhiteSpace(customServiceId))
        {
            ServicePriceEntry customEntry = prices.Find(x =>
                x != null &&
                x.serviceType == ServiceType.Outro &&
                x.customServiceId == customServiceId
            );

            if (customEntry != null)
                return Mathf.Max(0, customEntry.price);
        }

        ServicePriceEntry entry = prices.Find(x => x != null && x.serviceType == serviceType);

        if (entry != null)
            return Mathf.Max(0, entry.price);

        return 0;
    }

    public string GetDisplayName(ServiceType serviceType, string customServiceId = "")
    {
        if (serviceType == ServiceType.Outro && !string.IsNullOrWhiteSpace(customServiceId))
        {
            ServicePriceEntry customEntry = prices.Find(x =>
                x != null &&
                x.serviceType == ServiceType.Outro &&
                x.customServiceId == customServiceId
            );

            if (customEntry != null && !string.IsNullOrWhiteSpace(customEntry.displayName))
                return customEntry.displayName;
        }

        ServicePriceEntry entry = prices.Find(x => x != null && x.serviceType == serviceType);

        if (entry != null && !string.IsNullOrWhiteSpace(entry.displayName))
            return entry.displayName;

        return serviceType.ToString();
    }

    [ContextMenu("Criar Tabela Padrão")]
    public void CreateDefaultPrices()
    {
        prices = new List<ServicePriceEntry>
        {
            new ServicePriceEntry
            {
                serviceType = ServiceType.CorteDeCabelo,
                displayName = "Corte de Cabelo",
                price = 35
            },
            new ServicePriceEntry
            {
                serviceType = ServiceType.CorteDeCabeloEBarba,
                displayName = "Corte de Cabelo e Barba",
                price = 55
            },
            new ServicePriceEntry
            {
                serviceType = ServiceType.Barba,
                displayName = "Barba",
                price = 25
            },
            new ServicePriceEntry
            {
                serviceType = ServiceType.AcabamentoPezinho,
                displayName = "Acabamento/Pezinho",
                price = 15
            },
            new ServicePriceEntry
            {
                serviceType = ServiceType.DesignDeSobrancelhas,
                displayName = "Design de Sobrancelhas",
                price = 20
            },
            new ServicePriceEntry
            {
                serviceType = ServiceType.Hidratacao,
                displayName = "Hidratação",
                price = 40
            }
        };
    }
}