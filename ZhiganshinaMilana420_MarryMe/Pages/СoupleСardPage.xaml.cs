using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Reflection;
using Microsoft.Office.Interop.Word;
using Application = Microsoft.Office.Interop.Word.Application;
using System.IO;
using System.Globalization;
using ZhiganshinaMilana420_MarryMe.Pages.DecorationForder;
using ZhiganshinaMilana420_MarryMe.Pages.DressFolder;
using ZhiganshinaMilana420_MarryMe.Pages.ClothingFolder;
using ZhiganshinaMilana420_MarryMe.Pages.HostFolder;
using ZhiganshinaMilana420_MarryMe.Pages.PhotographerVideographerFolder;
using ZhiganshinaMilana420_MarryMe.Pages.BouquetFolder;
using ZhiganshinaMilana420_MarryMe.Pages.StylistFolder;
using ZhiganshinaMilana420_MarryMe.Pages.MusicianFolder;
using ZhiganshinaMilana420_MarryMe.Pages.TransferFolder;
using ZhiganshinaMilana420_MarryMe.Pages.CakeFolder;
using ZhiganshinaMilana420_MarryMe.Pages.AccessoryFolder;

namespace ZhiganshinaMilana420_MarryMe.Pages
{
    /// <summary>
    /// Логика взаимодействия для СoupleСardPage.xaml
    /// </summary>
    public partial class СoupleСardPage : System.Windows.Controls.Page
    {
        public class CategoryItem
        {
            public int Number { get; set; }
            public string Category { get; set; }
            public string Id { get; set; }
            public string Date { get; set; }
            public Visibility CancelButtonVisibility { get; set; }
        }

        public static Gromm Grooms { get; set; } = new Gromm();
        public static List<Couple> Couples { get; set; } = new List<Couple>();
        Couple contextCouple;
        public static Couple Couple1 { get; set; } = new Couple();
        public static Couple Cou { get; set; } = new Couple();
        public static CoupleFavorites CoupleFavorites { get; set; } = new CoupleFavorites();
        public static Restaurant Restaurant { get; set; } = new Restaurant();

        public СoupleСardPage(Couple couple)
        {
            InitializeComponent();
            contextCouple = couple ?? throw new ArgumentNullException(nameof(couple));
            Couple1 = couple;
            Cou = couple;

            try
            {
                var groom = DbConnection.MarryMe.Gromm.FirstOrDefault(g => g.Id == couple.GroomId);
                var bride = DbConnection.MarryMe.Bride.FirstOrDefault(b => b.Id == couple.BrideId);

                NameGroomTb.Text = groom != null ? $"{groom.Surname} {groom.Name} {groom.Patronymic}" : "Имя не указано";
                NameBrideTb.Text = bride != null ? $"{bride.Surname} {bride.Name} {bride.Patronymic}" : "Имя не указано";

                LoadFavoriteItems();
            }
            catch (Exception ex)
            {                   
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }                   

            this.DataContext = this;
        }
        private void SelectCategory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null)
            {
                int categoryNumber;
                if (int.TryParse(button.Tag.ToString(), out categoryNumber))
                {
                    switch (categoryNumber)
                    {
                        case 1:
                            NavigationService.Navigate(new RestaurantMenuPage(contextCouple, CoupleFavorites));
                            break;
                        case 2:
                            NavigationService.Navigate(new DecorationMenuPage(contextCouple, CoupleFavorites));
                            break;
                        case 3:
                            NavigationService.Navigate(new DressMenuPage(contextCouple, CoupleFavorites));
                            break;
                        case 4:
                            NavigationService.Navigate(new ClothingMenuPage(contextCouple, CoupleFavorites));
                            break;
                        case 5:
                            NavigationService.Navigate(new HostMenuPage(contextCouple, CoupleFavorites));
                            break;
                        case 6:
                            NavigationService.Navigate(new PhotographeMenuPage(contextCouple, CoupleFavorites));
                            break;
                        case 7:
                            NavigationService.Navigate(new BouquetMenuPage(contextCouple, CoupleFavorites));
                            break;
                        case 8:
                            NavigationService.Navigate(new StylistMenuPage(contextCouple, CoupleFavorites));
                            break;
                        case 9:
                            NavigationService.Navigate(new MusicianMenuPage(contextCouple, CoupleFavorites));
                            break;
                        case 10:
                            NavigationService.Navigate(new TransferMenuPage(contextCouple, CoupleFavorites));
                            break;
                        case 11:
                            NavigationService.Navigate(new CakeMenuPage(contextCouple, CoupleFavorites));
                            break;
                        case 12:
                            NavigationService.Navigate(new AccessoryMenuPage(contextCouple, CoupleFavorites));
                            break;
                    }
                }
            }
        }
        private void LoadFavoriteItems()
        {
            try
            {
                CoupleFavorites = DbConnection.MarryMe.CoupleFavorites
                    .FirstOrDefault(c => c.CoupleId == contextCouple.Id) ?? new CoupleFavorites();

                var restaurant = DbConnection.MarryMe.Restaurant
                    .FirstOrDefault(r => r.Id == CoupleFavorites.RestaurantId);
                var restaurantBooking = DbConnection.MarryMe.RestaurantBookingDates
                    .FirstOrDefault(b => b.CoupleId == contextCouple.Id &&
                               b.RestaurantId == CoupleFavorites.RestaurantId &&
                               b.Status == true);
                string restaurantDate = "Не забронирован";
                if (restaurantBooking != null)
                {
                    restaurantDate = restaurantBooking.BookingDate.ToString("dd.MM.yyyy");
                }

                var decoration = DbConnection.MarryMe.Decoration
                    .FirstOrDefault(r => r.Id == CoupleFavorites.DecorationId);
                string decorationBooking = "Не забронирован";
                if (CoupleFavorites.DecorationId != null)
                {
                    decorationBooking = "Выбрано";
                }


                var dress = DbConnection.MarryMe.Dress
                    .FirstOrDefault(d => d.Id == CoupleFavorites.DressBriedId);
                string dressBooking = "Не забронирован";
                if (CoupleFavorites.DressBriedId != null)
                {
                    dressBooking = "Выбрано";
                }

                var clothingGromm = DbConnection.MarryMe.Clothing
                    .FirstOrDefault(c => c.Id == CoupleFavorites.ClothingGrommId);
                string clothingBooking = "Не забронирован";
                if (CoupleFavorites.ClothingGrommId != null)
                {
                    clothingBooking = "Выбрано";
                }

                var host = DbConnection.MarryMe.Host
                    .FirstOrDefault(h => h.Id == CoupleFavorites.HostId);
                var hostBooking = DbConnection.MarryMe.HostBookingDates
                    .FirstOrDefault(b => b.CoupleId == contextCouple.Id &&
                               b.HostId == CoupleFavorites.HostId &&
                               b.Status == true);
                string hostDate = "Не забронирован";
                if (hostBooking != null)
                {
                    hostDate = hostBooking.BookingDate.ToString("dd.MM.yyyy");
                }

                var PhotographerVideographer = DbConnection.MarryMe.PhotographerVideographer
                    .FirstOrDefault(p => p.Id == CoupleFavorites.PhotographerVideographerId);
                var PhotographerVideographerBooking = DbConnection.MarryMe.PhotographerVideographerBookingDates
                    .FirstOrDefault(b => b.CoupleId == contextCouple.Id &&
                               b.PhotographerVideographerId == CoupleFavorites.PhotographerVideographerId &&
                               b.Status == true);
                string PhotographerVideographerDate = "Не забронирован";
                if (PhotographerVideographerBooking != null)
                {
                    PhotographerVideographerDate = PhotographerVideographerBooking.BookingDate.ToString("dd.MM.yyyy");
                }

                var Bouquet = DbConnection.MarryMe.Bouquet
                    .FirstOrDefault(b => b.Id == CoupleFavorites.BouquetId);
                string BouquetBooking = "Не забронирован";
                if (CoupleFavorites.BouquetId != null)
                {
                    BouquetBooking = "Выбрано";
                }

                var Stylist = DbConnection.MarryMe.Stylist
                    .FirstOrDefault(s => s.Id == CoupleFavorites.StylistId);
                var StylistBooking = DbConnection.MarryMe.StylistBookingDates
                    .FirstOrDefault(b => b.CoupleId == contextCouple.Id &&
                               b.StylistId == CoupleFavorites.StylistId &&
                               b.Status == true);
                string StylistDate = "Не забронирован";
                if (StylistBooking != null)
                {
                    StylistDate = StylistBooking.BookingDate.ToString("dd.MM.yyyy");
                }

                var Musician = DbConnection.MarryMe.Musician
                    .FirstOrDefault(m => m.Id == CoupleFavorites.MusicianId);
                var MusicianBooking = DbConnection.MarryMe.MusicianBookingDates
                    .FirstOrDefault(b => b.CoupleId == contextCouple.Id &&
                               b.MusicianId == CoupleFavorites.MusicianId &&
                               b.Status == true);
                string MusicianDate = "Не забронирован";
                if (MusicianBooking != null)
                {
                    MusicianDate = MusicianBooking.BookingDate.ToString("dd.MM.yyyy");
                }

                var Transfer = DbConnection.MarryMe.Transfer
                    .FirstOrDefault(t => t.Id == CoupleFavorites.TransferId);
                var TransferBooking = DbConnection.MarryMe.TransferBookingDates
                    .FirstOrDefault(b => b.CoupleId == contextCouple.Id &&
                               b.TransferId == CoupleFavorites.TransferId &&
                               b.Status == true);
                string TransferDate = "Не забронирован";
                if (TransferBooking != null)
                {
                    TransferDate = TransferBooking.BookingDate.ToString("dd.MM.yyyy");
                }

                var Cake = DbConnection.MarryMe.Cake
                    .FirstOrDefault(c => c.Id == CoupleFavorites.CakeId);
                string CakeBooking = "Не забронирован";
                if (CoupleFavorites.CakeId != null)
                {
                    CakeBooking = "Выбрано";
                }

                var Accessory = DbConnection.MarryMe.Accessory
                    .FirstOrDefault(c => c.Id == CoupleFavorites.AccessoryId);
                string AccessoryBooking = "Не забронирован";
                if (CoupleFavorites.AccessoryId != null)
                {
                    AccessoryBooking = "Выбрано";
                }

                var items = new List<CategoryItem>
                {
                new CategoryItem
                {
                    Number = 1,
                    Category = "Ресторан",
                    Id = restaurant != null ? restaurant.Name : "Не выбран",
                    Date = restaurantDate,
                    CancelButtonVisibility = (restaurant != null || restaurantBooking != null) ?
                        Visibility.Visible : Visibility.Collapsed
                },
                new CategoryItem
                {
                    Number = 2,
                    Category = "Декорации",
                    Id = decoration != null ? decoration.Name : "Не выбран",
                    Date = decorationBooking,
                    CancelButtonVisibility = decoration != null ?
                        Visibility.Visible : Visibility.Collapsed
                },
                new CategoryItem
                {
                    Number = 3,
                    Category = "Свадебное платье",
                    Id = dress != null ? dress.Name : "Не выбран",
                    Date = dressBooking,
                    CancelButtonVisibility = dress != null ?
                        Visibility.Visible : Visibility.Collapsed
                },
                new CategoryItem
                {
                    Number = 4,
                    Category = "Смокинг",
                    Id = clothingGromm != null ? clothingGromm.Name : "Не выбран",
                    Date = clothingBooking,
                    CancelButtonVisibility = clothingGromm != null ?
                        Visibility.Visible : Visibility.Collapsed
                },
                new CategoryItem
                {
                    Number = 5,
                    Category = "Ведущий",
                    Id = host != null ? host.Surname + " " + host.Name + " " + host.Patronymic : "Не выбран",
                    Date = hostDate,
                    CancelButtonVisibility = (host != null || hostBooking != null) ?
                        Visibility.Visible : Visibility.Collapsed
                },
                new CategoryItem
                {
                    Number = 6,
                    Category = "Фото и видео",
                    Id = PhotographerVideographer != null ? PhotographerVideographer.TeamName : "Не выбран",
                    Date = PhotographerVideographerDate,
                    CancelButtonVisibility = (PhotographerVideographer != null || PhotographerVideographerBooking != null) ?
                        Visibility.Visible : Visibility.Collapsed
                },
                new CategoryItem
                {
                    Number = 7,
                    Category = "Букет невесты",
                    Id = Bouquet != null ? Bouquet.Name : "Не выбран",
                    Date = BouquetBooking,
                    CancelButtonVisibility = Bouquet != null ?
                        Visibility.Visible : Visibility.Collapsed
                },
                new CategoryItem
                {
                    Number = 8,
                    Category = "Стилист",
                    Id = Stylist != null ? Stylist.TeamName : "Не выбран",
                    Date = StylistDate,
                    CancelButtonVisibility = (Stylist != null || StylistBooking != null) ?
                        Visibility.Visible : Visibility.Collapsed
                },
                new CategoryItem
                {
                    Number = 9,
                    Category = "Музыкальная группа",
                    Id = Musician != null ? Musician.TeamName : "Не выбран",
                    Date = MusicianDate,
                    CancelButtonVisibility = (Musician != null || MusicianBooking != null) ?
                        Visibility.Visible : Visibility.Collapsed
                },
                new CategoryItem
                {
                    Number = 10,
                    Category = "Трансфер",
                    Id = Transfer != null ? Transfer.Name : "Не выбран",
                    Date = TransferDate,
                    CancelButtonVisibility = (Transfer != null || TransferBooking != null) ?
                        Visibility.Visible : Visibility.Collapsed
                },
                new CategoryItem
                {
                    Number = 11,
                    Category = "Торт",
                    Id = Cake != null ? Cake.Name : "Не выбран",
                    Date = CakeBooking,
                    CancelButtonVisibility = Cake != null ?
                        Visibility.Visible : Visibility.Collapsed
                },
                new CategoryItem
                {
                    Number = 12,
                    Category = "Ювелирные украшения",
                    Id = Accessory != null ? Accessory.Name : "Не выбран",
                    Date = AccessoryBooking,
                    CancelButtonVisibility = Accessory != null ?
                        Visibility.Visible : Visibility.Collapsed
                },
                };
                FavoritClientLv.ItemsSource = items;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
                FavoritClientLv.ItemsSource = new List<CategoryItem>();
            }
        }
        private void CancelSelection_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null)
            {
                int categoryNumber;
                if (int.TryParse(button.Tag.ToString(), out categoryNumber))
                {
                    try
                    {
                        // Получаем текущие элементы ListView
                        var items = FavoritClientLv.ItemsSource as List<CategoryItem>;
                        if (items == null) return;

                        // Находим элемент, который нужно обновить
                        var itemToUpdate = items.FirstOrDefault(i => i.Number == categoryNumber);
                        if (itemToUpdate == null) return;

                        switch (categoryNumber)
                        {
                            case 1: // Ресторан
                                if (CoupleFavorites.RestaurantId != null)
                                {
                                    // Удаляем бронирование даты
                                    var bookings = DbConnection.MarryMe.RestaurantBookingDates
                                        .Where(b => b.CoupleId == contextCouple.Id &&
                                              b.RestaurantId == CoupleFavorites.RestaurantId)
                                        .ToList();
                                    DbConnection.MarryMe.RestaurantBookingDates.RemoveRange(bookings);

                                    // Сбрасываем выбор ресторана
                                    CoupleFavorites.RestaurantId = null;

                                    // Обновляем данные в ListView
                                    itemToUpdate.Id = "Не выбран";
                                    itemToUpdate.Date = "Не забронирован";
                                    itemToUpdate.CancelButtonVisibility = Visibility.Collapsed;
                                }
                                break;

                            case 2: // Декорации
                                if (CoupleFavorites.DecorationId != null)
                                {
                                    CoupleFavorites.DecorationId = null;
                                    itemToUpdate.Id = "Не выбран";
                                    itemToUpdate.Date = "Не забронирован";
                                    itemToUpdate.CancelButtonVisibility = Visibility.Collapsed;
                                }
                                break;

                            case 3: // Свадебное платье
                                if (CoupleFavorites.DressBriedId != null)
                                {
                                    CoupleFavorites.DressBriedId = null;
                                    itemToUpdate.Id = "Не выбран";
                                    itemToUpdate.Date = "Не забронирован";
                                    itemToUpdate.CancelButtonVisibility = Visibility.Collapsed;
                                }
                                break;

                            case 4: // Смокинг
                                if (CoupleFavorites.ClothingGrommId != null)
                                {
                                    CoupleFavorites.ClothingGrommId = null;
                                    itemToUpdate.Id = "Не выбран";
                                    itemToUpdate.Date = "Не забронирован";
                                    itemToUpdate.CancelButtonVisibility = Visibility.Collapsed;
                                }
                                break;

                            case 5: // Ведущий
                                if (CoupleFavorites.HostId != null)
                                {
                                    var bookings = DbConnection.MarryMe.HostBookingDates
                                        .Where(b => b.CoupleId == contextCouple.Id &&
                                              b.HostId == CoupleFavorites.HostId)
                                        .ToList();
                                    DbConnection.MarryMe.HostBookingDates.RemoveRange(bookings);

                                    CoupleFavorites.HostId = null;
                                    itemToUpdate.Id = "Не выбран";
                                    itemToUpdate.Date = "Не забронирован";
                                    itemToUpdate.CancelButtonVisibility = Visibility.Collapsed;
                                }
                                break;

                            case 6: // Фото и видео
                                if (CoupleFavorites.PhotographerVideographerId != null)
                                {
                                    var bookings = DbConnection.MarryMe.PhotographerVideographerBookingDates
                                        .Where(b => b.CoupleId == contextCouple.Id &&
                                              b.PhotographerVideographerId == CoupleFavorites.PhotographerVideographerId)
                                        .ToList();
                                    DbConnection.MarryMe.PhotographerVideographerBookingDates.RemoveRange(bookings);

                                    CoupleFavorites.PhotographerVideographerId = null;
                                    itemToUpdate.Id = "Не выбран";
                                    itemToUpdate.Date = "Не забронирован";
                                    itemToUpdate.CancelButtonVisibility = Visibility.Collapsed;
                                }
                                break;

                            case 7: // Букет невесты
                                if (CoupleFavorites.BouquetId != null)
                                {
                                    CoupleFavorites.BouquetId = null;
                                    itemToUpdate.Id = "Не выбран";
                                    itemToUpdate.Date = "Не забронирован";
                                    itemToUpdate.CancelButtonVisibility = Visibility.Collapsed;
                                }
                                break;

                            case 8: // Стилист
                                if (CoupleFavorites.StylistId != null)
                                {
                                    var bookings = DbConnection.MarryMe.StylistBookingDates
                                        .Where(b => b.CoupleId == contextCouple.Id &&
                                              b.StylistId == CoupleFavorites.StylistId)
                                        .ToList();
                                    DbConnection.MarryMe.StylistBookingDates.RemoveRange(bookings);

                                    CoupleFavorites.StylistId = null;
                                    itemToUpdate.Id = "Не выбран";
                                    itemToUpdate.Date = "Не забронирован";
                                    itemToUpdate.CancelButtonVisibility = Visibility.Collapsed;
                                }
                                break;

                            case 9: // Музыкальная группа
                                if (CoupleFavorites.MusicianId != null)
                                {
                                    var bookings = DbConnection.MarryMe.MusicianBookingDates
                                        .Where(b => b.CoupleId == contextCouple.Id &&
                                              b.MusicianId == CoupleFavorites.MusicianId)
                                        .ToList();
                                    DbConnection.MarryMe.MusicianBookingDates.RemoveRange(bookings);

                                    CoupleFavorites.MusicianId = null;
                                    itemToUpdate.Id = "Не выбран";
                                    itemToUpdate.Date = "Не забронирован";
                                    itemToUpdate.CancelButtonVisibility = Visibility.Collapsed;
                                }
                                break;

                            case 10: // Трансфер
                                if (CoupleFavorites.TransferId != null)
                                {
                                    var bookings = DbConnection.MarryMe.TransferBookingDates
                                        .Where(b => b.CoupleId == contextCouple.Id &&
                                              b.TransferId == CoupleFavorites.TransferId)
                                        .ToList();
                                    DbConnection.MarryMe.TransferBookingDates.RemoveRange(bookings);

                                    CoupleFavorites.TransferId = null;
                                    itemToUpdate.Id = "Не выбран";
                                    itemToUpdate.Date = "Не забронирован";
                                    itemToUpdate.CancelButtonVisibility = Visibility.Collapsed;
                                }
                                break;

                            case 11: // Торт
                                if (CoupleFavorites.CakeId != null)
                                {
                                    CoupleFavorites.CakeId = null;
                                    itemToUpdate.Id = "Не выбран";
                                    itemToUpdate.Date = "Не забронирован";
                                    itemToUpdate.CancelButtonVisibility = Visibility.Collapsed;
                                }
                                break;

                            case 12: // Ювелирные украшения
                                if (CoupleFavorites.AccessoryId != null)
                                {
                                    CoupleFavorites.AccessoryId = null;
                                    itemToUpdate.Id = "Не выбран";
                                    itemToUpdate.Date = "Не забронирован";
                                    itemToUpdate.CancelButtonVisibility = Visibility.Collapsed;
                                }
                                break;
                        }

                        // Сохраняем изменения в базе данных
                        DbConnection.MarryMe.SaveChanges();

                        // Обновляем отображение ListView
                        FavoritClientLv.Items.Refresh();

                        MessageBox.Show("Выбор успешно отменен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при отмене выбора: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        private string GetMonthNameLowercase(int month)
        {
            // Получаем название месяца в именительном падеже ("май")
            string monthName = CultureInfo.GetCultureInfo("ru-RU").DateTimeFormat.GetMonthName(month);

            // Переводим первую букву в нижний регистр
            return char.ToLower(monthName[0]) + monthName.Substring(1);
        }
        private string FormatSignatureName(string lastName, string firstName, string middleName)
        {
            // Берем первую букву имени и отчества
            string firstInitial = firstName.Length > 0 ? firstName[0].ToString() + "." : "";
            string middleInitial = middleName.Length > 0 ? middleName[0].ToString() + "." : "";

            // Формируем строку в формате "Фамилия И.О."
            return $"{lastName} {firstInitial}{middleInitial}";
        }
        private string GetMonthNamePrepositional(int month)
        {
            string[] months = {
                "января", "февраля", "марта", "апреля", "мая", "июня",
                "июля", "августа", "сентября", "октября", "ноября", "декабря"
            };
            return months[month - 1];
        }
        private void FillWeddingDateFields(Document document)
        {
            if (contextCouple.WeddingDate.HasValue)
            {
                DateTime date = contextCouple.WeddingDate.Value;

                // Заполняем день (число без ведущего нуля)
                FillField(document, "дата_с", date.Day.ToString());

                // Заполняем месяц в предложном падеже ("мае", "январе")
                FillField(document, "месяц_с", GetMonthNamePrepositional(date.Month));

                // Заполняем год
                FillField(document, "год_с", date.Year.ToString());
            }
            else
            {
                // Если дата не указана
                FillField(document, "дата_с", "[дата не указана]");
                FillField(document, "месяц_с", "[месяц не указан]");
                FillField(document, "год_с", "[год не указан]");
            }
        }

        private void GenerateWeddingContract()
        {
            try
            {
                DateTime today = DateTime.Today;
                // Получаем данные из ListView
                var items = FavoritClientLv.ItemsSource as List<CategoryItem>;

                // Создаем экземпляр Word
                var wordApp = new Application();
                wordApp.Visible = true; // Делаем Word видимым

                // Открываем шаблон документа
                string templatePath = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "Свадебный договор.docx");

                var document = wordApp.Documents.Open(templatePath);
                var groom = DbConnection.MarryMe.Gromm.FirstOrDefault(g => g.Id == contextCouple.GroomId);
                var bried = DbConnection.MarryMe.Bride.FirstOrDefault(g => g.Id == contextCouple.BrideId);

                // Форматируем ФИО для подписи
                string groomSignatureName = groom != null
                    ? FormatSignatureName(groom.Surname, groom.Name, groom.Patronymic ?? "")
                    : "Не указано";
                string briedSignatureName = bried != null
                    ? FormatSignatureName(bried.Surname, bried.Name, bried.Patronymic ?? "")
                    : "Не указано";

                // Заполняем основные поля
                FillField(document, "номер_договора", $"0{CoupleFavorites.Id}".ToString());
                
                FillField(document, "Жениха", NameGroomTb.Text);
                FillField(document, "Невесты", NameBrideTb.Text);
                
                FillField(document, "дата", today.Day.ToString());
                FillField(document, "месяц", GetMonthNameLowercase(today.Month));
                FillField(document, "год", today.Year.ToString());

                FillWeddingDateFields(document);
                FillWeddingDateFields(document);

                FillField(document, "ФИО_Жениха", NameGroomTb.Text);
                FillField(document, "ФИО_Невесты", NameBrideTb.Text);
                
                FillField(document, "номер_паспорта_ж", groom.PassportNumber);
                FillField(document, "номер_паспорта_н", bried.PassportNumber);
                
                FillField(document, "серия_паспорта_ж", groom.PassportSeries);
                FillField(document, "серия_паспорта_н", bried.PassportSeries);
                
                FillField(document, "адрес_паспорта_ж", groom.PassportAddress);
                FillField(document, "адрес_паспорта_н", bried.PassportAddress);

                FillField(document, "адрес_проживания_ж", groom.Addresss);
                FillField(document, "адрес_проживания_н", bried.Addresss);

                FillField(document, "номер_телефона_ж", groom.PhoneNumber);
                FillField(document, "номер_телефона_н", bried.PhoneNumber);

                FillField(document, "фио_подписи_жениха", groomSignatureName);
                FillField(document, "фио_подписи_невесты", briedSignatureName);


                // Заполняем таблицу
                if (document.Tables.Count > 0)
                {
                    var table = document.Tables[1]; // Первая таблица в документе

                    foreach (var item in items)
                    {
                        if (item.Number <= table.Rows.Count - 1) // -1 для заголовка
                        {
                            var row = table.Rows[item.Number + 1];
                            row.Cells[3].Range.Text = item.Id ?? ""; // Название
                            row.Cells[5].Range.Text = item.Date ?? ""; // Дата
                        }
                    }
                }

                // Путь для сохранения: Рабочий стол\Договора MaryMe
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string folderPath = System.IO.Path.Combine(desktopPath, "Договора MaryMe");

                // Создаем папку, если её нет
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Базовое имя файла
                string baseFileName = $"Договор № 0{CoupleFavorites.Id} ({NameGroomTb.Text} и {NameBrideTb.Text})";
                string extension = ".docx";
                string savePath = System.IO.Path.Combine(folderPath, baseFileName + extension);

                // Проверяем существование файла и добавляем номер (2), (3) и т.д.
                int counter = 1;
                while (File.Exists(savePath))
                {
                    savePath = System.IO.Path.Combine(folderPath, $"{baseFileName} ({counter}){extension}");
                    counter++;
                }

                // Сохраняем документ (не закрываем Word!)
                document.SaveAs2(savePath);

                // Показываем сообщение (необязательно, можно убрать)
                contextCouple.ContractPath = savePath;
                DbConnection.MarryMe.SaveChanges();

                MessageBox.Show($"Договор успешно сохранен: {savePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании договора: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Вспомогательный метод для заполнения полей
        private void FillField(Document document, string fieldName, string value)
        {
            try
            {
                var range = document.Content;
                range.Find.ClearFormatting();
                if (range.Find.Execute(FindText: fieldName))
                {
                    range.Select();
                    range.Text = value;
                }
            }
            catch { }
        }

        private void GenerateContractBt_Click(object sender, RoutedEventArgs e)
        {
            GenerateWeddingContract();
        }

        private void ExitBt_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new HomePage(UserInfo.User));
        }

        private void SeatingNavigateBt_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new SeatingPage());
        }
    }
}
