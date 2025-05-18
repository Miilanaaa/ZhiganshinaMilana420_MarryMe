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

namespace ZhiganshinaMilana420_MarryMe.Pages.MusicianFolder
{
    /// <summary>
    /// Логика взаимодействия для CardMusicianPage.xaml
    /// </summary>
    public partial class CardMusicianPage : Page
    {
        private int _currentPhotoIndex = 0;
        private List<MusicianPhoto> _musicianPhotos;
        public static CoupleFavorites coupleFavorites { get; set; }
        Musician contextMusician;
        Couple contextCouple;

        public static Musician musician1 = new Musician();
        public static Couple couple1 = new Couple();
        public static CoupleFavorites coupleFavorites1 = new CoupleFavorites();
        public static List<Musician> musicians { get; set; }
        public CardMusicianPage(Musician musician, Couple couple, CoupleFavorites coupleFavorites)
        {
            InitializeComponent();
            contextCouple = couple;
            musician1 = musician;
            couple1 = couple;
            coupleFavorites1 = coupleFavorites;
            contextMusician = musician ?? throw new ArgumentNullException(nameof(musician));
            LoadData();
        }
        private void LoadData()
        {
            try
            {
                _musicianPhotos = DbConnection.MarryMe.MusicianPhoto
                    .Where(p => p.MusicianId == contextMusician.Id).ToList();

                UpdatePhotoDisplay();

                // Вывод данных ресторана
                NameTb.Text = contextMusician.TeamName;
                DescriptionTb.Text = contextMusician.Description;
                PriceTb.Text = contextMusician.Price.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }
        private void UpdatePhotoDisplay()
        {
            if (_musicianPhotos == null || _musicianPhotos.Count == 0)
            {
                CurrentImage.Source = null;
                PhotoCounter.Text = "Нет фотографий";
                return;
            }

            // Обеспечиваем цикличность перелистывания
            if (_currentPhotoIndex >= _musicianPhotos.Count)
                _currentPhotoIndex = 0;
            else if (_currentPhotoIndex < 0)
                _currentPhotoIndex = _musicianPhotos.Count - 1;

            // Обновляем отображаемое изображение
            var currentPhoto = _musicianPhotos[_currentPhotoIndex];
            try
            {
                CurrentImage.Source = ConvertByteArrayToBitmapImage(currentPhoto.Photo);
                PhotoCounter.Text = $"{_currentPhotoIndex + 1} / {_musicianPhotos.Count}";
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
                var activeBooking = DbConnection.MarryMe.MusicianBookingDates
                    .FirstOrDefault(b => b.BookingDate == contextCouple.WeddingDate &&
                                       b.Status == true &&
                                       DbConnection.MarryMe.CoupleFavorites
                                           .Any(cf => cf.CoupleId == contextCouple.Id &&
                                                     cf.MusicianId == b.MusicianId));

                // Если активная бронь есть - деактивируем ее
                if (activeBooking != null)
                {
                    activeBooking.Status = false;
                }

                // Проверяем, не забронирован ли уже этот ресторан на эту дату
                bool isRestaurantBooked = DbConnection.MarryMe.MusicianBookingDates
                    .Any(b => b.MusicianId == contextMusician.Id &&
                             b.BookingDate == contextCouple.WeddingDate &&
                             b.Status == true);

                if (isRestaurantBooked)
                {
                    MessageBox.Show("Этота группа уже забронирована на вашу дату свадьбы. Пожалуйста, выберите другую команду.",
                                  "Команда занят",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                    NavigationService.Navigate(new СoupleСardPage(contextCouple));
                    return;
                }

                // Создаем новую бронь
                MusicianBookingDates newBooking = new MusicianBookingDates
                {
                    MusicianId = contextMusician.Id,
                    BookingDate = (DateTime)contextCouple.WeddingDate,
                    Status = true,
                    CoupleId = contextCouple.Id,
                };

                DbConnection.MarryMe.MusicianBookingDates.Add(newBooking);

                // Обновляем избранное
                var favorite = DbConnection.MarryMe.CoupleFavorites
                    .FirstOrDefault(cf => cf.CoupleId == contextCouple.Id);

                if (favorite == null)
                {
                    favorite = new CoupleFavorites
                    {
                        CoupleId = contextCouple.Id,
                        MusicianId = contextMusician.Id
                    };
                    DbConnection.MarryMe.CoupleFavorites.Add(favorite);
                }
                else
                {
                    favorite.MusicianId = contextMusician.Id;
                }

                // Сохраняем все изменения
                DbConnection.MarryMe.SaveChanges();

                MessageBox.Show("Группа успешно забронирована на вашу дату свадьбы!",
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
