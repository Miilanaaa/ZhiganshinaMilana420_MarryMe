using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

namespace ZhiganshinaMilana420_MarryMe.Pages.BouquetFolder
{
    /// <summary>
    /// Логика взаимодействия для BouquetPage.xaml
    /// </summary>
    public partial class BouquetPage : Page
    {
        public static List<Bouquet> bouquets { get; set; }
        public static List<BouquetType> typees { get; set; }
        public BouquetPage()
        {
            InitializeComponent();
            bouquets = new List<Bouquet>(DbConnection.MarryMe.Bouquet.ToList());
            BouquetLV.ItemsSource = bouquets;

            typees = new List<BouquetType>(DbConnection.MarryMe.BouquetType.ToList());
            typees.Insert(0, new BouquetType() { Name = "Все" });
            FilterCb.SelectedIndex = 0;
            this.DataContext = this;
        }
        public void Refresh()
        {
            var filterDres = DbConnection.MarryMe.Bouquet.ToList();
            var category = FilterCb.SelectedItem as BouquetType;

            if (category != null && category.Id != 0)
            {
                filterDres = filterDres.Where(c => c.BouquetTypeId == category.Id).ToList();
            }

            if (SearchTb.Text.Length > 0)
            {
                filterDres = filterDres.Where(r => r.Name.ToLower().Contains(SearchTb.Text.Trim().ToLower())).ToList();
            }
            BouquetLV.ItemsSource = filterDres;
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
            NavigationService.Navigate(new AddBouquetPage());
        }
        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Bouquet bouquet)
            {
                NavigationService.Navigate(new EditBouquetPage(bouquet));
            }
        }
        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Bouquet bouquet)
            {
                try
                {
                    MessageBoxResult result = MessageBox.Show("Вы точно хотите удалить этот товар?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        DbConnection.MarryMe.Bouquet.Remove(bouquet);
                        DbConnection.MarryMe.SaveChanges();
                        NavigationService.Navigate(new BouquetPage());
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
