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

namespace ZhiganshinaMilana420_MarryMe.Pages.MusicianFolder
{
    /// <summary>
    /// Логика взаимодействия для EditMusicianPage.xaml
    /// </summary>
    public partial class EditMusicianPage : Page
    {
        private ObservableCollection<MusicianPhoto> photos = new ObservableCollection<MusicianPhoto>();
        public static List<Musician> musicians { get; set; }
        Musician contextMusician;
        public static Musician musician1 = new Musician();
        public static Musician mus { get; set; }
        public EditMusicianPage(Musician musician)
        {
            InitializeComponent();
            UploadProgress.Visibility = Visibility.Collapsed;

            // Загружаем фото только для текущего ресторана
            photos = new ObservableCollection<MusicianPhoto>(
                DbConnection.MarryMe.MusicianPhoto
                    .Where(p => p.MusicianId == musician.Id)
                    .ToList()
            );

            PhotosLv.ItemsSource = photos;

            contextMusician = musician;
            mus = musician;
            musician1 = musician;
            this.DataContext = this;

            if (musician1 != null)
            {
                NameTb.Text = contextMusician.TeamName;
                PriceTb.Text = contextMusician.Price.ToString();
            }
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
                    var existingPhotos = DbConnection.MarryMe.MusicianPhoto
                        .Where(p => p.MusicianId == musician1.Id)
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
                            photos.Add(new MusicianPhoto
                            {
                                Photo = imageData,
                                MusicianId = musician1.Id
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
                        photo.MusicianId = musician1.Id;

                        // Добавляем в контекст
                        DbConnection.MarryMe.MusicianPhoto.Add(photo);
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
            if (sender is Button button && button.DataContext is MusicianPhoto photo)
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
                            var dbPhoto = DbConnection.MarryMe.MusicianPhoto
                                .FirstOrDefault(p => p.Id == photo.Id);

                            if (dbPhoto != null)
                            {
                                DbConnection.MarryMe.MusicianPhoto.Remove(dbPhoto);
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
            NavigationService.Navigate(new MusicianPage());
        }

        private void EditBt_Click(object sender, RoutedEventArgs e)
        {
            Musician musician = musician1;
            if (string.IsNullOrWhiteSpace(NameTb.Text) ||
               string.IsNullOrWhiteSpace(PriceTb.Text))
            {
                MessageBox.Show("Заполните все данные!!!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                musician.TeamName = NameTb.Text;
                musician.Price = Convert.ToDecimal(PriceTb.Text);
                DbConnection.MarryMe.SaveChanges();
                MessageBox.Show("Данные изменены!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.Navigate(new MusicianPage());
            }
        }
    }
}
