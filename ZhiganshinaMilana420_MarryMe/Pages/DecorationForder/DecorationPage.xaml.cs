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

namespace ZhiganshinaMilana420_MarryMe.Pages.DecorationForder
{
    /// <summary>
    /// Логика взаимодействия для DecorationPage.xaml
    /// </summary>
    public partial class DecorationPage : Page
    {
        public static List<DecorationPhoto> decorationPhotos = new List<DecorationPhoto>();
        public static List<Decoration> decorations { get; set; }
        public DecorationPage()
        {
            InitializeComponent();
            decorations = new List<Decoration>(DbConnection.MarryMe.Decoration.ToList());
            DecorationLV.ItemsSource = decorations;
            decorationPhotos = new List<DecorationPhoto>(DbConnection.MarryMe.DecorationPhoto.ToList());
            //typees = new List<RestaurantType>(DbConnection.MarryMe.RestaurantType.ToList());
            //typees.Insert(0, new RestaurantType() { Name = "Все" });
            //FilterCb.SelectedIndex = 0;

            this.DataContext = this;
        }
        public void Refresh()
        {
            var filterRestaurant = DbConnection.MarryMe.Decoration.ToList();
            //var category = FilterCb.SelectedItem as RestaurantType;

            //if (category != null && category.Id != 0)
            //{
            //    filterRestaurant = filterRestaurant.Where(c => c.RestaurantTypeId == category.Id).ToList();
            //}

            if (SearchTb.Text.Length > 0)
            {
                filterRestaurant = filterRestaurant.Where(r => r.Name.ToLower().StartsWith(SearchTb.Text.Trim().ToLower())).ToList();
            }
            DecorationLV.ItemsSource = filterRestaurant;
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Decoration decoration)
            {
                NavigationService.Navigate(new EditDecorationPage(decoration));
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddDecorationPage());
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Decoration decoration)
            {
                try
                {
                    MessageBoxResult result = MessageBox.Show("Вы точно хотите удалить эту услугу?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        DbConnection.MarryMe.Decoration.Remove(decoration);
                        DbConnection.MarryMe.SaveChanges();
                        NavigationService.Navigate(new DecorationPage());
                        MessageBox.Show("Товар успешно удалён!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch
                {
                    MessageBox.Show("Информацию о ресторане невозможно удалить, он забронирован гостями", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
