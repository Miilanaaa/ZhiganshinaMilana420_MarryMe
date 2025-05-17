using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZhiganshinaMilana420_MarryMe.Pages.ClothingFolder;

namespace ZhiganshinaMilana420_MarryMe.Pages
{
    /// <summary>
    /// Логика взаимодействия для CollectionPage.xaml
    /// </summary>
    public partial class CollectionPage : Page
    {
        public CollectionPage()
        {
            InitializeComponent();
        }
        private void RestaurantBt_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RestaurantForder.RestaurantPage());
        }

        private void DressBt_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DressFolder.DressPage());
        }

        private void TransferBt_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new TransferFolder.TransferPage());
        }

        private void HostBt_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new HostFolder.HostPage());
        }

        private void DecorationBt_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AccessoryFolder.AccessoryPage());
        }

        private void StylistBt_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new StylistFolder.StylistPage());
        }

        private void CakeBt_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CakeFolder.CakePage());
        }

        private void BouquetBt_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new BouquetFolder.BouquetPage());
        }

        private void MusicBt_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MusicianFolder.MusicianPage());
        }

        private void VideoBt_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PhotographerVideographerFolder.PhotographerVideographerPage());
        }

        private void TuxedoBt_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ClothingFolder.ClothingPage());
        }

        private void RegistryОfficeBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DecorationForder.DecorationPage());
        }
    }
}
