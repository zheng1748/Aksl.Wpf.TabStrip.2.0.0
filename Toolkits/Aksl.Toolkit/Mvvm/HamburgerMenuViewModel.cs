using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Aksl.Mvvm;

public class HamburgerMenuViewModel : BindableBase
{
    #region Constructors
    public HamburgerMenuViewModel()
    {
    }
    #endregion

    #region HamburgerMenu Properties
    //  private Brush _paneBackground = new SolidColorBrush(Colors.Transparent);
    // private Brush _paneBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D3D3D3"));
    public Brush PaneBackground
    {
        get => field;
        set => SetProperty<Brush>(ref field, value);
    } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D3D3D3"));

    public GridLength OpenPaneGridLength
    {
        get
        {
            return new GridLength(OpenPaneLength);
        }
    }

    //private double _openPaneLength = 320d;
    public double OpenPaneLength
    {
        get => field;
        set => SetProperty<double>(ref field, value);
    } = 320d;

    public GridLength CompactPaneGridLength
    {
        get
        {
            return new GridLength(CompactPaneLength);
        }
    }

    // private double _compactPaneLength = 48d;
    public double CompactPaneLength
    {
        get => field;
        set => SetProperty<double>(ref field, value);
    } = 48d;

    public virtual bool IsPaneOpen
    {
        get => field;
        set
        {
            if (SetProperty<bool>(ref field, value))
            {
                VisualState = GetVisualState();
            }
        }
    }

    public List<SplitViewDisplayMode> DisplayModeList
    {
        get => Enum.GetValues(typeof(SplitViewDisplayMode)).Cast<SplitViewDisplayMode>().ToList();
    }

    //   private SplitViewDisplayMode _selectedDisplayMode = SplitViewDisplayMode.Overlay;
    public SplitViewDisplayMode SelectedDisplayMode
    {
        get => field;
        set
        {
            if (SetProperty<SplitViewDisplayMode>(ref field, value))
            {
                VisualState = GetVisualState();
            }
        }
    } = SplitViewDisplayMode.Overlay;

    public List<SplitViewPanePlacement> PanePlacementList
    {
        get
        {
            return Enum.GetValues(typeof(SplitViewPanePlacement)).Cast<SplitViewPanePlacement>().ToList();
        }
    }

    // private SplitViewPanePlacement _selectedPanePlacement = SplitViewPanePlacement.Left;
    public SplitViewPanePlacement SelectedPlacement
    {
        get => field;
        set
        {
            if (SetProperty<SplitViewPanePlacement>(ref field, value))
            {
                VisualState = GetVisualState();
            }
        }
    }

    public string VisualState
    {
        get => field;
        set => SetProperty<string>(ref field, value);
    }
    #endregion

    #region Get HamburgerMenu State Method
    private bool IsCompact
    {
        get
        {
            return SelectedDisplayMode switch
            {
                SplitViewDisplayMode.CompactInline or SplitViewDisplayMode.CompactOverlay => true,
                _ => false,
            };
        }
    }

    private bool IsInline
    {
        get
        {
            return SelectedDisplayMode switch
            {
                SplitViewDisplayMode.CompactInline or SplitViewDisplayMode.Inline => true,
                _ => false
            };
        }
    }

    protected virtual string GetVisualState()
    {
        string state;

        if (IsPaneOpen)
        {
            state = "Open";
            state += IsInline ? "Inline" : SelectedDisplayMode.ToString();
        }
        else
        {
            state = "Closed";
            if (IsCompact)
            {
                state += "Compact";
            }
            //else
            //{
            //    return state;
            //}
        }

        state += SelectedPlacement.ToString();

        return state;
    }
    #endregion
}
