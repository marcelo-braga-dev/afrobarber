using System.Collections.Generic;
using UnityEngine;

public class SofaSeatGroup : MonoBehaviour
{
    [SerializeField] private List<WaitingSeat> seats = new List<WaitingSeat>();

    public bool TryGetFreeSeat(ClientNPC client, out WaitingSeat freeSeat)
    {
        for (int i = 0; i < seats.Count; i++)
        {
            if (seats[i] != null && seats[i].TryReserve(client))
            {
                freeSeat = seats[i];
                return true;
            }
        }

        freeSeat = null;
        return false;
    }
}