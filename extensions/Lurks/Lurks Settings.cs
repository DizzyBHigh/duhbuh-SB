using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

// duhBuh Lurks settings action.
// Add references in the Streamer.bot C# action:
// PresentationCore.dll
// PresentationFramework.dll
// WindowsBase.dll
// System.Xaml.dll (if your Streamer.bot build requires it)

public class CPHInline
{
    private const string Prefix = "duhbuh_lurks_";

    public bool Execute()
    {
        ShowSettings();
        return true;
    }

    private void ShowSettings()
    {
        Window window = new Window();
        window.Title = "duhBuh - Lurks Settings";
        window.Width = 900;
        window.Height = 720;
        window.MinWidth = 720;
        window.MinHeight = 560;
        window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        window.Background = new SolidColorBrush(Color.FromRgb(22, 25, 35));
        window.Foreground = Brushes.White;

        Grid root = new Grid();
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        TextBlock header = new TextBlock
        {
            Text = "duhBuh  •  Lurks",
            FontSize = 28,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(24, 20, 24, 18)
        };
        Grid.SetRow(header, 0);
        root.Children.Add(header);

        TabControl tabs = new TabControl { Margin = new Thickness(18, 0, 18, 12) };
        tabs.Items.Add(BuildGeneralTab());
        tabs.Items.Add(BuildMessagesTab());
        tabs.Items.Add(BuildTranslationsTab());
        tabs.Items.Add(BuildHelpTab());
        Grid.SetRow(tabs, 1);
        root.Children.Add(tabs);

        StackPanel buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(18, 8, 18, 18)
        };

        Button save = Button("Save", 110);
        Button saveExit = Button("Save & Exit", 130);
        Button reset = Button("Reset", 100);
        Button exit = Button("Exit", 100);

        save.Click += delegate { SaveAll(tabs); };
        saveExit.Click += delegate { SaveAll(tabs); window.Close(); };
        reset.Click += delegate { ResetSettings(tabs); };
        exit.Click += delegate { window.Close(); };

        buttons.Children.Add(save);
        buttons.Children.Add(saveExit);
        buttons.Children.Add(reset);
        buttons.Children.Add(exit);
        Grid.SetRow(buttons, 2);
        root.Children.Add(buttons);

        window.Content = root;
        window.ShowDialog();
    }

    private TabItem BuildGeneralTab()
    {
        StackPanel panel = Panel();
        panel.Children.Add(Title("General Settings"));

        AddCheck(panel, "Use 24h format", "Use HH:mm instead of AM/PM when displaying lurk times.", "24hFormat", true);
        AddCheck(panel, "Remove Unpresent Lurkers", "Remove lurkers after 15 minutes continuously absent from Present Viewers.", "removeUnpresentLurkers", true);
        AddCheck(panel, "Unlurk Chatters", "End a lurk after the configured number of chat messages.", "chattingUnlurks", true);
        AddSlider(panel, "Unlurk Chatters Threshold", "Number of chat messages required to end the lurk.", "chattingUnlurksThreshold", 1, 10, 3);
        AddCheck(panel, "Send Messages As Replies", "Post chat responses as replies instead of normal messages.", "postMessagesAsReplies", true);
        AddSlider(panel, "Leaderboard Ranks", "Number of leaderboard entries to display.", "leaderboardRankAmount", 3, 20, 5);

        panel.Children.Add(Title("Advanced"));
        Button resetStats = Button("Reset All Lurk Times", 190);
        resetStats.Tag = "RESET_LURKS";
        resetStats.Click += delegate
        {
            MessageBoxResult first = MessageBox.Show("Reset all lurk counts and total times? This cannot be undone.", "duhBuh Lurks", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (first != MessageBoxResult.Yes) return;
            MessageBoxResult second = MessageBox.Show("Are you really sure? This cannot be undone.", "duhBuh Lurks", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (second != MessageBoxResult.Yes) return;
            CPH.UnsetAllTwitchUsersVar("duhbuh_lurks_count", true);
            CPH.UnsetAllTwitchUsersVar("duhbuh_lurks_totalSeconds", true);
            MessageBox.Show("Lurk statistics reset.", "duhBuh Lurks", MessageBoxButton.OK, MessageBoxImage.Information);
        };
        panel.Children.Add(resetStats);

        TabItem item = new TabItem { Header = "General", Content = Scroll(panel) };
        return item;
    }

    private TabItem BuildMessagesTab()
    {
        StackPanel panel = Panel();
        panel.Children.Add(Title("Chat Responses"));
        AddText(panel, "Lurk Start", "Available: %user%, %lurkerCount%", "messagesLurkStart", "@%user% is now lurking!");
        AddText(panel, "Already Lurking", "Available: %user%, %lurkStartTime%", "messagesAlreadyLurking", "@%user%, you've already entered the lurk at %lurkStartTime%!");
        AddText(panel, "Lurk End", "Available: %user%, %lurkTime%, %lurkerCount%", "messagesLurkEnd", "@%user%, welcome back! Your lurk has lasted for %lurkTime%.");
        AddText(panel, "Lurk End (hasn't lurked)", "Available: %user%", "messagesLurkEndHasntLurked", "@%user%, you haven't been lurking.");
        AddText(panel, "Lurk Check", "Available: %lurkerCount%", "messagesLurkCheck", "There are currently %lurkerCount% lurkers:");
        AddText(panel, "Lurk Check (No one lurking)", "Available: %user%", "messagesLurkCheckNoOneLurking", "@%user%, no one's currently lurking.");
        AddText(panel, "Lurk Stats", "Available: %user%, %lurkCount%, %totalLurkTime%, %averageLurkTime%", "messagesLurkStats", "@%user%, you have been lurking for %lurkCount% times and a total time of %totalLurkTime%. Your average lurking time is %averageLurkTime%.");
        AddText(panel, "Lurk Stats (hasn't lurked yet)", "Available: %user%", "messagesLurkStatsHasntLurkedYet", "@%user%, you haven't ever lurked yet.");
        AddText(panel, "Leaderboard Infix", "Used between the lurk count and total time in leaderboard output.", "messagesLeaderboardInfix", "times for a total of");
        AddText(panel, "Leaderboard Own Rank", "Available: %user%, %rank%, %lurkCount%, %totalLurkTime%", "messagesLeaderboardOwnRank", "@%user%, your own rank is #%rank% with %lurkCount% lurks and a total lurk time of %totalLurkTime%");
        return new TabItem { Header = "Chat Responses", Content = Scroll(panel) };
    }

    private TabItem BuildTranslationsTab()
    {
        StackPanel panel = Panel();
        panel.Children.Add(Title("Translations"));
        AddText(panel, "second/seconds", "Use singular/plural separated by '/'.", "translationSeconds", "second/seconds");
        AddText(panel, "minute/minutes", "Use singular/plural separated by '/'.", "translationMinutes", "minute/minutes");
        AddText(panel, "hour/hours", "Use singular/plural separated by '/'.", "translationHours", "hour/hours");
        AddText(panel, "day/days", "Use singular/plural separated by '/'.", "translationDays", "day/days");
        AddText(panel, "for", "Translation for the word 'for'.", "translationFor", "for");
        AddText(panel, "and", "Translation for the word 'and'.", "translationAnd", "and");
        return new TabItem { Header = "Translations", Content = Scroll(panel) };
    }

    private TabItem BuildHelpTab()
    {
        StackPanel panel = Panel();
        panel.Children.Add(Title("Help"));
        panel.Children.Add(new TextBlock
        {
            Text = "duhBuh Lurks\n\nThe runtime action accepts the argument 'duhbuhLurksAction' with one of: start, end, check, stats, leaderboard, chatunlurk.\n\nUse Twitch command actions to pass the appropriate action value and the viewer's userName/displayName.\n\nThe Present Viewers trigger should call RemoveUnpresentLurkers() when the remove-unpresent option is enabled.",
            TextWrapping = TextWrapping.Wrap,
            FontSize = 15,
            Margin = new Thickness(6, 6, 6, 20)
        });
        return new TabItem { Header = "Help", Content = Scroll(panel) };
    }

    private StackPanel Panel()
    {
        return new StackPanel { Margin = new Thickness(18), Orientation = Orientation.Vertical };
    }

    private ScrollViewer Scroll(UIElement child)
    {
        return new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Content = child };
    }

    private TextBlock Title(string text)
    {
        return new TextBlock { Text = text, FontSize = 24, FontWeight = FontWeights.Bold, Margin = new Thickness(4, 4, 4, 18) };
    }

    private Button Button(string text, double width)
    {
        Button b = new Button { Content = text, Width = width, Height = 38, Margin = new Thickness(4), Padding = new Thickness(10, 4, 10, 4) };
        return b;
    }

    private void AddCheck(StackPanel panel, string label, string description, string key, bool defaultValue)
    {
        CheckBox box = new CheckBox
        {
            Content = label,
            IsChecked = GetBool(key, defaultValue),
            FontSize = 16,
            Margin = new Thickness(4, 10, 4, 2),
            Tag = Prefix + key
        };
        panel.Children.Add(box);
        panel.Children.Add(Description(description));
    }

    private void AddSlider(StackPanel panel, string label, string description, string key, int min, int max, int defaultValue)
    {
        StackPanel row = new StackPanel { Margin = new Thickness(4, 10, 4, 2) };
        TextBlock text = new TextBlock { Text = label, FontSize = 16 };
        Slider slider = new Slider { Minimum = min, Maximum = max, Value = GetInt(key, defaultValue), TickFrequency = 1, IsSnapToTickEnabled = true, Tag = Prefix + key };
        TextBlock value = new TextBlock { FontSize = 13, Margin = new Thickness(0, 2, 0, 0) };
        value.Text = ((int)slider.Value).ToString();
        slider.ValueChanged += delegate { value.Text = ((int)slider.Value).ToString(); };
        row.Children.Add(text);
        row.Children.Add(slider);
        row.Children.Add(value);
        panel.Children.Add(row);
        panel.Children.Add(Description(description));
    }

    private void AddText(StackPanel panel, string label, string description, string key, string defaultValue)
    {
        StackPanel row = new StackPanel { Margin = new Thickness(4, 10, 4, 2) };
        row.Children.Add(new TextBlock { Text = label, FontSize = 16 });
        TextBox box = new TextBox { Text = GetString(key, defaultValue), MinHeight = 34, AcceptsReturn = false, Tag = Prefix + key, Margin = new Thickness(0, 4, 0, 0) };
        row.Children.Add(box);
        panel.Children.Add(row);
        panel.Children.Add(Description(description));
    }

    private TextBlock Description(string text)
    {
        return new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap, Opacity = 0.7, Margin = new Thickness(4, 0, 4, 8) };
    }

    private void SaveAll(TabControl tabs)
    {
        SaveVisualTree(tabs);
        CPH.LogInfo("[duhBuh Lurks] Settings saved.");
    }

    private void SaveVisualTree(DependencyObject root)
    {
        int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < count; i++)
        {
            DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
            CheckBox check = child as CheckBox;
            if (check != null && check.Tag is string && ((string)check.Tag).StartsWith(Prefix))
                CPH.SetGlobalVar((string)check.Tag, check.IsChecked == true, true);

            Slider slider = child as Slider;
            if (slider != null && slider.Tag is string && ((string)slider.Tag).StartsWith(Prefix))
                CPH.SetGlobalVar((string)slider.Tag, (int)slider.Value, true);

            TextBox box = child as TextBox;
            if (box != null && box.Tag is string && ((string)box.Tag).StartsWith(Prefix))
                CPH.SetGlobalVar((string)box.Tag, box.Text ?? "", true);

            SaveVisualTree(child);
        }
    }

    private void ResetSettings(TabControl tabs)
    {
        MessageBoxResult result = MessageBox.Show("Reset all duhBuh Lurks settings to their defaults?", "duhBuh Lurks", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;

        SetDefault("24hFormat", true);
        SetDefault("removeUnpresentLurkers", true);
        SetDefault("chattingUnlurks", true);
        SetDefault("chattingUnlurksThreshold", 3);
        SetDefault("postMessagesAsReplies", true);
        SetDefault("leaderboardRankAmount", 5);
        SetDefault("messagesLurkStart", "@%user% is now lurking!");
        SetDefault("messagesAlreadyLurking", "@%user%, you've already entered the lurk at %lurkStartTime%!");
        SetDefault("messagesLurkEnd", "@%user%, welcome back! Your lurk has lasted for %lurkTime%.");
        SetDefault("messagesLurkEndHasntLurked", "@%user%, you haven't been lurking.");
        SetDefault("messagesLurkCheck", "There are currently %lurkerCount% lurkers:");
        SetDefault("messagesLurkCheckNoOneLurking", "@%user%, no one's currently lurking.");
        SetDefault("messagesLurkStats", "@%user%, you have been lurking for %lurkCount% times and a total time of %totalLurkTime%. Your average lurking time is %averageLurkTime%.");
        SetDefault("messagesLurkStatsHasntLurkedYet", "@%user%, you haven't ever lurked yet.");
        SetDefault("messagesLeaderboardInfix", "times for a total of");
        SetDefault("messagesLeaderboardOwnRank", "@%user%, your own rank is #%rank% with %lurkCount% lurks and a total lurk time of %totalLurkTime%");
        SetDefault("translationSeconds", "second/seconds");
        SetDefault("translationMinutes", "minute/minutes");
        SetDefault("translationHours", "hour/hours");
        SetDefault("translationDays", "day/days");
        SetDefault("translationFor", "for");
        SetDefault("translationAnd", "and");

        MessageBox.Show("Defaults restored. Re-open the settings window to refresh the controls.", "duhBuh Lurks", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void SetDefault(string key, object value) { CPH.SetGlobalVar(Prefix + key, value, true); }

    private bool GetBool(string key, bool fallback)
    {
        try { return CPH.GetGlobalVar<bool?>(Prefix + key, true) ?? fallback; } catch { return fallback; }
    }

    private int GetInt(string key, int fallback)
    {
        try { return CPH.GetGlobalVar<int?>(Prefix + key, true) ?? fallback; } catch { return fallback; }
    }

    private string GetString(string key, string fallback)
    {
        try { return CPH.GetGlobalVar<string>(Prefix + key, true) ?? fallback; } catch { return fallback; }
    }
}
