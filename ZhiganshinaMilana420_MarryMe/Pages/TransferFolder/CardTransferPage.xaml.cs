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

namespace ZhiganshinaMilana420_MarryMe.Pages.TransferFolder
{
    /// <summary>
    /// Логика взаимодействия для CardTransferPage.xaml
    /// </summary>
    public partial class CardTransferPage : Page
    {
        private int _currentPhotoIndex = 0;
        private List<TransferPhoto> _transferPhotos;
        public static CoupleFavorites coupleFavorites { get; set; }
        Transfer contextTransfer;
        Couple contextCouple;

        public static Transfer transfer1 = new Transfer();
        public static Couple couple1 = new Couple();
        public static CoupleFavorites coupleFavorites1 = new CoupleFavorites();
        public static List<Transfer> transfers { get; set; }
        public CardTransferPage(Transfer transfer, Couple couple, CoupleFavorites coupleFavorites)
        {
            InitializeComponent();
            contextCouple = couple;
            transfer1 = transfer;
            couple1 = couple;
            coupleFavorites1 = coupleFavorites;
            contextTransfer= transfer ?? throw new ArgumentNullException(nameof(transfer));
            LoadData();
        }
        private void LoadData()
        {
            try
            {
                _transferPhotos = DbConnection.MarryMe.TransferPhoto
                    .Where(p => p.TransferId == contextTransfer.Id).ToList();

                UpdatePhotoDisplay();

                // Вывод данных ресторана
                NameTb.Text = contextTransfer.Name;
                DescriptionTb.Text = contextTransfer.Description;
                PriceTb.Text = contextTransfer.Price.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }
        private void UpdatePhotoDisplay()
        {
            if (_transferPhotos == null || _transferPhotos.Count == 0)
            {
                CurrentImage.Source = null;
                PhotoCounter.Text = "Нет фотографий";
                return;
            }

            // Обеспечиваем цикличность перелистывания
            if (_currentPhotoIndex >= _transferPhotos.Count)
                _currentPhotoIndex = 0;
            else if (_currentPhotoIndex < 0)
                _currentPhotoIndex = _transferPhotos.Count - 1;

            // Обновляем отображаемое изображение
            var currentPhoto = _transferPhotos[_currentPhotoIndex];
            try
            {
                CurrentImage.Source = ConvertByteArrayToBitmapImage(currentPhoto.Photo);
                PhotoCounter.Text = $"{_currentPhotoIndex + 1} / {_transferPhotos.Count}";
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
                var activeBooking = DbConnection.MarryMe.TransferBookingDates
                    .FirstOrDefault(b => b.BookingDate == contextCouple.WeddingDate &&
                                       b.Status == true &&
                                       DbConnection.MarryMe.CoupleFavorites
                                           .Any(cf => cf.CoupleId == contextCouple.Id &&
                                                     cf.TransferId == b.TransferId));

                // Если активная бронь есть - деактивируем ее
                if (activeBooking != null)
                {
                    activeBooking.Status = false;
                }

                // Проверяем, не забронирован ли уже этот ресторан на эту дату
                bool isRestaurantBooked = DbConnection.MarryMe.TransferBookingDates
                    .Any(b => b.TransferId == contextTransfer.Id &&
                             b.BookingDate == contextCouple.WeddingDate &&
                             b.Status == true);

                if (isRestaurantBooked)
                {
                    MessageBox.Show("Этот трансфер уже забронирована. Пожалуйста, выберите другую команду.",
                                  "Команда занят",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                    NavigationService.Navigate(new СoupleСardPage(contextCouple));
                    return;
                }

                TransferBookingDates newBooking = new TransferBookingDates
                {
                    TransferId = contextTransfer.Id,
                    BookingDate = (DateTime)contextCouple.WeddingDate,
                    Status = true,
                    CoupleId = contextCouple.Id,
                };

                DbConnection.MarryMe.TransferBookingDates.Add(newBooking);

                // Обновляем избранное
                var favorite = DbConnection.MarryMe.CoupleFavorites
                    .FirstOrDefault(cf => cf.CoupleId == contextCouple.Id);

                if (favorite == null)
                {
                    favorite = new CoupleFavorites
                    {
                        CoupleId = contextCouple.Id,
                        TransferId = contextTransfer.Id
                    };
                    DbConnection.MarryMe.CoupleFavorites.Add(favorite);
                }
                else
                {
                    favorite.TransferId = contextTransfer.Id;
                }

                // Сохраняем все изменения
                DbConnection.MarryMe.SaveChanges();

                MessageBox.Show("Трфнсфер успешно забронирован на вашу дату свадьбы!",
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
