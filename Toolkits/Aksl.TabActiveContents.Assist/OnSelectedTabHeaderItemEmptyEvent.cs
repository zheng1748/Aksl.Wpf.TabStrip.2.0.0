
using Prism.Events;

namespace Aksl.TabBits
{
    public class OnSelectedTabHeaderItemEmptyEvent : PubSubEvent<OnSelectedTabHeaderItemEmptyEvent>
    {
        #region Constructors
        public OnSelectedTabHeaderItemEmptyEvent()
        {
            IsEmpty=true;
        }
        #endregion

        #region Properties
        public bool IsEmpty { get; set; }
        #endregion
    }
}