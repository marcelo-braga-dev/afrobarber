using System.Collections;
using TMPro;
using UnityEngine;

public class RestUIController : MonoBehaviour
{
    [Header("Painel")]
    [SerializeField] private GameObject restPanel;
    [SerializeField] private TMP_Text restText;

    [Header("Configuração")]
    [SerializeField] private float restMessageDuration = 3f;
    [SerializeField] private string restMessage = "Horário de descanso";

    private bool isResting;

    public void StartRestSequence()
    {
        if (isResting)
            return;

        StartCoroutine(RestRoutine());
    }

    private IEnumerator RestRoutine()
    {
        isResting = true;

        if (restPanel != null)
            restPanel.SetActive(true);

        if (restText != null)
            restText.text = restMessage;

        yield return new WaitForSeconds(restMessageDuration);

        if (GameTimeSystem.Instance != null)
            GameTimeSystem.Instance.RestUntilNextWorkdayStart(false);

        if (PlayerEnergySystem.Instance != null)
            PlayerEnergySystem.Instance.ApplyManualRestRecovery();

        if (restPanel != null)
            restPanel.SetActive(false);

        isResting = false;
    }
}