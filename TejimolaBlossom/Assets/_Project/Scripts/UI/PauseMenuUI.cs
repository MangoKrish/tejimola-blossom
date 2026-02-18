using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tejimola.Core;
using Tejimola.Utils;

namespace Tejimola.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject chapterSelectPanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button chapterSelectButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Chapter Select")]
        [SerializeField] private Button[] chapterButtons;
        [SerializeField] private TextMeshProUGUI[] chapterLabels;

        [Header("Info")]
        [SerializeField] private TextMeshProUGUI currentChapterText;
        [SerializeField] private TextMeshProUGUI playTimeText;

        [Header("Settings")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Toggle subtitlesToggle;

        private bool isPaused;
        private float playTime;
        private bool buttonsSetUp;

        void Awake()
        {
            // Set up buttons in Awake so they work even if this GO starts inactive
            SetupButtons();
            HidePanels();
        }

        void OnEnable()
        {
            // Re-setup buttons if needed (e.g. after scene reload)
            if (!buttonsSetUp)
                SetupButtons();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused)
                    Resume();
                else
                    Pause();
            }

            if (!isPaused)
                playTime += Time.deltaTime;
        }

        void SetupButtons()
        {
            if (buttonsSetUp) return;
            buttonsSetUp = true;

            resumeButton?.onClick.AddListener(Resume);
            saveButton?.onClick.AddListener(SaveGame);
            loadButton?.onClick.AddListener(LoadGame);
            chapterSelectButton?.onClick.AddListener(ShowChapterSelect);
            settingsButton?.onClick.AddListener(ShowSettings);
            mainMenuButton?.onClick.AddListener(ReturnToMainMenu);

            // Chapter buttons
            string[] chapterNames = { "Act I: The Happy Home", "Act II: The Descent",
                                       "Act III: Spirit Awakens", "Act IV: Confrontation", "Epilogue" };
            GameAct[] chapterActs = { GameAct.Act1_HappyHome, GameAct.Act2_Descent,
                                      GameAct.Act3_DomArrival, GameAct.Act4_Confrontation, GameAct.Epilogue };

            if (chapterButtons != null)
            {
                for (int i = 0; i < chapterButtons.Length && i < chapterNames.Length; i++)
                {
                    if (chapterButtons[i] == null) continue;
                    int index = i;
                    if (chapterLabels != null && i < chapterLabels.Length && chapterLabels[i] != null)
                        chapterLabels[i].text = chapterNames[i];
                    chapterButtons[i].onClick.AddListener(() => LoadChapter(chapterActs[index]));
                }
            }
        }

        void Pause()
        {
            if (GameManager.Instance.CurrentPhase == GamePhase.Rhythm) return;
            if (GameManager.Instance.CurrentAct == GameAct.MainMenu) return;

            isPaused = true;
            GameManager.Instance.PauseGame();
            Show();

            if (currentChapterText != null)
                currentChapterText.text = $"Chapter: {FormatActName(GameManager.Instance.CurrentAct)}";
            if (playTimeText != null)
            {
                int hours = (int)(playTime / 3600);
                int minutes = (int)((playTime % 3600) / 60);
                int seconds = (int)(playTime % 60);
                playTimeText.text = $"{hours:00}:{minutes:00}:{seconds:00}";
            }
        }

        void Resume()
        {
            isPaused = false;
            GameManager.Instance.ResumeGame();
            HidePanels();
        }

        void SaveGame()
        {
            SaveManager.Instance.SaveGame();
            EventManager.Instance.Publish<string>(EventManager.Events.ShowNotification, "Game Saved!");
        }

        void LoadGame()
        {
            HidePanels();
            isPaused = false;
            SaveManager.Instance.LoadGame();
        }

        void ShowChapterSelect()
        {
            pausePanel?.SetActive(false);
            chapterSelectPanel?.SetActive(true);

            // Enable only chapters the player has reached
            if (chapterButtons != null)
            {
                GameAct[] acts = { GameAct.Act1_HappyHome, GameAct.Act2_Descent,
                                   GameAct.Act3_DomArrival, GameAct.Act4_Confrontation, GameAct.Epilogue };
                for (int i = 0; i < chapterButtons.Length && i < acts.Length; i++)
                {
                    if (chapterButtons[i] == null) continue;
                    // Enable act if it's the first act or player has reached/passed this act
                    bool unlocked = (int)GameManager.Instance.CurrentAct >= (int)acts[i] || i == 0;
                    chapterButtons[i].interactable = unlocked;
                }
            }
        }

        void ShowSettings()
        {
            pausePanel?.SetActive(false);
            settingsPanel?.SetActive(true);

            GameSettings settings = SaveManager.Instance.LoadSettings();
            if (masterVolumeSlider != null) masterVolumeSlider.value = settings.masterVolume;
            if (musicVolumeSlider != null) musicVolumeSlider.value = settings.musicVolume;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = settings.sfxVolume;
            if (subtitlesToggle != null) subtitlesToggle.isOn = settings.subtitlesEnabled;
        }

        public void BackToPause()
        {
            chapterSelectPanel?.SetActive(false);
            settingsPanel?.SetActive(false);
            pausePanel?.SetActive(true);
        }

        void LoadChapter(GameAct act)
        {
            isPaused = false;
            Time.timeScale = 1f;
            HidePanels();

            // Reset relevant game state for clean chapter start
            GameManager.Instance.SetAct(act);
            string sceneName = ActToSceneName(act);
            SceneLoader.Instance.LoadSceneWithTitle(sceneName, FormatActName(act));
        }

        void ReturnToMainMenu()
        {
            isPaused = false;
            Time.timeScale = 1f;
            HidePanels();
            SceneLoader.Instance.LoadScene("MainMenu");
        }

        void Show()
        {
            pausePanel?.SetActive(true);
            chapterSelectPanel?.SetActive(false);
            settingsPanel?.SetActive(false);
        }

        void HidePanels()
        {
            pausePanel?.SetActive(false);
            chapterSelectPanel?.SetActive(false);
            settingsPanel?.SetActive(false);
        }

        string ActToSceneName(GameAct act)
        {
            return act switch
            {
                GameAct.Act1_HappyHome   => "Act1_HappyHome",
                GameAct.Act1_Funeral     => "Act1_Funeral",
                GameAct.Act2_Descent     => "Act2_Descent",
                GameAct.Act2_Dheki       => "Act2_Dheki",
                GameAct.Act2_Burial      => "Act2_Burial",
                GameAct.Act3_DomArrival  => "Act3_DomArrival",
                GameAct.Act3_DualTimeline => "Act3_DualTimeline",
                GameAct.Act4_Confrontation => "Act4_Confrontation",
                GameAct.Epilogue         => "Epilogue",
                _                        => "MainMenu"
            };
        }

        string FormatActName(GameAct act)
        {
            return act switch
            {
                GameAct.Act1_HappyHome      => "Act I: The Happy Home",
                GameAct.Act1_Funeral        => "Act I: The Funeral",
                GameAct.Act2_Descent        => "Act II: The Descent",
                GameAct.Act2_Dheki          => "Act II: The Dheki",
                GameAct.Act2_Burial         => "Act II: The Burial",
                GameAct.Act3_DomArrival     => "Act III: Spirit Awakens",
                GameAct.Act3_DualTimeline   => "Act III: Dual Timelines",
                GameAct.Act4_Confrontation  => "Act IV: Confrontation",
                GameAct.Epilogue            => "Epilogue",
                _                           => "Tejimola"
            };
        }
    }
}
