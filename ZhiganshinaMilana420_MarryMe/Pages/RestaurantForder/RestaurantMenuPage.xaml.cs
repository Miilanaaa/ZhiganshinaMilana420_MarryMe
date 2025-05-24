using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
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

namespace ZhiganshinaMilana420_MarryMe.Pages.RestaurantForder
{
    /// <summary>
    /// Логика взаимодействия для RestaurantMenuPage.xaml
    /// </summary>
    public partial class RestaurantMenuPage : Page
    {
        public static List<Restaurant> allRestaurants { get; set; }
        private List<Restaurant> filteredRestaurants;
        private List<Restaurant> displayedRestaurants;
        Couple contextCouple;
        public static CoupleFavorites coupleFavorites1 = new CoupleFavorites();

        private int currentPage = 1;
        private int itemsPerPage = 8;
        private int totalPages;
        public RestaurantMenuPage(Couple couple, CoupleFavorites coupleFavorites)
        {
            InitializeComponent();
            contextCouple = couple;
            coupleFavorites1 = coupleFavorites;

            // Загружаем все рестораны
            LoadAllRestaurants();

            SearchTb.TextChanged += SearchTb_TextChanged;
        }
        private void LoadAllRestaurants()
        {
            allRestaurants = new List<Restaurant>(DbConnection.MarryMe.Restaurant.ToList());
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

            // Применяем фильтр по цене
            decimal minPrice, maxPrice;
            if (!decimal.TryParse(PriceFromTb.Text, out minPrice)) minPrice = 0;
            if (!decimal.TryParse(PriceToTb.Text, out maxPrice)) maxPrice = decimal.MaxValue;

            // Получаем выбранную дату
            DateTime? selectedDate = DateBookingDp.SelectedDate;

            // Применяем фильтр по вместимости
            var capacityFilter = CapacityFilterCb.SelectedItem as ComboBoxItem;
            int minCapacity = 0;
            int maxCapacity = int.MaxValue;

            if (capacityFilter?.Tag != null && capacityFilter.Tag.ToString() != "Все вместимости")
            {
                var capacityRange = capacityFilter.Tag.ToString().Split('-');
                minCapacity = int.Parse(capacityRange[0]);
                maxCapacity = int.Parse(capacityRange[1]);
            }

            filteredRestaurants = allRestaurants
                .Where(r => r.Name.ToLower().Contains(searchText)) // Фильтр по названию
                .Where(r => r.Price >= minPrice && r.Price <= maxPrice) // Фильтр по цене
                .Where(r => r.Сapacity >= minCapacity && r.Сapacity <= maxCapacity) // Фильтр по вместимости
                .Where(r => selectedDate == null || !IsRestaurantBooked(r.Id, selectedDate.Value)) // Фильтр по дате
                .ToList();

            // Применяем сортировку
            switch (SortCb.SelectedIndex)
            {
                case 1: // По возрастанию цены
                    filteredRestaurants = filteredRestaurants.OrderBy(r => r.Price).ToList();
                    break;
                case 2: // По убыванию цены
                    filteredRestaurants = filteredRestaurants.OrderByDescending(r => r.Price).ToList();
                    break;
                case 3: // По возрастанию вместимости
                    filteredRestaurants = filteredRestaurants.OrderBy(r => r.Сapacity).ToList();
                    break;
                case 4: // По убыванию вместимости
                    filteredRestaurants = filteredRestaurants.OrderByDescending(r => r.Сapacity).ToList();
                    break;
            }

            // Обновляем пагинацию
            currentPage = 1;
            InitializePagination();
        }
        private void CapacityFilterCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFiltersAndSort();
        }
        private bool IsRestaurantBooked(int restaurantId, DateTime date)
        {
            return DbConnection.MarryMe.RestaurantBookingDates
                .Any(b => b.RestaurantId == restaurantId &&
                         b.BookingDate == date.Date &&
                         b.Status == true); // Только активные бронирования
        }
        private void InitializePagination()
        {
            // Вычисляем общее количество страниц
            totalPages = (int)Math.Ceiling((double)filteredRestaurants.Count / itemsPerPage);

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
            displayedRestaurants = filteredRestaurants
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            RestaurantLV.ItemsSource = displayedRestaurants;

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
        private void RestaurantLV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RestaurantLV.SelectedItem is Restaurant restaurant )
            {
                restaurant = RestaurantLV.SelectedItem as Restaurant;
                NavigationService.Navigate(new CardRestaurantPage(restaurant, contextCouple, coupleFavorites1));
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
    }
}
