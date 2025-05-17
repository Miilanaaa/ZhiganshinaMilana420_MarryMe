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

namespace ZhiganshinaMilana420_MarryMe.Pages.PhotographerVideographerFolder
{
    /// <summary>
    /// Логика взаимодействия для PhotographerVideographerPage.xaml
    /// </summary>
    public partial class PhotographerVideographerPage : Page
    {
        public static List<PhotographerType> typees { get; set; }
        public static List<PhotographerVideographer> photographerVideographers { get; set; }
        public PhotographerVideographerPage()
        {
            InitializeComponent();
            photographerVideographers = new List<PhotographerVideographer>(DbConnection.MarryMe.PhotographerVideographer.ToList());
            PhotographerVideographerLV.ItemsSource = photographerVideographers;

            typees = new List<PhotographerType>(DbConnection.MarryMe.PhotographerType.ToList());
            typees.Insert(0, new PhotographerType() { Name = "Все" });
            FilterCb.SelectedIndex = 0;
            this.DataContext = this;
        }
        public void Refresh()
        {
            var filterRestaurant = DbConnection.MarryMe.PhotographerVideographer.ToList();
            var category = FilterCb.SelectedItem as PhotographerType;

            if (category != null && category.Id != 0)
            {
                filterRestaurant = filterRestaurant.Where(c => c.PhotographerTypeId == category.Id).ToList();
            }

            if (SearchTb.Text.Length > 0)
            {
                filterRestaurant = filterRestaurant.Where(r => r.TeamName.ToLower().Contains(SearchTb.Text.Trim().ToLower())).ToList();
            }
            PhotographerVideographerLV.ItemsSource = filterRestaurant;
        }

        private void SearchTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            Refresh();
        }

        private void FilterCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Refresh();
        }
        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is PhotographerVideographer photographerVideographer)
            {
                NavigationService.Navigate(new EditPhotographerVideographerPage(photographerVideographer));
            }

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddPhotographerVideographerPage());
        }
        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is PhotographerVideographer photographerVideographer)
            {
                try
                {
                    MessageBoxResult result = MessageBox.Show("Вы точно хотите удалить эту услугу?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        DbConnection.MarryMe.PhotographerVideographer.Remove(photographerVideographer);
                        DbConnection.MarryMe.SaveChanges();
                        NavigationService.Navigate(new PhotographerVideographerPage());
                        MessageBox.Show("Услуга успешно удалёна!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch
                {
                    MessageBox.Show("Информацию об услуге невозможно удалить, она забронирована клиентами", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }
    }
}
