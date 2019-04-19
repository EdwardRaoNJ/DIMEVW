// // Copyright (c) Microsoft. All rights reserved.
// // Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Windows;
using Knowte;

namespace VideoViewerDemo
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Path.Combine(AppDomain.
                CurrentDomain.BaseDirectory
                .SolutionFolder(),
                @"Knowte\bin\Debug\Knowte.exe"));
                       
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}



public static class Extensions
{
    public static string UpperFolder(this string pFolderName, Int32 pLevel)
    {
        List<string> TheList = new List<string>();

        while (!string.IsNullOrEmpty(pFolderName))
        {
            var temp = Directory.GetParent(pFolderName);
            if (temp == null)
            {
                break;
            }

            pFolderName = Directory.GetParent(pFolderName).FullName;
            TheList.Add(pFolderName);

        }

        if (TheList.Count > 0 && pLevel > 0)
        {
            if (pLevel - 1 <= TheList.Count - 1)
            {
                return TheList[pLevel - 1];
            }
            else
            {
                return pFolderName;
            }
        }
        else
        {
            return pFolderName;
        }
    }
    public static string SolutionFolder(this string pSender)
    {
        return pSender.UpperFolder(4);
    }
}