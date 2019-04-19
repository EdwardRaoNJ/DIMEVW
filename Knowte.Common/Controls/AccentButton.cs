﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Knowte.Common.Controls
{
    public class AccentButton : Button
    {
        public Brush AccentForeground
        {
            get { return (Brush)GetValue(AccentForegroundProperty); }

            set { SetValue(AccentForegroundProperty, value); }
        }
      
        public static readonly DependencyProperty AccentForegroundProperty = 
            DependencyProperty.Register("AccentForeground", typeof(Brush), typeof(AccentButton), new PropertyMetadata(null));
    
        static AccentButton()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AccentButton), new FrameworkPropertyMetadata(typeof(AccentButton)));
        }
     
    }
}