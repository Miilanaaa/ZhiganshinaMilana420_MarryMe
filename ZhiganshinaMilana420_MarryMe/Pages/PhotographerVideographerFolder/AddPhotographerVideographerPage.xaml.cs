using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using ZhiganshinaMilana420_MarryMe.Pages.RestaurantForder;
using Path = System.IO.Path;

namespace ZhiganshinaMilana420_MarryMe.Pages.PhotographerVideographerFolder
{
    /// <summary>
    /// Логика взаимодействия для AddPhotographerVideographerPage.xaml
    /// </summary>
    public partial class AddPhotographerVideographerPage : Page
    {
        private ObservableCollection<PhotographerVideographerPhoto> photos = new ObservableCollection<PhotographerVideographerPhoto>();
        public static List<PhotographerVideographer> photographerVideographers { get; set; }

        public static PhotographerVideographer photographerVideographer1 = new PhotographerVideographer();
        public static PhotographerVideographer pho { get; set; }
        public static List<PhotographerType> photographerTypes { get; set; }
        public AddPhotographerVideographerPage()
        {
            InitializeComponent();
            UploadProgress.Visibility = Visibility.Collapsed;

            photographerTypes = new List<PhotographerType>(DbConnection.MarryMe.PhotographerType.ToList());
            PhotosLv.ItemsSource = photos;
            this.DataContext = this;
        }
        private void ApplyErrorStyle(Control control)
        {
            control.BorderBrush = Brushes.Red;
            control.BorderThickness = new Thickness(2);
            control.ToolTip = "Это поле обязательно для заполнения";
        }

        private void ResetAllErrorStyles()
        {
            ClearErrorStyle(NameTb);
            ClearErrorStyle(PriceTb);
            ClearErrorStyle(TypeTb);
            ClearErrorStyle(DescriptionTb);
        }
        private void NameTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearErrorStyle(NameTb);
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
                    var existingPhotos = DbConnection.MarryMe.PhotographerVideographerPhoto
                        .Where(p => p.PhotographerVideographerId == photographerVideographer1.Id)
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
                            photos.Add(new PhotographerVideographerPhoto
                            {
                                Photo = imageData,
                                PhotographerVideographerId = photographerVideographer1.Id
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
            if (photographerVideographer1 == null || photographerVideographer1.Id == 0)
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
                        photo.PhotographerVideographerId = photographerVideographer1.Id;
                        DbConnection.MarryMe.PhotographerVideographerPhoto.Add(photo);
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
            if (sender is Button button && button.DataContext is PhotographerVideographerPhoto photo)
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
                            var dbPhoto = DbConnection.MarryMe.PhotographerVideographerPhoto
                                .FirstOrDefault(p => p.Id == photo.Id);

                            if (dbPhoto != null)
                            {
                                DbConnection.MarryMe.PhotographerVideographerPhoto.Remove(dbPhoto);
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
            NavigationService.Navigate(new PhotographerVideographerPage());
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
                PhotographerVideographer photographerVideographer = new PhotographerVideographer()
                {
                    TeamName = NameTb.Text,
                    PhotographerTypeId = (TypeTb.SelectedItem as PhotographerType)?.Id ?? 0,
                    Price = Convert.ToInt32(PriceTb.Text),
                    Description = DescriptionTb.Text
                };

                DbConnection.MarryMe.PhotographerVideographer.Add(photographerVideographer);
                DbConnection.MarryMe.SaveChanges();

                // КРИТИЧЕСКОЕ ИЗМЕНЕНИЕ: Обновляем ID ресторана
                photographerVideographer1 = photographerVideographer; // Или restaurant1.Id = restaurant.Id;

                MessageBox.Show("Команда добавлена! Теперь вы можете добавить фото",
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
    }
}
