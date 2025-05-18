using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
using Application = Microsoft.Office.Interop.Word.Application;
using Page = System.Windows.Controls.Page;
using Paragraph = Microsoft.Office.Interop.Word.Paragraph;
using Path = System.IO.Path;
using Table = Microsoft.Office.Interop.Word.Table;
using WdOrientation = Microsoft.Office.Interop.Word.WdOrientation;
using WdParagraphAlignment = Microsoft.Office.Interop.Word.WdParagraphAlignment;

namespace ZhiganshinaMilana420_MarryMe.Pages
{
    /// <summary>
    /// Логика взаимодействия для ReportsPage.xaml
    /// </summary>
    public partial class ReportsPage : Page
    {
        public RestaurantBookingDates _context = new RestaurantBookingDates();

        public ReportsPage()
        {
            InitializeComponent();
            YearTextBox.Text = CurrentYear.ToString();
            ActivityYearTextBox.Text = ActivityYear.ToString();
            LoadClientActivityData();

            // Загружаем данные для активной вкладки
            if (AktivCb.SelectedItem != null)
                AktivCb_SelectionChanged(null, null);
        }

        private void FilterBookings(object sender, RoutedEventArgs e)
        {
            if (SortCb.SelectedItem == null || DateBooking.SelectedDate == null)
            {
                BookingsListView.ItemsSource = null;
                return;
            }

            var selectedType = ((ComboBoxItem)SortCb.SelectedItem).Content.ToString();
            var selectedDate = DateBooking.SelectedDate.Value;

            // Загружаем все необходимые данные в память перед обработкой
            var couples = DbConnection.MarryMe.Couple.ToList();
            var brides = DbConnection.MarryMe.Bride.ToList();
            var grooms = DbConnection.MarryMe.Gromm.ToList();

            switch (selectedType)
            {
                case "Рестораны":
                    LoadRestaurantBookings(selectedDate, couples, brides, grooms);
                    break;
                case "Трансфер":
                    LoadTransferBookings(selectedDate, couples, brides, grooms);
                    break;
                case "Ведущие":
                    LoadHostBookings(selectedDate, couples, brides, grooms);
                    break;
                case "Стилисты":
                    LoadStylistBookings(selectedDate, couples, brides, grooms);
                    break;
                case "Музыканты":
                    LoadMusicianBookings(selectedDate, couples, brides, grooms);
                    break;
                case "Фото и видео":
                    LoadPhotographerBookings(selectedDate, couples, brides, grooms);
                    break;
            }
        }

        private void LoadRestaurantBookings(DateTime selectedDate, List<Couple> couples, List<Bride> brides, List<Gromm> grooms)
        {
            var bookings = DbConnection.MarryMe.RestaurantBookingDates
                .Where(b => b.Status == true && DbFunctions.TruncateTime(b.BookingDate) == selectedDate.Date)
                .ToList() // Материализуем запрос
                .Join(DbConnection.MarryMe.Restaurant.ToList(),
                    b => b.RestaurantId,
                    r => r.Id,
                    (b, r) => new { Booking = b, Restaurant = r })
                .Join(couples,
                    br => br.Booking.CoupleId,
                    c => c.Id,
                    (br, c) => new
                    {
                        Id = br.Restaurant.Id,
                        Name = br.Restaurant.Name,
                        Description = br.Restaurant.Description,
                        br.Booking.BookingDate,
                        Couple = c
                    })
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Description,
                    x.BookingDate,
                    CoupleName = GetCoupleName(x.Couple, brides, grooms)
                })
                .ToList();

            BookingsListView.ItemsSource = bookings;
        }

        private void LoadTransferBookings(DateTime selectedDate, List<Couple> couples, List<Bride> brides, List<Gromm> grooms)
        {
            var bookings = DbConnection.MarryMe.TransferBookingDates
                .Where(b => b.Status == true && DbFunctions.TruncateTime(b.BookingDate) == selectedDate.Date)
                .ToList()
                .Join(DbConnection.MarryMe.Transfer.ToList(),
                    b => b.TransferId,
                    t => t.Id,
                    (b, t) => new { Booking = b, Transfer = t })
                .Join(couples,
                    bt => bt.Booking.CoupleId,
                    c => c.Id,
                    (bt, c) => new
                    {
                        Id = bt.Transfer.Id,
                        Name = bt.Transfer.Name + " (" + bt.Transfer.NumberСars + " авто)",
                        Description = bt.Transfer.Description + " | Цена: " + bt.Transfer.Price + " руб.",
                        bt.Booking.BookingDate,
                        Couple = c
                    })
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Description,
                    x.BookingDate,
                    CoupleName = GetCoupleName(x.Couple, brides, grooms)
                })
                .ToList();

            BookingsListView.ItemsSource = bookings;
        }

        private void LoadHostBookings(DateTime selectedDate, List<Couple> couples, List<Bride> brides, List<Gromm> grooms)
        {
            var bookings = DbConnection.MarryMe.HostBookingDates
                .Where(b => b.Status == true && DbFunctions.TruncateTime(b.BookingDate) == selectedDate.Date)
                .ToList()
                .Join(DbConnection.MarryMe.Host.ToList(),
                    b => b.HostId,
                    h => h.Id,
                    (b, h) => new { Booking = b, Host = h })
                .Join(couples,
                    bh => bh.Booking.CoupleId,
                    c => c.Id,
                    (bh, c) => new
                    {
                        Id = bh.Host.Id,
                        Name = bh.Host.Surname + " " + bh.Host.Name + " " + bh.Host.Patronymic,
                        Description = bh.Host.Description + " | Цена: " + bh.Host.Price + " руб.",
                        bh.Booking.BookingDate,
                        Couple = c
                    })
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Description,
                    x.BookingDate,
                    CoupleName = GetCoupleName(x.Couple, brides, grooms)
                })
                .ToList();

            BookingsListView.ItemsSource = bookings;
        }

        private void LoadStylistBookings(DateTime selectedDate, List<Couple> couples, List<Bride> brides, List<Gromm> grooms)
        {
            var bookings = DbConnection.MarryMe.StylistBookingDates
                .Where(b => b.Status == true && DbFunctions.TruncateTime(b.BookingDate) == selectedDate.Date)
                .ToList()
                .Join(DbConnection.MarryMe.Stylist.ToList(),
                    b => b.StylistId,
                    s => s.Id,
                    (b, s) => new { Booking = b, Stylist = s })
                .Join(couples,
                    bs => bs.Booking.CoupleId,
                    c => c.Id,
                    (bs, c) => new
                    {
                        Id = bs.Stylist.Id,
                        Name = bs.Stylist.TeamName,
                        Description = bs.Stylist.Description + " | Цена: " + bs.Stylist.Price + " руб.",
                        bs.Booking.BookingDate,
                        Couple = c
                    })
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Description,
                    x.BookingDate,
                    CoupleName = GetCoupleName(x.Couple, brides, grooms)
                })
                .ToList();

            BookingsListView.ItemsSource = bookings;
        }

        private void LoadMusicianBookings(DateTime selectedDate, List<Couple> couples, List<Bride> brides, List<Gromm> grooms)
        {
            var bookings = DbConnection.MarryMe.MusicianBookingDates
                .Where(b => b.Status == true && DbFunctions.TruncateTime(b.BookingDate) == selectedDate.Date)
                .ToList()
                .Join(DbConnection.MarryMe.Musician.ToList(),
                    b => b.MusicianId,
                    m => m.Id,
                    (b, m) => new { Booking = b, Musician = m })
                .Join(couples,
                    bm => bm.Booking.CoupleId,
                    c => c.Id,
                    (bm, c) => new
                    {
                        Id = bm.Musician.Id,
                        Name = bm.Musician.TeamName,
                        Description = bm.Musician.Description + " | Цена: " + bm.Musician.Price + " руб.",
                        bm.Booking.BookingDate,
                        Couple = c
                    })
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Description,
                    x.BookingDate,
                    CoupleName = GetCoupleName(x.Couple, brides, grooms)
                })
                .ToList();

            BookingsListView.ItemsSource = bookings;
        }

        private void LoadPhotographerBookings(DateTime selectedDate, List<Couple> couples, List<Bride> brides, List<Gromm> grooms)
        {
            var bookings = DbConnection.MarryMe.PhotographerVideographerBookingDates
                .Where(b => b.Status == true && DbFunctions.TruncateTime(b.BookingDate) == selectedDate.Date)
                .ToList()
                .Join(DbConnection.MarryMe.PhotographerVideographer.ToList(),
                    b => b.PhotographerVideographerId,
                    p => p.Id,
                    (b, p) => new { Booking = b, Photographer = p })
                .Join(couples,
                    bp => bp.Booking.CoupleId,
                    c => c.Id,
                    (bp, c) => new
                    {
                        Id = bp.Photographer.Id,
                        Name = bp.Photographer.TeamName,
                        Description = bp.Photographer.Description + " | Цена: " + bp.Photographer.Price + " руб.",
                        bp.Booking.BookingDate,
                        Couple = c
                    })
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Description,
                    x.BookingDate,
                    CoupleName = GetCoupleName(x.Couple, brides, grooms)
                })
                .ToList();

            BookingsListView.ItemsSource = bookings;
        }

        private string GetCoupleName(Couple couple, List<Bride> brides, List<Gromm> grooms)
        {
            if (couple == null) return "Неизвестная пара";

            var bride = brides.FirstOrDefault(b => b.Id == couple.BrideId);
            var groom = grooms.FirstOrDefault(g => g.Id == couple.GroomId);

            if (bride != null && groom != null)
            {
                return bride.Surname + " " + bride.Name + " " + bride.Patronymic + " и " +
                       groom.Surname + " " + groom.Name + " " + groom.Patronymic;
            }
            return "Неизвестная пара";
        }
        private void ExportToWord_Click(object sender, RoutedEventArgs e)
        {
            if (BookingsListView.Items.Count == 0)
            {
                System.Windows.MessageBox.Show("Нет данных для экспорта", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // Получаем выбранную категорию и дату
                string category = ((ComboBoxItem)SortCb.SelectedItem)?.Content?.ToString() ?? "Неизвестная категория";
                string selectedDate = DateBooking.SelectedDate?.ToString("dd.MM.yyyy") ?? "дата не выбрана";

                // Создаем папки если их нет
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string reportsFolder = Path.Combine(desktopPath, "Отчеты MerryMe");
                string bookingsFolder = Path.Combine(reportsFolder, "Бронирования");

                Directory.CreateDirectory(reportsFolder);
                Directory.CreateDirectory(bookingsFolder);

                // Генерируем имя файла
                string fileName = $"{category} {selectedDate}.docx";
                string filePath = Path.Combine(bookingsFolder, fileName);

                // Проверяем существование файла и добавляем номер если нужно
                int counter = 1;
                while (File.Exists(filePath))
                {
                    fileName = $"{category} {selectedDate} ({counter}).docx";
                    filePath = Path.Combine(bookingsFolder, fileName);
                    counter++;
                }

                // Создаем Word приложение
                Application wordApp = new Application();
                Document doc = wordApp.Documents.Add();

                // Добавляем заголовок
                Paragraph title = doc.Paragraphs.Add();
                title.Range.Text = $"Категория: {category}\nДата: {selectedDate}";
                title.Range.Font.Bold = 1;
                title.Range.Font.Size = 14;
                title.Range.InsertParagraphAfter();

                // Создаем таблицу
                Paragraph tableParagraph = doc.Paragraphs.Add();
                Table wordTable = doc.Tables.Add(tableParagraph.Range, BookingsListView.Items.Count + 1, 5);
                wordTable.Borders.Enable = 1;

                // Заголовки таблицы
                wordTable.Cell(1, 1).Range.Text = "Наименование";
                wordTable.Cell(1, 2).Range.Text = "Номер";
                wordTable.Cell(1, 3).Range.Text = "Описание";
                wordTable.Cell(1, 4).Range.Text = "Пара";
                wordTable.Cell(1, 5).Range.Text = "Дата бронирования";

                // Форматируем заголовки
                for (int i = 1; i <= 5; i++)
                {
                    wordTable.Cell(1, i).Range.Font.Bold = 1;
                    wordTable.Cell(1, i).Range.Font.Size = 12;
                }

                // Заполняем таблицу данными
                for (int i = 0; i < BookingsListView.Items.Count; i++)
                {
                    dynamic item = BookingsListView.Items[i];
                    wordTable.Cell(i + 2, 1).Range.Text = item.Name?.ToString() ?? "";
                    wordTable.Cell(i + 2, 2).Range.Text = item.Id?.ToString() ?? "";
                    wordTable.Cell(i + 2, 3).Range.Text = item.Description?.ToString() ?? "";
                    wordTable.Cell(i + 2, 4).Range.Text = item.CoupleName?.ToString() ?? "";
                    wordTable.Cell(i + 2, 5).Range.Text = item.BookingDate?.ToString("dd.MM.yyyy") ?? "";
                }

                // Автоподбор ширины столбцов
                wordTable.Columns.AutoFit();

                // Сохраняем документ
                doc.SaveAs2(filePath);
                wordApp.Visible = true; // Оставляем Word открытым

                System.Windows.MessageBox.Show($"Документ успешно сохранен:\n{filePath}", "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при экспорте в Word:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Класс для хранения статистики по месяцам
        public class MonthlyActivity
        {
            public string Name { get; set; }
            public int January { get; set; }
            public int February { get; set; }
            public int March { get; set; }
            public int April { get; set; }
            public int May { get; set; }
            public int June { get; set; }
            public int July { get; set; }
            public int August { get; set; }
            public int September { get; set; }
            public int October { get; set; }
            public int November { get; set; }
            public int December { get; set; }

            public int Total => January + February + March + April + May + June +
                              July + August + September + October + November + December;
        }

        private int _activityYear = DateTime.Now.Year;
        public int ActivityYear
        {
            get => _activityYear;
            set
            {
                _activityYear = value;
                if (AktivCb.SelectedItem != null)
                    AktivCb_SelectionChanged(null, null);
            }
        }

        private void ApplyActivityYearFilter_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(ActivityYearTextBox.Text, out int year))
            {
                ActivityYear = year;
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите корректный год", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AktivCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AktivCb.SelectedItem == null) return;

            var selectedType = ((ComboBoxItem)AktivCb.SelectedItem).Content.ToString();

            switch (selectedType)
            {
                case "Рестораны": LoadRestaurantActivity(); break;
                case "Декорации": LoadDecorationActivity(); break;
                case "Трансфер": LoadTransferActivity(); break;
                case "Ведущие": LoadHostActivity(); break;
                case "Платье": LoadDressActivity(); break;
                case "Костюм": LoadSuitActivity(); break;
                case "Украшения": LoadJewelryActivity(); break;
                case "Стилисты": LoadStylistActivity(); break;
                case "Торт": LoadCakeActivity(); break;
                case "Букет": LoadBouquetActivity(); break;
                case "Музыканты": LoadMusicianActivity(); break;
                case "Фото и видео": LoadPhotographerActivity(); break;
            }
        }

        private void SetMonthCount(MonthlyActivity activity, int month, int count)
        {
            switch (month)
            {
                case 1: activity.January = count; break;
                case 2: activity.February = count; break;
                case 3: activity.March = count; break;
                case 4: activity.April = count; break;
                case 5: activity.May = count; break;
                case 6: activity.June = count; break;
                case 7: activity.July = count; break;
                case 8: activity.August = count; break;
                case 9: activity.September = count; break;
                case 10: activity.October = count; break;
                case 11: activity.November = count; break;
                case 12: activity.December = count; break;
            }
        }

        private void AddTotalRow(List<MonthlyActivity> activityData)
        {
            var totalRow = new MonthlyActivity { Name = "Итого" };
            foreach (var item in activityData)
            {
                totalRow.January += item.January;
                totalRow.February += item.February;
                totalRow.March += item.March;
                totalRow.April += item.April;
                totalRow.May += item.May;
                totalRow.June += item.June;
                totalRow.July += item.July;
                totalRow.August += item.August;
                totalRow.September += item.September;
                totalRow.October += item.October;
                totalRow.November += item.November;
                totalRow.December += item.December;
            }
            activityData.Add(totalRow);
        }

        private void LoadRestaurantActivity()
        {
            var activityData = new List<MonthlyActivity>();
            var restaurants = DbConnection.MarryMe.Restaurant.ToList();

            foreach (var restaurant in restaurants)
            {
                var monthlyActivity = new MonthlyActivity { Name = restaurant.Name };
                for (int month = 1; month <= 12; month++)
                {
                    var count = DbConnection.MarryMe.CoupleFavorites
                        .Count(cf => cf.RestaurantId == restaurant.Id &&
                                    cf.Couple.WeddingDate.HasValue &&
                                    cf.Couple.WeddingDate.Value.Year == ActivityYear &&
                                    cf.Couple.WeddingDate.Value.Month == month);
                    SetMonthCount(monthlyActivity, month, count);
                }
                activityData.Add(monthlyActivity);
            }
            AddTotalRow(activityData);
            ActivityListView.ItemsSource = activityData;
        }

        private void LoadDecorationActivity()
        {
            var activityData = new List<MonthlyActivity>();
            var decorations = DbConnection.MarryMe.Decoration.ToList();

            foreach (var decoration in decorations)
            {
                var monthlyActivity = new MonthlyActivity { Name = decoration.Name };
                for (int month = 1; month <= 12; month++)
                {
                    var count = DbConnection.MarryMe.CoupleFavorites
                        .Count(cf => cf.DecorationId == decoration.Id &&
                                    cf.Couple.WeddingDate.HasValue &&
                                    cf.Couple.WeddingDate.Value.Year == ActivityYear &&
                                    cf.Couple.WeddingDate.Value.Month == month);
                    SetMonthCount(monthlyActivity, month, count);
                }
                activityData.Add(monthlyActivity);
            }
            AddTotalRow(activityData);
            ActivityListView.ItemsSource = activityData;
        }

        private void LoadTransferActivity()
        {
            var activityData = new List<MonthlyActivity>();
            var transfers = DbConnection.MarryMe.Transfer.ToList();

            foreach (var transfer in transfers)
            {
                var monthlyActivity = new MonthlyActivity { Name = transfer.Name };
                for (int month = 1; month <= 12; month++)
                {
                    var count = DbConnection.MarryMe.CoupleFavorites
                        .Count(cf => cf.TransferId == transfer.Id &&
                                    cf.Couple.WeddingDate.HasValue &&
                                    cf.Couple.WeddingDate.Value.Year == ActivityYear &&
                                    cf.Couple.WeddingDate.Value.Month == month);
                    SetMonthCount(monthlyActivity, month, count);
                }
                activityData.Add(monthlyActivity);
            }
            AddTotalRow(activityData);
            ActivityListView.ItemsSource = activityData;
        }

        private void LoadHostActivity()
        {
            var activityData = new List<MonthlyActivity>();
            var hosts = DbConnection.MarryMe.Host.ToList();

            foreach (var host in hosts)
            {
                var monthlyActivity = new MonthlyActivity { Name = $"{host.Surname} {host.Name}" };
                for (int month = 1; month <= 12; month++)
                {
                    var count = DbConnection.MarryMe.CoupleFavorites
                        .Count(cf => cf.HostId == host.Id &&
                                    cf.Couple.WeddingDate.HasValue &&
                                    cf.Couple.WeddingDate.Value.Year == ActivityYear &&
                                    cf.Couple.WeddingDate.Value.Month == month);
                    SetMonthCount(monthlyActivity, month, count);
                }
                activityData.Add(monthlyActivity);
            }
            AddTotalRow(activityData);
            ActivityListView.ItemsSource = activityData;
        }

        private void LoadDressActivity()
        {
            var activityData = new List<MonthlyActivity>();
            var dresses = DbConnection.MarryMe.Dress.ToList();

            foreach (var dress in dresses)
            {
                var monthlyActivity = new MonthlyActivity { Name = dress.Name };
                for (int month = 1; month <= 12; month++)
                {
                    var count = DbConnection.MarryMe.CoupleFavorites
                        .Count(cf => cf.DressBriedId == dress.Id &&
                                    cf.Couple.WeddingDate.HasValue &&
                                    cf.Couple.WeddingDate.Value.Year == ActivityYear &&
                                    cf.Couple.WeddingDate.Value.Month == month);
                    SetMonthCount(monthlyActivity, month, count);
                }
                activityData.Add(monthlyActivity);
            }
            AddTotalRow(activityData);
            ActivityListView.ItemsSource = activityData;
        }

        private void LoadSuitActivity()
        {
            var activityData = new List<MonthlyActivity>();
            var suits = DbConnection.MarryMe.Clothing.ToList();

            foreach (var suit in suits)
            {
                var monthlyActivity = new MonthlyActivity { Name = suit.Name };
                for (int month = 1; month <= 12; month++)
                {
                    var count = DbConnection.MarryMe.CoupleFavorites
                        .Count(cf => cf.ClothingGrommId == suit.Id &&
                                    cf.Couple.WeddingDate.HasValue &&
                                    cf.Couple.WeddingDate.Value.Year == ActivityYear &&
                                    cf.Couple.WeddingDate.Value.Month == month);
                    SetMonthCount(monthlyActivity, month, count);
                }
                activityData.Add(monthlyActivity);
            }
            AddTotalRow(activityData);
            ActivityListView.ItemsSource = activityData;
        }

        private void LoadJewelryActivity()
        {
            var activityData = new List<MonthlyActivity>();
            var jewelries = DbConnection.MarryMe.Accessory.ToList();

            foreach (var jewelry in jewelries)
            {
                var monthlyActivity = new MonthlyActivity { Name = jewelry.Name };
                for (int month = 1; month <= 12; month++)
                {
                    var count = DbConnection.MarryMe.CoupleFavorites
                        .Count(cf => cf.AccessoryId == jewelry.Id &&
                                    cf.Couple.WeddingDate.HasValue &&
                                    cf.Couple.WeddingDate.Value.Year == ActivityYear &&
                                    cf.Couple.WeddingDate.Value.Month == month);
                    SetMonthCount(monthlyActivity, month, count);
                }
                activityData.Add(monthlyActivity);
            }
            AddTotalRow(activityData);
            ActivityListView.ItemsSource = activityData;
        }

        private void LoadStylistActivity()
        {
            var activityData = new List<MonthlyActivity>();
            var stylists = DbConnection.MarryMe.Stylist.ToList();

            foreach (var stylist in stylists)
            {
                var monthlyActivity = new MonthlyActivity { Name = stylist.TeamName };
                for (int month = 1; month <= 12; month++)
                {
                    var count = DbConnection.MarryMe.CoupleFavorites
                        .Count(cf => cf.StylistId == stylist.Id &&
                                    cf.Couple.WeddingDate.HasValue &&
                                    cf.Couple.WeddingDate.Value.Year == ActivityYear &&
                                    cf.Couple.WeddingDate.Value.Month == month);
                    SetMonthCount(monthlyActivity, month, count);
                }
                activityData.Add(monthlyActivity);
            }
            AddTotalRow(activityData);
            ActivityListView.ItemsSource = activityData;
        }

        private void LoadCakeActivity()
        {
            var activityData = new List<MonthlyActivity>();
            var cakes = DbConnection.MarryMe.Cake.ToList();

            foreach (var cake in cakes)
            {
                var monthlyActivity = new MonthlyActivity { Name = cake.Name };
                for (int month = 1; month <= 12; month++)
                {
                    var count = DbConnection.MarryMe.CoupleFavorites
                        .Count(cf => cf.CakeId == cake.Id &&
                                    cf.Couple.WeddingDate.HasValue &&
                                    cf.Couple.WeddingDate.Value.Year == ActivityYear &&
                                    cf.Couple.WeddingDate.Value.Month == month);
                    SetMonthCount(monthlyActivity, month, count);
                }
                activityData.Add(monthlyActivity);
            }
            AddTotalRow(activityData);
            ActivityListView.ItemsSource = activityData;
        }

        private void LoadBouquetActivity()
        {
            var activityData = new List<MonthlyActivity>();
            var bouquets = DbConnection.MarryMe.Bouquet.ToList();

            foreach (var bouquet in bouquets)
            {
                var monthlyActivity = new MonthlyActivity { Name = bouquet.Name };
                for (int month = 1; month <= 12; month++)
                {
                    var count = DbConnection.MarryMe.CoupleFavorites
                        .Count(cf => cf.BouquetId == bouquet.Id &&
                                    cf.Couple.WeddingDate.HasValue &&
                                    cf.Couple.WeddingDate.Value.Year == ActivityYear &&
                                    cf.Couple.WeddingDate.Value.Month == month);
                    SetMonthCount(monthlyActivity, month, count);
                }
                activityData.Add(monthlyActivity);
            }
            AddTotalRow(activityData);
            ActivityListView.ItemsSource = activityData;
        }

        private void LoadMusicianActivity()
        {
            var activityData = new List<MonthlyActivity>();
            var musicians = DbConnection.MarryMe.Musician.ToList();

            foreach (var musician in musicians)
            {
                var monthlyActivity = new MonthlyActivity { Name = musician.TeamName };
                for (int month = 1; month <= 12; month++)
                {
                    var count = DbConnection.MarryMe.CoupleFavorites
                        .Count(cf => cf.MusicianId == musician.Id &&
                                    cf.Couple.WeddingDate.HasValue &&
                                    cf.Couple.WeddingDate.Value.Year == ActivityYear &&
                                    cf.Couple.WeddingDate.Value.Month == month);
                    SetMonthCount(monthlyActivity, month, count);
                }
                activityData.Add(monthlyActivity);
            }
            AddTotalRow(activityData);
            ActivityListView.ItemsSource = activityData;
        }

        private void LoadPhotographerActivity()
        {
            var activityData = new List<MonthlyActivity>();
            var photographers = DbConnection.MarryMe.PhotographerVideographer.ToList();

            foreach (var photographer in photographers)
            {
                var monthlyActivity = new MonthlyActivity { Name = photographer.TeamName };
                for (int month = 1; month <= 12; month++)
                {
                    var count = DbConnection.MarryMe.CoupleFavorites
                        .Count(cf => cf.PhotographerVideographerId == photographer.Id &&
                                    cf.Couple.WeddingDate.HasValue &&
                                    cf.Couple.WeddingDate.Value.Year == ActivityYear &&
                                    cf.Couple.WeddingDate.Value.Month == month);
                    SetMonthCount(monthlyActivity, month, count);
                }
                activityData.Add(monthlyActivity);
            }
            AddTotalRow(activityData);
            ActivityListView.ItemsSource = activityData;
        }

        private void ExportActivityToWord_Click(object sender, RoutedEventArgs e)
        {
            if (ActivityListView.Items.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта", "Информация",
                               MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Application wordApp = null;
            Document doc = null;

            try
            {
                string category = ((ComboBoxItem)AktivCb.SelectedItem)?.Content?.ToString() ?? "Неизвестная категория";

                // Создаем папки
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string reportsFolder = Path.Combine(desktopPath, "Отчеты MerryMe");
                string activityFolder = Path.Combine(reportsFolder, "Активность коллекций");
                Directory.CreateDirectory(reportsFolder);
                Directory.CreateDirectory(activityFolder);

                // Генерируем уникальное имя файла
                string fileName = $"{category} {ActivityYear}.docx";
                string filePath = Path.Combine(activityFolder, fileName);
                int counter = 1;
                while (File.Exists(filePath))
                {
                    fileName = $"{category} {ActivityYear} ({counter}).docx";
                    filePath = Path.Combine(activityFolder, fileName);
                    counter++;
                }

                // Создаем документ Word
                wordApp = new Application { Visible = false };
                doc = wordApp.Documents.Add();
                doc.PageSetup.Orientation = WdOrientation.wdOrientLandscape;

                // Добавляем заголовок
                Paragraph title = doc.Paragraphs.Add();
                title.Range.Text = $"Категория: {category}\nГод: {ActivityYear}";
                title.Range.Font.Bold = 1;
                title.Range.Font.Size = 16;
                title.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                title.Range.InsertParagraphAfter();

                // Создаем таблицу
                int rowsCount = ActivityListView.Items.Count + 1;
                int columnsCount = 14;
                Table wordTable = doc.Tables.Add(doc.Range(doc.Content.End - 1), rowsCount, columnsCount);
                wordTable.Borders.Enable = 1;
                wordTable.AllowAutoFit = true;

                // Заголовки таблицы
                string[] headers = { "Название", "Январь", "Февраль", "Март", "Апрель",
                           "Май", "Июнь", "Июль", "Август", "Сентябрь",
                           "Октябрь", "Ноябрь", "Декабрь", "Итого" };

                for (int i = 0; i < headers.Length; i++)
                {
                    wordTable.Cell(1, i + 1).Range.Text = headers[i];
                    wordTable.Cell(1, i + 1).Range.Font.Bold = 1;
                    wordTable.Cell(1, i + 1).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                }

                // Заполняем данные
                for (int i = 0; i < ActivityListView.Items.Count; i++)
                {
                    var item = (MonthlyActivity)ActivityListView.Items[i];
                    wordTable.Cell(i + 2, 1).Range.Text = item.Name;
                    wordTable.Cell(i + 2, 2).Range.Text = item.January.ToString();
                    wordTable.Cell(i + 2, 3).Range.Text = item.February.ToString();
                    wordTable.Cell(i + 2, 4).Range.Text = item.March.ToString();
                    wordTable.Cell(i + 2, 5).Range.Text = item.April.ToString();
                    wordTable.Cell(i + 2, 6).Range.Text = item.May.ToString();
                    wordTable.Cell(i + 2, 7).Range.Text = item.June.ToString();
                    wordTable.Cell(i + 2, 8).Range.Text = item.July.ToString();
                    wordTable.Cell(i + 2, 9).Range.Text = item.August.ToString();
                    wordTable.Cell(i + 2, 10).Range.Text = item.September.ToString();
                    wordTable.Cell(i + 2, 11).Range.Text = item.October.ToString();
                    wordTable.Cell(i + 2, 12).Range.Text = item.November.ToString();
                    wordTable.Cell(i + 2, 13).Range.Text = item.December.ToString();
                    wordTable.Cell(i + 2, 14).Range.Text = item.Total.ToString();

                    for (int j = 2; j <= 14; j++)
                        wordTable.Cell(i + 2, j).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                }

                wordTable.Columns.AutoFit();
                doc.SaveAs2(filePath);
                wordApp.Visible = true;

                MessageBox.Show($"Документ успешно сохранен:\n{filePath}",
                               "Экспорт завершен",
                               MessageBoxButton.OK,
                               MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в Word:\n{ex.Message}",
                               "Ошибка",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
            }
            finally
            {
                if (doc != null) Marshal.ReleaseComObject(doc);
                if (wordApp != null) Marshal.ReleaseComObject(wordApp);
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        public class ClientActivity
        {
            public string Name { get; set; }
            public int January { get; set; }
            public int February { get; set; }
            public int March { get; set; }
            public int April { get; set; }
            public int May { get; set; }
            public int June { get; set; }
            public int July { get; set; }
            public int August { get; set; }
            public int September { get; set; }
            public int October { get; set; }
            public int November { get; set; }
            public int December { get; set; }

            public int Total
            {
                get
                {
                    return January + February + March + April + May + June +
                           July + August + September + October + November + December;
                }
            }
        }

        private int _currentYear = DateTime.Now.Year;

        public int CurrentYear
        {
            get { return _currentYear; }
            set
            {
                _currentYear = value;
                LoadClientActivityData();
            }
        }

        private void ApplyYearFilter_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(YearTextBox.Text, out int year))
            {
                CurrentYear = year;
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите корректный год", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadClientActivityData()
        {
            var activityData = new List<ClientActivity>();

            // Завершенные свадьбы (статус 2)
            var completed = new ClientActivity { Name = "Завершенные свадьбы" };

            // Отмененные свадьбы (статус 3)
            var canceled = new ClientActivity { Name = "Отмененные свадьбы" };

            // Итоговая строка
            var total = new ClientActivity { Name = "Итого" };

            for (int month = 1; month <= 12; month++)
            {
                // Получаем даты начала и конца месяца
                DateTime startDate = new DateTime(CurrentYear, month, 1);
                DateTime endDate = startDate.AddMonths(1).AddDays(-1);

                // Завершенные свадьбы
                int completedCount = DbConnection.MarryMe.Couple
                    .Count(c => c.WeddingStatusId == 2 &&
                                c.WeddingDate.HasValue &&
                                c.WeddingDate.Value >= startDate &&
                                c.WeddingDate.Value <= endDate);

                // Отмененные свадьбы
                int canceledCount = DbConnection.MarryMe.Couple
                    .Count(c => c.WeddingStatusId == 3 &&
                                c.WeddingDate.HasValue &&
                                c.WeddingDate.Value >= startDate &&
                                c.WeddingDate.Value <= endDate);

                // Устанавливаем значения для каждого месяца
                switch (month)
                {
                    case 1:
                        completed.January = completedCount;
                        canceled.January = canceledCount;
                        total.January = completedCount + canceledCount;
                        break;
                    case 2:
                        completed.February = completedCount;
                        canceled.February = canceledCount;
                        total.February = completedCount + canceledCount;
                        break;
                    case 3:
                        completed.March = completedCount;
                        canceled.March = canceledCount;
                        total.March = completedCount + canceledCount;
                        break;
                    case 4:
                        completed.April = completedCount;
                        canceled.April = canceledCount;
                        total.April = completedCount + canceledCount;
                        break;
                    case 5:
                        completed.May = completedCount;
                        canceled.May = canceledCount;
                        total.May = completedCount + canceledCount;
                        break;
                    case 6:
                        completed.June = completedCount;
                        canceled.June = canceledCount;
                        total.June = completedCount + canceledCount;
                        break;
                    case 7:
                        completed.July = completedCount;
                        canceled.July = canceledCount;
                        total.July = completedCount + canceledCount;
                        break;
                    case 8:
                        completed.August = completedCount;
                        canceled.August = canceledCount;
                        total.August = completedCount + canceledCount;
                        break;
                    case 9:
                        completed.September = completedCount;
                        canceled.September = canceledCount;
                        total.September = completedCount + canceledCount;
                        break;
                    case 10:
                        completed.October = completedCount;
                        canceled.October = canceledCount;
                        total.October = completedCount + canceledCount;
                        break;
                    case 11:
                        completed.November = completedCount;
                        canceled.November = canceledCount;
                        total.November = completedCount + canceledCount;
                        break;
                    case 12:
                        completed.December = completedCount;
                        canceled.December = canceledCount;
                        total.December = completedCount + canceledCount;
                        break;
                }
            }

            activityData.Add(completed);
            activityData.Add(canceled);
            activityData.Add(total);

            ClientActivityListView.ItemsSource = activityData;
        }

        private void ExportClientActivityToWord_Click(object sender, RoutedEventArgs e)
        {
            if (ClientActivityListView.Items.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта", "Информация",
                               MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Application wordApp = null;
            Document doc = null;

            try
            {
                // Создаем папки если их нет
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string reportsFolder = Path.Combine(desktopPath, "Отчеты MerryMe");
                string activityFolder = Path.Combine(reportsFolder, "Активность клиентов");

                Directory.CreateDirectory(reportsFolder);
                Directory.CreateDirectory(activityFolder);

                // Генерируем имя файла
                string fileName = $"Активность {CurrentYear}.docx";
                string filePath = Path.Combine(activityFolder, fileName);

                // Проверяем существование файла
                int counter = 1;
                while (File.Exists(filePath))
                {
                    fileName = $"Активность {CurrentYear} ({counter}).docx";
                    filePath = Path.Combine(activityFolder, fileName);
                    counter++;
                }

                // Создаем Word приложение
                wordApp = new Application();
                doc = wordApp.Documents.Add();

                // Устанавливаем альбомную ориентацию
                doc.PageSetup.Orientation = WdOrientation.wdOrientLandscape;

                // Добавляем заголовок
                Paragraph title = doc.Paragraphs.Add();
                title.Range.Text = $"Активность клиентов за {CurrentYear} год";
                title.Range.Font.Bold = 1;
                title.Range.Font.Size = 16;
                title.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                title.Range.InsertParagraphAfter();

                // Создаем таблицу
                int rowsCount = ClientActivityListView.Items.Count + 1; // +1 для заголовка
                int columnsCount = 14; // Название + 12 месяцев + Итого
                Table wordTable = doc.Tables.Add(doc.Range(doc.Content.End - 1), rowsCount, columnsCount);

                // Настраиваем таблицу
                wordTable.Borders.Enable = 1;
                wordTable.AllowAutoFit = true;

                // Заполняем заголовки таблицы
                string[] headers = { "Показатель", "Январь", "Февраль", "Март", "Апрель",
                           "Май", "Июнь", "Июль", "Август", "Сентябрь",
                           "Октябрь", "Ноябрь", "Декабрь", "Итого" };

                for (int i = 0; i < headers.Length; i++)
                {
                    wordTable.Cell(1, i + 1).Range.Text = headers[i];
                    wordTable.Cell(1, i + 1).Range.Font.Bold = 1;
                    wordTable.Cell(1, i + 1).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                }

                // Заполняем таблицу данными
                for (int i = 0; i < ClientActivityListView.Items.Count; i++)
                {
                    var item = (ClientActivity)ClientActivityListView.Items[i];

                    wordTable.Cell(i + 2, 1).Range.Text = item.Name;
                    wordTable.Cell(i + 2, 2).Range.Text = item.January.ToString();
                    wordTable.Cell(i + 2, 3).Range.Text = item.February.ToString();
                    wordTable.Cell(i + 2, 4).Range.Text = item.March.ToString();
                    wordTable.Cell(i + 2, 5).Range.Text = item.April.ToString();
                    wordTable.Cell(i + 2, 6).Range.Text = item.May.ToString();
                    wordTable.Cell(i + 2, 7).Range.Text = item.June.ToString();
                    wordTable.Cell(i + 2, 8).Range.Text = item.July.ToString();
                    wordTable.Cell(i + 2, 9).Range.Text = item.August.ToString();
                    wordTable.Cell(i + 2, 10).Range.Text = item.September.ToString();
                    wordTable.Cell(i + 2, 11).Range.Text = item.October.ToString();
                    wordTable.Cell(i + 2, 12).Range.Text = item.November.ToString();
                    wordTable.Cell(i + 2, 13).Range.Text = item.December.ToString();
                    wordTable.Cell(i + 2, 14).Range.Text = item.Total.ToString();

                    // Выравниваем числа по центру
                    for (int j = 2; j <= 14; j++)
                    {
                        wordTable.Cell(i + 2, j).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                    }
                }

                // Автоподбор ширины столбцов
                wordTable.Columns.AutoFit();

                // Сохраняем документ
                doc.SaveAs2(filePath);

                // Показываем Word пользователю
                wordApp.Visible = true;

                MessageBox.Show($"Документ успешно сохранен:\n{filePath}",
                               "Экспорт завершен",
                               MessageBoxButton.OK,
                               MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в Word:\n{ex.Message}",
                               "Ошибка",
                               MessageBoxButton.OK,
                               MessageBoxImage.Error);
            }
            finally
            {
                // Освобождаем ресурсы
                if (doc != null)
                    Marshal.ReleaseComObject(doc);

                if (wordApp != null)
                    Marshal.ReleaseComObject(wordApp);
            }
        }

    }
}
