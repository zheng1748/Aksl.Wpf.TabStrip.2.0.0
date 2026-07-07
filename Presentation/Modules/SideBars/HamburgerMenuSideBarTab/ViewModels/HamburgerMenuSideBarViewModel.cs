using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using Unity;

using Aksl.Infrastructure;

namespace Aksl.Modules.HamburgerMenuSideBarTab.ViewModels
{
    public class HamburgerMenuSideBarViewModel : BindableBase
    {
        #region Members
        private readonly IMenuService _menuService;
        #endregion

        #region Constructors
        public HamburgerMenuSideBarViewModel()
        {
            _menuService = PrismUnityExtensions.GetMenuService();

            AllLeafHamburgerMenuSideBarItems = new();
        }
        #endregion

        #region Properties
        public ObservableCollection<HamburgerMenuSideBarItemViewModel> AllLeafHamburgerMenuSideBarItems { get; set; }

        public HamburgerMenuSideBarItemViewModel SelectedHamburgerMenuSideBarItem
        {
            get; 
            set => SetProperty(ref field, value);
        }

        public bool IsPaneOpen
        {
            get;
            set
            {
                if (SetProperty<bool>(ref field, value))
                {
                    foreach (var hmbi in AllLeafHamburgerMenuSideBarItems)
                    {
                        hmbi.IsPaneOpen = field;
                    }
                }
            }
        } = false;

        public bool IsLoading
        {
            get;
            set => SetProperty<bool>(ref field, value);
        } = false;
        #endregion
    }
}
