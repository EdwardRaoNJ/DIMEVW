﻿using Digimezzo.Utilities.Settings;
using Knowte.Common.Base;
using Knowte.Common.IO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Timers;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Xml.Linq;

namespace Knowte.Common.Services.Appearance
{
    public class AppearanceService : IAppearanceService
    {
        private string colorSchemesSubDirectory = Path.Combine(SettingsClient.ApplicationFolder(), ApplicationPaths.ColorSchemesSubDirectory);
        private const int WM_DWMCOLORIZATIONCOLORCHANGED = 0x320;
        private bool followWindowsColor = false;
        private List<ColorScheme> colorSchemes = new List<ColorScheme>();
        private FileSystemWatcher colorSchemeWatcher;
        private Timer colorSchemeTimer = new Timer();
        private double colorSchemeTimeoutSeconds = 0.2;
        private ColorScheme[] builtInColorSchemes = {
                                                        new ColorScheme {
                                                            Name = "Blue",
                                                            AccentColor = "#1D7DD4"
                                                        },
                                                        new ColorScheme {
                                                            Name = "Green",
                                                            AccentColor = "#7FB718"
                                                        },
                                                        new ColorScheme {
                                                            Name = "Yellow",
                                                            AccentColor = "#F09609"
                                                        },
                                                        new ColorScheme {
                                                            Name = "Purple",
                                                            AccentColor = "#A835B2"
                                                        },
                                                        new ColorScheme {
                                                            Name = "Pink",
                                                            AccentColor = "#CE0058"
                                                        }
                                                    };
     
        public string ColorSchemesSubDirectory
        {
            get { return this.colorSchemesSubDirectory; }
            set { this.colorSchemesSubDirectory = value; }
        }
  
        public AppearanceService()
        {
            // Initialize the ColorSchemes directory
            // -------------------------------------

            // If the ColorSchemes subdirectory doesn't exist, create it.
            if (!Directory.Exists(this.ColorSchemesSubDirectory))
            {
                Directory.CreateDirectory(Path.Combine(this.ColorSchemesSubDirectory));
            }

            // Create the example ColorScheme
            // ------------------------------

            string exampleColorSchemeFile = Path.Combine(this.ColorSchemesSubDirectory, "Red.xml");

            if (File.Exists(exampleColorSchemeFile))
            {
                File.Delete(exampleColorSchemeFile);
            }

            XDocument exampleColorSchemeXml = XDocument.Parse("<ColorScheme><AccentColor>#D7350F</AccentColor></ColorScheme>");
            exampleColorSchemeXml.Save(exampleColorSchemeFile);


            // Create the "How to create ColorSchemes.txt" file
            // ------------------------------------------------

            string howToFile = Path.Combine(this.ColorSchemesSubDirectory, "How to create ColorSchemes.txt");

            if (File.Exists(howToFile))
            {
                File.Delete(howToFile);
            }

            string[] lines = {
                                "How to create ColorSchemes?",
                                "---------------------------",
                                "",
                                "1. Copy and rename the file Red.xml",
                                "2. Open the file and edit the color code of AccentColor",
                                "3. Your ColorScheme appears automatically in " + ProductInformation.ApplicationName
                                };

            File.WriteAllLines(howToFile, lines, System.Text.Encoding.UTF8);

            // Get the available ColorSchemes
            // ------------------------------

            this.GetAllColorSchemes();

            // Configure the ColorSchemeTimer
            // ------------------------------

            this.colorSchemeTimer.Interval = TimeSpan.FromSeconds(this.colorSchemeTimeoutSeconds).TotalMilliseconds;
            this.colorSchemeTimer.Elapsed += new ElapsedEventHandler(ColorSchemeTimer_Elapsed);

            // Start the ColorSchemeWatcher
            // ----------------------------

            this.colorSchemeWatcher = new FileSystemWatcher(this.ColorSchemesSubDirectory);
            this.colorSchemeWatcher.EnableRaisingEvents = true;

            this.colorSchemeWatcher.Changed += new FileSystemEventHandler(WatcherChangedHandler);
            this.colorSchemeWatcher.Deleted += new FileSystemEventHandler(WatcherChangedHandler);
            this.colorSchemeWatcher.Created += new FileSystemEventHandler(WatcherChangedHandler);
            this.colorSchemeWatcher.Renamed += new RenamedEventHandler(WatcherRenamedHandler);
        }
    
        private void ColorSchemeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.colorSchemeTimer.Stop();
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.GetAllColorSchemes();
                this.ColorSchemesChanged(this, new EventArgs());
            });
        }

        private void WatcherRenamedHandler(object sender, RenamedEventArgs e)
        {
            // Using a Timer here prevents that consecutive WatcherRenamed events trigger multiple ColorSchemesChanged events
            this.colorSchemeTimer.Stop();
            this.colorSchemeTimer.Start();
        }

        private void WatcherChangedHandler(object sender, FileSystemEventArgs e)
        {
            // Using a Timer here prevents that consecutive WatcherChanged events trigger multiple ColorSchemesChanged events
            this.colorSchemeTimer.Stop();
            this.colorSchemeTimer.Start();
        }
    
        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {

            if (msg == WM_DWMCOLORIZATIONCOLORCHANGED & this.followWindowsColor)
            {
                this.ApplyColorScheme(true);
            }

            return IntPtr.Zero;
        }

        private string GetWindowsDWMColor()
        {
            string returnColor = "";

            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\DWM");

                if (key != null)
                {
                    object value = null;

                    value = key.GetValue("ColorizationColor");

                    Color color = (Color)ColorConverter.ConvertFromString(string.Format("#{0:X6}", value));

                    // We trim the first 3 characters: # and the 2 characters indicating opacity. We want opacity=1.0
                    var trimmedColor = "#" + color.ToString().Substring(3);

                    returnColor = trimmedColor;
                }

            }
            catch (Exception)
            {
            }

            return returnColor;
        }

        private void GetBuiltInColorSchemes()
        {
            foreach (ColorScheme cs in this.builtInColorSchemes)
            {
                this.colorSchemes.Add(cs);
            }

        }

        private void GetAllColorSchemes()
        {
            this.colorSchemes.Clear();
            this.GetBuiltInColorSchemes();

            var dirInfo = new System.IO.DirectoryInfo(this.ColorSchemesSubDirectory);

            foreach (System.IO.FileInfo fileInfo in dirInfo.GetFiles("*.xml"))
            {
                // We put everything in a try catch, because the user might have made errors in his XML.
                try
                {
                    var doc = XDocument.Load(fileInfo.FullName);
                    String colorSchemeName = fileInfo.Name.Replace(".xml", "");
                    XElement colorSchemeElement = (from t in doc.Elements("ColorScheme")
                                                   select t).Single();

                    ColorScheme colorScheme = new ColorScheme
                    {
                        Name = colorSchemeName,
                        AccentColor = colorSchemeElement.Element("AccentColor").Value
                    };

                    try
                    {
                        // This is some sort of validation of the XML file. If the conversion from String to Color doesn't work, the XML is invalid.
                        Color accentColor = (Color)ColorConverter.ConvertFromString(colorScheme.AccentColor);
                        this.colorSchemes.Add(colorScheme);
                    }
                    catch (Exception)
                    {
                        //Logger.Instance.Error("Exception: {0}", ex.Message)
                    }
                }
                catch (Exception)
                {
                    //Logger.Instance.Error("Exception: {0}", ex.Message)
                }
            }
        }

        private ResourceDictionary GetCurrentThemeDictionary()
        {
            // Determine the current theme by looking at the app resources and return the 
            // first dictionary having the resource key 'RG_WindowBorderBrush' defined.
            return (from dict in Application.Current.Resources.MergedDictionaries
                    where dict.Contains("RG_AppearanceServiceBrush")
                    select dict).FirstOrDefault();
        }

        private void ReApplyTheme()
        {
            ResourceDictionary currentThemeDict = this.GetCurrentThemeDictionary();

            if (currentThemeDict != null)
            {
                var newThemeDict = new ResourceDictionary { Source = currentThemeDict.Source };

                // Prevent exceptions by adding the new dictionary before removing the old one
                Application.Current.Resources.MergedDictionaries.Add(newThemeDict);
                Application.Current.Resources.MergedDictionaries.Remove(currentThemeDict);
            }

        }
   
        public void WatchWindowsColor(Window win)
        {
            IntPtr windowHandle = (new WindowInteropHelper(win)).Handle;
            HwndSource src = HwndSource.FromHwnd(windowHandle);
            src.AddHook(new HwndSourceHook(WndProc));
        }

        public List<ColorScheme> GetColorSchemes()
        {
            return this.colorSchemes;
        }

        public ColorScheme GetColorScheme(string name)
        {
            // Set the default theme in case the theme is not found by using the For loop
            ColorScheme returnVal = this.colorSchemes[0];

            foreach (ColorScheme item in this.colorSchemes)
            {
                if (item.Name == name)
                {
                    returnVal = item;
                }
            }

            return returnVal;
        }

        public List<string> GetThemes()
        {
            List<string> returnList = new List<string>();

            foreach (string exp in Defaults.Themes)
            {
                returnList.Add(exp);
            }

            return returnList;
        }

        public void ApplyTheme(string name)
        {
            ResourceDictionary currentThemeDict = this.GetCurrentThemeDictionary();

            var newThemeDict = new ResourceDictionary { Source = new Uri(string.Format("/{0};component/Resources/Themes/{1}.xaml", Assembly.GetExecutingAssembly().GetName().Name, name), UriKind.RelativeOrAbsolute) };

            // Prevent exceptions by adding the new dictionary before removing the old one
            Application.Current.Resources.MergedDictionaries.Add(newThemeDict);
            Application.Current.Resources.MergedDictionaries.Remove(currentThemeDict);

            if (AppearanceChanged != null)
            {
                AppearanceChanged(this, null);
            }
        }

        public void ApplyColorScheme(bool followWindowsColor, string selectedColorScheme = "")
        {
            this.followWindowsColor = followWindowsColor;

            Color accentColor = default(Color);

            if (this.followWindowsColor)
            {
                try
                {
                    // This should never fail. But just in case, don't apply the ColorScheme
                    accentColor = (Color)ColorConverter.ConvertFromString(GetWindowsDWMColor());
                }
                catch (Exception)
                {
                    return;
                }
            }
            else
            {
                ColorScheme cs = this.GetColorScheme(selectedColorScheme);

                try
                {
                    // This can fail if the user created a XML file with incorrect color codes. 
                    // In case this fails, don't apply the ColorScheme
                    accentColor = (Color)ColorConverter.ConvertFromString(cs.AccentColor);
                }
                catch (Exception)
                {
                    return;
                }
            }

            Application.Current.Resources["RG_AccentColor"] = accentColor;
            Application.Current.Resources["RG_AccentBrush"] = new SolidColorBrush(accentColor);

            // Re-apply theme to ensure brushes referencing AccentColor1 and AccentColor2 are updated
            ReApplyTheme();

            if (AppearanceChanged != null)
            {
                AppearanceChanged(this, null);
            }
        }
    
        public event ColorSchemesChangedEventHandler ColorSchemesChanged = delegate { };
        public event AppearanceChangedEventHandler AppearanceChanged = delegate { };
    }
}
