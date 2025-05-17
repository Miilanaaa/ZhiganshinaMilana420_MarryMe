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

namespace ZhiganshinaMilana420_MarryMe.Pages.DressFolder
{
    /// <summary>
    /// Логика взаимодействия для DressPage.xaml
    /// </summary>
    public partial class DressPage : Page
    {
        public static List<DressTypy> typees { get; set; }
        public static List<PhotoDress> photoDresses = new List<PhotoDress>();
        public static List<Dress> dresses {  get; set; }
        public DressPage()
        {
            InitializeComponent();
            dresses = new List<Dress>(DbConnection.MarryMe.Dress.ToList());
            DressLV.ItemsSource = dresses;
            photoDresses = new List<PhotoDress>(DbConnection.MarryMe.PhotoDress.ToList());
            
            typees = new List<DressTypy>(DbConnection.MarryMe.DressTypy.ToList());
            typees.Insert(0, new DressTypy() { Name = "Все" });
            FilterCb.SelectedIndex = 0;
            this.DataContext = this;
        }
        public void Refresh()
        {
            var filterDres = DbConnection.MarryMe.Dress.ToList();
            var category = FilterCb.SelectedItem as DressTypy;

            if (category != null && category.Id != 0)
            {
                filterDres = filterDres.Where(c => c.DressTypeId == category.Id).ToList();
            }

            if (SearchTb.Text.Length > 0)
            {
                filterDres = filterDres.Where(r => r.Name.ToLower().StartsWith(SearchTb.Text.Trim().ToLower())).ToList();
            }
            DressLV.ItemsSource = filterDres;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddDressPage());
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Dress dress)
            {
                NavigationService.Navigate(new EditDressPage(dress));
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Dress dress)
            {
                try
                {
                    MessageBoxResult result = MessageBox.Show("Вы точно хотите удалить этот товар?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        DbConnection.MarryMe.Dress.Remove(dress);
                        DbConnection.MarryMe.SaveChanges();
                        NavigationService.Navigate(new DressPage());
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
        private void SearchTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            Refresh();
        }

        private void FilterCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Refresh();
        }
    }
}
