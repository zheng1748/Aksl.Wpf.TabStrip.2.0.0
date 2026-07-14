using System.Collections.Generic;
using System.Windows;

namespace Aksl.TabStripPlus
{
    public class TabPlusInformation
    {
        #region Constructors
        public TabPlusInformation()
        {
        }
        #endregion

        #region Properties

        public string Name { get; set; }

        public string Title { get; set; }

        public string IconKind { get; set; }

        public string ViewName { get; set; }

        public DependencyObject ViewElement { get; set; }

        public Visibility CloseTabButtonVisibility { get; set; } = Visibility.Visible;
        #endregion
    }
}
