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

namespace ZhiganshinaMilana420_MarryMe.Pages.MusicianFolder
{
    /// <summary>
    /// Логика взаимодействия для MusicianPage.xaml
    /// </summary>
    public partial class MusicianPage : Page
    {
        public static List<MusicianType> typees { get; set; }
        public static List<Musician> musicians { get; set; }
        public MusicianPage()
        {
            InitializeComponent();
            musicians = new List<Musician>(DbConnection.MarryMe.Musician.ToList());
            MusicianLV.ItemsSource = musicians;

            typees = new List<MusicianType>(DbConnection.MarryMe.MusicianType.ToList());
            typees.Insert(0, new MusicianType() { Name = "Все" });
            FilterCb.SelectedIndex = 0;
            this.DataContext = this;
        }
        public void Refresh()
        {
            var filterRestaurant = DbConnection.MarryMe.Musician.ToList();
            var category = FilterCb.SelectedItem as MusicianType;

            if (category != null && category.Id != 0)
            {
                filterRestaurant = filterRestaurant.Where(c => c.MusicianTypeId == category.Id).ToList();
            }

            if (SearchTb.Text.Length > 0)
            {
                filterRestaurant = filterRestaurant.Where(r => r.TeamName.ToLower().Contains(SearchTb.Text.Trim().ToLower())).ToList();
            }
            MusicianLV.ItemsSource = filterRestaurant;
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
            NavigationService.Navigate(new AddMusicianPage());
        }
        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Musician musician)
            {
                NavigationService.Navigate(new EditMusicianPage(musician));
            }

        }
        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Musician musician)
            {
                try
                {
                    MessageBoxResult result = MessageBox.Show("Вы точно хотите удалить эту группу?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        DbConnection.MarryMe.Musician.Remove(musician);
                        DbConnection.MarryMe.SaveChanges();
                        NavigationService.Navigate(new MusicianPage());
                        MessageBox.Show("Группа успешно удалёна!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch
                {
                    MessageBox.Show("Информацию о группе невозможно удалить, она забронирована клиентами", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }
    }
}
