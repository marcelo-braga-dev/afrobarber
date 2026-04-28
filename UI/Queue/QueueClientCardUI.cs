using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QueueClientCardUI : MonoBehaviour
{
    [Header("Textos")]
    [SerializeField] private TMP_Text clientNameText;
    [SerializeField] private TMP_Text arrivalTimeText;
    [SerializeField] private TMP_Text waitingTimeText;
    [SerializeField] private TMP_Text patienceStateText;

    [Header("UI Extra")]
    [SerializeField] private Slider patienceSlider;

    [Header("Atualização")]
    [SerializeField] private float refreshIntervalSeconds = 0.25f;

    private ClientQueueData currentData;
    private ClientPatience cachedPatience;
    private float refreshTimer;

    public void Setup(ClientQueueData data)
    {
        currentData = data;

        cachedPatience = currentData != null && currentData.clientNPC != null
            ? currentData.clientNPC.GetComponent<ClientPatience>()
            : null;

        if (patienceSlider != null)
        {
            patienceSlider.minValue = 0f;
            patienceSlider.maxValue = 1f;
        }

        refreshTimer = 0f;
        Refresh();
    }

    private void Update()
    {
        if (currentData == null)
            return;

        refreshTimer += Time.deltaTime;

        if (refreshTimer < refreshIntervalSeconds)
            return;

        refreshTimer = 0f;
        Refresh();
    }

    private void Refresh()
    {
        if (currentData == null)
            return;

        float currentGameMinutes = 0f;

        if (BarberQueueSystem.Instance != null)
            currentGameMinutes = BarberQueueSystem.Instance.GetCurrentGameMinutes();

        float waitingMinutes = currentData.GetWaitingMinutes(currentGameMinutes);
        float patiencePercent = currentData.GetPatiencePercent(currentGameMinutes);

        ClientPatience.PatienceState state = ClientPatience.PatienceState.Calm;

        if (cachedPatience == null && currentData.clientNPC != null)
            cachedPatience = currentData.clientNPC.GetComponent<ClientPatience>();

        if (cachedPatience != null)
            state = cachedPatience.EvaluateState(patiencePercent);

        if (clientNameText != null)
            clientNameText.text = currentData.clientName;

        if (arrivalTimeText != null)
            arrivalTimeText.text = "Chegada: " + FormatGameTime(currentData.arrivalGameMinutes);

        if (waitingTimeText != null)
            waitingTimeText.text = "Espera: " + FormatDuration(waitingMinutes);

        if (patienceStateText != null)
            patienceStateText.text = "Estado: " + GetStateLabel(state);

        if (patienceSlider != null)
            patienceSlider.value = patiencePercent;
    }

    private string FormatGameTime(float totalMinutes)
    {
        int minutes = Mathf.FloorToInt(totalMinutes);
        int dayMinutes = minutes % 1440;
        int hours = dayMinutes / 60;
        int mins = dayMinutes % 60;

        return $"{hours:00}:{mins:00}";
    }

    private string FormatDuration(float totalMinutes)
    {
        int minutes = Mathf.FloorToInt(totalMinutes);
        int hours = minutes / 60;
        int mins = minutes % 60;

        if (hours > 0)
            return $"{hours}h {mins}min";

        return $"{mins}min";
    }

    private string GetStateLabel(ClientPatience.PatienceState state)
    {
        switch (state)
        {
            case ClientPatience.PatienceState.Calm:
                return "Calmo";

            case ClientPatience.PatienceState.Waiting:
                return "Aguardando";

            case ClientPatience.PatienceState.Impatient:
                return "Impaciente";

            case ClientPatience.PatienceState.Angry:
                return "Irritado";

            case ClientPatience.PatienceState.LeavingSoon:
                return "Quase indo embora";

            default:
                return "Desconhecido";
        }
    }
}