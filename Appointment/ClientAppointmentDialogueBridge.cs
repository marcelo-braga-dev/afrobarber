using UnityEngine;

public class ClientAppointmentDialogueBridge : MonoBehaviour
{
    public static ClientAppointmentDialogueBridge Instance { get; private set; }

    [Header("Objeto opcional do sistema de diálogo")]
    [SerializeField] private GameObject dialogueReceiver;

    [Header("Nome do método no sistema de diálogo")]
    [SerializeField] private string dialogueMethodName = "AddMessageToChat";

    [Header("Debug")]
    [SerializeField] private bool logMessages = true;

    private void Awake()
    {
        Instance = this;
    }

    public void SendClientMessage(string clientName, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        string finalMessage = $"{clientName}: {message}";

        if (logMessages)
            Debug.Log($"[Agenda/Diálogo] {finalMessage}");

        if (dialogueReceiver != null && !string.IsNullOrWhiteSpace(dialogueMethodName))
        {
            dialogueReceiver.SendMessage(
                dialogueMethodName,
                finalMessage,
                SendMessageOptions.DontRequireReceiver
            );
        }
    }
}