using System.Collections;
using UnityEngine;
using Tejimola.Core;
using Tejimola.UI;
using Tejimola.Utils;

namespace Tejimola.Gameplay
{
    public class BossController : MonoBehaviour
    {
        [Header("Boss Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;

        [Header("Phase 1 - Navigate")]
        [SerializeField] private float phase1MoveSpeed = 3f;
        [SerializeField] private Transform[] obstacleSpawnPoints;
        [SerializeField] private GameObject obstaclePrefab;
        [SerializeField] private float obstacleSpawnInterval = 2f;

        [Header("Phase 2 - Spirit Orbs")]
        [SerializeField] private float phase2MoveSpeed = 4f;
        [SerializeField] private float orbSlowDuration = 3f;
        [SerializeField] private GameObject spiritOrbProjectile;

        [Header("Phase 3 - Barrel Pursuit")]
        [SerializeField] private float barrelSpeed = 6f;
        [SerializeField] private GameObject spikedBarrelPrefab;
        [SerializeField] private Transform barrelSpawnPoint;
        [SerializeField] private GameObject vineObstaclePrefab;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer bossRenderer;
        [SerializeField] private Animator bossAnimator;
        [SerializeField] private ParticleSystem corruptionVFX;
        [SerializeField] private Color corruptedColor = new Color(0.545f, 0f, 0.345f, 1f);

        [Header("Audio")]
        [SerializeField] private AudioClip bossMusic;
        [SerializeField] private AudioClip phaseTransitionSound;
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip defeatSound;

        // State
        private BossPhase currentPhase = BossPhase.Phase1_Navigate;
        private bool isActive;
        private bool isSlowed;
        private float phase1Timer;
        private Transform playerTransform;
        private int phase2OrbsUsed;
        private int phase3BarrelsLaunched;
        private int phase3MaxBarrels = 3;

        // Events
        public System.Action<BossPhase> OnPhaseChanged;
        public System.Action<float, float> OnHealthChanged; // current, max
        public System.Action OnBossDefeated;

        void Start()
        {
            currentHealth = maxHealth;
            var player = FindFirstObjectByType<Characters.DomBehaviour>();
            if (player != null)
                playerTransform = player.transform;
        }

        public void StartBossFight()
        {
            isActive = true;
            currentHealth = maxHealth;
            currentPhase = BossPhase.Phase1_Navigate;
            GameManager.Instance.SetPhase(GamePhase.BossFight);

            if (bossMusic != null)
                AudioManager.Instance.PlayMusic(bossMusic);

            // Wire health bar to HUD
            var hud = FindFirstObjectByType<Tejimola.UI.GameHUD>();
            if (hud != null)
            {
                OnHealthChanged += hud.UpdateBossHealth;
                hud.SetBossName("RANIMA");
            }

            EventManager.Instance.Publish<BossPhase>(EventManager.Events.BossPhaseChanged, currentPhase);
            OnPhaseChanged?.Invoke(currentPhase);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            StartCoroutine(Phase1Routine());
        }

        void Update()
        {
            if (!isActive || GameManager.Instance.IsGamePaused) return;

            switch (currentPhase)
            {
                case BossPhase.Phase1_Navigate:
                    UpdatePhase1();
                    break;
                case BossPhase.Phase2_SpiritOrbs:
                    UpdatePhase2();
                    break;
                case BossPhase.Phase3_BarrelPursuit:
                    UpdatePhase3();
                    break;
            }
        }

        // PHASE 1: Navigate twisted environment, avoid Ranima's attacks
        IEnumerator Phase1Routine()
        {
            float duration = 30f; // 30 seconds of phase 1
            float timer = 0f;

            while (timer < duration && currentPhase == BossPhase.Phase1_Navigate)
            {
                // Spawn obstacles periodically
                if (timer % obstacleSpawnInterval < Time.deltaTime)
                {
                    SpawnObstacle();
                }
                timer += Time.deltaTime;
                yield return null;
            }

            if (isActive)
                TransitionToPhase(BossPhase.Phase2_SpiritOrbs);
        }

        void UpdatePhase1()
        {
            if (playerTransform == null) return;

            // Boss slowly drifts toward player
            float speed = isSlowed ? phase1MoveSpeed * 0.3f : phase1MoveSpeed;
            Vector3 dir = (playerTransform.position - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;

            // Face player
            if (bossRenderer != null)
                bossRenderer.flipX = dir.x < 0;

            // Pulse corruption effect
            if (corruptionVFX != null && !corruptionVFX.isPlaying)
                corruptionVFX.Play();
        }

        void SpawnObstacle()
        {
            if (obstaclePrefab == null || obstacleSpawnPoints == null || obstacleSpawnPoints.Length == 0)
                return;

            Transform spawnPoint = obstacleSpawnPoints[Random.Range(0, obstacleSpawnPoints.Length)];
            var obstacle = Instantiate(obstaclePrefab, spawnPoint.position, Quaternion.identity);
            Destroy(obstacle, 5f);
        }

        // PHASE 2: Use collected spirit orbs to slow and weaken the boss
        void UpdatePhase2()
        {
            if (playerTransform == null) return;

            float speed = isSlowed ? phase2MoveSpeed * 0.2f : phase2MoveSpeed;
            Vector3 dir = (playerTransform.position - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;

            if (bossRenderer != null)
                bossRenderer.flipX = dir.x < 0;

            // Player uses spirit orbs (detected via input)
            if (Input.GetKeyDown(KeyCode.F) && GameManager.Instance.SpiritOrbCount > 0)
            {
                UseOrbOnBoss();
            }

            // Check if enough orbs used to weaken
            if (phase2OrbsUsed >= 3)
            {
                TransitionToPhase(BossPhase.Phase3_BarrelPursuit);
            }
        }

        void UseOrbOnBoss()
        {
            if (GameManager.Instance.UseSpiritOrb())
            {
                phase2OrbsUsed++;
                isSlowed = true;
                TakeDamage(maxHealth * 0.15f);

                // VFX
                if (spiritOrbProjectile != null)
                {
                    var orb = Instantiate(spiritOrbProjectile, playerTransform.position, Quaternion.identity);
                    var orbMover = orb.AddComponent<OrbProjectile>();
                    orbMover.Initialize(transform, 10f);
                }

                if (hitSound != null)
                    AudioManager.Instance.PlaySFX(hitSound);

                StartCoroutine(SlowRecovery());
            }
        }

        IEnumerator SlowRecovery()
        {
            yield return new WaitForSeconds(orbSlowDuration);
            isSlowed = false;
        }

        // PHASE 3: Spiked barrel pursuit - dodge barrels and vine obstacles
        void UpdatePhase3()
        {
            if (playerTransform == null) return;

            // Boss menacingly moves toward backyard area
            Vector3 dir = (playerTransform.position - transform.position).normalized;
            transform.position += dir * (phase2MoveSpeed * 0.5f) * Time.deltaTime;

            // The spiked barrel logic is managed by spawned barrel objects
        }

        void TransitionToPhase(BossPhase newPhase)
        {
            currentPhase = newPhase;
            EventManager.Instance.Publish<BossPhase>(EventManager.Events.BossPhaseChanged, newPhase);
            OnPhaseChanged?.Invoke(newPhase);

            if (phaseTransitionSound != null)
                AudioManager.Instance.PlaySFX(phaseTransitionSound);

            // Flash screen
            EventManager.Instance.Publish(EventManager.Events.FadeOut);

            switch (newPhase)
            {
                case BossPhase.Phase2_SpiritOrbs:
                    phase2OrbsUsed = 0;
                    break;
                case BossPhase.Phase3_BarrelPursuit:
                    StartCoroutine(Phase3BarrelSequence());
                    break;
            }
        }

        IEnumerator Phase3BarrelSequence()
        {
            yield return new WaitForSeconds(1f);

            for (int i = 0; i < phase3MaxBarrels; i++)
            {
                SpawnSpikedBarrel();
                yield return new WaitForSeconds(4f);

                // Spawn vine obstacles
                SpawnVineObstacles();
                yield return new WaitForSeconds(2f);
            }

            // After surviving all barrels, defeat the boss
            // The barrel itself (vine/roots) rolls at the boss and causes their undoing
            DefeatBoss();
        }

        void SpawnSpikedBarrel()
        {
            if (spikedBarrelPrefab == null || barrelSpawnPoint == null) return;

            var barrel = Instantiate(spikedBarrelPrefab, barrelSpawnPoint.position, Quaternion.identity);
            var barrelScript = barrel.GetComponent<SpikedBarrel>();
            if (barrelScript == null)
                barrelScript = barrel.AddComponent<SpikedBarrel>();

            barrelScript.Initialize(playerTransform, barrelSpeed);
            phase3BarrelsLaunched++;
        }

        void SpawnVineObstacles()
        {
            if (vineObstaclePrefab == null) return;
            if (playerTransform == null) return; // guard against null player

            Vector3 basePos = playerTransform.position;
            for (int i = 0; i < 3; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-3f, 3f) + 5f + i * 3f, 0, 0);
                var vine = Instantiate(vineObstaclePrefab, basePos + offset, Quaternion.identity);
                Destroy(vine, 8f);
            }
        }

        public void TakeDamage(float amount)
        {
            currentHealth -= amount;
            currentHealth = Mathf.Max(0, currentHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // Flash boss sprite
            StartCoroutine(DamageFlash());

            if (currentHealth <= 0)
            {
                DefeatBoss();
            }
        }

        IEnumerator DamageFlash()
        {
            if (bossRenderer != null)
            {
                Color original = bossRenderer.color;
                bossRenderer.color = Color.white;
                yield return new WaitForSeconds(0.1f);
                bossRenderer.color = original;
            }
        }

        void DefeatBoss()
        {
            if (!isActive) return; // prevent double-call
            isActive = false;

            // Per doc: barrel rolls at boss, doesn't cause harm to player directly
            // Boss's corrupted forms fade
            StartCoroutine(DefeatSequence());
        }

        IEnumerator DefeatSequence()
        {
            if (defeatSound != null)
                AudioManager.Instance.PlaySFX(defeatSound);

            // Corruption fades
            if (corruptionVFX != null)
                corruptionVFX.Stop();

            // Boss fades
            float timer = 0f;
            float fadeDuration = 3f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                if (bossRenderer != null)
                {
                    Color c = bossRenderer.color;
                    c.a = Mathf.Lerp(1f, 0f, timer / fadeDuration);
                    bossRenderer.color = c;
                }
                yield return null;
            }

            OnBossDefeated?.Invoke();
            EventManager.Instance.Publish(EventManager.Events.BossDefeated);
            GameManager.Instance.SetPhase(GamePhase.Cutscene);

            // Transition to epilogue
            yield return new WaitForSeconds(2f);
            SceneLoader.Instance.LoadSceneWithTitle("Epilogue", "EPILOGUE", 3f);
        }
    }

}
