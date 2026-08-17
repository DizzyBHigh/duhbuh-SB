using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

// duhBuhUI custom dropdown. This deliberately does not use WPF ComboBox for
the visible control or its popup. WPF is only the window/input foundation.
public sealed class DuhBuhUICustomDropdown : Control
{
    private string[] _options = new string[0];
    private int _selectedIndex = -1;
    private bool _focused;
    private Popup _popup;
    private Window _ownerWindow;

    private Color _popupBackground = Color.FromRgb(28, 30, 34);
    private Color _panelBackground = Color.FromRgb(36, 39, 45);
