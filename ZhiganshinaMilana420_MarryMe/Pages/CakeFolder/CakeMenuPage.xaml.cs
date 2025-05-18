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
using ZhiganshinaMilana420_MarryMe.Pages.BouquetFolder;

namespace ZhiganshinaMilana420_MarryMe.Pages.CakeFolder
{
    /// <summary>
    /// Логика взаимодействия для CakeMenuPage.xaml
    /// </summary>
    public partial class CakeMenuPage : Page
    {
        public static List<CakeType> typees { get; set; }
        public static List<Cake> allCake { get; set; }
        private List<Cake> filteredCake;
        private List<Cake> displayedCake;
        Couple contextCouple;
        public static CoupleFavorites coupleFavorites1 = new CoupleFavorites();

        private int currentPage = 1;
        private int itemsPerPage = 8;
        private int totalPages;
        public CakeMenuPage(Couple couple, CoupleFavorites coupleFavorites)
        {
            InitializeComponent();
            contextCouple = couple;
            coupleFavorites1 = coupleFavorites;
            typees = new List<CakeType>(DbConnection.MarryMe.CakeType.ToList());
            typees.Insert(0, new CakeType() { Name = "Все" });
            FilterCb.SelectedIndex = 0;
            // Загружаем все рестораны
            LoadAllRestaurants();

            SearchTb.TextChanged += SearchTb_TextChanged;
            FilterCb.SelectionChanged += FilterCb_SelectionChanged;
            this.DataContext = this;
        }
        private void LoadAllRestaurants()
        {
            allCake = new List<Cake>(DbConnection.MarryMe.Cake.ToList());
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
            // Фильтр по поисковой строке
            string searchText = SearchTb.Text?.ToLower() ?? string.Empty;
            var type = FilterCb.SelectedItem as CakeType;

            // Фильтр по цене (от и до)
            decimal minPrice, maxPrice;
            if (!decimal.TryParse(PriceFromTb.Text, out minPrice)) minPrice = 0;
            if (!decimal.TryParse(PriceToTb.Text, out maxPrice)) maxPrice = decimal.MaxValue;

            // Фильтр по весу (от)
            decimal minWeight;
            if (!decimal.TryParse(WeightTb.Text, out minWeight)) minWeight = 0;

            // Начинаем со всех тортов
            filteredCake = allCake
                .Where(r => r.Name.ToLower().Contains(searchText)) // Поиск по названию
                .Where(r => r.Price >= minPrice && r.Price <= maxPrice) // Фильтр по цене
                .ToList();

            // Если указан минимальный вес (> 0), фильтруем
            if (minWeight > 0)
            {
                filteredCake = filteredCake.Where(r => r.Weight >= minWeight).ToList();
            }

            // Фильтр по типу (если выбран не "Все")
            if (type != null && type.Name != "Все")
            {
                filteredCake = filteredCake.Where(r => r.CakeTypeId == type.Id).ToList();
            }

            // Сортировка
            switch (SortCb.SelectedIndex)
            {
                case 1: // По возрастанию цены
                    filteredCake = filteredCake.OrderBy(r => r.Price).ToList();
                    break;
                case 2: // По убыванию цены
                    filteredCake = filteredCake.OrderByDescending(r => r.Price).ToList();
                    break;
            }

            // Обновляем пагинацию
            currentPage = 1;
            InitializePagination();
        }
        private void InitializePagination()
        {
            // Вычисляем общее количество страниц
            totalPages = (int)Math.Ceiling((double)filteredCake.Count / itemsPerPage);

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
            displayedCake = filteredCake
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            CakeLV.ItemsSource = displayedCake;

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
        private void CakeLV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CakeLV.SelectedItem is Cake cake)
            {
                cake = CakeLV.SelectedItem as Cake;
                NavigationService.Navigate(new CardCakePage(cake, contextCouple, coupleFavorites1));
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
