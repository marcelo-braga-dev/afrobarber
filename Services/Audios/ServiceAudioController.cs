using System.Collections.Generic;
using UnityEngine;

public class ServiceAudioController : MonoBehaviour
{
    [System.Serializable]
    public class ServiceAudioRule
    {
        public ServiceActionType actionType;
        public ProductCategory productCategory;
        public AudioClip clip;
        public bool loop = true;
        [Range(0f, 1f)] public float volume = 0.8f;
    }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<ServiceAudioRule> audioRules = new List<ServiceAudioRule>();

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    public void PlayStepAudio(ServiceActionPlanStep step)
    {
        if (step == null || audioSource == null)
            return;

        ProductCategory? category = GetProductCategory(step.productId);
        ServiceAudioRule rule = FindRule(step.actionType, category);

        if (rule == null || rule.clip == null)
        {
            StopAudio();
            return;
        }

        audioSource.clip = rule.clip;
        audioSource.loop = rule.loop;
        audioSource.volume = rule.volume;
        audioSource.Play();
    }

    public void StopAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    private ServiceAudioRule FindRule(ServiceActionType actionType, ProductCategory? category)
    {
        for (int i = 0; i < audioRules.Count; i++)
        {
            ServiceAudioRule rule = audioRules[i];

            if (rule.actionType != actionType)
                continue;

            if (category.HasValue && rule.productCategory == category.Value)
                return rule;
        }

        for (int i = 0; i < audioRules.Count; i++)
        {
            ServiceAudioRule rule = audioRules[i];

            if (rule.actionType == actionType)
                return rule;
        }

        return null;
    }

    private ProductCategory? GetProductCategory(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId) || InventoryManager.Instance == null)
            return null;

        ProductData product = InventoryManager.Instance.GetProductDataById(productId);

        if (product == null)
            return null;

        return product.category;
    }
}