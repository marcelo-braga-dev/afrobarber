using System.Collections;
using UnityEngine;

public class NPCInteractionIndicator : MonoBehaviour
{
    [SerializeField] private GameObject indicatorRoot;
    [SerializeField] private InteractionAvailabilityType state = InteractionAvailabilityType.None;
    [SerializeField] private bool blinkWhenUrgent = true;
    [SerializeField] private float blinkInterval = 0.35f;

    private Coroutine blinkRoutine;

    public InteractionAvailabilityType State => state;

    public void SetState(InteractionAvailabilityType newState)
    {
        state = newState;
        ApplyState();
    }

    private void OnEnable()
    {
        ApplyState();
    }

    private void OnDisable()
    {
        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);
    }

    private void ApplyState()
    {
        bool visible = state != InteractionAvailabilityType.None;

        if (indicatorRoot != null)
            indicatorRoot.SetActive(visible);

        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        bool shouldBlink = blinkWhenUrgent && (state == InteractionAvailabilityType.Urgent || state == InteractionAvailabilityType.NeedResponse);
        if (shouldBlink && indicatorRoot != null)
            blinkRoutine = StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine()
    {
        while (true)
        {
            indicatorRoot.SetActive(!indicatorRoot.activeSelf);
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}
