using CorrentesDaNoite.Audio;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CorrentesDaNoite.UI
{
    public class CutscenePlayer : MonoBehaviour
    {
        const float SLIDE_ZOOM = 0.08f;
        const float SLIDE_PAN = 30f;
        const float SLIDE_MOTION_TIME = 8f;
        const float SKIP_HINT_ALPHA = 0.45f;
        const float AMBIENCE_VOLUME = 0.3f;
        const float AMBIENCE_FADE = 1f;

        public CutsceneData cutsceneData;
        public Image slideImage;
        public TextMeshProUGUI slideText;
        public CutsceneFadeController fadeController;
        public bool playOnStart = true;
        public Key advanceKey = Key.Space;
        public GamepadButton advanceGamepadButton = GamepadButton.South;
        public float minAdvanceTime = 0.5f;
        public float holdSkipTime = 0.8f;
        public bool useTypewriter = true;
        public float charactersPerSecond = 30f;
        public bool autoAdvanceEnabled = true;
        public UnityEvent onCutsceneFinished;
        public string nextSceneName;
        public bool useScreenFadeOnExit = true;
        public float exitFadeDuration = 1f;
        public Color exitFadeColor = Color.black;
        public bool showCreditsOnFinish;
        public CutsceneFadeController creditsFadeController;
        public CanvasGroup creditsCanvasGroup;
        public string menuSceneName;
        [SerializeField] TMP_Text _skipHint;
        [SerializeField] AudioSource _ambience;

        protected int currentSlideIndex;
        protected bool isPlaying;
        protected bool advanceQueued;
        protected bool autoAdvanceQueued;
        protected bool skipAll;
        protected float slideStartTime;
        protected float holdTimer;
        protected bool typingComplete = true;
        protected Coroutine typewriterRoutine;

        Vector2 _imagePosition;
        float _slideShownAt;
        float _panDirection;
        bool _hintGamepad;
        Coroutine _ambienceFade;

        protected void Start()
        {
            if (slideImage != null) _imagePosition = slideImage.rectTransform.anchoredPosition;
            if (_skipHint != null) _skipHint.alpha = 0f;
            if (playOnStart)
                StartCutscene();

            MusicManager.GetOrCreate().PlayMusic("Menu", 1.5f);
            var asc = FindFirstObjectByType<AudioStateController>();
            asc?.SetMenuState();
        }

        private void Update()
        {
            if (!isPlaying) return;
            MoveSlide();
            UpdateSkipHint();
        }

        public void StartCutscene()
        {
            if (isPlaying)
                return;

            StartCoroutine(PlayCutscene());
        }

        protected IEnumerator PlayCutscene()
        {
            if (cutsceneData == null || cutsceneData.SlideCount == 0)
                yield break;

            isPlaying = true;
            currentSlideIndex = 0;
            skipAll = false;
            advanceQueued = false;
            autoAdvanceQueued = false;
            holdTimer = 0f;
            typingComplete = true;

            if (fadeController == null)
                fadeController = GetComponent<CutsceneFadeController>();

            ApplySlide(currentSlideIndex);

            if (fadeController != null)
            {
                fadeController.SetAlpha(0f);
                bool fadedIn = false;
                fadeController.FadeIn(() => fadedIn = true);
                yield return new WaitUntil(() => fadedIn);
                slideStartTime = Time.time;
            }
            else
            {
                slideStartTime = Time.time;
            }

            while (isPlaying && !skipAll)
            {
                HandleInput();
                TryAutoAdvance();

                if ((advanceQueued || autoAdvanceQueued) && CanAdvanceSlide())
                {
                    advanceQueued = false;
                    autoAdvanceQueued = false;
                    bool hasNext = currentSlideIndex + 1 < cutsceneData.SlideCount;
                    if (hasNext)
                    {
                        yield return TransitionToSlide(currentSlideIndex + 1);
                    }
                    else
                    {
                        skipAll = true;
                    }
                }

                yield return null;
            }

            yield return FadeOutAndFinish();
        }

        protected void HandleInput()
        {
            bool pressed = WasAdvancePressedThisFrame();
            bool held = IsAdvanceHeld();

            if (pressed)
            {
                if (useTypewriter && !typingComplete)
                {
                    CompleteTypewriter();
                    return;
                }
                advanceQueued = true;
            }

            if (held)
                holdTimer += Time.deltaTime;
            else
                holdTimer = 0f;

            if (holdTimer >= holdSkipTime && CanAdvanceSlide())
                skipAll = true;
        }

        protected bool WasAdvancePressedThisFrame()
        {
            bool keyboardPressed = Keyboard.current != null && advanceKey != Key.None && Keyboard.current[advanceKey] != null && Keyboard.current[advanceKey].wasPressedThisFrame;
            bool gamepadPressed = false;

            if (Gamepad.current != null)
            {
                ButtonControl button = GetGamepadButton(Gamepad.current, advanceGamepadButton);
                gamepadPressed = button != null && button.wasPressedThisFrame;
            }

            return keyboardPressed || gamepadPressed;
        }

        protected bool IsAdvanceHeld()
        {
            bool keyboardHeld = Keyboard.current != null && advanceKey != Key.None && Keyboard.current[advanceKey] != null && Keyboard.current[advanceKey].isPressed;
            bool gamepadHeld = false;

            if (Gamepad.current != null)
            {
                ButtonControl button = GetGamepadButton(Gamepad.current, advanceGamepadButton);
                gamepadHeld = button != null && button.isPressed;
            }

            return keyboardHeld || gamepadHeld;
        }

        protected ButtonControl GetGamepadButton(Gamepad gamepad, GamepadButton button)
        {
            if (gamepad == null)
                return null;

            switch (button)
            {
                case GamepadButton.South: return gamepad.buttonSouth;
                case GamepadButton.North: return gamepad.buttonNorth;
                case GamepadButton.East: return gamepad.buttonEast;
                case GamepadButton.West: return gamepad.buttonWest;
                case GamepadButton.Start: return gamepad.startButton;
                case GamepadButton.Select: return gamepad.selectButton;
                case GamepadButton.LeftStick: return gamepad.leftStickButton;
                case GamepadButton.RightStick: return gamepad.rightStickButton;
                case GamepadButton.LeftShoulder: return gamepad.leftShoulder;
                case GamepadButton.RightShoulder: return gamepad.rightShoulder;
                case GamepadButton.DpadUp: return gamepad.dpad.up;
                case GamepadButton.DpadDown: return gamepad.dpad.down;
                case GamepadButton.DpadLeft: return gamepad.dpad.left;
                case GamepadButton.DpadRight: return gamepad.dpad.right;
                default: return null;
            }
        }

        protected bool CanAdvanceSlide()
        {
            float minTime = Mathf.Max(minAdvanceTime, cutsceneData.GetMinDisplayTime(currentSlideIndex));
            return Time.time - slideStartTime >= minTime;
        }

        protected void TryAutoAdvance()
        {
            if (!autoAdvanceEnabled)
                return;

            if (!typingComplete)
                return;

            float autoTime = cutsceneData.GetAutoAdvanceTime(currentSlideIndex);
            if (autoTime <= 0f)
                return;

            float elapsed = Time.time - slideStartTime;
            float minTime = Mathf.Max(minAdvanceTime, cutsceneData.GetMinDisplayTime(currentSlideIndex));
            float required = Mathf.Max(autoTime, minTime);

            if (elapsed >= required)
                autoAdvanceQueued = true;
        }

        protected IEnumerator TransitionToSlide(int nextIndex)
        {
            if (fadeController != null)
            {
                bool fadedOut = false;
                fadeController.FadeOut(() => fadedOut = true);
                yield return new WaitUntil(() => fadedOut);
            }

            ApplySlide(nextIndex);
            currentSlideIndex = nextIndex;

            if (fadeController != null)
            {
                bool fadedIn = false;
                fadeController.FadeIn(() => fadedIn = true);
                yield return new WaitUntil(() => fadedIn);
                slideStartTime = Time.time;
            }
            else
            {
                slideStartTime = Time.time;
            }
        }

        protected IEnumerator FadeOutAndFinish()
        {
            if (fadeController != null)
            {
                bool fadedOut = false;
                fadeController.FadeOut(() => fadedOut = true);
                yield return new WaitUntil(() => fadedOut);
            }

            CompleteCutscene();
        }

        protected void CompleteCutscene()
        {
            isPlaying = false;
            if (_skipHint != null) _skipHint.alpha = 0f;
            onCutsceneFinished?.Invoke();

            if (showCreditsOnFinish)
            {
                ShowCredits();
                return;
            }

            if (!string.IsNullOrEmpty(nextSceneName))
            {
                if (useScreenFadeOnExit && FadeController.Instance != null)
                {
                    FadeController.Instance.SetFadeColor(exitFadeColor);
                    FadeController.Instance.SetAlpha(0f);
                    FadeController.Instance.FadeOut(() => SceneManager.LoadScene(nextSceneName), exitFadeDuration);
                }
                else
                {
                    SceneManager.LoadScene(nextSceneName);
                }
            }
        }

        protected void ShowCredits()
        {
            if (creditsFadeController != null)
            {
                if (!creditsFadeController.gameObject.activeSelf)
                    creditsFadeController.gameObject.SetActive(true);

                if (creditsCanvasGroup != null)
                {
                    creditsCanvasGroup.interactable = false;
                    creditsCanvasGroup.blocksRaycasts = false;
                }

                creditsFadeController.SetAlpha(0f);
                creditsFadeController.FadeIn(() =>
                {
                    if (creditsCanvasGroup != null)
                    {
                        creditsCanvasGroup.interactable = true;
                        creditsCanvasGroup.blocksRaycasts = true;
                    }
                });
                return;
            }

            if (creditsCanvasGroup != null)
            {
                if (!creditsCanvasGroup.gameObject.activeSelf)
                    creditsCanvasGroup.gameObject.SetActive(true);

                creditsCanvasGroup.alpha = 1f;
                creditsCanvasGroup.interactable = true;
                creditsCanvasGroup.blocksRaycasts = true;
            }
        }

        protected void ApplySlide(int index)
        {
            CutsceneData.CutsceneSlide slide = cutsceneData.GetSlide(index);
            if (slide?.voice != null) Audio.AudioManager.Instance?.PlayConfig(new Audio.AudioConfig { clip = slide.voice, volume = 1f, pitch = 1f });
            if (slide?.ambience != null && _ambience != null && slide.ambience != _ambience.clip) SwapAmbience(slide.ambience);
            _slideShownAt = Time.time;
            _panDirection = index % 2 == 0 ? 1f : -1f;
            if (slideImage != null)
                slideImage.sprite = slide != null ? slide.image : null;

            if (slideText != null)
            {
                typingComplete = !useTypewriter || slide == null || string.IsNullOrEmpty(slide.text);
                if (typewriterRoutine != null)
                    StopCoroutine(typewriterRoutine);

                if (typingComplete)
                {
                    slideText.text = slide != null ? slide.text : string.Empty;
                }
                else
                {
                    slideText.text = string.Empty;
                    typewriterRoutine = StartCoroutine(TypewriterRoutine(slide.text));
                }
            }
        }

        // zoom e deslize lentos pra imagem parada parecer cena
        void MoveSlide()
        {
            if (slideImage == null) return;
            float t = Mathf.Clamp01((Time.time - _slideShownAt) / SLIDE_MOTION_TIME);
            slideImage.rectTransform.localScale = Vector3.one * (1f + SLIDE_ZOOM * t);
            slideImage.rectTransform.anchoredPosition = _imagePosition + Vector2.right * (SLIDE_PAN * _panDirection * t);
        }

        // fica mais forte enquanto segura o botao de pular
        void UpdateSkipHint()
        {
            if (_skipHint == null) return;
            bool gamepad = ActiveInputDevice.UsingGamepad;
            if (_skipHint.text.Length == 0 || gamepad != _hintGamepad)
            {
                _hintGamepad = gamepad;
                _skipHint.text = "Segure " + (gamepad ? (ActiveInputDevice.IsPlayStation ? "X" : "A") : "Espaço") + " para pular";
            }
            _skipHint.alpha = Mathf.Lerp(SKIP_HINT_ALPHA, 1f, holdTimer / holdSkipTime);
        }

        void SwapAmbience(AudioClip clip)
        {
            if (_ambienceFade != null) StopCoroutine(_ambienceFade);
            _ambienceFade = StartCoroutine(FadeAmbience(clip));
        }

        // abaixa o som do slide anterior antes de trocar
        IEnumerator FadeAmbience(AudioClip clip)
        {
            while (_ambience.isPlaying && _ambience.volume > 0f)
            {
                _ambience.volume = Mathf.MoveTowards(_ambience.volume, 0f, Time.deltaTime / AMBIENCE_FADE);
                yield return null;
            }

            _ambience.volume = 0f;
            _ambience.clip = clip;
            _ambience.Play();
            while (_ambience.volume < AMBIENCE_VOLUME)
            {
                _ambience.volume = Mathf.MoveTowards(_ambience.volume, AMBIENCE_VOLUME, Time.deltaTime / AMBIENCE_FADE);
                yield return null;
            }
        }

        protected IEnumerator TypewriterRoutine(string fullText)
        {
            typingComplete = false;
            float startTime = Time.time;
            if (charactersPerSecond <= 0f)
            {
                slideText.text = fullText;
                typingComplete = true;
                yield break;
            }

            float delay = 1f / charactersPerSecond;
            for (int i = 0; i < fullText.Length; i++)
            {
                slideText.text = fullText.Substring(0, i + 1);
                yield return new WaitForSeconds(delay);
            }

            typingComplete = true;
            slideStartTime = Mathf.Max(slideStartTime, startTime);
        }

        protected void CompleteTypewriter()
        {
            CutsceneData.CutsceneSlide slide = cutsceneData.GetSlide(currentSlideIndex);
            if (slideText == null)
            {
                typingComplete = true;
                return;
            }

            if (typewriterRoutine != null)
            {
                StopCoroutine(typewriterRoutine);
                typewriterRoutine = null;
            }

            slideText.text = slide != null ? slide.text : string.Empty;
            typingComplete = true;
            slideStartTime = Time.time;
        }
    }
}
