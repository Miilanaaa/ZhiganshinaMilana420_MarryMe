using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

namespace ZhiganshinaMilana420_MarryMe.Pages.ClothingFolder
{
    /// <summary>
    /// Логика взаимодействия для CardClothingPage.xaml
    /// </summary>
    public partial class CardClothingPage : Page
    {
        private int _currentPhotoIndex = 0;
        private List<ClothingPhoto> _clothingPhotos;
        public static CoupleFavorites coupleFavorites { get; set; }
        Clothing contextClothing;
        Couple contextCouple;

        public static Clothing clothinf1 = new Clothing();
        public static Couple couple1 = new Couple();
        public static CoupleFavorites coupleFavorites1 = new CoupleFavorites();
        public static List<Clothing> clothings { get; set; }
        public CardClothingPage(Clothing clothing, Couple couple, CoupleFavorites coupleFavorites)
        {
            InitializeComponent();
            contextCouple = couple;
            clothinf1 = clothing;
            couple1 = couple;
            coupleFavorites1 = coupleFavorites;
            contextClothing = clothing ?? throw new ArgumentNullException(nameof(clothing));
            LoadData();
        }
        private void LoadData()
        {
            try
            {
                _clothingPhotos = DbConnection.MarryMe.ClothingPhoto
                    .Where(p => p.ClothingId == contextClothing.Id).ToList();

                UpdatePhotoDisplay();

                // Вывод данных ресторана
                NameTb.Text = contextClothing.Name;
                DescriptionTb.Text = contextClothing.Description;
                PriceTb.Text = contextClothing.Price.ToString();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }
        private void UpdatePhotoDisplay()
        {
            if (_clothingPhotos == null || _clothingPhotos.Count == 0)
            {
                CurrentImage.Source = null;
                PhotoCounter.Text = "Нет фотографий";
                return;
            }

            // Обеспечиваем цикличность перелистывания
            if (_currentPhotoIndex >= _clothingPhotos.Count)
                _currentPhotoIndex = 0;
            else if (_currentPhotoIndex < 0)
                _currentPhotoIndex = _clothingPhotos.Count - 1;

            // Обновляем отображаемое изображение
            var currentPhoto = _clothingPhotos[_currentPhotoIndex];
            try
            {
                CurrentImage.Source = ConvertByteArrayToBitmapImage(currentPhoto.Photo);
                PhotoCounter.Text = $"{_currentPhotoIndex + 1} / {_clothingPhotos.Count}";
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
                        ClothingGrommId = contextClothing.Id
                    };
                    DbConnection.MarryMe.CoupleFavorites.Add(favorite);
                }
                else
                {
                    favorite.ClothingGrommId = contextClothing.Id;
                }

                // Сохраняем все изменения
                DbConnection.MarryMe.SaveChanges();

                MessageBox.Show("Костюм успешно забронирован!",
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
