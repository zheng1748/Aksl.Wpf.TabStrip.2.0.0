

using Prism.Events;

namespace Aksl.Tabs
{
    public class OnActiveTabItemEvent : PubSubEvent<OnActiveTabItemEvent>
    {
        #region Constructors
        public OnActiveTabItemEvent()
        {
        }
        #endregion

        #region Properties
        public TabInformation SelectedTabInfo { get; set; }
        #endregion
    }
}