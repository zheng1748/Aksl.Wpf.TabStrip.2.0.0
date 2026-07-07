using System;

using Prism.Events;
using Prism.Mvvm;

using Aksl.Infrastructure;
using Aksl.Toolkit.Controls;

namespace Aksl.Modules.HamburgerMenuNavigationSideBar.ViewModels
{
    public class MenuItemHeaderViewModel : BindableBase
    {
        #region Members
        private readonly MenuItemViewModel _headerMenuItemViewModel;
        #endregion

        #region Constructors
        public MenuItemHeaderViewModel(MenuItemViewModel headerMenuItemViewModel)
        {
            _headerMenuItemViewModel = headerMenuItemViewModel;

            HeaderTitle = headerMenuItemViewModel.Title;
            IconKind= headerMenuItemViewModel.IconKind;
        }
        #endregion

        #region Properties
        private string _headerTitle;
        public string HeaderTitle
        {
            get => _headerTitle;
            set => SetProperty<string>(ref _headerTitle, value);
        }

        private PackIconKind _iconKind;
        public PackIconKind IconKind
        {
            get => _iconKind;
            set => SetProperty<PackIconKind>(ref _iconKind, value);
        }

        //public PackIconKind IconKind
        //{
        //    get
        //    {
        //        PackIconKind kind = PackIconKind.None;

        //        _ = Enum.TryParse(_menuItem.IconKind, out kind);

        //        return kind;
        //    }
        //}
        #endregion
    }
}
