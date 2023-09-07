using UnityEngine;

namespace Broccollie.UI
{
    [DefaultExecutionOrder(-110)]
    public class GroupUI : MonoBehaviour
    {
        [Header("Group")]
        [SerializeField] private UIStates _triggerState = UIStates.Select;
        [SerializeField] private UIStates _triggeredState = UIStates.Default;
        [SerializeField] private BaseUIElement[] _elements = null;

        private void OnEnable()
        {
            switch (_triggerState)
            {
                case UIStates.Active:
                    for (int i = 0; i < _elements.Length; i++)
                        if (_elements[i].TryGetComponent<IActive>(out var active))
                            active.OnActive += UnselectOthers;
                    break;

                case UIStates.InActive:
                    for (int i = 0; i < _elements.Length; i++)
                        if (_elements[i].TryGetComponent<IActive>(out var active))
                            active.OnInActive += UnselectOthers;
                    break;

                case UIStates.Interactive:
                    for (int i = 0; i < _elements.Length; i++)
                        if (_elements[i].TryGetComponent<IInteractive>(out var interactive))
                            interactive.OnInteractive += UnselectOthers;
                    break;

                case UIStates.NonInteractive:
                    for (int i = 0; i < _elements.Length; i++)
                        if (_elements[i].TryGetComponent<IInteractive>(out var interactive))
                            interactive.OnNonInteractive += UnselectOthers;
                    break;

                case UIStates.Default:
                    for (int i = 0; i < _elements.Length; i++)
                        if (_elements[i].TryGetComponent<IDefault>(out var deFault))
                            deFault.OnDefault += UnselectOthers;
                    break;

                case UIStates.Hover:
                    for (int i = 0; i < _elements.Length; i++)
                        if (_elements[i].TryGetComponent<IHover>(out var hover))
                            hover.OnHover += UnselectOthers;
                    break;

                case UIStates.Press:
                    for (int i = 0; i < _elements.Length; i++)
                        if (_elements[i].TryGetComponent<IPress>(out var press))
                            press.OnPress += UnselectOthers;
                    break;

                case UIStates.Select:
                    for (int i = 0; i < _elements.Length; i++)
                        if (_elements[i].TryGetComponent<ISelect>(out var select))
                            select.OnSelect += UnselectOthers;
                    break;
            }
        }

        private void OnDisable()
        {
            switch (_triggerState)
            {
                case UIStates.Active:
                    for (int i = 0; i < _elements.Length; i++)
                        if (_elements[i].TryGetComponent<IActive>(out var active))
                            active.OnActive -= UnselectOthers;
                    break;

                case UIStates.InActive:
                    for (int i = 0; i < _elements.Length; i++)
                        if (_elements[i].TryGetComponent<IActive>(out var active))
                            active.OnInActive -= UnselectOthers;
                    break;

                case UIStates.Interactive:
                    for (int i = 0; i < _elements.Length; i++)
                        if (_elements[i].TryGetComponent<IInteractive>(out var interactive))
                            interactive.OnInteractive -= UnselectOthers;
                    break;

                case UIStates.NonInteractive:
                    for (int i = 0; i < _elements.Length; i++)
                        if (_elements[i].TryGetComponent<IInteractive>(out var interactive))
                            interactive.OnNonInteractive -= UnselectOthers;
                    break;

                case UIStates.Default:
                    for (int i = 0; i < _elements.Length; i++)
                        if (_elements[i].TryGetComponent<IDefault>(out var deFault))
                            deFault.OnDefault -= UnselectOthers;
                    break;

                case UIStates.Hover:
                    for (int i = 0; i < _elements.Length; i++)
                        if (_elements[i].TryGetComponent<IHover>(out var hover))
                            hover.OnHover -= UnselectOthers;
                    break;

                case UIStates.Press:
                    for (int i = 0; i < _elements.Length; i++)
                        if (_elements[i].TryGetComponent<IPress>(out var press))
                            press.OnPress -= UnselectOthers;
                    break;

                case UIStates.Select:
                    for (int i = 0; i < _elements.Length; i++)
                        if (_elements[i].TryGetComponent<ISelect>(out var select))
                            select.OnSelect -= UnselectOthers;
                    break;
            }
        }

        #region Subscribers
        private void UnselectOthers(UIEventArgs args)
        {
            for (int i = 0; i < _elements.Length; i++)
            {
                if (_elements[i] == args.Sender || !_elements[i].IsRaycastInteractive) continue;
                _ = _elements[i].ExecuteBehaviorAsync(_triggeredState, false, false);
            }
        }

        #endregion
    }
}
