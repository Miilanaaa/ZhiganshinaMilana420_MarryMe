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
using ZhiganshinaMilana420_MarryMe.Pages.RestaurantForder;

namespace ZhiganshinaMilana420_MarryMe.Pages.HostFolder
{
    /// <summary>
    /// Логика взаимодействия для HostPage.xaml
    /// </summary>
    public partial class HostPage : Page
    {
        public static List<HostPhoto> hostPhotos = new List<HostPhoto>();
        public static List<Host> hosts {  get; set; }
        public HostPage()
        {
            InitializeComponent();
            hosts = new List<Host>(DbConnection.MarryMe.Host.ToList());
            HostLV.ItemsSource = hosts;

            this.DataContext = this;
        }

        private void SearchTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            var filterRestaurant = DbConnection.MarryMe.Host.ToList();
            if (SearchTb.Text.Length > 0)
            {
                filterRestaurant = filterRestaurant.Where(r => r.Surname.ToLower().StartsWith(SearchTb.Text.Trim().ToLower())||
                                                          r.Name.ToLower().StartsWith(SearchTb.Text.Trim().ToLower())).ToList();
            }
            HostLV.ItemsSource = filterRestaurant;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddHostPage());
        }
        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            if(sender is Button button && button.DataContext is Host host)
            {
                NavigationService.Navigate(new EditHostPage(host));
            }
        }
        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Host host)
            {
                try
                {
                    MessageBoxResult result = MessageBox.Show("Вы точно хотите удалить данные этого ведущего?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        DbConnection.MarryMe.Host.Remove(host);
                        DbConnection.MarryMe.SaveChanges();
                        NavigationService.Navigate(new HostPage());
                        MessageBox.Show("Данные о ведущем успешно удалены!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch
                {
                    MessageBox.Show("Информацию о ведущем невозможно удалить, он забронирован гостями", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }
    }
}
