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

namespace ZhiganshinaMilana420_MarryMe.Pages.AccessoryFolder
{
    /// <summary>
    /// Логика взаимодействия для CardAccessoryPage.xaml
    /// </summary>
    public partial class CardAccessoryPage : Page
    {
        private int _currentPhotoIndex = 0;
        private List<AccessoryPhoto> _accessoryPhotos;
        public static CoupleFavorites coupleFavorites { get; set; }
        Accessory contextAccessory;
        Couple contextCouple;

        public static Accessory accessory1 = new Accessory();
        public static Couple couple1 = new Couple();
        public static CoupleFavorites coupleFavorites1 = new CoupleFavorites();
        public static List<Accessory> accessories { get; set; }
        public CardAccessoryPage(Accessory accessory, Couple couple, CoupleFavorites coupleFavorites)
        {
            InitializeComponent();
            contextCouple = couple;
            accessory1 = accessory;
            couple1 = couple;
            coupleFavorites1 = coupleFavorites;
            contextAccessory = accessory ?? throw new ArgumentNullException(nameof(accessory));
            LoadData();
        }
        private void LoadData()
        {
            try
            {
                _accessoryPhotos = DbConnection.MarryMe.AccessoryPhoto
                    .Where(p => p.AccessoryId == contextAccessory.Id).ToList();

                UpdatePhotoDisplay();

                // Вывод данных ресторана
                NameTb.Text = contextAccessory.Name;
                DescriptionTb.Text = contextAccessory.Description;
                PriceTb.Text = contextAccessory.Price.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }
        private void UpdatePhotoDisplay()
        {
            if (_accessoryPhotos == null || _accessoryPhotos.Count == 0)
            {
                CurrentImage.Source = null;
                PhotoCounter.Text = "Нет фотографий";
                return;
            }

            // Обеспечиваем цикличность перелистывания
            if (_currentPhotoIndex >= _accessoryPhotos.Count)
                _currentPhotoIndex = 0;
            else if (_currentPhotoIndex < 0)
                _currentPhotoIndex = _accessoryPhotos.Count - 1;

            // Обновляем отображаемое изображение
            var currentPhoto = _accessoryPhotos[_currentPhotoIndex];
            try
            {
                CurrentImage.Source = ConvertByteArrayToBitmapImage(currentPhoto.Photo);
                PhotoCounter.Text = $"{_currentPhotoIndex + 1} / {_accessoryPhotos.Count}";
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
                // Обновляем избранное
                var favorite = DbConnection.MarryMe.CoupleFavorites
                    .FirstOrDefault(cf => cf.CoupleId == contextCouple.Id);

                if (favorite == null)
                {
                    favorite = new CoupleFavorites
                    {
                        CoupleId = contextCouple.Id,
                        AccessoryId = contextAccessory.Id
                    };
                    DbConnection.MarryMe.CoupleFavorites.Add(favorite);
                }
                else
                {
                    favorite.AccessoryId = contextAccessory.Id;
                }

                // Сохраняем все изменения
                DbConnection.MarryMe.SaveChanges();

                MessageBox.Show("Украшения успешно забронировано!",
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
