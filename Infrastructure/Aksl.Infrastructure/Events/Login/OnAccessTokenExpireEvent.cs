using System;

using Prism.Events;

namespace Aksl.Infrastructure.Events
{
    public class OnAccessTokenExpiredEvent : PubSubEvent<OnAccessTokenExpiredEvent>
    {
        #region Constructors
        public OnAccessTokenExpiredEvent()
        {
            IsExpired=true;
        }
        #endregion

        #region Properties
        public bool IsExpired { get; set; }
        #endregion
    }
}
