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
using ZhiganshinaMilana420_MarryMe.Pages.DressFolder;

namespace ZhiganshinaMilana420_MarryMe.Pages.ClothingFolder
{
    /// <summary>
    /// Логика взаимодействия для ClothingMenuPage.xaml
    /// </summary>
    public partial class ClothingMenuPage : Page
    {
        public static List<ClothingType> typees { get; set; }
        public static List<Clothing> allClothing { get; set; }
        private List<Clothing> filteredClothing;
        private List<Clothing> displayedClothing;
        Couple contextCouple;
        public static CoupleFavorites coupleFavorites1 = new CoupleFavorites();

        private int currentPage = 1;
        private int itemsPerPage = 8;
        private int totalPages;
        public ClothingMenuPage(Couple couple, CoupleFavorites coupleFavorites)
        {
            InitializeComponent();
            contextCouple = couple;
            coupleFavorites1 = coupleFavorites;
            typees = new List<ClothingType>(DbConnection.MarryMe.ClothingType.ToList());
            typees.Insert(0, new ClothingType() { Name = "Все" });
            FilterCb.SelectedIndex = 0;
            // Загружаем все рестораны
            LoadAllRestaurants();

            SearchTb.TextChanged += SearchTb_TextChanged;
            FilterCb.SelectionChanged += FilterCb_SelectionChanged;
            this.DataContext = this;
        }
        private void LoadAllRestaurants()
        {
            allClothing = new List<Clothing>(DbConnection.MarryMe.Clothing.ToList());
            ApplyFiltersAndSort();
        }
        private void FilterCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
            var type = FilterCb.SelectedItem as DressTypy;

            // Применяем фильтр по цене
            decimal minPrice, maxPrice;
            if (!decimal.TryParse(PriceFromTb.Text, out minPrice)) minPrice = 0;
            if (!decimal.TryParse(PriceToTb.Text, out maxPrice)) maxPrice = decimal.MaxValue;

            // Start with all dresses
            filteredClothing= allClothing
                .Where(r => r.Name.ToLower().Contains(searchText)) // Фильтр по названию
                .Where(r => r.Price >= minPrice && r.Price <= maxPrice) // Фильтр по цене
                .ToList();

            // Apply type filter only if not "Все" (All) is selected
            if (type != null && type.Name != "Все")
            {
                filteredClothing = filteredClothing.Where(r => r.ClothingTypeId == type.Id).ToList();
            }

            // Применяем сортировку
            switch (SortCb.SelectedIndex)
            {
                case 1: // По возрастанию
                    filteredClothing = filteredClothing.OrderBy(r => r.Price).ToList();
                    break;
                case 2: // По убыванию
                    filteredClothing = filteredClothing.OrderByDescending(r => r.Price).ToList();
                    break;
            }

            // Обновляем пагинацию
            currentPage = 1;
            InitializePagination();
        }
        private void InitializePagination()
        {
            // Вычисляем общее количество страниц
            totalPages = (int)Math.Ceiling((double)filteredClothing.Count / itemsPerPage);

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
            displayedClothing = filteredClothing
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            ClothingLV.ItemsSource = displayedClothing;

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
        private void ClothingLV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClothingLV.SelectedItem is Clothing clothing)
            {
                clothing = ClothingLV.SelectedItem as Clothing;
                NavigationService.Navigate(new CardClothingPage(clothing, contextCouple, coupleFavorites1));
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
    }
}
