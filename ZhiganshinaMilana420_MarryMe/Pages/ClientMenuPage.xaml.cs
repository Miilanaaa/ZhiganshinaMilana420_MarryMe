using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ZhiganshinaMilana420_MarryMe.DB;

namespace ZhiganshinaMilana420_MarryMe.Pages
{
    public partial class ClientMenuPage : Page
    {
        public static List<Couple> couples { get; set; }
        public static List<Gromm> gromms { get; set; }
        public static List<Bride> brides { get; set; }
        public static List<WeddingStatus> statuses { get; set; }

        public ClientMenuPage()
        {
            InitializeComponent();
            LoadData();
            this.DataContext = this;

            // Установка фильтра по умолчанию (Id = 1)
            FilterCb.SelectedItem = statuses.FirstOrDefault(s => s.Id == 1);

            // Подписка на события для обновления данных при изменении фильтров
            SearchTb.TextChanged += UpdateData;
            FilterCb.SelectionChanged += UpdateData;
            DateTaskDp.SelectedDateChanged += UpdateData;
        }

        private void LoadData()
        {
            couples = DbConnection.MarryMe.Couple.ToList();
            statuses = DbConnection.MarryMe.WeddingStatus.ToList();
            gromms = DbConnection.MarryMe.Gromm.ToList();
            brides = DbConnection.MarryMe.Bride.ToList();

            UpdateData(null, null);
        }

        private void UpdateData(object sender, EventArgs e)
        {
            var filteredCouples = couples.AsQueryable();

            // Фильтрация по поиску
            if (!string.IsNullOrWhiteSpace(SearchTb.Text))
            {
                var searchText = SearchTb.Text.ToLower();
                filteredCouples = filteredCouples.Where(c =>
                    c.Gromm.Surname.ToLower().Contains(searchText) ||
                    c.Gromm.Name.ToLower().Contains(searchText) ||
                    c.Gromm.Patronymic.ToLower().Contains(searchText) ||
                    c.Bride.Surname.ToLower().Contains(searchText) ||
                    c.Bride.Name.ToLower().Contains(searchText) ||
                    c.Bride.Patronymic.ToLower().Contains(searchText));
            }

            // Фильтрация по статусу
            if (FilterCb.SelectedItem is WeddingStatus selectedStatus)
            {
                filteredCouples = filteredCouples.Where(c => c.WeddingStatusId == selectedStatus.Id);
            }

            // Фильтрация по дате
            if (DateTaskDp.SelectedDate.HasValue)
            {
                var selectedDate = DateTaskDp.SelectedDate.Value;
                filteredCouples = filteredCouples.Where(c => c.WeddingDate == selectedDate);
            }

            // Сортировка по дате (ближайшие сначала)
            var sortedCouples = filteredCouples.OrderBy(c => c.WeddingDate).ToList();

            CoupleLV.ItemsSource = sortedCouples;
        }

        private void CoupleLV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CoupleLV.SelectedItem is Couple couple)
            {
                NavigationService.Navigate(new EditClientPage(couple));
            }
        }

        private void FinishWeddingBtt_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var couple = button.DataContext as Couple;

            var result = MessageBox.Show($"Вы уверены, что хотите завершить свадьбу пары {couple.Gromm.Surname} {couple.Gromm.Name} и {couple.Bride.Surname} {couple.Bride.Name}?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    couple.WeddingStatusId = 2; // 2 - Завершена

                    var gromm = DbConnection.MarryMe.Gromm.FirstOrDefault(g => g.Id == couple.GroomId);
                    if (gromm != null)
                    {
                        gromm.PassportNumber = null;
                        gromm.PassportSeries = null;
                        gromm.PassportAddress = null;
                        gromm.Addresss = null;
                    }

                    var bride = DbConnection.MarryMe.Bride.FirstOrDefault(b => b.Id == couple.BrideId);
                    if (bride != null)
                    {
                        bride.PassportNumber = null;
                        bride.PassportSeries = null;
                        bride.PassportAddress = null;
                        bride.Addresss = null;
                    }

                    DbConnection.MarryMe.SaveChanges();
                    LoadData();
                    CoupleLV.Items.Refresh();
                    MessageBox.Show("Свадьба успешно завершена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelWeddingBtt_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var couple = button.DataContext as Couple;

            var result = MessageBox.Show($"Вы уверены, что хотите отменить свадьбу пары {couple.Gromm.Surname} {couple.Gromm.Name} и {couple.Bride.Surname} {couple.Bride.Name}?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    couple.WeddingStatusId = 3; // 3 - Отменена

                    var gromm = DbConnection.MarryMe.Gromm.FirstOrDefault(g => g.Id == couple.GroomId);
                    if (gromm != null)
                    {
                        gromm.PassportNumber = null;
                        gromm.PassportSeries = null;
                        gromm.PassportAddress = null;
                        gromm.Addresss = null;
                    }

                    var bride = DbConnection.MarryMe.Bride.FirstOrDefault(b => b.Id == couple.BrideId);
                    if (bride != null)
                    {
                        bride.PassportNumber = null;
                        bride.PassportSeries = null;
                        bride.PassportAddress = null;
                        bride.Addresss = null;
                    }

                    DbConnection.MarryMe.SaveChanges();
                    LoadData();
                    MessageBox.Show("Свадьба успешно отменена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}