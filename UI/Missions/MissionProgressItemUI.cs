using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionProgressItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Button claimButton;

    private MissionDefinition mission;

    private void Awake()
    {
        if (claimButton != null)
            claimButton.onClick.AddListener(HandleClaim);
    }

    private void OnDestroy()
    {
        if (claimButton != null)
            claimButton.onClick.RemoveListener(HandleClaim);
    }

    public void Setup(MissionDefinition missionDefinition)
    {
        mission = missionDefinition;
        Refresh();
    }

    public void Refresh()
    {
        if (MissionSystem.Instance == null || mission == null)
            return;

        if (titleText != null)
            titleText.text = mission.title;

        if (descriptionText != null)
            descriptionText.text = mission.description;

        if (progressText != null)
            progressText.text = MissionSystem.Instance.GetProgressLabel(mission);

        if (progressSlider != null)
            progressSlider.value = MissionSystem.Instance.GetCurrentTierProgress01(mission);

        bool canClaim = MissionSystem.Instance.CanClaimTier(mission);

        if (claimButton != null)
            claimButton.gameObject.SetActive(canClaim);

        if (rewardText != null)
            rewardText.text = BuildCurrentTierRewardText();
    }

    private string BuildCurrentTierRewardText()
    {
        if (mission == null)
            return "Sem metas";

        if (MissionSystem.Instance == null)
            return "Sem metas";

        return $"Próxima recompensa: {MissionSystem.Instance.GetActiveTierRewardSummary(mission)}";
    }

    private void HandleClaim()
    {
        if (MissionSystem.Instance == null || mission == null)
            return;

        MissionSystem.Instance.ClaimCurrentTier(mission);
        Refresh();
    }
}
