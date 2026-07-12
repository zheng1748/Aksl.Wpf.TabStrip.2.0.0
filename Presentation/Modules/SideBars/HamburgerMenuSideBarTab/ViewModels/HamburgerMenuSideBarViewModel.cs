using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Aksl.Tabs;
using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Unity;

namespace Aksl.Modules.HamburgerMenuSideBarTab.ViewModels
{
    public class HamburgerMenuSideBarViewModel : BindableBase
    {
        #region Members
        protected readonly IEventAggregator _eventAggregator; 
        private readonly IDialogViewService _dialogViewService;
        private readonly IMenuService _menuService;
        #endregion

        #region Constructors
        public HamburgerMenuSideBarViewModel()
        {
            _eventAggregator = PrismUnityExtensions.GetEventAggregator();
            _dialogViewService = PrismUnityExtensions.GetDialogViewService();
            _menuService = PrismUnityExtensions.GetMenuService();

            AllLeafHamburgerMenuSideBarItems = new();

            RegisterActiveTabItemEvent();
            RegisterOnSelectedTabItemEmptyEvent();
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

        //public bool IsLoading
        //{
        //    get;
        //    set => SetProperty<bool>(ref field, value);
        //} = false;
        #endregion

        #region Register SelectedTabItem Empty Event
        private void RegisterOnSelectedTabItemEmptyEvent()
        {
            _eventAggregator.GetEvent<OnSelectedTabItemEmptyEvent>().Subscribe(async (oatie) =>
            {
                try
                {
                    if (SelectedHamburgerMenuSideBarItem is not null)
                    {
                        SelectedHamburgerMenuSideBarItem.IsSelected = false;
                    }
                }
                catch (Exception ex)
                {
                    await _dialogViewService.AlertAsync(message: $"Exception : \"{ex.Message}\"", title: "Error: Selected TabItem Is Empty");
                }
            }, ThreadOption.UIThread, true);
        }
        #endregion

        #region Register Active TabItem Event
        private void RegisterActiveTabItemEvent()
        {
            _eventAggregator.GetEvent<OnActiveTabItemEvent>().Subscribe(async (oatie) =>
            {
                var currentTabInfo = oatie.SelectedTabInfo;

                try
                {
                    SetSelectedHamburgerMenuItem();

                    #region Set Selected HamburgerMenuItem Method
                    void SetSelectedHamburgerMenuItem()
                    {
                        var matchHamburgerMenuSideBartem = AllLeafHamburgerMenuSideBarItems.FirstOrDefault(hmi => hmi.Name.Equals(currentTabInfo.Name, StringComparison.InvariantCultureIgnoreCase));

                        if (matchHamburgerMenuSideBartem is not null)
                        {
                            if (matchHamburgerMenuSideBartem == SelectedHamburgerMenuSideBarItem)
                            {
                                return;
                            }

                            if (SelectedHamburgerMenuSideBarItem is not null)
                            {
                                matchHamburgerMenuSideBartem.IsSelected = true;
                                Debug.Assert(AllLeafHamburgerMenuSideBarItems.Count(hmi => hmi.IsSelected) == 1);
                            }
                        }
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    await _dialogViewService.AlertAsync(message: $"Exception : \"{ex.Message}\"", title: "Error: Active TabItem");
                }
            }, ThreadOption.UIThread, true);
        }
        #endregion
    }
}
