using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Aksl.Infrastructure;

namespace Aksl.Modules.HamburgerMenuNavigationSideBarTab.ViewModels
{
    public class GroupedMenuViewModel : GroupedMenuViewModelBase
    {
        #region Members
        private readonly MenuItemViewModel _headerMenuItemViewModel;
        private IEnumerable<MenuItemViewModel> _leafMenuItemViewModels;
        #endregion

        #region Constructors
        public GroupedMenuViewModel(int groupIndex, MenuItemViewModel headerMenuItemViewModel, IEnumerable<MenuItemViewModel> leafMenuItemViewModels) : base()
        {
            GroupIndex = groupIndex;
            _headerMenuItemViewModel = headerMenuItemViewModel;
            _leafMenuItemViewModels = leafMenuItemViewModels;

            MenuItemHeader = new(_headerMenuItemViewModel);

            CreateMenuContentViewModel();

            IsGrouped = true;
        }
        #endregion

        #region Properties
        public int GroupIndex { get; }
        public bool IsGrouped { get; set; }
        public MenuItemHeaderViewModel MenuItemHeader { get; set; }
        public MenuContentViewModel MenuContent { get; private set; }

        //private MenuItemViewModel _selectedMenuItem;
        public MenuItemViewModel SelectedMenuItem
        {
            get => field;
            set => SetProperty(ref field, value);
        }

        public bool IsPaneOpen
        {
            get => field;
            set
            {
                if (SetProperty<bool>(ref field, value))
                {
                     MenuContent.IsPaneOpen = field;
                }
            }
        }
        #endregion

        #region Create MenuContent ViewModel Method
        private void CreateMenuContentViewModel()
        {
            MenuContentViewModel menuContentViewModel = new(GroupIndex, _leafMenuItemViewModels);
            AddPropertyChanged();

            void AddPropertyChanged()
            {
                menuContentViewModel.PropertyChanged += (sender, e) =>
                {
                    if (sender is MenuContentViewModel mcvm)
                    {
                        if (e.PropertyName == nameof(MenuContentViewModel.SelectedMenuItem))
                        {
                            SelectedMenuItem = mcvm.SelectedMenuItem;
                        }
                    }
                };
            }

            MenuContent = menuContentViewModel;
        }
        #endregion
    }
}
