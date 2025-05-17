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

namespace ZhiganshinaMilana420_MarryMe.Pages.PhotographerVideographerFolder
{
    /// <summary>
    /// Логика взаимодействия для CardPhotographePage.xaml
    /// </summary>
    public partial class CardPhotographePage : Page
    {
        private int _currentPhotoIndex = 0;
        private List<PhotographerVideographerPhoto> _photoPhotos;
        public static CoupleFavorites coupleFavorites { get; set; }
        PhotographerVideographer contextPhotographerVideographer;
        Couple contextCouple;

        public static PhotographerVideographer photographerVideographer1 = new PhotographerVideographer();
        public static Couple couple1 = new Couple();
        public static CoupleFavorites coupleFavorites1 = new CoupleFavorites();
        public static List<PhotographerVideographer> photographerVideographers { get; set; }

        public CardPhotographePage(PhotographerVideographer photographerVideographer, Couple couple, CoupleFavorites coupleFavorites)
        {
            InitializeComponent();
            contextCouple = couple;
            photographerVideographer1 = photographerVideographer;
            couple1 = couple;
            coupleFavorites1 = coupleFavorites;
            contextPhotographerVideographer = photographerVideographer ?? throw new ArgumentNullException(nameof(photographerVideographer));
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _photoPhotos = DbConnection.MarryMe.PhotographerVideographerPhoto
                    .Where(p => p.PhotographerVideographerId == contextPhotographerVideographer.Id).ToList();

                UpdatePhotoDisplay();

                // Вывод данных ресторана
                NameTb.Text = contextPhotographerVideographer.TeamName;
                DescriptionTb.Text = contextPhotographerVideographer.Description;
                PriceTb.Text = contextPhotographerVideographer.Price.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }
        private void UpdatePhotoDisplay()
        {
            if (_photoPhotos == null || _photoPhotos.Count == 0)
            {
                CurrentImage.Source = null;
                PhotoCounter.Text = "Нет фотографий";
                return;
            }

            // Обеспечиваем цикличность перелистывания
            if (_currentPhotoIndex >= _photoPhotos.Count)
                _currentPhotoIndex = 0;
            else if (_currentPhotoIndex < 0)
                _currentPhotoIndex = _photoPhotos.Count - 1;

            // Обновляем отображаемое изображение
            var currentPhoto = _photoPhotos[_currentPhotoIndex];
            try
            {
                CurrentImage.Source = ConvertByteArrayToBitmapImage(currentPhoto.Photo);
                PhotoCounter.Text = $"{_currentPhotoIndex + 1} / {_photoPhotos.Count}";
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
                var activeBooking = DbConnection.MarryMe.PhotographerVideographerBookingDates
                    .FirstOrDefault(b => b.BookingDate == contextCouple.WeddingDate &&
                                       b.Status == true &&
                                       DbConnection.MarryMe.CoupleFavorites
                                           .Any(cf => cf.CoupleId == contextCouple.Id &&
                                                     cf.PhotographerVideographerId == b.PhotographerVideographerId));

                // Если активная бронь есть - деактивируем ее
                if (activeBooking != null)
                {
                    activeBooking.Status = false;
                }

                // Проверяем, не забронирован ли уже этот ресторан на эту дату
                bool isRestaurantBooked = DbConnection.MarryMe.PhotographerVideographerBookingDates
                    .Any(b => b.PhotographerVideographerId == contextPhotographerVideographer.Id &&
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
                PhotographerVideographerBookingDates newBooking = new PhotographerVideographerBookingDates
                {
                    PhotographerVideographerId = contextPhotographerVideographer.Id,
                    BookingDate = (DateTime)contextCouple.WeddingDate,
                    Status = true,
                    CoupleId = contextCouple.Id,
                };

                DbConnection.MarryMe.PhotographerVideographerBookingDates.Add(newBooking);

                // Обновляем избранное
                var favorite = DbConnection.MarryMe.CoupleFavorites
                    .FirstOrDefault(cf => cf.CoupleId == contextCouple.Id);

                if (favorite == null)
                {
                    favorite = new CoupleFavorites
                    {
                        CoupleId = contextCouple.Id,
                        PhotographerVideographerId = contextPhotographerVideographer.Id
                    };
                    DbConnection.MarryMe.CoupleFavorites.Add(favorite);
                }
                else
                {
                    favorite.PhotographerVideographerId = contextPhotographerVideographer.Id;
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
