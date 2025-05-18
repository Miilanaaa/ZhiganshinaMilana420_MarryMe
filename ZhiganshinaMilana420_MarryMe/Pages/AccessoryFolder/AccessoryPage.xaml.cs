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

namespace ZhiganshinaMilana420_MarryMe.Pages.AccessoryFolder
{
    /// <summary>
    /// Логика взаимодействия для AccessoryPage.xaml
    /// </summary>
    public partial class AccessoryPage : Page
    {
        public static List<Accessory> accessories { get; set; }
        public static List<AccessoryType> typees { get; set; }
        public AccessoryPage()
        {
            InitializeComponent();
            accessories = new List<Accessory>(DbConnection.MarryMe.Accessory.ToList());
            AccessoryLV.ItemsSource = accessories;

            typees = new List<AccessoryType>(DbConnection.MarryMe.AccessoryType.ToList());
            typees.Insert(0, new AccessoryType() { Name = "Все" });
            FilterCb.SelectedIndex = 0;
            this.DataContext = this;
        }

        public void Refresh()
        {
            var filterDres = DbConnection.MarryMe.Accessory.ToList();
            var category = FilterCb.SelectedItem as AccessoryType;

            if (category != null && category.Id != 0)
            {
                filterDres = filterDres.Where(c => c.AccessoryTypeId == category.Id).ToList();
            }

            if (SearchTb.Text.Length > 0)
            {
                filterDres = filterDres.Where(r => r.Name.ToLower().Contains(SearchTb.Text.Trim().ToLower())).ToList();
            }
            AccessoryLV.ItemsSource = filterDres;
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
            NavigationService.Navigate(new AddAccessoryPage());
        }
        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Accessory accessory)
            {
                NavigationService.Navigate(new EditAccessoryPage(accessory));
            }
        }
        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Accessory accessory)
            {
                try
                {
                    MessageBoxResult result = MessageBox.Show("Вы точно хотите удалить этот товар?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        DbConnection.MarryMe.Accessory.Remove(accessory);
                        DbConnection.MarryMe.SaveChanges();
                        NavigationService.Navigate(new AccessoryPage());
                        MessageBox.Show("Товар успешно удалён!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch
                {
                    MessageBox.Show("Информацию о товаре невозможно удалить, он забронирован клиентами", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }
    }
}
