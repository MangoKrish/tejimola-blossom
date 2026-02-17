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

        void Start()
        {
            SetupButtons();
            Hide();
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

            for (int i = 0; i < chapterButtons.Length && i < chapterNames.Length; i++)
            {
                int index = i;
                if (chapterLabels != null && i < chapterLabels.Length)
                    chapterLabels[i].text = chapterNames[i];
                chapterButtons[i].onClick.AddListener(() => LoadChapter(chapterActs[index]));
            }
        }

        void Pause()
        {
            if (GameManager.Instance.CurrentPhase == GamePhase.Rhythm) return; // Don't pause during rhythm
            if (GameManager.Instance.CurrentAct == GameAct.MainMenu) return;

            isPaused = true;
            GameManager.Instance.PauseGame();
            Show();

            // Update info
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
            Hide();
        }

        void SaveGame()
        {
            SaveManager.Instance.SaveGame();
            EventManager.Instance.Publish<string>(EventManager.Events.ShowNotification, "Game Saved!");
        }

        void LoadGame()
        {
            Hide();
            isPaused = false;
            SaveManager.Instance.LoadGame();
        }

        void ShowChapterSelect()
        {
            pausePanel?.SetActive(false);
            chapterSelectPanel?.SetActive(true);

            // Enable only chapters the player has reached
            // For now, enable all chapters player has flags for
        }

        void ShowSettings()
        {
            pausePanel?.SetActive(false);
            settingsPanel?.SetActive(true);

            // Load current settings
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
            Hide();
            GameManager.Instance.SetAct(act);
            SceneLoader.Instance.LoadSceneWithTitle(act.ToString(), FormatActName(act));
        }

        void ReturnToMainMenu()
        {
            isPaused = false;
            Time.timeScale = 1f;
            Hide();
            SceneLoader.Instance.LoadScene("MainMenu");
        }

        string FormatActName(GameAct act)
        {
            return act switch
            {
                GameAct.Act1_HappyHome => "Act I: The Happy Home",
                GameAct.Act1_Funeral => "Act I: The Funeral",
                GameAct.Act2_Descent => "Act II: The Descent",
                GameAct.Act2_Dheki => "Act II: The Dheki",
                GameAct.Act2_Burial => "Act II: The Burial",
                GameAct.Act3_DomArrival => "Act III: Spirit Awakens",
                GameAct.Act3_DualTimeline => "Act III: Dual Timelines",
                GameAct.Act4_Confrontation => "Act IV: Confrontation",
                GameAct.Epilogue => "Epilogue",
                _ => "Tejimola"
            };
        }

        void Show()
        {
            pausePanel?.SetActive(true);
            chapterSelectPanel?.SetActive(false);
            settingsPanel?.SetActive(false);
        }

        void Hide()
        {
            pausePanel?.SetActive(false);
            chapterSelectPanel?.SetActive(false);
            settingsPanel?.SetActive(false);
        }
    }
}
