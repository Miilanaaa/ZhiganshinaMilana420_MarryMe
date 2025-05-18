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
using ZhiganshinaMilana420_MarryMe.Pages.MusicianFolder;

namespace ZhiganshinaMilana420_MarryMe.Pages.TransferFolder
{
    /// <summary>
    /// Логика взаимодействия для TransferMenuPage.xaml
    /// </summary>
    public partial class TransferMenuPage : Page
    {
        public static List<TransferType> typees { get; set; }
        public static List<Transfer> allTransfer { get; set; }
        private List<Transfer> filteredTransfer;
        private List<Transfer> displayedTransfer;
        Couple contextCouple;
        public static CoupleFavorites coupleFavorites1 = new CoupleFavorites();

        private int currentPage = 1;
        private int itemsPerPage = 8;
        private int totalPages;
        public TransferMenuPage(Couple couple, CoupleFavorites coupleFavorites)
        {
            InitializeComponent();
            contextCouple = couple;
            coupleFavorites1 = coupleFavorites;

            // Загружаем все рестораны
            LoadAllRestaurants();

            SearchTb.TextChanged += SearchTb_TextChanged;

            typees = new List<TransferType>(DbConnection.MarryMe.TransferType.ToList());
            typees.Insert(0, new TransferType() { Name = "Все" });
            FilterCb.SelectedIndex = 0;
            this.DataContext = this;
        }
        private void LoadAllRestaurants()
        {
            allTransfer = new List<Transfer>(DbConnection.MarryMe.Transfer.ToList());
            ApplyFiltersAndSort();
        }
        private void SearchTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFiltersAndSort();
        }
        private void ApplyFiltersAndSort()
        {
            // Применяем фильтр по поисковой строке
            string searchText = SearchTb.Text?.ToLower() ?? string.Empty;
            var type = FilterCb.SelectedItem as TransferType;

            // Применяем фильтр по цене
            decimal minPrice, maxPrice;
            if (!decimal.TryParse(PriceFromTb.Text, out minPrice)) minPrice = 0;
            if (!decimal.TryParse(PriceToTb.Text, out maxPrice)) maxPrice = decimal.MaxValue;

            // Получаем выбранную дату
            DateTime? selectedDate = DateBookingDp.SelectedDate;

            filteredTransfer = allTransfer
                .Where(r => r.Name.ToLower().Contains(searchText)) // Фильтр по названию
                .Where(r => r.Price >= minPrice && r.Price <= maxPrice) // Фильтр по цене
                .Where(r => selectedDate == null || !IsTransferBooked(r.Id, selectedDate.Value)) // Фильтр по дате
                .ToList();

            if (type != null && type.Name != "Все")
            {
                filteredTransfer = filteredTransfer.Where(r => r.TransferTypeId == type.Id).ToList();
            }

            // Применяем сортировку
            switch (SortCb.SelectedIndex)
            {
                case 1: // По возрастанию
                    filteredTransfer = filteredTransfer.OrderBy(r => r.Price).ToList();
                    break;
                case 2: // По убыванию
                    filteredTransfer = filteredTransfer.OrderByDescending(r => r.Price).ToList();
                    break;
            }

            // Обновляем пагинацию
            currentPage = 1;
            InitializePagination();
        }
        private bool IsTransferBooked(int photoId, DateTime date)
        {
            return DbConnection.MarryMe.TransferBookingDates
                .Any(b => b.TransferId == photoId &&
                         b.BookingDate == date.Date &&
                         b.Status == true); // Только активные бронирования
        }
        private void InitializePagination()
        {
            // Вычисляем общее количество страниц
            totalPages = (int)Math.Ceiling((double)filteredTransfer.Count / itemsPerPage);

            // Очищаем панель пагинации
            PaginationPanel.Children.Clear();
            PaginationPanel.Children.Add(PrevPageBtn);

            // Создаем кнопки для каждой страницы
            for (int i = 1; i <= totalPages; i++)
            {
                var pageBtn = new Button
                {
                    Content = i.ToString(),
                    Width = 40,
                    Height = 40,
                    FontSize = 15,
                    Margin = new Thickness(5, 0, 5, 0),
                    Tag = i
                };
                pageBtn.Click += PageBtn_Click;

                if (i == currentPage)
                {
                    pageBtn.Background = Brushes.LightGray;
                }

                PaginationPanel.Children.Add(pageBtn);
            }

            PaginationPanel.Children.Add(NextPageBtn);

            LoadPageData();
        }

        private void LoadPageData()
        {
            displayedTransfer = filteredTransfer
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            TransferLV.ItemsSource = displayedTransfer;

            UpdatePaginationButtons();
        }

        private void UpdatePaginationButtons()
        {
            foreach (var child in PaginationPanel.Children)
            {
                if (child is Button btn && btn.Tag is int pageNumber)
                {
                    btn.Background = pageNumber == currentPage ? Brushes.LightGray : Brushes.Transparent;
                }
            }

            PrevPageBtn.IsEnabled = currentPage > 1;
            NextPageBtn.IsEnabled = currentPage < totalPages;
        }


        private void PageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int pageNumber)
            {
                currentPage = pageNumber;
                LoadPageData();
            }
        }

        private void PrevPageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadPageData();
            }
        }

        private void NextPageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                LoadPageData();
            }
        }
        private void TransferLV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TransferLV.SelectedItem is Transfer transfer)
            {
                transfer = TransferLV.SelectedItem as Transfer;
                NavigationService.Navigate(new CardTransferPage(transfer, contextCouple, coupleFavorites1));
            }
        }

        private void ExitBt_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new СoupleСardPage(contextCouple));
        }

        private void ApplyPriceFilter_Click(object sender, RoutedEventArgs e)
        {
            ApplyFiltersAndSort();
        }

        private void SortCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFiltersAndSort();
        }
        private void DateBookingDp_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFiltersAndSort();
        }

        private void FilterCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFiltersAndSort();
        }
    }
}
