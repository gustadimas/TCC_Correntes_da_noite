using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using CorrentesDaNoite.Audio;

namespace CorrentesDaNoite.UI
{
    public class ReactiveButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
    {
        [Header("Sprite Settings")]
        [SerializeField] bool useSprite = true;
        [SerializeField] Sprite normalSprite;
        [SerializeField] Sprite clickSprite;
        [SerializeField] Sprite selectSprite;

        [Header("Toggle Image Component")]
        [Tooltip("Ativa/desativa o componente Image de outro GameObject quando interage com o botao")]
        [SerializeField] Image imageToToggle;
        [SerializeField] bool enableImageOnSelect;
        [SerializeField] bool enableImageOnClick = false;

        [Header("Color Settings")]
        [SerializeField] private bool useColor;
        [SerializeField] Color normalColor = Color.white;
        [SerializeField] Color clickColor = Color.green;
        [SerializeField] Color selectColor = Color.blue;

        [Header("Audio")]
        [SerializeField] AudioEvent hoverAudioEvent = AudioEvent.UIHover;
        [SerializeField] AudioEvent clickAudioEvent = AudioEvent.UIClick;

        [Header("Scaling Settings")]
        [SerializeField] private bool useScaling;
        [SerializeField] Vector3 normalScale = Vector3.one;
        [SerializeField] Vector3 clickScale = Vector3.one * 0.9f;
        [SerializeField] Vector3 selectScale = Vector3.one * 1.05f;
        [SerializeField] float scaleTransitionSpeed = 10f;

        Image _buttonImage;
        TextMeshProUGUI _buttonText;
        RectTransform _rectTransform;
        Selectable _selectable;
        Vector3 _targetScale;
        bool _isSelected;

        private void Awake()
        {
            _buttonImage = GetComponent<Image>();
            _buttonText = GetComponentInChildren<TextMeshProUGUI>();
            _rectTransform = GetComponent<RectTransform>();
            _selectable = GetComponent<Selectable>();

            _targetScale = normalScale;
            ApplyNormalState();
        }

        private void Update()
        {
            if (useScaling) _rectTransform.localScale = Vector3.Lerp(_rectTransform.localScale, _targetScale, Time.unscaledDeltaTime * scaleTransitionSpeed);
        }

        // o mouse seleciona o botao pra nao ficar um destacado pelo teclado e outro pelo mouse
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_selectable != null && _selectable.interactable) EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isSelected) ApplySelectState();
            else ApplyNormalState();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ApplyClickState();
            PlayAudio(clickAudioEvent);
        }

        public void OnSubmit(BaseEventData eventData) => PlayAudio(clickAudioEvent);

        public void OnSelect(BaseEventData eventData)
        {
            _isSelected = true;
            ApplySelectState();
            PlayAudio(hoverAudioEvent);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            _isSelected = false;
            ApplyNormalState();
        }

        void ApplyNormalState() => ApplyState(normalSprite, normalColor, normalScale, false);
        void ApplySelectState() => ApplyState(selectSprite, selectColor, selectScale, enableImageOnSelect);
        void ApplyClickState() => ApplyState(clickSprite, clickColor, clickScale, enableImageOnClick);

        void ApplyState(Sprite sprite, Color color, Vector3 scale, bool showImage)
        {
            if (useSprite && _buttonImage != null) _buttonImage.sprite = sprite;
            if (useColor && !useSprite && _buttonText != null) _buttonText.color = color;
            else if (useColor && _buttonImage != null) _buttonImage.color = color;
            if (useScaling) _targetScale = scale;
            if (imageToToggle != null) imageToToggle.enabled = showImage;
        }

        void PlayAudio(AudioEvent audioEvent) => AudioManager.Instance?.PlayEvent(audioEvent, null);
    }
}