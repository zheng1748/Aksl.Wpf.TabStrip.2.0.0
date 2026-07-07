using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aksl.Toolkit.Controls
{
    public static class PackIconExtensions
    {
        #region To IconKind Method
        public static PackIconKind ToPackIconKind(this string iconKind)
        {
            PackIconKind kind = PackIconKind.None;

            _ = Enum.TryParse(iconKind, out kind);

            return kind;
        }
        #endregion
    }
}
