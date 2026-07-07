using System.Collections.Generic;
using System.Collections.ObjectModel;

using Prism.Events;
using Prism.Mvvm;

using Aksl.Infrastructure;

namespace Aksl.Modules.HamburgerMenuNavigationSideBarTab.ViewModels
{
    public class NoGroupedMenuViewModel : GroupedMenuViewModelBase
    {
        #region Members
        //private readonly MenuItem _menuItem;
        //private IEnumerable<MenuItemViewModel> _leafMenuItemViewModels;
        #endregion

        #region Constructors
        //public NoGroupedMenuViewModel(int index, MenuItem menuItems)
        //{
        //    Index = index;
        //    _menuItem = menuItems;

        //    NoGroupedMenuItems = new();
        //}

        public NoGroupedMenuViewModel(int index, IEnumerable<MenuItemViewModel> leafMenuItemViewModels)
        {
            Index = index;
            NoGroupedMenuItems = new(leafMenuItemViewModels);

            //NoGroupedMenuItems = new();

            IsGrouped = false;
        }
        #endregion

        #region Properties
        public int Index { get; }

        //  public ObservableCollection<NoGroupedMenuItemViewModel> NoGroupedMenuItems { get; private set; }
        public ObservableCollection<MenuItemViewModel> NoGroupedMenuItems { get; private set; }

        public bool IsGrouped { get; set; }

        //private NoGroupedMenuItemViewModel _selectedNoGroupedMenuItem;
        //public NoGroupedMenuItemViewModel SelectedNoGroupedMenuItem
        //{
        //    get => _selectedNoGroupedMenuItem;
        //    set => SetProperty(ref _selectedNoGroupedMenuItem, value);
        //}

        public MenuItemViewModel SelectedNoGroupedMenuItem
        {
            get => field;
            set => SetProperty(ref field, value);
        }

        private bool _isPaneOpen = false;
        public bool IsPaneOpen
        {
            get => field;
            set
            {
                if (SetProperty<bool>(ref field, value))
                {
                    foreach (var ngmivm in NoGroupedMenuItems)
                    {
                        ngmivm.IsPaneOpen = field;
                    }
                }
            }
        }

        //private bool _isLoading;
        //public bool IsLoading
        //{
        //    get => _isLoading;
        //    set => SetProperty<bool>(ref _isLoading, value);
        //}
        #endregion

        #region Clear Selected NoGroupeMenuItem Method
        internal void ClearSelectedNoGroupeMenuItem()
        {
            if (SelectedNoGroupedMenuItem is not null)
            {
                SelectedNoGroupedMenuItem.IsSelected = false;
                // SelectedNoGroupedMenuItem = null;
            }
        }

        //internal void ResetSelectedNoGroupeMenuItem(NoGroupedMenuItemViewModel selectedNoGroupedMenuItem)
        //{
        //    if (selectedNoGroupedMenuItem is not null)
        //    {
        //        SelectedNoGroupedMenuItem = selectedNoGroupedMenuItem;
        //    }
        //}
        #endregion

        #region Create MenuItem ViewModel Method
        internal void CreateMenuItemViewModels()
        {
            //IsLoading = true;

            //NoGroupedMenuItemViewModel noGroupedMenuItemViewModel = new(Index, _menuItem);

            // NoGroupedMenuItems.Add(noGroupedMenuItemViewModel);

            //IsLoading = false;
        }
        #endregion
    }
}
