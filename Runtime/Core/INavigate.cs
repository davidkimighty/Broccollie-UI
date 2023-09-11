using System;
using UnityEngine.EventSystems;

namespace Broccollie.UI
{
    public interface INavigate : IMoveHandler 
    {
        public event Action<UIEventArgs, MoveDirection> OnNavigate;

        public void Navigate(MoveDirection direction, bool invokeEvent = true);

        public void Navigate(AxisEventData eventData, bool invokeEvent = true);
    }
}
