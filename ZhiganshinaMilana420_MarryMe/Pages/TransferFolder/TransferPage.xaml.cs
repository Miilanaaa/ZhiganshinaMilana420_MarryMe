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

namespace ZhiganshinaMilana420_MarryMe.Pages.TransferFolder
{
    /// <summary>
    /// Логика взаимодействия для TransferPage.xaml
    /// </summary>
    public partial class TransferPage : Page
    {
        public static List<TransferType> typees { get; set; }
        public static List<TransferPhoto> transferPhotos = new List<TransferPhoto>();
        public static List<Transfer> transfers {  get; set; }
        public TransferPage()
        {
            InitializeComponent();
            transfers = new List<Transfer>(DbConnection.MarryMe.Transfer.ToList());
            TransferLV.ItemsSource = transfers;
            transferPhotos = new List<TransferPhoto>(DbConnection.MarryMe.TransferPhoto.ToList());
            typees = new List<TransferType>(DbConnection.MarryMe.TransferType.ToList());
            typees.Insert(0, new TransferType() { Name = "Все" });
            FilterCb.SelectedIndex = 0;

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
            NavigationService.Navigate(new AddTransferPage());
        }

        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Transfer transfer)
            {
                NavigationService.Navigate(new EditTransferPage(transfer));
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Transfer transfer)
            {
                try
                {
                    MessageBoxResult result = MessageBox.Show("Вы точно хотите удалить эту услугу?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        DbConnection.MarryMe.Transfer.Remove(transfer);
                        DbConnection.MarryMe.SaveChanges();
                        NavigationService.Navigate(new TransferPage());
                        MessageBox.Show("Товар успешно удалён!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch
                {
                    MessageBox.Show("Информацию о транспорте невозможно удалить, он забронирован гостями", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }
    }
}
