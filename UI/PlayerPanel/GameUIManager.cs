using UnityEngine;
using UnityEngine.InputSystem;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [Header("UIs Principais")]
    [SerializeField] private GameObject uiFinanceiro;
    [SerializeField] private GameObject uiGestao;
    [SerializeField] private GameObject uiInventario;
    [SerializeField] private GameObject uiLoja;
    [SerializeField] private GameObject uiNotificacao;
    [SerializeField] private GameObject uiAtendimento;
    [SerializeField] private GameObject uiFila;
    [SerializeField] private GameObject uiAgenda;
    [SerializeField] private GameObject uiHistoricoAtendimento;
    [SerializeField] private GameObject uiAvaliacaoAtendimento;
    [SerializeField] private GameObject uiReputacaoDetalhada;
    [SerializeField] private GameObject uiMissoes;

    [Header("Configuração")]
    [SerializeField] private bool notificationCanStayWithOtherUI = false;

    [Header("Input")]
    [SerializeField] private Key closeAllKey = Key.Escape;

    private GameObject currentOpenUI;

    public GameObject CurrentOpenUI => currentOpenUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            HideAll();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current[closeAllKey].wasPressedThisFrame)
            HideAll();
    }

    public void OpenFinanceiro()
    {
        bool willOpen = uiFinanceiro != null && !uiFinanceiro.activeSelf;

        ToggleExclusiveUI(uiFinanceiro);

        if (willOpen && FinanceUIController.Instance != null)
            FinanceUIController.Instance.RefreshUI();
    }

    public void OpenGestao()
    {
        ToggleExclusiveUI(uiGestao);
    }

    public void OpenInventario()
    {
        bool willOpen = uiInventario != null && !uiInventario.activeSelf;

        ToggleExclusiveUI(uiInventario);

        if (willOpen && InventoryUIManager.Instance != null)
            InventoryUIManager.Instance.RefreshUI();
    }

    public void OpenLoja()
    {
        bool willOpen = uiLoja != null && !uiLoja.activeSelf;

        ToggleExclusiveUI(uiLoja);

        if (willOpen)
        {
            ShopManager shop = FindFirstObjectByType<ShopManager>();
            if (shop != null)
                shop.RefreshShopUI();
        }
    }

    public void OpenAtendimento()
    {
        ToggleExclusiveUI(uiAtendimento);
    }

    public void OpenFila()
    {
        bool willOpen = uiFila != null && !uiFila.activeSelf;

        ToggleExclusiveUI(uiFila);

        if (willOpen)
        {
            QueueUIManager queueUI = FindFirstObjectByType<QueueUIManager>();
            if (queueUI != null)
                queueUI.RefreshQueueUI();
        }
    }

    public void OpenAgenda()
    {
        bool willOpen = uiAgenda != null && !uiAgenda.activeSelf;

        ToggleExclusiveUI(uiAgenda);

        if (willOpen)
        {
            AppointmentPanelUI agendaUI = FindFirstObjectByType<AppointmentPanelUI>();
            if (agendaUI != null)
                agendaUI.Refresh();
        }
    }

    public void OpenHistoricoAtendimento()
    {
        bool willOpen = uiHistoricoAtendimento != null && !uiHistoricoAtendimento.activeSelf;

        ToggleExclusiveUI(uiHistoricoAtendimento);

        if (willOpen)
        {
            ServiceHistoryUI historyUI = FindFirstObjectByType<ServiceHistoryUI>();
            if (historyUI != null)
                historyUI.RefreshHistoryUI();
        }
    }

    public void OpenAvaliacaoAtendimento()
    {
        ToggleExclusiveUI(uiAvaliacaoAtendimento);
    }

    public void OpenReputacaoDetalhada()
    {
        ToggleExclusiveUI(uiReputacaoDetalhada);
    }

    public void OpenNotificacao()
    {
        if (notificationCanStayWithOtherUI)
        {
            ToggleIndependentUI(uiNotificacao);
            return;
        }

        ToggleExclusiveUI(uiNotificacao);
    }

    public void OpenMissoes()
    {
        bool willOpen = uiMissoes != null && !uiMissoes.activeSelf;

        ToggleExclusiveUI(uiMissoes);

        if (willOpen)
        {
            MissionsPanelUI missionsUI = FindFirstObjectByType<MissionsPanelUI>();
            if (missionsUI != null)
                missionsUI.Refresh();
        }
    }

    public void CloseMissoes() => CloseUI(uiMissoes);
    public void CloseFinanceiro() => CloseUI(uiFinanceiro);
    public void CloseGestao() => CloseUI(uiGestao);
    public void CloseInventario() => CloseUI(uiInventario);
    public void CloseLoja() => CloseUI(uiLoja);
    public void CloseNotificacao() => CloseUI(uiNotificacao);
    public void CloseAtendimento() => CloseUI(uiAtendimento);
    public void CloseFila() => CloseUI(uiFila);
    public void CloseAgenda() => CloseUI(uiAgenda);
    public void CloseHistoricoAtendimento() => CloseUI(uiHistoricoAtendimento);
    public void CloseAvaliacaoAtendimento() => CloseUI(uiAvaliacaoAtendimento);
    public void CloseReputacaoDetalhada() => CloseUI(uiReputacaoDetalhada);

    private void ToggleExclusiveUI(GameObject targetUI)
    {
        if (targetUI == null)
            return;

        if (targetUI.activeSelf)
        {
            targetUI.SetActive(false);

            if (currentOpenUI == targetUI)
                currentOpenUI = null;

            return;
        }

        HideAll();
        targetUI.SetActive(true);
        currentOpenUI = targetUI;
    }

    private void ToggleIndependentUI(GameObject targetUI)
    {
        if (targetUI == null)
            return;

        targetUI.SetActive(!targetUI.activeSelf);
    }

    public void CloseUI(GameObject targetUI)
    {
        if (targetUI == null)
            return;

        targetUI.SetActive(false);

        if (currentOpenUI == targetUI)
            currentOpenUI = null;
    }

    public void HideAll()
    {
        if (uiFinanceiro != null) uiFinanceiro.SetActive(false);
        if (uiGestao != null) uiGestao.SetActive(false);
        if (uiInventario != null) uiInventario.SetActive(false);
        if (uiLoja != null) uiLoja.SetActive(false);
        if (uiNotificacao != null) uiNotificacao.SetActive(false);
        if (uiAtendimento != null) uiAtendimento.SetActive(false);
        if (uiFila != null) uiFila.SetActive(false);
        if (uiAgenda != null) uiAgenda.SetActive(false);
        if (uiHistoricoAtendimento != null) uiHistoricoAtendimento.SetActive(false);
        if (uiAvaliacaoAtendimento != null) uiAvaliacaoAtendimento.SetActive(false);
        if (uiReputacaoDetalhada != null) uiReputacaoDetalhada.SetActive(false);
        if (uiMissoes != null) uiMissoes.SetActive(false);

        currentOpenUI = null;
    }

    public bool IsAnyUIOpen()
    {
        return
            IsUIOpen(uiFinanceiro) ||
            IsUIOpen(uiGestao) ||
            IsUIOpen(uiInventario) ||
            IsUIOpen(uiLoja) ||
            IsUIOpen(uiNotificacao) ||
            IsUIOpen(uiAtendimento) ||
            IsUIOpen(uiFila) ||
            IsUIOpen(uiAgenda) ||
            IsUIOpen(uiHistoricoAtendimento) ||
            IsUIOpen(uiAvaliacaoAtendimento) ||
            IsUIOpen(uiReputacaoDetalhada) ||
            IsUIOpen(uiMissoes);
    }

    public bool IsBlockingUIOpen()
    {
        return
            IsUIOpen(uiFinanceiro) ||
            IsUIOpen(uiGestao) ||
            IsUIOpen(uiInventario) ||
            IsUIOpen(uiLoja) ||
            IsUIOpen(uiAtendimento) ||
            IsUIOpen(uiFila) ||
            IsUIOpen(uiAgenda) ||
            IsUIOpen(uiHistoricoAtendimento) ||
            IsUIOpen(uiAvaliacaoAtendimento) ||
            IsUIOpen(uiReputacaoDetalhada) ||
            IsUIOpen(uiMissoes) ||
            (!notificationCanStayWithOtherUI && IsUIOpen(uiNotificacao));
    }

    private bool IsUIOpen(GameObject ui)
    {
        return ui != null && ui.activeSelf;
    }
}