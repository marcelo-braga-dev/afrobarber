using System.Collections.Generic;
using UnityEngine;

public class WaitingAreaManager : MonoBehaviour
{
    [SerializeField] private List<SofaSeatGroup> sofaGroups = new List<SofaSeatGroup>();

    public bool TryReserveAnySeat(ClientNPC client, out WaitingSeat reservedSeat)
    {
        for (int i = 0; i < sofaGroups.Count; i++)
        {
            if (sofaGroups[i] != null && sofaGroups[i].TryGetFreeSeat(client, out reservedSeat))
            {
                return true;
            }
        }

        reservedSeat = null;
        return false;
    }
}