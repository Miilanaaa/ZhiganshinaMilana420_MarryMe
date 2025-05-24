using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
using static ZhiganshinaMilana420_MarryMe.Pages.RestaurantForder.RestaurantEditPage;
using Path = System.IO.Path;

namespace ZhiganshinaMilana420_MarryMe.Pages.RestaurantForder
{
    /// <summary>
    /// Логика взаимодействия для RestaurantEditPage.xaml
    /// </summary>
    public partial class RestaurantEditPage : Page
    {
        private ObservableCollection<RestaurantPhoto> photos = new ObservableCollection<RestaurantPhoto>();
        public static List<Restaurant> restaurants { get; set; }
        public static Restaurant restaurant1 = new Restaurant();
        public static Restaurant res { get; set; }
        public static List<RestaurantType> restaurantTypes { get; set; }

        public RestaurantEditPage(Restaurant restaurant)
        {
            InitializeComponent();
            UploadProgress.Visibility = Visibility.Collapsed;

            restaurantTypes = new List<RestaurantType>(DbConnection.MarryMe.RestaurantType.ToList());
            restaurant1 = restaurant;
            res = restaurant;

            // Load photos for the restaurant
            photos = new ObservableCollection<RestaurantPhoto>(
                DbConnection.MarryMe.RestaurantPhoto
                    .Where(p => p.RestaurantId == restaurant.Id)
                    .ToList()
            );

            PhotosLv.ItemsSource = photos;
            this.DataContext = this;

            // Initialize fields with restaurant data
            NameTb.Text = restaurant.Name;
            AddressTb.Text = restaurant.Address;
            PriceTb.Text = restaurant.Price.ToString();
            DescriptionTb.Text = restaurant.Description;
            CapacityTb.Text = restaurant.Сapacity.ToString();

            if (restaurant.RestaurantTypeId != null)
            {
                TypeTb.SelectedItem = restaurantTypes.FirstOrDefault(t => t.Id == restaurant.RestaurantTypeId);
            }

            InitializeMenuDocumentDisplay();
        }

        private void ApplyErrorStyle(Control control)
        {
            control.BorderBrush = Brushes.Red;
            control.BorderThickness = new Thickness(2);
            control.ToolTip = "Это поле обязательно для заполнения";
        }

        private void ClearErrorStyle(Control control)
        {
            control.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFABADB3"));
            control.BorderThickness = new Thickness(1);
            control.ToolTip = null;
        }

        private void ResetAllErrorStyles()
        {
            ClearErrorStyle(NameTb);
            ClearErrorStyle(AddressTb);
            ClearErrorStyle(PriceTb);
            ClearErrorStyle(TypeTb);
            ClearErrorStyle(DescriptionTb);
            ClearErrorStyle(CapacityTb);
        }

        private void NameTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearErrorStyle(NameTb);
        }
        private void CapacityTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearErrorStyle(CapacityTb);
        }

        private void AddressTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearErrorStyle(AddressTb);
        }

        private void PriceTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearErrorStyle(PriceTb);
        }

        private void TypeTb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearErrorStyle(TypeTb);
        }

        private void DescriptionTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearErrorStyle(DescriptionTb);
        }

        private void SelectPhotos_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Изображения (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
                Title = "Выберите фотографии"
            };

            if (openFileDialog.ShowDialog().GetValueOrDefault())
            {
                try
                {
                    var existingPhotos = DbConnection.MarryMe.RestaurantPhoto
                        .Where(p => p.RestaurantId == restaurant1.Id)
                        .AsEnumerable()
                        .ToList();

                    foreach (string filePath in openFileDialog.FileNames)
                    {
                        byte[] imageData = File.ReadAllBytes(filePath);

                        bool alreadyExists = existingPhotos.Any(p => p.Photo.SequenceEqual(imageData)) ||
                                            photos.Any(p => p.Photo != null && p.Photo.SequenceEqual(imageData));

                        if (!alreadyExists)
                        {
                            photos.Add(new RestaurantPhoto
                            {
                                Photo = imageData,
                                RestaurantId = restaurant1.Id
                            });
                        }
                        else
                        {
                            MessageBox.Show($"Фото '{Path.GetFileName(filePath)}' уже существует");
                        }
                    }

                    PhotosLv.ItemsSource = null;
                    PhotosLv.ItemsSource = photos;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при выборе файлов: {ex.Message}");
                }
            }
        }
        private async void UploadPhotos_Click(object sender, RoutedEventArgs e)
        {
            var newPhotos = photos.Where(p => p.Id == 0).ToList();
            if (newPhotos.Count == 0)
            {
                MessageBox.Show("Нет новых фото для сохранения");
                return;
            }

            try
            {
                UploadProgress.Maximum = newPhotos.Count;
                UploadProgress.Value = 0;
                UploadProgress.Visibility = Visibility.Visible;

                foreach (var photo in newPhotos)
                {
                    try
                    {
                        photo.RestaurantId = restaurant1.Id;
                        DbConnection.MarryMe.RestaurantPhoto.Add(photo);
                        await DbConnection.MarryMe.SaveChangesAsync();

                        PhotosLv.ItemsSource = null;
                        PhotosLv.ItemsSource = photos;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка сохранения фото: {ex.Message}");
                        continue;
                    }
                    finally
                    {
                        UploadProgress.Value++;
                    }
                }

                MessageBox.Show($"Сохранено {newPhotos.Count} фото");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Общая ошибка: {ex.Message}");
            }
            finally
            {
                UploadProgress.Visibility = Visibility.Collapsed;
            }
        }

        private async void DeletePhoto_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is RestaurantPhoto photo)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить это фото?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (photo.Id > 0)
                        {
                            var dbPhoto = DbConnection.MarryMe.RestaurantPhoto
                                .FirstOrDefault(p => p.Id == photo.Id);

                            if (dbPhoto != null)
                            {
                                DbConnection.MarryMe.RestaurantPhoto.Remove(dbPhoto);
                                await DbConnection.MarryMe.SaveChangesAsync();
                            }
                        }

                        photos.Remove(photo);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении фото: {ex.Message}");
                    }
                }
            }
        }

        private void ExitBt_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RestaurantPage());
        }

        private void SaveBt_Click(object sender, RoutedEventArgs e)
        {
            ResetAllErrorStyles();
            bool hasErrors = false;

            if (string.IsNullOrWhiteSpace(NameTb.Text))
            {
                ApplyErrorStyle(NameTb);
                hasErrors = true;
            }

            if (string.IsNullOrWhiteSpace(AddressTb.Text))
            {
                ApplyErrorStyle(AddressTb);
                hasErrors = true;
            }
            if (string.IsNullOrWhiteSpace(CapacityTb.Text))
            {
                ApplyErrorStyle(CapacityTb);
                hasErrors = true;
            }

            if (string.IsNullOrWhiteSpace(PriceTb.Text) || !int.TryParse(PriceTb.Text, out _))
            {
                ApplyErrorStyle(PriceTb);
                hasErrors = true;
            }

            if (TypeTb.SelectedItem == null)
            {
                ApplyErrorStyle(TypeTb);
                hasErrors = true;
            }

            if (string.IsNullOrWhiteSpace(DescriptionTb.Text))
            {
                ApplyErrorStyle(DescriptionTb);
                hasErrors = true;
            }

            if (hasErrors)
            {
                MessageBox.Show("Заполните все обязательные поля корректно!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                restaurant1.Name = NameTb.Text;
                restaurant1.RestaurantTypeId = (TypeTb.SelectedItem as RestaurantType)?.Id ?? 0;
                restaurant1.Price = Convert.ToInt32(PriceTb.Text);
                restaurant1.Address = AddressTb.Text;
                restaurant1.Description = DescriptionTb.Text;
                restaurant1.Сapacity = Convert.ToInt32(CapacityTb.Text);

                DbConnection.MarryMe.SaveChanges();

                MessageBox.Show("Изменения сохранены!",
                               "Успех",
                               MessageBoxButton.OK,
                               MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                               "Ошибка",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
            }
        }

        private void AddMenuBt_Click(object sender, RoutedEventArgs e)
        {
            ResetAllErrorStyles();
            bool hasErrors = false;

            if (string.IsNullOrWhiteSpace(NameTb.Text))
            {
                ApplyErrorStyle(NameTb);
                hasErrors = true;
            }

            if (string.IsNullOrWhiteSpace(AddressTb.Text))
            {
                ApplyErrorStyle(AddressTb);
                hasErrors = true;
            }
            if (string.IsNullOrWhiteSpace(CapacityTb.Text))
            {
                ApplyErrorStyle(CapacityTb);
                hasErrors = true;
            }

            if (string.IsNullOrWhiteSpace(PriceTb.Text) || !int.TryParse(PriceTb.Text, out _))
            {
                ApplyErrorStyle(PriceTb);
                hasErrors = true;
            }

            if (TypeTb.SelectedItem == null)
            {
                ApplyErrorStyle(TypeTb);
                hasErrors = true;
            }

            if (string.IsNullOrWhiteSpace(DescriptionTb.Text))
            {
                ApplyErrorStyle(DescriptionTb);
                hasErrors = true;
            }

            if (hasErrors)
            {
                MessageBox.Show("Заполните обязательные поля перед созданием меню",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
                return;
            }

            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string baseFolderPath = Path.Combine(desktopPath, "Информация о коллекциях MarryMe");
                string menusFolderPath = Path.Combine(baseFolderPath, "Меню ресторанов");

                Directory.CreateDirectory(baseFolderPath);
                Directory.CreateDirectory(menusFolderPath);

                var wordApp = new Microsoft.Office.Interop.Word.Application();
                var document = wordApp.Documents.Add();
                wordApp.Visible = true;

                var range = document.Content;
                range.Text = $"Меню ресторана {restaurant1.Name}\n\n";
                range.Font.Name = "Times New Roman";
                range.Font.Size = 14;

                string fileName = $"Меню {restaurant1.Name} (ID {restaurant1.Id}).docx";
                string savePath = Path.Combine(menusFolderPath, fileName);
                document.SaveAs2(savePath);

                restaurant1.DocumentMenu = savePath;
                DbConnection.MarryMe.SaveChanges();

                MenuDocumentBorder.Visibility = Visibility.Visible;
                MenuDocumentTb.Text = $"Меню";

                MessageBox.Show("Файл меню успешно создан и сохранен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании меню: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenMenuDocument_Click(object sender, RoutedEventArgs e)
        {
            if (restaurant1 != null && !string.IsNullOrEmpty(restaurant1.DocumentMenu) && File.Exists(restaurant1.DocumentMenu))
            {
                try
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = restaurant1.DocumentMenu,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось открыть документ: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Документ меню не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void InitializeMenuDocumentDisplay()
        {
            if (restaurant1 != null && !string.IsNullOrEmpty(restaurant1.DocumentMenu))
            {
                MenuDocumentBorder.Visibility = Visibility.Visible;
                MenuDocumentTb.Text = $"Меню";
            }
            else
            {
                MenuDocumentBorder.Visibility = Visibility.Collapsed;
            }
        }
    }
}


