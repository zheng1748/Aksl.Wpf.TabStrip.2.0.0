using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Aksl.ActiveContents
{
    public record ContentInformation
    {
        #region Constructors
        public ContentInformation()
        {
        }
        #endregion

        #region Properties

        public string Name { get; set; }

        public string Title { get; set; }

        public string ViewName { get; set; }

        public DependencyObject ViewElement { get; set; }
        #endregion
    }
}
