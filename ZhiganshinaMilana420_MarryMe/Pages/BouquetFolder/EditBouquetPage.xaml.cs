using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using ZhiganshinaMilana420_MarryMe.Pages.CakeFolder;
using ZhiganshinaMilana420_MarryMe.Pages.DressFolder;
using Path = System.IO.Path;

namespace ZhiganshinaMilana420_MarryMe.Pages.BouquetFolder
{
    /// <summary>
    /// Логика взаимодействия для EditBouquetPage.xaml
    /// </summary>
    public partial class EditBouquetPage : Page
    {
        private ObservableCollection<BouquetPhoto> photos = new ObservableCollection<BouquetPhoto>();
        public static List<Bouquet> bouquets { get; set; }
        Bouquet contextBouquet;
        public static Bouquet bouquet1 = new Bouquet();
        public static Bouquet bou { get; set; }
        public static List<BouquetType> bouquetTypes { get; set; }
        public EditBouquetPage(Bouquet bouquet)
        {
            InitializeComponent();
            UploadProgress.Visibility = Visibility.Collapsed;

            bouquetTypes = new List<BouquetType>(DbConnection.MarryMe.BouquetType.ToList());
            contextBouquet = bouquet;
            bou = bouquet;
            bouquet1 = bouquet;

            // Загружаем фото только для текущего ресторана
            photos = new ObservableCollection<BouquetPhoto>(
            DbConnection.MarryMe.BouquetPhoto
                    .Where(p => p.BouquetId == bouquet.Id)
                    .ToList()
            );

            PhotosLv.ItemsSource = photos;
            this.DataContext = this;

            NameTb.Text = bouquet.Name;
            PriceTb.Text = bouquet.Price.ToString();
            DescriptionTb.Text = bouquet.Description;
            if (bouquet.BouquetTypeId != null)
            {
                TypeTb.SelectedItem = bouquetTypes.FirstOrDefault(t => t.Id == bouquet.BouquetTypeId);
            }
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
            ClearErrorStyle(PriceTb);
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
                    var existingPhotos = DbConnection.MarryMe.BouquetPhoto
                        .Where(p => p.BouquetId == bouquet1.Id)
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
                            photos.Add(new BouquetPhoto
                            {
                                Photo = imageData,
                                BouquetId = bouquet1.Id
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
            // Получаем только новые фото (те, у которых Id == 0)
            var newPhotos = photos.Where(p => p.Id == 0).ToList();

            if (newPhotos.Count == 0)
            {
                MessageBox.Show("Нет новых фотографий для сохранения");
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
                        // Устанавливаем RestaurantId для новых фото
                        photo.BouquetId = bouquet1.Id;

                        // Добавляем в контекст
                        DbConnection.MarryMe.BouquetPhoto.Add(photo);
                        await DbConnection.MarryMe.SaveChangesAsync();

                        UploadProgress.Value++;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении фото: {ex.Message}");
                    }
                }

                MessageBox.Show($"Успешно сохранено {newPhotos.Count} новых фотографий");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении в базу данных: {ex.Message}");
            }
            finally
            {
                UploadProgress.Visibility = Visibility.Collapsed;
            }
        }

        private async void DeletePhoto_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is BouquetPhoto photo)
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
                            var dbPhoto = DbConnection.MarryMe.BouquetPhoto
                                .FirstOrDefault(p => p.Id == photo.Id);

                            if (dbPhoto != null)
                            {
                                DbConnection.MarryMe.BouquetPhoto.Remove(dbPhoto);
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
            NavigationService.Navigate(new BouquetPage());
        }

        private void EditBt_Click(object sender, RoutedEventArgs e)
        {
            ResetAllErrorStyles();
            bool hasErrors = false;

            if (string.IsNullOrWhiteSpace(NameTb.Text))
            {
                ApplyErrorStyle(NameTb);
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
                bouquet1.Name = NameTb.Text;
                bouquet1.Price = Convert.ToInt32(PriceTb.Text);
                bouquet1.BouquetTypeId = (TypeTb.SelectedItem as BouquetType)?.Id ?? 0;
                bouquet1.Description = DescriptionTb.Text;

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
    }
}
