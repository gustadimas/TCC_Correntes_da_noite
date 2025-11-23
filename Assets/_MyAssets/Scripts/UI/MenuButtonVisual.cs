using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CorrentesDaNoite.UI
{
    public class MenuButtonVisual : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [Header("Referencias")]
        [SerializeField] private Image selectionIndicator;
        [SerializeField] private TMPro.TextMeshProUGUI labelText;

        [Header("Animacao do Indicador")]
        [SerializeField] private Sprite[] animationFrames;
        [SerializeField] private float frameRate = 12f;
        [SerializeField] private bool loopAnimation = true;

        [Header("Cores do Texto")]
        [SerializeField] private Color normalTextColor = new Color(1f, 1f, 0f, 1f);
        [SerializeField] private Color selectedTextColor = new Color(0f, 0f, 0f, 1f);

        [Header("Configuracoes")]
        [SerializeField] private bool showOnHover = true;
        [SerializeField] private bool showOnSelect = true;
        [SerializeField] private float fadeDuration = 0.2f;

        [Header("Som (Opcional)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip selectSound;

        private bool _isSelected;
        private bool _isHovered;
        private bool _isAnimating;
        private float _targetAlpha;
        private float _currentAlpha;
        private int _currentFrameIndex;
        private float _frameTimer;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();

            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.blocksRaycasts = true;

            if (selectionIndicator != null)
            {
                SetIndicatorAlpha(0f);
            }
        }

        private void Start()
        {
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
            {
                OnSelect(null);
            }
        }

        private void Update()
        {
            if (!Mathf.Approximately(_currentAlpha, _targetAlpha))
            {
                _currentAlpha = Mathf.MoveTowards(_currentAlpha, _targetAlpha, Time.deltaTime / fadeDuration);
                SetIndicatorAlpha(_currentAlpha);
            }

            if (_isAnimating && animationFrames != null && animationFrames.Length > 0)
            {
                _frameTimer += Time.deltaTime;
                float frameDuration = 1f / frameRate;

                if (_frameTimer >= frameDuration)
                {
                    _frameTimer -= frameDuration;
                    NextFrame();
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!showOnHover || (_button != null && !_button.interactable))
            {
                return;
            }

            _isHovered = true;
            Show();
            PlayHoverSound();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            if (!_isSelected)
            {
                Hide();
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (!showOnSelect || (_button != null && !_button.interactable))
            {
                return;
            }

            _isSelected = true;
            Show();
            SetTextColor(selectedTextColor);
            PlayHoverSound();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            _isSelected = false;
            SetTextColor(normalTextColor);

            if (!_isHovered)
            {
                Hide();
            }
        }

        public void PlaySelectSound()
        {
            if (audioSource != null && selectSound != null)
            {
                audioSource.PlayOneShot(selectSound);
            }
        }

        public void ForceShow()
        {
            _targetAlpha = 1f;
            _currentAlpha = 1f;
            _isAnimating = true;
            SetIndicatorAlpha(1f);
            UpdateFrame();
        }

        public void ForceHide()
        {
            _targetAlpha = 0f;
            _currentAlpha = 0f;
            _isAnimating = false;
            _currentFrameIndex = 0;
            SetIndicatorAlpha(0f);
        }

        public void SetFrameRate(float newFrameRate) => frameRate = Mathf.Max(1f, newFrameRate);
        public void SetLoop(bool loop) => loopAnimation = loop;

        private void Show()
        {
            _targetAlpha = 1f;
            _isAnimating = true;
            _currentFrameIndex = 0;
            _frameTimer = 0f;
            UpdateFrame();
        }

        private void Hide()
        {
            _targetAlpha = 0f;
            _isAnimating = false;
            _currentFrameIndex = 0;
            _frameTimer = 0f;
        }

        private void NextFrame()
        {
            _currentFrameIndex++;

            if (_currentFrameIndex >= animationFrames.Length)
            {
                _currentFrameIndex = loopAnimation ? 0 : animationFrames.Length - 1;
            }

            UpdateFrame();
        }

        private void UpdateFrame()
        {
            if (selectionIndicator != null && animationFrames != null && _currentFrameIndex < animationFrames.Length)
            {
                selectionIndicator.sprite = animationFrames[_currentFrameIndex];
            }
        }

        private void SetIndicatorAlpha(float alpha)
        {
            if (selectionIndicator == null)
            {
                return;
            }

            Color color = selectionIndicator.color;
            color.a = alpha;
            selectionIndicator.color = color;
        }

        private void PlayHoverSound()
        {
            if (audioSource != null && hoverSound != null)
            {
                audioSource.PlayOneShot(hoverSound);
            }
        }

        private void SetTextColor(Color color)
        {
            if (labelText != null)
            {
                labelText.color = color;
            }
        }

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool testShow;

        private void OnValidate()
        {
            if (testShow && Application.isPlaying)
            {
                ForceShow();
                testShow = false;
            }
        }
#endif
    }
}
