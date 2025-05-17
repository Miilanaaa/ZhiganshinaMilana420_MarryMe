using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using ZhiganshinaMilana420_MarryMe.DB.Partikal;
using ZhiganshinaMilana420_MarryMe.Pages;

namespace ZhiganshinaMilana420_MarryMe
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var paletteHelper = new PaletteHelper();
            var baseTheme = BaseTheme.Light; var primaryColor = (Color)FindResource("PrimaryHueMid");
            var secondaryColor = (Color)FindResource("SecondaryAccentMid");
            var theme = Theme.Create(baseTheme, primaryColor, secondaryColor);
            paletteHelper.SetTheme(theme);
            Application.Current.Resources["MaterialDesignPaper"] = FindResource("BackgroundPrimaryBrush");
            Application.Current.Resources["MaterialDesignDivider"] = FindResource("BorderColorBrush");
        }
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            MessageClass.ErrorMessage(e.Exception.Message);
        }
    }
}
