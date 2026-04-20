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
}