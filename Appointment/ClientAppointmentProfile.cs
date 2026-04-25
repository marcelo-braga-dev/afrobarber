using UnityEngine;

public class ClientAppointmentProfile : MonoBehaviour
{
    [Header("Pontualidade")]
    [Min(0f)] public float earlyArrivalToleranceMinutes = 8f;
    [Min(0f)] public float lateArrivalToleranceMinutes = 12f;

    [Range(0f, 1f)]
    public float punctuality = 0.75f;

    [Header("Movimento")]
    [Min(0.1f)] public float averageWalkSpeed = 2.5f;

    [Header("Paciência com atraso")]
    [Min(0f)] public float maxDelayToleranceMinutes = 25f;

    [Header("Diálogos")]
    public string onTimeMessage = "Cheguei!";
    public string lateMessage = "Me desculpe! Demorei um pouco para chegar.";
    public string overbookingMessage = "Ainda dá tempo de me atender?";
    public string cascadeDelayMessage = "Ainda vai demorar muito?";
}