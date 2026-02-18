using UnityEngine;
using Tejimola.Core;
using Tejimola.Utils;
using Tejimola.Characters;

namespace Tejimola.Gameplay
{
    public class StealthManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EnemyAI ranimaAI;
        [SerializeField] private TejimolaBehaviour tejimola;

        [Header("Settings")]
        [SerializeField] private int maxCatches = GameConstants.MaxCatches;

        private bool stealthActive;

        void Start()
        {
            EventManager.Instance.Subscribe(EventManager.Events.StealthComplete, OnStealthComplete);
        }

        void OnDestroy()
        {
            EventManager.Instance.Unsubscribe(EventManager.Events.StealthComplete, OnStealthComplete);
        }

        public void StartStealth()
        {
            stealthActive = true;
            GameManager.Instance.SetPhase(GamePhase.Stealth);
            if (ranimaAI != null)
                ranimaAI.StartPatrol();
        }

        public void StopStealth()
        {
            stealthActive = false;
            if (ranimaAI != null)
                ranimaAI.StopPatrol();
        }

        void OnStealthComplete()
        {
            StopStealth();
            GameManager.Instance.SetPhase(GamePhase.Exploration);
        }
    }

}
