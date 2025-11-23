using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem.Controls;

namespace CorrentesDaNoite.UI
{
    public class CutscenePlayer : MonoBehaviour
    {
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

        protected int currentSlideIndex;
        protected bool isPlaying;
        protected bool advanceQueued;
        protected bool autoAdvanceQueued;
        protected bool skipAll;
        protected float slideStartTime;
        protected float holdTimer;
        protected bool typingComplete = true;
        protected Coroutine typewriterRoutine;

        protected void Start()
        {
            if (playOnStart)
                StartCutscene();
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

            if (Gamepad.current != null && advanceGamepadButton != GamepadButton.None)
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

            if (Gamepad.current != null && advanceGamepadButton != GamepadButton.None)
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
