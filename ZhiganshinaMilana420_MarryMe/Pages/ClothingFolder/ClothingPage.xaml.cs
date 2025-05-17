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

namespace ZhiganshinaMilana420_MarryMe.Pages.ClothingFolder
{
    /// <summary>
    /// Логика взаимодействия для ClothingPage.xaml
    /// </summary>
    public partial class ClothingPage : Page
    {
        public static List<ClothingType> typees { get; set; }
        public static List<ClothingPhoto> photoClothing = new List<ClothingPhoto>();
        public static List<Clothing> clothings { get; set; }
        public ClothingPage()
        {
            InitializeComponent();
            clothings = new List<Clothing>(DbConnection.MarryMe.Clothing.ToList());
            ClothingLV.ItemsSource = clothings;
            photoClothing = new List<ClothingPhoto>(DbConnection.MarryMe.ClothingPhoto.ToList());
            typees = new List<ClothingType>(DbConnection.MarryMe.ClothingType.ToList());
            typees.Insert(0, new ClothingType() { Name = "Все" });
            FilterCb.SelectedIndex = 0;
            this.DataContext = this;
        }
        public void Refresh()
        {
            var filterDres = DbConnection.MarryMe.Clothing.ToList();
            var category = FilterCb.SelectedItem as ClothingType;

            if (category != null && category.Id != 0)
            {
                filterDres = filterDres.Where(c => c.ClothingTypeId == category.Id).ToList();
            }

            if (SearchTb.Text.Length > 0)
            {
                filterDres = filterDres.Where(r => r.Name.ToLower().StartsWith(SearchTb.Text.Trim().ToLower())).ToList();
            }
            ClothingLV.ItemsSource = filterDres;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddClothingPage());
        }
        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Clothing clothing)
            {
                NavigationService.Navigate(new EditClothingPage(clothing));
            }
        }
        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Clothing clothing)
            {
                try
                {
                    MessageBoxResult result = MessageBox.Show("Вы точно хотите удалить этот товар?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        DbConnection.MarryMe.Clothing.Remove(clothing);
                        DbConnection.MarryMe.SaveChanges();
                        NavigationService.Navigate(new ClothingPage());
                        MessageBox.Show("Товар успешно удалён!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch
                {
                    MessageBox.Show("Информацию о товаре невозможно удалить, он забронирован гостями", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
