

using Prism.Events;

namespace Aksl.TabBits
{
    public class OnActiveTabHeaderItemEvent : PubSubEvent<OnActiveTabHeaderItemEvent>
    {
        #region Constructors
        public OnActiveTabHeaderItemEvent()
        {
        }
        #endregion

        #region Properties
        public TabInformation SelectedTabInfo { get; set; }
        #endregion
    }
}