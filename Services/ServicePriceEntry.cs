using System;
using UnityEngine;

[Serializable]
public class ServicePriceEntry
{
    public ServiceType serviceType;
    public string customServiceId;
    public string displayName;
    public int price;
}