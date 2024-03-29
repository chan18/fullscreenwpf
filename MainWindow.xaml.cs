﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using Application = System.Windows.Application;


namespace test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly InterceptKeys.LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        void OnClick1(object sender, RoutedEventArgs e)
        {
            btn1.Background = Brushes.LightBlue;
        }

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                //WindowState = WindowState.Maximized;

                //WindowStyle = WindowStyle.None;

                // need to remove this statement with out any side effects.
                //ShutdownMode = ShutdownMode.OnExplicitShutdown;

                // need to analyze this statement.
                //Application.Current.Exit += new ExitEventHandler(Current_Exit);

                /*
                 * - opening apps on-click. google chrome!
                 */
                // @"%AppData%\..\Local\Google\Chrome\Application\chrome.exe" this is working statement.
                //Process.Start(@"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe", "http://www.google.com");

            }
            catch (Exception ex)
            {

                throw ex;
            }
           

            try
            {
                _hookID = InterceptKeys.SetHook(_proc);
            }
            catch
            {
                DetachKeyboardHook();
            }

        }

        void Current_Exit(object sender, ExitEventArgs e)
        {
            DetachKeyboardHook();
        }

        /// <summary>
        /// Detach the keyboard hook; call during shutdown to prevent calls as we unload
        /// </summary>
        private static void DetachKeyboardHook()
        {
            if (_hookID != IntPtr.Zero)
                InterceptKeys.UnhookWindowsHookEx(_hookID);
        }

        public static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                bool alt = (Keys.Alt) != 0;

                bool control = (Keys.Control) != 0;

                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                if (alt && key == Keys.F4)
                {
                    //System.Windows.Application.Current.Shutdown();
                    return (IntPtr)1; // Handled.
                }

                if (!AllowKeyboardInput(alt, control, key))
                {
                    return (IntPtr)1; // Handled.
                }
            }

            return InterceptKeys.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        /// <summary>Determines whether the specified keyboard input should be allowed to be processed by the system.</summary>
        /// <remarks>Helps block unwanted keys and key combinations that could exit the app, make system changes, etc.</remarks>
        public static bool AllowKeyboardInput(bool alt, bool control, Keys key)
        {
            // Disallow various special keys.
            if (key <= Keys.Back || key == Keys.None ||
                key == Keys.Menu || key == Keys.Pause ||
                key == Keys.Help)
            {
                return false;
            }

            // Disallow ranges of special keys.
            // Currently leaves volume controls enabled; consider if this makes sense.
            // Disables non-existing Keys up to 65534, to err on the side of caution for future keyboard expansion.
            if ((key >= Keys.LWin && key <= Keys.Sleep) ||
                (key >= Keys.KanaMode && key <= Keys.HanjaMode) ||
                (key >= Keys.IMEConvert && key <= Keys.IMEModeChange) ||
                (key >= Keys.BrowserBack && key <= Keys.BrowserHome) ||
                (key >= Keys.MediaNextTrack && key <= Keys.LaunchApplication2) ||
                (key >= Keys.ProcessKey && key <= (Keys)65534))
            {
                return false;
            }

            // Disallow specific key combinations. (These component keys would be OK on their own.)
            if ((alt && key == Keys.Tab) ||
                (alt && key == Keys.Space) ||
                (control && key == Keys.Escape))
            {
                return false;
            }

            if ((alt && control) && key == Keys.Delete) {
                return false;
            }
            
            // Allow anything else (like letters, numbers, spacebar, braces, and so on).
            return true;
        }

    }
}
