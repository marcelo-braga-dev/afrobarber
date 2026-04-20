using UnityEngine;

public class ClientPatience : MonoBehaviour
{
    public enum PatienceState
    {
        Calm,
        Waiting,
        Impatient,
        Angry,
        LeavingSoon
    }

    [Header("Configuração")]
    [SerializeField] private ClientNPC clientNPC;
    [SerializeField] private float maxPatienceMinutes = 120f;

    [Header("Leitura")]
    [SerializeField] private float currentWaitingMinutes;
    [SerializeField] private float patiencePercent;
    [SerializeField] private PatienceState currentState;
    [SerializeField] private PatienceState lastAnnouncedState;

    public float MaxPatienceMinutes => maxPatienceMinutes;
    public float CurrentWaitingMinutes => currentWaitingMinutes;
    public float PatiencePercent => patiencePercent;
    public PatienceState CurrentState => currentState;

    private void Reset()
    {
        clientNPC = GetComponent<ClientNPC>();
    }

    private void Update()
    {
        if (clientNPC == null || BarberQueueSystem.Instance == null)
            return;

        ClientQueueData data = BarberQueueSystem.Instance.GetClientData(clientNPC);
        if (data == null)
            return;

        float currentGameMinutes = BarberQueueSystem.Instance.GetCurrentGameMinutes();
        currentWaitingMinutes = data.GetWaitingMinutes(currentGameMinutes);
        patiencePercent = data.GetPatiencePercent(currentGameMinutes);
        currentState = EvaluateState(patiencePercent);

        if (currentState != lastAnnouncedState)
        {
            AnnounceState(currentState);
            lastAnnouncedState = currentState;
        }
    }

    public void SetupPatience(float patienceMinutes)
    {
        maxPatienceMinutes = patienceMinutes;
    }

    public PatienceState EvaluateState(float percent)
    {
        if (percent < 0.25f)
            return PatienceState.Calm;

        if (percent < 0.50f)
            return PatienceState.Waiting;

        if (percent < 0.75f)
            return PatienceState.Impatient;

        if (percent < 0.95f)
            return PatienceState.Angry;

        return PatienceState.LeavingSoon;
    }

    private void AnnounceState(PatienceState state)
    {
        if (clientNPC == null)
            return;

        NPCIdentity identity = clientNPC.GetComponent<NPCIdentity>();
        NPCInteractionIndicator indicator = clientNPC.GetComponentInChildren<NPCInteractionIndicator>(true);

        string line = null;
        InteractionAvailabilityType indicatorState = InteractionAvailabilityType.Conversation;

        switch (state)
        {
            case PatienceState.Impatient:
                line = "Ainda vai demorar muito?";
                indicatorState = InteractionAvailabilityType.NeedResponse;
                break;

            case PatienceState.Angry:
                line = "Já estou esperando faz tempo...";
                indicatorState = InteractionAvailabilityType.Urgent;
                break;

            case PatienceState.LeavingSoon:
                line = "Você vai conseguir me atender hoje?";
                indicatorState = InteractionAvailabilityType.Urgent;
                break;
        }

        if (!string.IsNullOrWhiteSpace(line))
        {
            GlobalDialogueManager.Instance?.AddNpcMessage(identity, line, DialogueContextType.Queue);
            indicator?.SetState(indicatorState);
        }
    }
}