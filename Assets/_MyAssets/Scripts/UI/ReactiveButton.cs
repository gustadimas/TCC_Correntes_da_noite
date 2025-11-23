using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CorrentesDaNoite.UI
{
    public class ReactiveButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler
    {
        [Header("Sprite Settings")]
        [SerializeField] bool useSprite = true;
        [SerializeField] Sprite normalSprite;
        [SerializeField] Sprite hoverSprite;
        [SerializeField] Sprite clickSprite;
        [SerializeField] Sprite selectSprite;

        [Header("Toggle Image Component")]
        [Tooltip("Ativa/desativa o componente Image de outro GameObject quando interage com o botao")]
        [SerializeField] Image imageToToggle;
        [SerializeField] bool enableImageOnHover;
        [SerializeField] bool enableImageOnSelect;
        [SerializeField] bool enableImageOnClick = false;

        [Header("Color Settings")]
        [SerializeField] private bool useColor;
        [SerializeField] Color normalColor = Color.white;
        [SerializeField] Color hoverColor = Color.gray;
        [SerializeField] Color clickColor = Color.green;
        [SerializeField] Color selectColor = Color.blue;

        [Header("Scaling Settings")]
        [SerializeField] private bool useScaling;
        [SerializeField] Vector3 normalScale = Vector3.one;
        [SerializeField] Vector3 hoverScale = Vector3.one * 1.1f;
        [SerializeField] Vector3 clickScale = Vector3.one * 0.9f;
        [SerializeField] Vector3 selectScale = Vector3.one * 1.05f;
        [SerializeField] float scaleTransitionSpeed = 10f;

        Image _buttonImage;
        TextMeshProUGUI _buttonText;
        RectTransform _rectTransform;
        Vector3 _targetScale;
        bool _isSelected;

        private void Awake()
        {
            _buttonImage = GetComponent<Image>();
            _buttonText = GetComponentInChildren<TextMeshProUGUI>();
            _rectTransform = GetComponent<RectTransform>();

            _targetScale = normalScale;
            ApplyNormalState();
        }

        private void Update()
        {
            if (useScaling && _rectTransform != null)
            {
                _rectTransform.localScale = Vector3.Lerp(_rectTransform.localScale, _targetScale, Time.unscaledDeltaTime * scaleTransitionSpeed);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ApplyHoverState();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isSelected)
            {
                ApplySelectState();
            }
            else
            {
                ApplyNormalState();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ApplyClickState();
        }

        public void OnSelect(BaseEventData eventData)
        {
            _isSelected = true;
            ApplySelectState();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            _isSelected = false;
            ApplyNormalState();
        }

        void ApplyNormalState()
        {
            if (useSprite && _buttonImage != null)
            {
                _buttonImage.sprite = normalSprite;
            }

            if (useColor)
            {
                if (!useSprite && _buttonText != null)
                {
                    _buttonText.color = normalColor;
                }
                else if (_buttonImage != null)
                {
                    _buttonImage.color = normalColor;
                }
            }

            if (useScaling)
            {
                _targetScale = normalScale;
            }

            if (imageToToggle != null)
            {
                imageToToggle.enabled = false;
            }
        }

        void ApplyHoverState()
        {
            if (useSprite && _buttonImage != null)
            {
                _buttonImage.sprite = hoverSprite;
            }

            if (useColor)
            {
                if (!useSprite && _buttonText != null)
                {
                    _buttonText.color = hoverColor;
                }
                else if (_buttonImage != null)
                {
                    _buttonImage.color = hoverColor;
                }
            }

            if (useScaling)
            {
                _targetScale = hoverScale;
            }

            if (imageToToggle != null && enableImageOnHover)
            {
                imageToToggle.enabled = true;
            }
        }

        void ApplyClickState()
        {
            if (useSprite && _buttonImage != null)
            {
                _buttonImage.sprite = clickSprite;
            }

            if (useColor)
            {
                if (!useSprite && _buttonText != null)
                {
                    _buttonText.color = clickColor;
                }
                else if (_buttonImage != null)
                {
                    _buttonImage.color = clickColor;
                }
            }

            if (useScaling)
            {
                _targetScale = clickScale;
            }

            if (imageToToggle != null && enableImageOnClick)
            {
                imageToToggle.enabled = true;
            }
        }

        void ApplySelectState()
        {
            if (useSprite && _buttonImage != null)
            {
                _buttonImage.sprite = selectSprite;
            }

            if (useColor)
            {
                if (!useSprite && _buttonText != null)
                {
                    _buttonText.color = selectColor;
                }
                else if (_buttonImage != null)
                {
                    _buttonImage.color = selectColor;
                }
            }

            if (useScaling)
            {
                _targetScale = selectScale;
            }

            if (imageToToggle != null && enableImageOnSelect)
            {
                imageToToggle.enabled = true;
            }
        }
    }
}