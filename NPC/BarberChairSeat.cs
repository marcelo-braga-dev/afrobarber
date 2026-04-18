using UnityEngine;

public class BarberChairSeat : MonoBehaviour
{
    [Header("Configuração do assento")]
    [SerializeField] private Transform sitPoint;
    [SerializeField] private bool isOccupied = false;

    public Transform SitPoint => sitPoint;
    public bool IsOccupied => isOccupied;

    public void OccupySeat()
    {
        isOccupied = true;
    }

    public void FreeSeat()
    {
        isOccupied = false;
    }
}