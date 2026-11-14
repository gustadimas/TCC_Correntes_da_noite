using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using CorrentesDaNoite.Audio;

namespace CorrentesDaNoite.UI
{
    public class OptionSelector : Selectable, IPointerClickHandler
    {
        [SerializeField] TMP_Text _valueText;

        string[] _options = Array.Empty<string>();
        int _index;

        public event Action<int> Changed;

        public void Setup(string[] options, int index)
        {
            _options = options;
            _index = Mathf.Clamp(index, 0, options.Length - 1);
            _valueText.text = _options[_index];
        }

        public override void OnMove(AxisEventData eventData)
        {
            if (eventData.moveDir == MoveDirection.Left) Step(-1);
            else if (eventData.moveDir == MoveDirection.Right) Step(1);
            else base.OnMove(eventData);
        }

        // metade esquerda volta e a direita avanca
        public void OnPointerClick(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, eventData.position, eventData.pressEventCamera, out Vector2 point);
            Step(point.x < 0f ? -1 : 1);
        }

        // o mouse seleciona igual o ReactiveButton
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            if (IsInteractable()) EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            AudioManager.Instance?.PlayEvent(AudioEvent.UIHover, null);
        }

        void Step(int direction)
        {
            if (_options.Length < 2) return;
            _index = (_index + direction + _options.Length) % _options.Length;
            _valueText.text = _options[_index];
            AudioManager.Instance?.PlayEvent(AudioEvent.UIClick, null);
            Changed?.Invoke(_index);
        }
    }
}