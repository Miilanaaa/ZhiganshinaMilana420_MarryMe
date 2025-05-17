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

namespace ZhiganshinaMilana420_MarryMe.Pages.StylistFolder
{
    /// <summary>
    /// Логика взаимодействия для CardStylistPage.xaml
    /// </summary>
    public partial class CardStylistPage : Page
    {
        private int _currentPhotoIndex = 0;
        private List<StylistPhoto> _stylistPhotos;
        public static CoupleFavorites coupleFavorites { get; set; }
        Stylist contextStylist;
        Couple contextCouple;

        public static Stylist stylist1 = new Stylist();
        public static Couple couple1 = new Couple();
        public static CoupleFavorites coupleFavorites1 = new CoupleFavorites();
        public static List<Stylist> stylists { get; set; }
        public CardStylistPage(Stylist stylist, Couple couple, CoupleFavorites coupleFavorites)
        {
            InitializeComponent();
            contextCouple = couple;
            stylist1 = stylist;
            couple1 = couple;
            coupleFavorites1 = coupleFavorites;
            contextStylist = stylist ?? throw new ArgumentNullException(nameof(stylist));
            LoadData();
        }
        private void LoadData()
        {
            try
            {
                _stylistPhotos = DbConnection.MarryMe.StylistPhoto
                    .Where(p => p.StylistId == contextStylist.Id).ToList();

                UpdatePhotoDisplay();

                // Вывод данных ресторана
                NameTb.Text = contextStylist.TeamName;
                DescriptionTb.Text = contextStylist.Description;
                PriceTb.Text = contextStylist.Price.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }
        private void UpdatePhotoDisplay()
        {
            if (_stylistPhotos == null || _stylistPhotos.Count == 0)
            {
                CurrentImage.Source = null;
                PhotoCounter.Text = "Нет фотографий";
                return;
            }

            // Обеспечиваем цикличность перелистывания
            if (_currentPhotoIndex >= _stylistPhotos.Count)
                _currentPhotoIndex = 0;
            else if (_currentPhotoIndex < 0)
                _currentPhotoIndex = _stylistPhotos.Count - 1;

            // Обновляем отображаемое изображение
            var currentPhoto = _stylistPhotos[_currentPhotoIndex];
            try
            {
                CurrentImage.Source = ConvertByteArrayToBitmapImage(currentPhoto.Photo);
                PhotoCounter.Text = $"{_currentPhotoIndex + 1} / {_stylistPhotos.Count}";
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
                var activeBooking = DbConnection.MarryMe.StylistBookingDates
                    .FirstOrDefault(b => b.BookingDate == contextCouple.WeddingDate &&
                                       b.Status == true &&
                                       DbConnection.MarryMe.CoupleFavorites
                                           .Any(cf => cf.CoupleId == contextCouple.Id &&
                                                     cf.StylistId == b.StylistId));

                // Если активная бронь есть - деактивируем ее
                if (activeBooking != null)
                {
                    activeBooking.Status = false;
                }

                // Проверяем, не забронирован ли уже этот ресторан на эту дату
                bool isRestaurantBooked = DbConnection.MarryMe.StylistBookingDates
                    .Any(b => b.StylistId == contextStylist.Id &&
                             b.BookingDate == contextCouple.WeddingDate &&
                             b.Status == true);

                if (isRestaurantBooked)
                {
                    MessageBox.Show("Этота команда уже забронирована на вашу дату свадьбы. Пожалуйста, выберите другую команду.",
                                  "Команда занят",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                    NavigationService.Navigate(new СoupleСardPage(contextCouple));
                    return;
                }
                // Создаем новую бронь
                StylistBookingDates newBooking = new StylistBookingDates
                {
                    StylistId = contextStylist.Id,
                    BookingDate = (DateTime)contextCouple.WeddingDate,
                    Status = true,
                    CoupleId = contextCouple.Id,
                };

                DbConnection.MarryMe.StylistBookingDates.Add(newBooking);

                // Обновляем избранное
                var favorite = DbConnection.MarryMe.CoupleFavorites
                    .FirstOrDefault(cf => cf.CoupleId == contextCouple.Id);

                if (favorite == null)
                {
                    favorite = new CoupleFavorites
                    {
                        CoupleId = contextCouple.Id,
                        StylistId = contextStylist.Id
                    };
                    DbConnection.MarryMe.CoupleFavorites.Add(favorite);
                }
                else
                {
                    favorite.StylistId = contextStylist.Id;
                }

                // Сохраняем все изменения
                DbConnection.MarryMe.SaveChanges();

                MessageBox.Show("Команда успешно забронирована на вашу дату свадьбы!",
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
