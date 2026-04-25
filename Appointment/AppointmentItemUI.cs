using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AppointmentItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text clientText;
    [SerializeField] private TMP_Text serviceText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Image statusIndicator;

    [Header("Cores")]
    [SerializeField] private Color scheduledColor = Color.white;
    [SerializeField] private Color spawnedColor = Color.green;
    [SerializeField] private Color delayedColor = Color.yellow;
    [SerializeField] private Color overbookedColor = new Color(1f, 0.45f, 0f);
    [SerializeField] private Color cancelledColor = Color.red;

    public void Setup(ClientAppointmentData data)
    {
        if (data == null)
            return;

        if (timeText != null)
            timeText.text = data.TimeText;

        if (clientText != null)
            clientText.text = data.clientPrefab != null ? data.clientPrefab.name : "Cliente";

        if (serviceText != null)
            serviceText.text = data.requestData != null ? data.requestData.RequestName : "Serviço";

        if (statusText != null)
            statusText.text = GetStatusText(data.status);

        if (statusIndicator != null)
            statusIndicator.color = GetStatusColor(data.status);
    }

    private string GetStatusText(ClientAppointmentStatus status)
    {
        switch (status)
        {
            case ClientAppointmentStatus.Scheduled:
                return "Agendado";

            case ClientAppointmentStatus.WaitingToSpawn:
                return "Aguardando vaga";

            case ClientAppointmentStatus.Spawned:
                return "A caminho";

            case ClientAppointmentStatus.DelayedByQueue:
                return "Atrasado pela fila";

            case ClientAppointmentStatus.Overbooked:
                return "Overbooking";

            case ClientAppointmentStatus.Cancelled:
                return "Cancelado";

            case ClientAppointmentStatus.Missed:
                return "Perdido";

            default:
                return status.ToString();
        }
    }

    private Color GetStatusColor(ClientAppointmentStatus status)
    {
        switch (status)
        {
            case ClientAppointmentStatus.Spawned:
                return spawnedColor;

            case ClientAppointmentStatus.DelayedByQueue:
                return delayedColor;

            case ClientAppointmentStatus.Overbooked:
                return overbookedColor;

            case ClientAppointmentStatus.Cancelled:
            case ClientAppointmentStatus.Missed:
                return cancelledColor;

            default:
                return scheduledColor;
        }
    }
}