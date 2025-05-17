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

namespace ZhiganshinaMilana420_MarryMe.Pages.HostFolder
{
    /// <summary>
    /// Логика взаимодействия для EditHostPage.xaml
    /// </summary>
    public partial class EditHostPage : Page
    {
        private ObservableCollection<HostPhoto> photos = new ObservableCollection<HostPhoto>();
        public static List<Host> hosts {  get; set; }
        Host contextHost;
        public static Host host1 = new Host();
        public static Host hos { get; set; }
        public EditHostPage(Host host)
        {
            InitializeComponent();
            UploadProgress.Visibility = Visibility.Collapsed;

            contextHost = host;
            hos = host;
            host1 = host;
            photos = new ObservableCollection<HostPhoto>(
               DbConnection.MarryMe.HostPhoto
                   .Where(p => p.HostId == host.Id)
                   .ToList());

            PhotosLv.ItemsSource = photos;
            this.DataContext = this;

            SurnameTb.Text = host.Surname;
            NameTb.Text = host.Name;
            NameTb.Text = host.Name;
            PriceTb.Text = host.Price.ToString();
            DescriptionTb.Text = host.Description;
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
            ClearErrorStyle(SurnameTb);
            ClearErrorStyle(NameTb);
            ClearErrorStyle(PatronymicTb);
            ClearErrorStyle(PriceTb);
            ClearErrorStyle(DescriptionTb);
        }

        private void SurnameTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearErrorStyle(SurnameTb);
        }
        private void NameTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearErrorStyle(NameTb);
        }
        private void PatronymicTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearErrorStyle(PatronymicTb);
        }
        private void PriceTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearErrorStyle(PriceTb);
        }

        private void DescriptionTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearErrorStyle(DescriptionTb);
        }

        private void changeBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "*.png|*.png|*.jpeg|*.jpeg|*.jpg|*.jpg"
            };
            if (openFileDialog.ShowDialog().GetValueOrDefault())
            {
                host1.Photo = File.ReadAllBytes(openFileDialog.FileName);
                TestImg.Source = new BitmapImage(new Uri(openFileDialog.FileName));
            };
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
                    var existingPhotos = DbConnection.MarryMe.HostPhoto
                        .Where(p => p.HostId == host1.Id)
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
                            photos.Add(new HostPhoto
                            {
                                Photo = imageData,
                                HostId = host1.Id
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
                        photo.HostId = host1.Id;

                        // Добавляем в контекст
                        DbConnection.MarryMe.HostPhoto.Add(photo);
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
            if (sender is Button button && button.DataContext is HostPhoto photo)
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
                            var dbPhoto = DbConnection.MarryMe.HostPhoto
                                .FirstOrDefault(p => p.Id == photo.Id);

                            if (dbPhoto != null)
                            {
                                DbConnection.MarryMe.HostPhoto.Remove(dbPhoto);
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
            NavigationService.Navigate(new HostPage());
        }

        private void EditBt_Click(object sender, RoutedEventArgs e)
        {
            ResetAllErrorStyles();
            bool hasErrors = false;

            if (string.IsNullOrWhiteSpace(SurnameTb.Text))
            {
                ApplyErrorStyle(SurnameTb);
                hasErrors = true;
            }
            if (string.IsNullOrWhiteSpace(NameTb.Text))
            {
                ApplyErrorStyle(NameTb);
                hasErrors = true;
            }
            if (string.IsNullOrWhiteSpace(PatronymicTb.Text))
            {
                ApplyErrorStyle(PatronymicTb);
                hasErrors = true;
            }
            if (string.IsNullOrWhiteSpace(PriceTb.Text) || !int.TryParse(PriceTb.Text, out _))
            {
                ApplyErrorStyle(PriceTb);
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
                host1.Surname = SurnameTb.Text;
                host1.Name = NameTb.Text;
                host1.Patronymic = PatronymicTb.Text;
                host1.Price = Convert.ToInt32(PriceTb.Text);
                host1.Description = DescriptionTb.Text;
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
