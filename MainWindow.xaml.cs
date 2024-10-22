using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Documents;
using WindowsInput;
using WindowsInput.Native;
using System.Text.RegularExpressions;

namespace AutoClicker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Members

        private readonly InputSimulator simulator = new();
        private List<KeyData> keyDatas;
        private readonly List<VirtualKeyCode> pressedKeyCodes = [];
        
        private GlobalKeyboardHook _globalKeyboardHook;
        
        private bool isBusy;
        private bool isLooping = false;
        private int currentKey = 0;
        private int totalLoops;
        private int currentLoop;
        private bool loopUp;

        private System.Timers.Timer endlessLoop;
        private System.Timers.Timer forLoop;
        #endregion

        #region Constructer
        public MainWindow()
        {
            InitializeComponent();
            SaveSystem.GetSaves(savesListBox);
            _globalKeyboardHook = new GlobalKeyboardHook();
            _globalKeyboardHook.OnKeyDown += GlobalKeyboardHook_OnKeyDown;
            _globalKeyboardHook.OnKeyUp += GlobalKeyboardHook_OnKeyUp;            
        }
        #endregion

        #region Timers        
        private void StartEndlessLoop(object? sender, ElapsedEventArgs e)
        {
            if (!isBusy)
            {
                isBusy = true;               
                if (currentKey >= keyDatas.Count)
                {
                    if (loopUp)
                        UpAllPressedKey();
                    currentKey = 0;
                }
                
                PressOrReleaseKey(keyDatas[currentKey], endlessLoop);
                
                isBusy = false;
                currentKey++;
            }
        }

        private void StartForLoop(object? sender, ElapsedEventArgs e)
        {
            if (!isBusy)
            {
                isBusy = true;

                if (currentKey >= keyDatas.Count)
                {
                    if (loopUp)
                        UpAllPressedKey();
                    currentLoop++;
                    currentKey = 0;
                }

                PressOrReleaseKey(keyDatas[currentKey], forLoop);

                isBusy = false;
                currentKey++;
                if (currentLoop >= totalLoops)
                {
                    UpAllPressedKey();
                    forLoop.Dispose();
                }
            }
        }
        #endregion     

        #region KeyListBox Controls        
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            KeyControl k = new();
            k.OnChecked += CreateKeyUp;
            if(keyControlsListBox.SelectedItem != null)
                keyControlsListBox.Items.Insert(keyControlsListBox.Items.IndexOf(keyControlsListBox.SelectedItem) + 1, k);
            else
                keyControlsListBox.Items.Add(k);
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if(keyControlsListBox.Items.Count == 0) return;

            if(keyControlsListBox.SelectedItems.Count == 0)
            {
                keyControlsListBox.Items.Remove(keyControlsListBox.Items[^1]);
                return;
            }

            List<KeyControl> keys = keyControlsListBox.SelectedItems.Cast<KeyControl>().ToList();
            foreach (KeyControl k in keys)
            {
                keyControlsListBox.Items.Remove(k);
            }
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            List<KeyControl> keys = keyControlsListBox.SelectedItems.Cast<KeyControl>().ToList();
            foreach (KeyControl k in keys)
            {
                KeyControl newKey = new();
                newKey.OnChecked += CreateKeyUp;
                keyControlsListBox.Items.Insert(keyControlsListBox.Items.Count, newKey);
                newKey.PasteData(k.KeyString, k.PressKey, k.DelayString);
                if(k.KeyString != "")
                    newKey.LateSetup();
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            keyControlsListBox.Items.Clear();
        }
        #endregion

        #region SaveListBox Controls
        private void SavesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(savesListBox.SelectedItem != null)
                saveTextBox.Text = ((Label)savesListBox.SelectedItem).Content.ToString();
        }
        #endregion

        #region Save Controls

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            GetKeyDatas();
            if (!ValidateKeys()) return;
            SaveSystem.Save(savesListBox, saveTextBox.Text, keyDatas);
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            SaveSystem.Load(keyControlsListBox, savesListBox);
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            SaveSystem.Delete(savesListBox);
        }
        #endregion

        #region Loop Controls
        
        private void StartStop_Click(object sender, RoutedEventArgs e)
        {
            if (!isLooping)
            {
                GetKeyDatas();
                if(!ValidateKeys()) return;

                loopUp = (bool)loopUpCheck.IsChecked;

                if ((bool)endlessCheck.IsChecked)
                {
                    endlessLoop = new() { Interval = 10 };
                    endlessLoop.Elapsed += StartEndlessLoop;
                    endlessLoop.Disposed += LoopEnd;
                    endlessLoop.Start();
                }
                else
                {
                    currentLoop = 0;
                    totalLoops = int.Parse(loopAmount.Text);
                    forLoop = new() { Interval = 10 };
                    forLoop.Elapsed += StartForLoop;
                    forLoop.Disposed += LoopEnd;
                    forLoop.Start();
                }
                Dispatcher.Invoke(() => startStopButton.Content = "Stop(F6)");
                isLooping = true;
            }
            else
            {
                UpAllPressedKey();
                endlessLoop?.Dispose();
                forLoop?.Dispose();                
            }
        }

        private void LoopEnd(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() => 
            {
                startStopButton.Content = "Start(F6)";
                isLooping = false;                
            });
        }
        #endregion

        #region Functions
        private void PressOrReleaseKey(KeyData keyData, System.Timers.Timer loop)
        {
            Thread.Sleep(keyData.DelayTime);
            if (!loop.Enabled) return;
            Dispatcher.Invoke(() =>
            {
                VirtualKeyCode vK = (VirtualKeyCode)keyData.VirtualKey;
                if (keyData.PressKey)
                {
                    if (vK == VirtualKeyCode.LBUTTON)
                        simulator.Mouse.LeftButtonDown();
                    else if (vK == VirtualKeyCode.RBUTTON)
                        simulator.Mouse.RightButtonDown();
                    else
                        simulator.Keyboard.KeyDown(vK);
                    if (!pressedKeyCodes.Contains(vK)) pressedKeyCodes.Add(vK);
                }
                else
                {
                    if (vK == VirtualKeyCode.LBUTTON)
                        simulator.Mouse.LeftButtonUp();
                    else if (vK == VirtualKeyCode.RBUTTON)
                        simulator.Mouse.RightButtonUp();
                    else
                        simulator.Keyboard.KeyUp(vK);
                    pressedKeyCodes.Remove(vK);
                }
            });
        }

        private void CreateKeyUp(KeyControl k)
        {
            if (!(bool)autoKeyUpCheck.IsChecked) return;
            int index = keyControlsListBox.Items.IndexOf(k);
            KeyControl newKey = new()
            {
                KeyString = k.KeyString,
            };
            keyControlsListBox.Items.Insert(index + 1, newKey);
            newKey.OnChecked += CreateKeyUp;
            if(k.KeyString != "")
                newKey.LateSetup();
        }
        
        private void UpAllPressedKey()
        {
            pressedKeyCodes.ForEach(k => 
            {
                if (k == VirtualKeyCode.LBUTTON)
                    simulator.Mouse.LeftButtonUp();
                else if (k == VirtualKeyCode.RBUTTON)
                    simulator.Mouse.RightButtonUp();
                else
                    simulator.Keyboard.KeyUp(k);
            });
            pressedKeyCodes.Clear();
        }
        private bool ValidateKeys()
        {
            return keyDatas != null && keyDatas.Count != 0 && keyDatas.Any(k => k.VirtualKey != null);
        }
        private void GetKeyDatas()
        {
            List<KeyData>? newKeyDatas = keyControlsListBox.Items.Cast<KeyControl>().ToList().Select(k => k.KeyData).ToList();
            keyDatas = newKeyDatas;
        }

        private void PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = TextBoxValidater.IsNumerical(e.Text);
        }
        #endregion

        #region HotKey
        private void GlobalKeyboardHook_OnKeyDown(object sender, KeyEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if(e.Key == System.Windows.Input.Key.F5)
                {
                    StartStop_Click(null, null);
                }
            });
        }

        private void GlobalKeyboardHook_OnKeyUp(object sender, KeyEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (e.Key == System.Windows.Input.Key.F6)
                {
                    StartStop_Click(null, null);
                }
            });
        }
        #endregion
    }
}