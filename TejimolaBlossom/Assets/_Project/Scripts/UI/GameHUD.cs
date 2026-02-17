using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tejimola.Core;
using Tejimola.Utils;

namespace Tejimola.UI
{
    public class GameHUD : MonoBehaviour
    {
        [Header("Day Counter")]
        [SerializeField] private TextMeshProUGUI dayText;

        [Header("Spirit Pulse")]
        [SerializeField] private Image pulseCooldownFill;
        [SerializeField] private Image pulseIcon;
        [SerializeField] private GameObject pulseIndicator;

        [Header("Stealth")]
        [SerializeField] private GameObject stealthPanel;
        [SerializeField] private TextMeshProUGUI catchCountText;
        [SerializeField] private Image[] catchIcons;

        [Header("Exhaustion")]
        [SerializeField] private GameObject exhaustionPanel;
        [SerializeField] private Image exhaustionFill;
        [SerializeField] private TextMeshProUGUI exhaustionText;

        [Header("Objective")]
        [SerializeField] private TextMeshProUGUI objectiveText;
        [SerializeField] private CanvasGroup objectiveGroup;

        [Header("Interaction Prompt")]
        [SerializeField] private GameObject interactionPrompt;
        [SerializeField] private TextMeshProUGUI interactionText;

        [Header("Notification")]
        [SerializeField] private TextMeshProUGUI notificationText;
        [SerializeField] private CanvasGroup notificationGroup;

        [Header("Boss Health")]
        [SerializeField] private GameObject bossHealthPanel;
        [SerializeField] private Image bossHealthFill;
        [SerializeField] private TextMeshProUGUI bossNameText;

        private float notificationTimer;

        void Start()
        {
            SubscribeEvents();
            RefreshHUD();
            HideAll();
        }

        void OnDestroy()
        {
            UnsubscribeEvents();
        }

        void SubscribeEvents()
        {
            var em = EventManager.Instance;
            em.Subscribe<int>(EventManager.Events.CatchCountChanged, OnCatchCountChanged);
            em.Subscribe<float>(EventManager.Events.ExhaustionChanged, OnExhaustionChanged);
            em.Subscribe<GamePhase>(EventManager.Events.PhaseChanged, OnPhaseChanged);
            em.Subscribe<string>(EventManager.Events.ObjectiveUpdated, OnObjectiveUpdated);
            em.Subscribe<string>(EventManager.Events.ShowNotification, ShowNotification);
            em.Subscribe(EventManager.Events.InteractionAvailable, ShowInteractionPrompt);
            em.Subscribe(EventManager.Events.InteractionPerformed, HideInteractionPrompt);
            em.Subscribe(EventManager.Events.SpiritPulseActivated, OnPulseActivated);
            em.Subscribe(EventManager.Events.SpiritPulseReady, OnPulseReady);
        }

        void UnsubscribeEvents()
        {
            if (EventManager.Instance == null) return;
            var em = EventManager.Instance;
            em.Unsubscribe<int>(EventManager.Events.CatchCountChanged, OnCatchCountChanged);
            em.Unsubscribe<float>(EventManager.Events.ExhaustionChanged, OnExhaustionChanged);
            em.Unsubscribe<GamePhase>(EventManager.Events.PhaseChanged, OnPhaseChanged);
            em.Unsubscribe<string>(EventManager.Events.ObjectiveUpdated, OnObjectiveUpdated);
            em.Unsubscribe<string>(EventManager.Events.ShowNotification, ShowNotification);
            em.Unsubscribe(EventManager.Events.InteractionAvailable, ShowInteractionPrompt);
            em.Unsubscribe(EventManager.Events.InteractionPerformed, HideInteractionPrompt);
            em.Unsubscribe(EventManager.Events.SpiritPulseActivated, OnPulseActivated);
            em.Unsubscribe(EventManager.Events.SpiritPulseReady, OnPulseReady);
        }

        void Update()
        {
            UpdateDayCounter();
            UpdatePulseCooldown();
            UpdateNotification();
        }

        void RefreshHUD()
        {
            UpdateDayCounter();
        }

        void HideAll()
        {
            stealthPanel?.SetActive(false);
            exhaustionPanel?.SetActive(false);
            bossHealthPanel?.SetActive(false);
            interactionPrompt?.SetActive(false);
            if (notificationGroup != null)
                notificationGroup.alpha = 0f;
        }

        void UpdateDayCounter()
        {
            if (dayText != null)
                dayText.text = $"Day {GameManager.Instance.CurrentDay}";
        }

        void UpdatePulseCooldown()
        {
            if (pulseIndicator == null) return;

            bool isDom = GameManager.Instance.CurrentCharacter == ActiveCharacter.Dom;
            pulseIndicator.SetActive(isDom);

            if (isDom)
            {
                var dom = FindFirstObjectByType<Characters.DomBehaviour>();
                if (dom != null && pulseCooldownFill != null)
                {
                    pulseCooldownFill.fillAmount = dom.GetPulseCooldownPercent();
                }
            }
        }

        void OnPhaseChanged(GamePhase phase)
        {
            stealthPanel?.SetActive(phase == GamePhase.Stealth);
            exhaustionPanel?.SetActive(phase == GamePhase.Rhythm);
            bossHealthPanel?.SetActive(phase == GamePhase.BossFight);
        }

        void OnCatchCountChanged(int count)
        {
            if (catchCountText != null)
                catchCountText.text = $"{count}/{GameConstants.MaxCatches}";

            // Update catch icons
            if (catchIcons != null)
            {
                for (int i = 0; i < catchIcons.Length; i++)
                {
                    if (catchIcons[i] != null)
                        catchIcons[i].color = i < count ? Color.red : Color.gray;
                }
            }
        }

        void OnExhaustionChanged(float value)
        {
            if (exhaustionFill != null)
            {
                float percent = value / GameConstants.ExhaustionStart;
                exhaustionFill.fillAmount = percent;

                if (percent > 0.5f)
                    exhaustionFill.color = Color.Lerp(Color.yellow, GameColors.ForestGreen, (percent - 0.5f) * 2f);
                else
                    exhaustionFill.color = Color.Lerp(Color.red, Color.yellow, percent * 2f);
            }

            if (exhaustionText != null)
                exhaustionText.text = $"{value:F0}%";
        }

        void OnObjectiveUpdated(string objective)
        {
            if (objectiveText != null)
                objectiveText.text = objective;

            if (objectiveGroup != null)
            {
                objectiveGroup.alpha = 1f;
                // Auto-fade after 5 seconds handled in Update
            }
        }

        void ShowNotification(string message)
        {
            if (notificationText != null)
                notificationText.text = message;
            if (notificationGroup != null)
                notificationGroup.alpha = 1f;
            notificationTimer = 3f;
        }

        void UpdateNotification()
        {
            if (notificationTimer > 0)
            {
                notificationTimer -= Time.deltaTime;
                if (notificationTimer <= 1f && notificationGroup != null)
                    notificationGroup.alpha = notificationTimer;
            }
        }

        void ShowInteractionPrompt()
        {
            interactionPrompt?.SetActive(true);
            if (interactionText != null)
                interactionText.text = "[E] Interact";
        }

        void HideInteractionPrompt()
        {
            interactionPrompt?.SetActive(false);
        }

        void OnPulseActivated()
        {
            if (pulseIcon != null)
                pulseIcon.color = Color.gray;
        }

        void OnPulseReady()
        {
            if (pulseIcon != null)
                pulseIcon.color = GameColors.SpiritPurple;
        }

        public void UpdateBossHealth(float current, float max)
        {
            if (bossHealthFill != null)
                bossHealthFill.fillAmount = current / max;
        }

        public void SetBossName(string name)
        {
            if (bossNameText != null)
                bossNameText.text = name;
        }
    }
}
