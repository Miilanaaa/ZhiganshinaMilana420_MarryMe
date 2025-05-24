using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
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
using Path = System.IO.Path;

namespace ZhiganshinaMilana420_MarryMe.Pages.RestaurantForder
{
    /// <summary>
    /// Логика взаимодействия для RestaurantAddPage.xaml
    /// </summary>
    public partial class RestaurantAddPage : Page
    {
        private ObservableCollection<RestaurantPhoto> photos = new ObservableCollection<RestaurantPhoto>();
        public static List<Restaurant> restaurants { get; set; }
        
        public static Restaurant restaurant1 = new Restaurant();
        public static Restaurant res { get; set; }
        public static List<RestaurantType> restaurantTypes { get; set; }
        public RestaurantAddPage()
        {
            InitializeComponent();
            UploadProgress.Visibility = Visibility.Collapsed;

            restaurantTypes = new List<RestaurantType>(DbConnection.MarryMe.RestaurantType.ToList());
            PhotosLv.ItemsSource = photos;
            this.DataContext = this;

            InitializeMenuDocumentDisplay(); // Добавьте эту строку
        }

        // Методы для работы со стилями ошибок
        private void ApplyErrorStyle(Control control)
        {
            control.BorderBrush = Brushes.Red;
            control.BorderThickness = new Thickness(2);
            control.ToolTip = "Это поле обязательно для заполнения";
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

        private void ClearErrorStyle(Control control)
        {
            control.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFABADB3"));
            control.BorderThickness = new Thickness(1);
            control.ToolTip = null;
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
                    // Сначала загружаем все фото ресторана из БД в память
                    var existingPhotos = DbConnection.MarryMe.RestaurantPhoto
                        .Where(p => p.RestaurantId == restaurant1.Id)
                        .AsEnumerable() // Переключаемся на LINQ to Objects
                        .ToList();

                    foreach (string filePath in openFileDialog.FileNames)
                    {
                        byte[] imageData = File.ReadAllBytes(filePath);

                        // Проверяем дубликаты в памяти
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
            if (restaurant1 == null || restaurant1.Id == 0)
            {
                MessageBox.Show("Сначала сохраните ресторан!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

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

                        // Обновляем ItemsSource для отображения изменений
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
                        // Если фото уже сохранено в БД - удаляем и оттуда
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

                        // Удаляем из списка
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

        private void AddBt_Click(object sender, RoutedEventArgs e)
        {
            // Сбрасываем все подсветки ошибок
            ResetAllErrorStyles();

            bool hasErrors = false;

            // Проверка названия ресторана
            if (string.IsNullOrWhiteSpace(NameTb.Text))
            {
                ApplyErrorStyle(NameTb);
                hasErrors = true;
            }

            // Проверка адреса
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

            // Проверка цены
            if (string.IsNullOrWhiteSpace(PriceTb.Text) || !int.TryParse(PriceTb.Text, out _))
            {
                ApplyErrorStyle(PriceTb);
                hasErrors = true;
            }

            // Проверка типа ресторана
            if (TypeTb.SelectedItem == null)
            {
                ApplyErrorStyle(TypeTb);
                hasErrors = true;
            }

            // Проверка описания (если оно обязательно)
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
                Restaurant restaurant = new Restaurant()
                {
                    Name = NameTb.Text,
                    RestaurantTypeId = (TypeTb.SelectedItem as RestaurantType)?.Id ?? 0,
                    Price = Convert.ToInt32(PriceTb.Text),
                    Address = AddressTb.Text,
                    Description = DescriptionTb.Text,
                    Сapacity = Convert.ToInt32(CapacityTb.Text)
                };

                DbConnection.MarryMe.Restaurant.Add(restaurant);
                DbConnection.MarryMe.SaveChanges();

                restaurant1 = restaurant;

                MessageBox.Show("Ресторан добавлен! Теперь вы можете добавить фото",
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
            // Проверяем, заполнены ли обязательные поля
            ResetAllErrorStyles();
            bool hasErrors = false;

            if (string.IsNullOrWhiteSpace(NameTb.Text))
            {
                ApplyErrorStyle(NameTb);
                hasErrors = true;
            }

            // Проверка адреса
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

            // Проверка цены
            if (string.IsNullOrWhiteSpace(PriceTb.Text) || !int.TryParse(PriceTb.Text, out _))
            {
                ApplyErrorStyle(PriceTb);
                hasErrors = true;
            }

            // Проверка типа ресторана
            if (TypeTb.SelectedItem == null)
            {
                ApplyErrorStyle(TypeTb);
                hasErrors = true;
            }

            // Проверка описания (если оно обязательно)
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
                // Если ресторан еще не сохранен в БД
                if (restaurant1 == null || restaurant1.Id == 0)
                {
                    // Сначала сохраняем ресторан
                    AddBt_Click(null, null);

                    // Проверяем, что сохранение прошло успешно
                    if (restaurant1 == null || restaurant1.Id == 0)
                    {
                        MessageBox.Show("Не удалось сохранить ресторан", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                // Создаем папки
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string baseFolderPath = Path.Combine(desktopPath, "Информация о коллекциях MarryMe");
                string menusFolderPath = Path.Combine(baseFolderPath, "Меню ресторанов");

                Directory.CreateDirectory(baseFolderPath);
                Directory.CreateDirectory(menusFolderPath);

                // Создаем документ
                var wordApp = new Microsoft.Office.Interop.Word.Application();
                var document = wordApp.Documents.Add();
                wordApp.Visible = true;

                // Заполняем документ
                var range = document.Content;
                range.Text = $"Меню ресторана {restaurant1.Name}\n\n";
                range.Font.Name = "Times New Roman";
                range.Font.Size = 14;

                // Сохраняем документ
                string fileName = $"Меню {restaurant1.Name} (ID {restaurant1.Id}).docx";
                string savePath = Path.Combine(menusFolderPath, fileName);
                document.SaveAs2(savePath);

                // Обновляем информацию в базе данных
                restaurant1.DocumentMenu = savePath;
                DbConnection.MarryMe.SaveChanges();

                // Показываем информацию о документе
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
