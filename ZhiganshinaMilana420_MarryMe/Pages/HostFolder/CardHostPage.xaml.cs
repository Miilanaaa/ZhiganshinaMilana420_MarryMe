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

namespace ZhiganshinaMilana420_MarryMe.Pages.HostFolder
{
    /// <summary>
    /// Логика взаимодействия для CardHostPage.xaml
    /// </summary>
    public partial class CardHostPage : Page
    {
        private int _currentPhotoIndex = 0;
        private List<HostPhoto> _restaurantPhotos;
        public static CoupleFavorites coupleFavorites { get; set; }
        Host contextHost;
        Couple contextCouple;

        public static Host host1 = new Host();
        public static Couple couple1 = new Couple();
        public static CoupleFavorites coupleFavorites1 = new CoupleFavorites();

        public static List<Host> hosts { get; set; }
        public CardHostPage(Host host, Couple couple, CoupleFavorites coupleFavorites)
        {
            InitializeComponent();
            contextCouple = couple;
            host1 = host;
            couple1 = couple;
            coupleFavorites1 = coupleFavorites;
            contextHost= host ?? throw new ArgumentNullException(nameof(host));
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
                _restaurantPhotos = DbConnection.MarryMe.HostPhoto
                    .Where(p => p.HostId == contextHost.Id).ToList();

                UpdatePhotoDisplay();

                // Вывод данных ресторана
                NameTb.Text = $"{contextHost.Surname} {contextHost.Name} {contextHost.Patronymic}";
                DescriptionTb.Text = contextHost.Description;
                PriceTb.Text = $"{contextHost.Price} руб.".ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
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
        private void ToBookBtt_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                // Проверяем, есть ли у пары активная бронь (Status == true) на их дату свадьбы
                var activeBooking = DbConnection.MarryMe.HostBookingDates
                    .FirstOrDefault(b => b.BookingDate == contextCouple.WeddingDate &&
                                       b.Status == true &&
                                       DbConnection.MarryMe.CoupleFavorites
                                           .Any(cf => cf.CoupleId == contextCouple.Id &&
                                                     cf.HostId == b.HostId));

                // Если активная бронь есть - деактивируем ее
                if (activeBooking != null)
                {
                    activeBooking.Status = false;
                }

                // Проверяем, не забронирован ли уже этот ресторан на эту дату
                bool isRestaurantBooked = DbConnection.MarryMe.HostBookingDates
                    .Any(b => b.HostId == contextHost.Id &&
                             b.BookingDate == contextCouple.WeddingDate &&
                             b.Status == true);

                if (isRestaurantBooked)
                {
                    MessageBox.Show("Этот ведущий уже забронирован на вашу дату свадьбы. Пожалуйста, выберите другого ведущего.",
                                  "Ведущий занят",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                    NavigationService.Navigate(new СoupleСardPage(contextCouple));
                    return;
                }

                // Создаем новую бронь
                HostBookingDates newBooking = new HostBookingDates
                {
                    HostId = contextHost.Id,
                    BookingDate = (DateTime)contextCouple.WeddingDate,
                    Status = true,
                    CoupleId = contextCouple.Id
                };

                DbConnection.MarryMe.HostBookingDates.Add(newBooking);

                // Обновляем избранное
                var favorite = DbConnection.MarryMe.CoupleFavorites
                    .FirstOrDefault(cf => cf.CoupleId == contextCouple.Id);

                if (favorite == null)
                {
                    favorite = new CoupleFavorites
                    {
                        CoupleId = contextCouple.Id,
                        HostId = contextHost.Id
                    };
                    DbConnection.MarryMe.CoupleFavorites.Add(favorite);
                }
                else
                {
                    favorite.HostId = contextHost.Id;
                }
                // Сохраняем все изменения
                DbConnection.MarryMe.SaveChanges();

                MessageBox.Show("Ведущий успешно забронирован на вашу дату свадьбы!",
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
