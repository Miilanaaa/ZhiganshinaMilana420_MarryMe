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

namespace ZhiganshinaMilana420_MarryMe.Pages.CakeFolder
{
    /// <summary>
    /// Логика взаимодействия для CakePage.xaml
    /// </summary>
    public partial class CakePage : Page
    {
        public static List<Cake> cakes {  get; set; }
        public CakePage()
        {
            InitializeComponent();
            cakes = new List<Cake>(DbConnection.MarryMe.Cake.ToList());
            CakeLV.ItemsSource = cakes;

            this.DataContext = this;
        }

        private void SearchTb_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void FilterCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddCakePage());
        }
        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Cake cake)
            {
                NavigationService.Navigate(new EditCakePage(cake));
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Cake cake)
            {
                try
                {
                    MessageBoxResult result = MessageBox.Show("Вы точно хотите удалить этот товар?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        DbConnection.MarryMe.Cake.Remove(cake);
                        DbConnection.MarryMe.SaveChanges();
                        NavigationService.Navigate(new CakePage());
                        MessageBox.Show("Товар успешно удален!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch
                {
                    MessageBox.Show("Товар невозможно удалить, он забронирован клиентами", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }
    }
}
