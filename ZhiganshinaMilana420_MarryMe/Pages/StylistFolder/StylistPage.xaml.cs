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
using ZhiganshinaMilana420_MarryMe.DB;
using ZhiganshinaMilana420_MarryMe.Pages.HostFolder;
using ZhiganshinaMilana420_MarryMe.Pages.RestaurantForder;

namespace ZhiganshinaMilana420_MarryMe.Pages.StylistFolder
{
    /// <summary>
    /// Логика взаимодействия для StylistPage.xaml
    /// </summary>
    public partial class StylistPage : Page
    {
        public static List<Stylist> stylists { get; set; }
        public static List<StylistType> typees { get; set; }
        public StylistPage()
        {
            InitializeComponent();
            stylists = new List<Stylist>(DbConnection.MarryMe.Stylist.ToList());
            StylistLV.ItemsSource = stylists;

            typees = new List<StylistType>(DbConnection.MarryMe.StylistType.ToList());
            typees.Insert(0, new StylistType() { Name = "Все" });
            FilterCb.SelectedIndex = 0;
            this.DataContext = this;
        }

        public void Refresh()
        {
            var filterRestaurant = DbConnection.MarryMe.Stylist.ToList();
            var category = FilterCb.SelectedItem as StylistType;

            if (category != null && category.Id != 0)
            {
                filterRestaurant = filterRestaurant.Where(c => c.StylistTypeId == category.Id).ToList();
            }

            if (SearchTb.Text.Length > 0)
            {
                filterRestaurant = filterRestaurant.Where(r => r.TeamName.ToLower().Contains(SearchTb.Text.Trim().ToLower())).ToList();
            }
            StylistLV.ItemsSource = filterRestaurant;
        }

        private void SearchTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            Refresh();
        }

        private void FilterCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Refresh();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddStylistPage());
        }
        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Stylist stylist)
            {
                NavigationService.Navigate(new EditStylistPage(stylist));
            }
        }
        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Stylist stylist)
            {
                try
                {
                    MessageBoxResult result = MessageBox.Show("Вы точно хотите удалить этоу команду?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        DbConnection.MarryMe.Stylist.Remove(stylist);
                        DbConnection.MarryMe.SaveChanges();
                        NavigationService.Navigate(new StylistPage());
                        MessageBox.Show("Команда успешно удалёна!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch
                {
                    MessageBox.Show("Информацию о команде невозможно удалить, она забронирована гостями", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }
    }
}
