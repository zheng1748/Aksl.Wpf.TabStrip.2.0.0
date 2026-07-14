

using Prism.Events;

namespace Aksl.TabStripPlus
{
    public class OnActiveTabHeaderItemEvent : PubSubEvent<OnActiveTabHeaderItemEvent>
    {
        #region Constructors
        public OnActiveTabHeaderItemEvent()
        {
        }
        #endregion

        #region Properties
        public TabPlusInformation SelectedTabInfo { get; set; }
        #endregion
    }
}