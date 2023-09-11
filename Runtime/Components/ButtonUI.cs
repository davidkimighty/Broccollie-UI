using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Broccollie.UI
{
    public class ButtonUI : BaseUIElement, IActive, IInteractive, IDefault, IHover, IPress, ISelect, INavigate
    {
        private static readonly List<BaseUIElement> s_activeButtons = new();

        public event Action<UIEventArgs> OnActive;
        public event Action<UIEventArgs> OnInActive;
        public event Action<UIEventArgs> OnInteractive;
        public event Action<UIEventArgs> OnNonInteractive;
        public event Action<UIEventArgs> OnDefault;
        public event Action<UIEventArgs> OnHover;
        public event Action<UIEventArgs> OnPress;
        public event Action<UIEventArgs> OnSelect;
        public event Action<UIEventArgs, MoveDirection> OnNavigate;

        [SerializeField] private ButtonTypes _buttonType = ButtonTypes.Button;

        private bool _isInteractive = true;
        public bool IsInteractive
        {
            get => _isInteractive;
        }
        private bool _isHovered = false;
        public bool IsHovered
        {
            get => _isHovered;
        }
        private bool _isPressed = false;
        public bool IsPressed
        {
            get => _isPressed;
        }
        private bool _isSelected = false;
        public bool IsSelected
        {
            get => IsSelected;
        }

        private void OnEnable()
        {
            s_activeButtons.Add(this);
        }

        private void OnDisable()
        {
            s_activeButtons.Remove(this);
        }

        #region Public Functions
        public async Task SetActiveAsync(bool state, bool instantChange = false, bool playAudio = true, bool invokeEvent = true)
        {
            if (state)
                gameObject.SetActive(true);
            _currentState = state ? UIStates.Active : UIStates.InActive;

            if (_features != null && _features.Count > 0)
            {
                CancelCancellationToken();
                await this.ExecuteBehaviorAsync(_currentState, instantChange, playAudio);
                if (state)
                    await this.ExecuteBehaviorAsync(UIStates.Default, instantChange, playAudio);
            }

            if (state)
                OnActive?.Invoke(new UIEventArgs { Sender = this });
            else
            {
                OnInActive?.Invoke(new UIEventArgs { Sender = this });
                gameObject.SetActive(false);
            }
        }

        public async Task SetInteractiveAsync(bool state, bool instantChange = false, bool playAudio = true, bool invokeEvent = true)
        {
            _isInteractive = state;
            _currentState = state ? UIStates.Interactive : UIStates.NonInteractive;

            if (_features != null && _features.Count > 0)
            {
                CancelCancellationToken();
                await this.ExecuteBehaviorAsync(_currentState, instantChange, playAudio);
                if (state)
                    await this.ExecuteBehaviorAsync(UIStates.Default, instantChange, playAudio);
            }

            if (state)
                OnInteractive?.Invoke(new UIEventArgs { Sender = this });
            else
                OnNonInteractive?.Invoke(new UIEventArgs { Sender = this });
        }

        public async Task DefaultAsync(bool instantChange = false, bool playAudio = true, bool invokeEvent = true)
        {
            _isSelected = false;
            _currentState = UIStates.Default;

            if (_features != null && _features.Count > 0)
            {
                CancelCancellationToken();
                await this.ExecuteBehaviorAsync(UIStates.Default, instantChange, playAudio);
            }

            if (invokeEvent)
                OnDefault?.Invoke(new UIEventArgs { Sender = this });
        }

        public async Task HoverAsync(bool instantChange = false, bool playAudio = true, bool invokeEvent = true)
        {
            _isHovered = true;
            _currentState = UIStates.Hover;

            if (_features != null && _features.Count > 0)
            {
                CancelCancellationToken();
                await this.ExecuteBehaviorAsync(UIStates.Hover, instantChange, playAudio);
            }

            if (invokeEvent)
                OnHover?.Invoke(new UIEventArgs { Sender = this });
        }

        public async Task PressAsync(bool instantChange = false, bool playAudio = true, bool invokeEvent = true)
        {
            _currentState = UIStates.Press;

            if (_features != null && _features.Count > 0)
            {
                CancelCancellationToken();
                await this.ExecuteBehaviorAsync(UIStates.Press, instantChange, playAudio);
            }

            if (invokeEvent)
                OnPress?.Invoke(new UIEventArgs { Sender = this });
        }

        public async Task SelectAsync(bool instantChange = false, bool playAudio = true, bool invokeEvent = true)
        {
            switch (_buttonType)
            {
                case ButtonTypes.Button:
                    ButtonBehavior();
                    break;

                case ButtonTypes.Checkbox:
                    await CheckboxBehaviorAsync();
                    break;

                case ButtonTypes.Radio:
                    await RadioButtonBehaviorAsync();
                    break;
            }

            void ButtonBehavior()
            {
                if (invokeEvent)
                    OnSelect?.Invoke(new UIEventArgs { Sender = this });
            }

            async Task CheckboxBehaviorAsync()
            {
                bool usingFeature = _features != null && _features.Count > 0;
                if (usingFeature)
                    CancelCancellationToken();

                _isSelected = !_isSelected;
                if (_isSelected)
                {
                    _currentState = UIStates.Select;
                    if (usingFeature)
                        await this.ExecuteBehaviorAsync(UIStates.Select, instantChange, playAudio);

                    if (invokeEvent)
                        OnSelect?.Invoke(new UIEventArgs { Sender = this });
                }
                else
                {
                    _currentState = UIStates.Default;
                    if (usingFeature)
                        await this.ExecuteBehaviorAsync(UIStates.Default, instantChange, playAudio);

                    if (invokeEvent)
                        OnDefault?.Invoke(new UIEventArgs { Sender = this });
                }
            }

            async Task RadioButtonBehaviorAsync()
            {
                if (_isSelected) return;
                _isSelected = true;
                _currentState = UIStates.Select;

                if (_features != null && _features.Count > 0)
                {
                    CancelCancellationToken();
                    await this.ExecuteBehaviorAsync(UIStates.Select, instantChange, playAudio);
                }

                if (invokeEvent)
                    OnSelect?.Invoke(new UIEventArgs { Sender = this });
            }
        }

        public void Navigate(MoveDirection direction, bool invokeEvent = false)
        {
            this.MoveToNextSelectable(new(EventSystem.current) { moveDir = direction }, s_activeButtons);

            if (invokeEvent)
                OnNavigate?.Invoke(new UIEventArgs { Sender = this }, direction);
        }

        public void Navigate(AxisEventData eventData, bool invokeEvent = false)
        {
            this.MoveToNextSelectable(eventData, s_activeButtons);

            if (invokeEvent)
                OnNavigate?.Invoke(new UIEventArgs { Sender = this }, eventData.moveDir);
        }

        #endregion

        #region Pointer Callbacks
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (!_isRaycastInteractive || !_isInteractive) return;
            _ = HoverAsync();
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (!_isRaycastInteractive || !_isInteractive) return;
            _ = ExitAsync();
        }

        void ISelectHandler.OnSelect(BaseEventData eventData)
        {
            if (!_isRaycastInteractive || !_isInteractive) return;
            _ = HoverAsync();
        }

        void IDeselectHandler.OnDeselect(BaseEventData eventData)
        {
            if (!_isRaycastInteractive || !_isInteractive) return;
            _ = ExitAsync();
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (!_isRaycastInteractive || !_isInteractive) return;
            _ = PressAsync();
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (!_isRaycastInteractive || !_isInteractive) return;
            _ = ReleaseAsync();
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (!_isRaycastInteractive || !_isInteractive) return;
            _ = SelectAsync();
        }

        void ISubmitHandler.OnSubmit(BaseEventData eventData)
        {
            if (!_isRaycastInteractive || !_isInteractive) return;
            Select2Default();

            async void Select2Default()
            {
                await SelectAsync(false, true, true);
                if (_buttonType == ButtonTypes.Button)
                    await DefaultAsync();
            }
        }

        void IMoveHandler.OnMove(AxisEventData eventData)
        {
            if (!_isRaycastInteractive || !_isInteractive) return;
            Navigate(eventData);
        }

        #endregion

        private async Task ExitAsync(bool instantChange = false, bool playAudio = true)
        {
            _isHovered = false;
            if (_isPressed) return;

            await SelectOrDefaultBehavior(instantChange, playAudio, false);
        }

        private async Task ReleaseAsync(bool instantChange = false, bool playAudio = true)
        {
            _isPressed = false;
            await SelectOrDefaultBehavior(instantChange, playAudio, false);
        }

        private async Task SelectOrDefaultBehavior(bool instantChange, bool playAudio, bool invokeEvent)
        {
            bool usingFeature = _features != null && _features.Count > 0;
            if (usingFeature)
            {
                CancelCancellationToken();
                if (_isSelected)
                {
                    if (usingFeature)
                        await this.ExecuteBehaviorAsync(UIStates.Select, instantChange, playAudio);

                    if (invokeEvent)
                        OnSelect?.Invoke(new UIEventArgs { Sender = this });
                }
                else
                {
                    _currentState = UIStates.Default;
                    if (usingFeature)
                        await this.ExecuteBehaviorAsync(UIStates.Default, instantChange, playAudio);

                    if (invokeEvent)
                        OnDefault?.Invoke(new UIEventArgs { Sender = this });
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            //ChangeUIStateEditor();
        }

        public override void ChangeUIStateEditor()
        {
            switch (_currentState)
            {
                case UIStates.InActive:
                    _isHovered = _isPressed = _isSelected = false;
                    _ = this.ExecuteBehaviorAsync(UIStates.InActive, true, false);
                    gameObject.SetActive(false);
                    break;

                case UIStates.NonInteractive:
                    _isInteractive = _isHovered = _isPressed = _isSelected = false;
                    _ = this.ExecuteBehaviorAsync(UIStates.NonInteractive, true, false);
                    gameObject.SetActive(true);
                    break;

                case UIStates.Default:
                    _isHovered = _isPressed = _isSelected = false;
                    _ = this.ExecuteBehaviorAsync(UIStates.Default, true, false);
                    gameObject.SetActive(true);
                    break;

                case UIStates.Hover:
                    _isHovered = true;
                    _isPressed = _isSelected = false;
                    _ = this.ExecuteBehaviorAsync(UIStates.Hover, true, false);
                    gameObject.SetActive(true);
                    break;

                case UIStates.Press:
                    _isPressed = true;
                    _isHovered = _isSelected = false;
                    _ = this.ExecuteBehaviorAsync(UIStates.Press, true, false);
                    gameObject.SetActive(true);
                    break;

                case UIStates.Select:
                    _isSelected = true;
                    _isHovered = _isPressed = false;
                    _ = this.ExecuteBehaviorAsync(UIStates.Select, true, false);
                    gameObject.SetActive(true);
                    break;
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }

    public enum ButtonTypes { Button, Checkbox, Radio }
}
