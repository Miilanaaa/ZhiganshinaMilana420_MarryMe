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
using ZhiganshinaMilana420_MarryMe.Pages.PhotographerVideographerFolder;

namespace ZhiganshinaMilana420_MarryMe.Pages.MusicianFolder
{
    /// <summary>
    /// Логика взаимодействия для MusicianMenuPage.xaml
    /// </summary>
    public partial class MusicianMenuPage : Page
    {
        public static List<MusicianType> typees { get; set; }
        public static List<Musician> allMusician { get; set; }
        private List<Musician> filteredMusician;
        private List<Musician> displayedMusician;
        Couple contextCouple;
        public static CoupleFavorites coupleFavorites1 = new CoupleFavorites();

        private int currentPage = 1;
        private int itemsPerPage = 8;
        private int totalPages;
        public MusicianMenuPage(Couple couple, CoupleFavorites coupleFavorites)
        {
            InitializeComponent();
            contextCouple = couple;
            coupleFavorites1 = coupleFavorites;

            // Загружаем все рестораны
            LoadAllRestaurants();

            SearchTb.TextChanged += SearchTb_TextChanged;

            typees = new List<MusicianType>(DbConnection.MarryMe.MusicianType.ToList());
            typees.Insert(0, new MusicianType() { Name = "Все" });
            FilterCb.SelectedIndex = 0;
            this.DataContext = this;
        }
        private void LoadAllRestaurants()
        {
            allMusician= new List<Musician>(DbConnection.MarryMe.Musician.ToList());
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
            var type = FilterCb.SelectedItem as MusicianType;

            // Применяем фильтр по цене
            decimal minPrice, maxPrice;
            if (!decimal.TryParse(PriceFromTb.Text, out minPrice)) minPrice = 0;
            if (!decimal.TryParse(PriceToTb.Text, out maxPrice)) maxPrice = decimal.MaxValue;

            // Получаем выбранную дату
            DateTime? selectedDate = DateBookingDp.SelectedDate;

            filteredMusician= allMusician
                .Where(r => r.TeamName.ToLower().Contains(searchText)) // Фильтр по названию
                .Where(r => r.Price >= minPrice && r.Price <= maxPrice) // Фильтр по цене
                .Where(r => selectedDate == null || !IsMusicianBooked(r.Id, selectedDate.Value)) // Фильтр по дате
                .ToList();

            if (type != null && type.Name != "Все")
            {
                filteredMusician = filteredMusician.Where(r => r.MusicianTypeId == type.Id).ToList();
            }

            // Применяем сортировку
            switch (SortCb.SelectedIndex)
            {
                case 1: // По возрастанию
                    filteredMusician= filteredMusician.OrderBy(r => r.Price).ToList();
                    break;
                case 2: // По убыванию
                    filteredMusician = filteredMusician.OrderByDescending(r => r.Price).ToList();
                    break;
            }

            // Обновляем пагинацию
            currentPage = 1;
            InitializePagination();
        }
        private bool IsMusicianBooked(int photoId, DateTime date)
        {
            return DbConnection.MarryMe.MusicianBookingDates
                .Any(b => b.MusicianId == photoId &&
                         b.BookingDate == date.Date &&
                         b.Status == true); // Только активные бронирования
        }
        private void InitializePagination()
        {
            // Вычисляем общее количество страниц
            totalPages = (int)Math.Ceiling((double)filteredMusician.Count / itemsPerPage);

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
            displayedMusician= filteredMusician
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            MusicianLV.ItemsSource = displayedMusician;

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
        private void MusicianLV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MusicianLV.SelectedItem is Musician musician)
            {
                musician = MusicianLV.SelectedItem as Musician;
                NavigationService.Navigate(new CardMusicianPage(musician, contextCouple, coupleFavorites1));
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
