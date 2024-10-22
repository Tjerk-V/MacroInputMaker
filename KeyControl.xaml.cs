using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WindowsInput.Native;

namespace AutoClicker
{
    public partial class KeyControl : UserControl
    {
        #region Events
            public delegate void IsChecked(KeyControl k);
            public event IsChecked OnChecked;
        #endregion

        #region Properties
        public KeyData KeyData { get => new()
        {
            VirtualKey = VirtualKeyConverter.GetVirtualKey(keyTextBox.Text),
            PressKey = (bool)pressKeyCheck.IsChecked,
            DelayTime = int.Parse(dMilisec.Text),
        }; }
        public string KeyString { get => keyTextBox.Text; set => keyTextBox.Text = value; }
        public bool PressKey { get => (bool)pressKeyCheck.IsChecked; }
        public string DelayString { get => dMilisec.Text; }
        #endregion

        #region KeyList
        readonly List<string> keysList =
        [
            "A",
            "B",
            "C",
            "D",
            "E",
            "F",
            "G",
            "H",
            "I",
            "J",
            "K",
            "L",
            "M",
            "N",
            "O",
            "P",
            "Q",
            "R",
            "S",
            "T",
            "U",
            "V",
            "W",
            "X",
            "Y",
            "Z",
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "NUMPAD0",
            "NUMPAD1",
            "NUMPAD2",
            "NUMPAD3",
            "NUMPAD4",
            "NUMPAD5",
            "NUMPAD6",
            "NUMPAD7",
            "NUMPAD8",
            "NUMPAD9",
            "F1",
            "F2",
            "F3",
            "F4",
            "F5",
            "F6",
            "F7",
            "F8",
            "F9",
            "F10",
            "F11",
            "F12",
            "LMOUSE",
            "RMOUSE",
            "BACK",
            "TAB",
            "CLEAR",
            "RETURN",
            "PAUSE",
            "CAPITAL",
            "ESCAPE",
            "ACCEPT",
            "SPACE",
            "PRIOR",
            "NEXT",
            "END",
            "HOME",
            "LEFT",
            "UP",
            "RIGHT",
            "DOWN",
            "INSERT",
            "DELETE",
            "LSHIFT",
            "RSHIFT",
            "LCONTROL",
            "RCONTROL",
            "LMENU",
            "RMENU",
            "OEM_PLUS",
            "OEM_COMMA",
            "OEM_MINUS",
            "OEM_PERIOD"
        ];
        #endregion

        #region Constructer
        public KeyControl()
        {
            InitializeComponent();
            keysList.ForEach(l => keysListBox.Items.Add(CreateListBoxItem(l)));
            ExpandButton_Click(null, null);
            Height = 50;
        }
        #endregion

        #region Functions
        public void LateSetup()
        {
            keysList.ForEach(l => keysListBox.Items.Add(CreateListBoxItem(l)));
            ExpandButton_Click(null, null);
            Height = 50;
        }
        public void PasteData(string keyString, bool pressKey, string delayString)
        {
            keyTextBox.Text = keyString;
            pressKeyCheck.IsChecked = pressKey;
            dMilisec.Text = delayString;
        }
        ListBoxItem CreateListBoxItem(string content) => new() { Content = content };            
        
        private void RotateButton(int angle)
        {
            expandButton.RenderTransform = new RotateTransform(angle);            
        }
        #endregion

        #region WPF Events
        private void ExpandButton_Click(object sender, RoutedEventArgs e)
        {
            if (keysListBox.Visibility == Visibility.Hidden)
            {
                keyTextBox.Focus();
                keysListBox.Visibility = Visibility.Visible;
            }
            else
                keysListBox.Visibility = Visibility.Hidden;
        }

        private void KeysListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (keysListBox.SelectedItem != null)
            {
                keyTextBox.Text = ((ListBoxItem)keysListBox.SelectedItem).Content.ToString() ?? "";
                keysListBox.Visibility = Visibility.Hidden;
            }
        }

        private void KeysListBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (keysListBox.Visibility == Visibility.Visible)
            {
                RotateButton(180);
                Height = 215;
            }
            else
            {
                RotateButton(0);
                Height = 50;
            }
        }

        private void KeyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            keysListBox.Items.Clear();
            keysList.Where(k => k.Contains(keyTextBox.Text, StringComparison.CurrentCultureIgnoreCase)).ToList().ForEach(k => keysListBox.Items.Add(CreateListBoxItem(k)));
            if (keysListBox.Visibility != Visibility.Visible)
                keysListBox.Visibility = Visibility.Visible;
        }

        private void PressKeyCheck_Checked(object sender, RoutedEventArgs e)
        {
            OnChecked?.Invoke(this);
        }

        private void PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = TextBoxValidater.IsNumerical(e.Text);
        }

        private void KeyTextBox_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!keyTextBox.IsKeyboardFocused && keysListBox.Visibility == Visibility.Visible && !expandButton.IsKeyboardFocused)
                keysListBox.Visibility = Visibility.Hidden;
        }

        private void KeyTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Escape)
                keysListBox.Visibility = Visibility.Hidden;
        }
    }
    #endregion

    public class KeyData
    {
        public VirtualKeyCode? VirtualKey { get; set; }
        public bool PressKey { get; set; }
        public int DelayTime {  get; set; }
    }
}
