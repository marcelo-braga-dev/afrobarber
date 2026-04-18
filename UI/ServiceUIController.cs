using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class ServiceUIController : MonoBehaviour
{
    public static ServiceUIController Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private string message = "Realizando atendimento";

    [Header("Tempo")]
    [SerializeField] private float duration = 3f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (panel != null)
            panel.SetActive(false);
    }

    public void ShowServiceUI(Action onFinish)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ServiceRoutine(onFinish));
    }

    private IEnumerator ServiceRoutine(Action onFinish)
    {
        Debug.Log("[ServiceUIController] Iniciando UI de atendimento.");

        if (statusText != null)
            statusText.text = message;

        if (panel != null)
            panel.SetActive(true);
        else
            Debug.LogWarning("[ServiceUIController] panel não configurado.");

        yield return new WaitForSeconds(duration);

        if (panel != null)
            panel.SetActive(false);

        Debug.Log("[ServiceUIController] Finalizando UI de atendimento.");

        onFinish?.Invoke();
        currentRoutine = null;
    }
}