using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Логика взаимодействия для RestaurantPage.xaml
    /// </summary>
    public partial class RestaurantPage : Page
    {
        private List<Restaurant> allRestaurants;
        private List<Restaurant> filteredRestaurants;
        private List<Restaurant> displayedRestaurants;

        private int currentPage = 1;
        private int itemsPerPage = 6; // Since you have 2 columns, 6 items will make 3 rows
        private int totalPages;
        public static List<RestaurantType> typees {  get; set; }
        public static List<RestaurantPhoto> restaurantPhotos = new List<RestaurantPhoto>();
        public static List<Restaurant> restaurants {  get; set; }
        public RestaurantPage()
        {
            InitializeComponent();
            allRestaurants = new List<Restaurant>(DbConnection.MarryMe.Restaurant.ToList());
            restaurantPhotos = new List<RestaurantPhoto>(DbConnection.MarryMe.RestaurantPhoto.ToList());

            typees = new List<RestaurantType>(DbConnection.MarryMe.RestaurantType.ToList());
            typees.Insert(0, new RestaurantType() { Name = "Все" });
            FilterCb.SelectedIndex = 0;
            this.DataContext = this;

            ApplyFiltersAndSort(); // Initialize with all restaurants
        }

        public void Refresh()
        {
            ApplyFiltersAndSort();
        }

        private void ApplyFiltersAndSort()
        {
            var category = FilterCb.SelectedItem as RestaurantType;

            // Apply filters
            filteredRestaurants = allRestaurants
                .Where(r => category == null || category.Id == 0 || r.RestaurantTypeId == category.Id)
                .Where(r => SearchTb.Text.Length == 0 || r.Name.ToLower().StartsWith(SearchTb.Text.Trim().ToLower()))
                .ToList();

            // Update pagination
            currentPage = 1;
            InitializePagination();
        }

        private void InitializePagination()
        {
            // Calculate total pages
            totalPages = (int)Math.Ceiling((double)filteredRestaurants.Count / itemsPerPage);

            // Clear pagination panel
            PaginationPanel.Children.Clear();
            PaginationPanel.Children.Add(PrevPageBtn);

            // Create page buttons
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

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Restaurant restaurant)
            {
                NavigationService.Navigate(new RestaurantEditPage(restaurant));
            }
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RestaurantAddPage());
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if(sender is Button button && button.DataContext is Restaurant restaurant)
            {
                try
                {
                    MessageBoxResult result = MessageBox.Show("Вы точно хотите удалить эту услугу?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if(result == MessageBoxResult.Yes)
                    {
                        DbConnection.MarryMe.Restaurant.Remove(restaurant);
                        DbConnection.MarryMe.SaveChanges();
                        NavigationService.Navigate(new RestaurantPage());
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
