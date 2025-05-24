using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public partial class CardRestaurantPage : Page
    {
        private int _currentPhotoIndex = 0;
        private List<RestaurantPhoto> _restaurantPhotos;
        public static CoupleFavorites coupleFavorites { get; set; }
        Restaurant contextRestaurant;
        Couple contextCouple;

        public static Restaurant restaurant1 = new Restaurant();
        public static Couple couple1 = new Couple();
        public static CoupleFavorites coupleFavorites1 = new CoupleFavorites();

        public static List<Restaurant> restaurants { get; set; }

        public CardRestaurantPage(Restaurant restaurant, Couple couple, CoupleFavorites coupleFavorites)
        {
            InitializeComponent();
            contextCouple = couple;
            restaurant1 = restaurant;
            couple1 = couple;
            coupleFavorites1 = coupleFavorites;
            contextRestaurant = restaurant ?? throw new ArgumentNullException(nameof(restaurant));
            LoadData();
        }
        private void ApplyErrorStyle(Control control)
        {
            control.BorderBrush = Brushes.Red;
            control.BorderThickness = new Thickness(1);
            control.ToolTip = "Обязательное поле";
        }

        private void ClearErrorStyle(Control control)
        {
            control.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFABADB3"));
            control.BorderThickness = new Thickness(1);
            control.ToolTip = null;
        }

        private void LoadData()
        {
            try
            {
                _restaurantPhotos = DbConnection.MarryMe.RestaurantPhoto
                    .Where(p => p.RestaurantId == contextRestaurant.Id).ToList();

                UpdatePhotoDisplay();

                // Вывод данных ресторана
                NameTb.Text = contextRestaurant.Name;
                DescriptionTb.Text = contextRestaurant.Description;
                AddressTb.Text = contextRestaurant.Address;
                CapacityTb.Text = contextRestaurant.Сapacity.ToString();
                PriceTb.Text = contextRestaurant.Price.ToString();

                // Show menu button if menu exists
                if (!string.IsNullOrEmpty(contextRestaurant.DocumentMenu))
                {
                    OpenMenuBt.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void OpenMenuBt_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(contextRestaurant.DocumentMenu) &&
                System.IO.File.Exists(contextRestaurant.DocumentMenu))
            {
                try
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = contextRestaurant.DocumentMenu,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось открыть меню: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Меню для этого ресторана не найдено",
                              "Информация",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
        }

        private void UpdatePhotoDisplay()
        {
            if (_restaurantPhotos == null || _restaurantPhotos.Count == 0)
            {
                CurrentImage.Source = null;
                PhotoCounter.Text = "Нет фотографий";
                return;
            }

            // Обеспечиваем цикличность перелистывания
            if (_currentPhotoIndex >= _restaurantPhotos.Count)
                _currentPhotoIndex = 0;
            else if (_currentPhotoIndex < 0)
                _currentPhotoIndex = _restaurantPhotos.Count - 1;

            // Обновляем отображаемое изображение
            var currentPhoto = _restaurantPhotos[_currentPhotoIndex];
            try
            {
                CurrentImage.Source = ConvertByteArrayToBitmapImage(currentPhoto.Photo);
                PhotoCounter.Text = $"{_currentPhotoIndex + 1} / {_restaurantPhotos.Count}";
            }
            catch
            {
                CurrentImage.Source = null;
                PhotoCounter.Text = "Ошибка загрузки изображения";
            }
        }

        private BitmapImage ConvertByteArrayToBitmapImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;

            var image = new BitmapImage();
            using (var mem = new System.IO.MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        private void PrevPhoto_Click(object sender, RoutedEventArgs e)
        {
            _currentPhotoIndex--;
            UpdatePhotoDisplay();
        }

        private void NextPhoto_Click(object sender, RoutedEventArgs e)
        {
            _currentPhotoIndex++;
            UpdatePhotoDisplay();
        }
        private void MenuPriceTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearErrorStyle(MenuPriceTb);
        }

        private void ToBookBtt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Сбрасываем подсветку ошибок
                ClearErrorStyle(MenuPriceTb);

                // Проверяем заполненность поля MenuPriceTb
                if (string.IsNullOrWhiteSpace(MenuPriceTb.Text))
                {
                    ApplyErrorStyle(MenuPriceTb);
                    return;
                }

                // Проверяем корректность введенного числа
                if (!int.TryParse(MenuPriceTb.Text, out int menuPrice) || menuPrice <= 0)
                {
                    ApplyErrorStyle(MenuPriceTb);
                    MessageBox.Show("Некорректное значение стоимости меню. Введите положительное целое число.",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                    return;
                }

                // Проверяем, есть ли у пары активная бронь (Status == true) на их дату свадьбы
                var activeBooking = DbConnection.MarryMe.RestaurantBookingDates
                    .FirstOrDefault(b => b.BookingDate == contextCouple.WeddingDate &&
                                       b.Status == true &&
                                       DbConnection.MarryMe.CoupleFavorites
                                           .Any(cf => cf.CoupleId == contextCouple.Id &&
                                                     cf.RestaurantId == b.RestaurantId));

                // Если активная бронь есть - деактивируем ее
                if (activeBooking != null)
                {
                    activeBooking.Status = false;
                }

                // Проверяем, не забронирован ли уже этот ресторан на эту дату
                bool isRestaurantBooked = DbConnection.MarryMe.RestaurantBookingDates
                    .Any(b => b.RestaurantId == contextRestaurant.Id &&
                             b.BookingDate == contextCouple.WeddingDate &&
                             b.Status == true);

                if (isRestaurantBooked)
                {
                    MessageBox.Show("Этот ресторан уже забронирован на вашу дату свадьбы. Пожалуйста, выберите другой ресторан.",
                                  "Ресторан занят",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                    NavigationService.Navigate(new СoupleСardPage(contextCouple));
                    return;
                }

                // Создаем новую бронь
                RestaurantBookingDates newBooking = new RestaurantBookingDates
                {
                    RestaurantId = contextRestaurant.Id,
                    BookingDate = (DateTime)contextCouple.WeddingDate,
                    Status = true,
                    CoupleId = contextCouple.Id,
                    MenuPrice = menuPrice // Используем проверенное значение
                };

                DbConnection.MarryMe.RestaurantBookingDates.Add(newBooking);

                // Обновляем избранное
                var favorite = DbConnection.MarryMe.CoupleFavorites
                    .FirstOrDefault(cf => cf.CoupleId == contextCouple.Id);

                if (favorite == null)
                {
                    favorite = new CoupleFavorites
                    {
                        CoupleId = contextCouple.Id,
                        RestaurantId = contextRestaurant.Id
                    };
                    DbConnection.MarryMe.CoupleFavorites.Add(favorite);
                }
                else
                {
                    favorite.RestaurantId = contextRestaurant.Id;
                }

                // Сохраняем все изменения
                DbConnection.MarryMe.SaveChanges();

                MessageBox.Show("Ресторан успешно забронирован на вашу дату свадьбы!",
                              "Бронирование подтверждено",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);

                NavigationService.Navigate(new СoupleСardPage(contextCouple));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при бронировании: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
                NavigationService.Navigate(new СoupleСardPage(contextCouple));
            }
        }

        private void ExitBt_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
