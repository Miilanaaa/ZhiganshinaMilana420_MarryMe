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
            public int Price { get; set; }
            public string Date { get; set; }
            public Visibility CancelButtonVisibility { get; set; }
            public Visibility AdminButtonsVisibility { get; set; }
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
                WeddingBudgetTb.Text = couple != null ? $"{couple.WeddingBudget} руб." : "Бюджет не указан";
                NumberGuestsTb.Text = couple != null ? $"{couple.NumberGuests}" : "Количество не указано";
                WeddingDateTb.Text = couple != null && couple.WeddingDate.HasValue ?
                                     couple.WeddingDate.Value.ToString("dd.MM.yyyy") :
                                      "Дата не указана";

                LoadFavoriteItems();
            }
            catch (Exception ex)
            {                   
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");

            }

            if (UserInfo.User.RoleId == 2)
            {
                SeatingNavigateBt.Visibility = Visibility.Visible;
                GenerateContractBt.Visibility = Visibility.Visible;

            }
            else
            {
                SeatingNavigateBt.Visibility = Visibility.Hidden;
                GenerateContractBt.Visibility = Visibility.Hidden;
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

                bool isAdmin = UserInfo.User?.RoleId == 2; // Проверяем роль пользователя
                var adminButtonsVisibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;

                CoupleFavorites = DbConnection.MarryMe.CoupleFavorites
                    .FirstOrDefault(c => c.CoupleId == contextCouple.Id) ?? new CoupleFavorites();

                // Ресторан
                var restaurant = DbConnection.MarryMe.Restaurant
                    .FirstOrDefault(r => r.Id == CoupleFavorites.RestaurantId);
                var restaurantBooking = DbConnection.MarryMe.RestaurantBookingDates
                    .FirstOrDefault(b => b.CoupleId == contextCouple.Id &&
                               b.RestaurantId == CoupleFavorites.RestaurantId &&
                               b.Status == true);
                string restaurantDate = "Не забронирован";
                int restaurantPrice = 0;
                int restaurantMenuPrice = 0;
                int restaurantFinalPrice = 0;
                if (restaurantBooking != null)
                {
                    restaurantDate = restaurantBooking.BookingDate.ToString("dd.MM.yyyy");
                    restaurantPrice = restaurant != null ? (int)restaurant.Price : 0;
                    restaurantMenuPrice = restaurantBooking != null ? (int)restaurantBooking.MenuPrice : 0;
                    restaurantFinalPrice = restaurantPrice + (restaurantMenuPrice * (contextCouple.NumberGuests ?? 0));
                }

                // Декорации
                var decoration = DbConnection.MarryMe.Decoration
                    .FirstOrDefault(r => r.Id == CoupleFavorites.DecorationId);
                string decorationBooking = "Не забронирован";
                int decorationPrice = 0;
                if (CoupleFavorites.DecorationId != null)
                {
                    decorationBooking = "Выбрано";
                    decorationPrice = decoration != null ? (int)decoration.Price : 0;
                }

                // Свадебное платье
                var dress = DbConnection.MarryMe.Dress
                    .FirstOrDefault(d => d.Id == CoupleFavorites.DressBriedId);
                string dressBooking = "Не забронирован";
                int dressPrice = 0;
                if (CoupleFavorites.DressBriedId != null)
                {
                    dressBooking = "Выбрано";
                    dressPrice = dress != null ? (int)dress.Price : 0;
                }

                // Смокинг
                var clothingGromm = DbConnection.MarryMe.Clothing
                    .FirstOrDefault(c => c.Id == CoupleFavorites.ClothingGrommId);
                string clothingBooking = "Не забронирован";
                int clothingPrice = 0;
                if (CoupleFavorites.ClothingGrommId != null)
                {
                    clothingBooking = "Выбрано";
                    clothingPrice = clothingGromm != null ? (int)clothingGromm.Price : 0;
                }

                // Ведущий
                var host = DbConnection.MarryMe.Host
                    .FirstOrDefault(h => h.Id == CoupleFavorites.HostId);
                var hostBooking = DbConnection.MarryMe.HostBookingDates
                    .FirstOrDefault(b => b.CoupleId == contextCouple.Id &&
                               b.HostId == CoupleFavorites.HostId &&
                               b.Status == true);
                string hostDate = "Не забронирован";
                int hostPrice = 0;
                if (hostBooking != null)
                {
                    hostDate = hostBooking.BookingDate.ToString("dd.MM.yyyy");
                    hostPrice = host != null ? (int)host.Price : 0;
                }

                // Фото и видео
                var PhotographerVideographer = DbConnection.MarryMe.PhotographerVideographer
                    .FirstOrDefault(p => p.Id == CoupleFavorites.PhotographerVideographerId);
                var PhotographerVideographerBooking = DbConnection.MarryMe.PhotographerVideographerBookingDates
                    .FirstOrDefault(b => b.CoupleId == contextCouple.Id &&
                               b.PhotographerVideographerId == CoupleFavorites.PhotographerVideographerId &&
                               b.Status == true);
                string PhotographerVideographerDate = "Не забронирован";
                int photographerPrice = 0;
                if (PhotographerVideographerBooking != null)
                {
                    PhotographerVideographerDate = PhotographerVideographerBooking.BookingDate.ToString("dd.MM.yyyy");
                    photographerPrice = PhotographerVideographer != null ? (int)PhotographerVideographer.Price : 0;
                }

                // Букет невесты
                var Bouquet = DbConnection.MarryMe.Bouquet
                    .FirstOrDefault(b => b.Id == CoupleFavorites.BouquetId);
                string BouquetBooking = "Не забронирован";
                int bouquetPrice = 0;
                if (CoupleFavorites.BouquetId != null)
                {
                    BouquetBooking = "Выбрано";
                    bouquetPrice = Bouquet != null ? (int)Bouquet.Price : 0;
                }

                // Стилист
                var Stylist = DbConnection.MarryMe.Stylist
                    .FirstOrDefault(s => s.Id == CoupleFavorites.StylistId);
                var StylistBooking = DbConnection.MarryMe.StylistBookingDates
                    .FirstOrDefault(b => b.CoupleId == contextCouple.Id &&
                               b.StylistId == CoupleFavorites.StylistId &&
                               b.Status == true);
                string StylistDate = "Не забронирован";
                int stylistPrice = 0;
                if (StylistBooking != null)
                {
                    StylistDate = StylistBooking.BookingDate.ToString("dd.MM.yyyy");
                    stylistPrice = Stylist != null ? (int)Stylist.Price : 0;
                }

                // Музыкальная группа
                var Musician = DbConnection.MarryMe.Musician
                    .FirstOrDefault(m => m.Id == CoupleFavorites.MusicianId);
                var MusicianBooking = DbConnection.MarryMe.MusicianBookingDates
                    .FirstOrDefault(b => b.CoupleId == contextCouple.Id &&
                               b.MusicianId == CoupleFavorites.MusicianId &&
                               b.Status == true);
                string MusicianDate = "Не забронирован";
                int musicianPrice = 0;
                if (MusicianBooking != null)
                {
                    MusicianDate = MusicianBooking.BookingDate.ToString("dd.MM.yyyy");
                    musicianPrice = Musician != null ? (int)Musician.Price : 0;
                }

                // Трансфер
                var Transfer = DbConnection.MarryMe.Transfer
                    .FirstOrDefault(t => t.Id == CoupleFavorites.TransferId);
                var TransferBooking = DbConnection.MarryMe.TransferBookingDates
                    .FirstOrDefault(b => b.CoupleId == contextCouple.Id &&
                               b.TransferId == CoupleFavorites.TransferId &&
                               b.Status == true);
                string TransferDate = "Не забронирован";
                int transferPrice = 0;
                if (TransferBooking != null)
                {
                    TransferDate = TransferBooking.BookingDate.ToString("dd.MM.yyyy");
                    transferPrice = Transfer != null ? (int)Transfer.Price : 0;
                }

                // Торт
                var Cake = DbConnection.MarryMe.Cake
                    .FirstOrDefault(c => c.Id == CoupleFavorites.CakeId);
                string CakeBooking = "Не забронирован";
                int cakePrice = 0;
                if (CoupleFavorites.CakeId != null)
                {
                    CakeBooking = "Выбрано";
                    cakePrice = Cake != null ? (int)Cake.Price : 0;
                }

                // Ювелирные украшения
                var Accessory = DbConnection.MarryMe.Accessory
                    .FirstOrDefault(c => c.Id == CoupleFavorites.AccessoryId);
                string AccessoryBooking = "Не забронирован";
                int accessoryPrice = 0;
                if (CoupleFavorites.AccessoryId != null)
                {
                    AccessoryBooking = "Выбрано";
                    accessoryPrice = Accessory != null ? (int)Accessory.Price : 0;
                }

                var items = new List<CategoryItem>
                {
                    new CategoryItem
                    {
                        Number = 1,
                        Category = "Ресторан",
                        Id = restaurant != null ? restaurant.Name : "Не выбран",
                        Price = restaurantFinalPrice,
                        Date = restaurantDate,
                        CancelButtonVisibility = (restaurant != null || restaurantBooking != null) ?
                            Visibility.Visible : Visibility.Collapsed,
                        AdminButtonsVisibility = adminButtonsVisibility
                    },
                    new CategoryItem
                    {
                        Number = 2,
                        Category = "Декорации",
                        Id = decoration != null ? decoration.Name : "Не выбран",
                        Price = decorationPrice,
                        Date = decorationBooking,
                        CancelButtonVisibility = decoration != null ?
                            Visibility.Visible : Visibility.Collapsed,
                        AdminButtonsVisibility = adminButtonsVisibility
                    },
                    new CategoryItem
                    {
                        Number = 3,
                        Category = "Свадебное платье",
                        Id = dress != null ? dress.Name : "Не выбран",
                        Price = dressPrice,
                        Date = dressBooking,
                        CancelButtonVisibility = dress != null ?
                            Visibility.Visible : Visibility.Collapsed,
                        AdminButtonsVisibility = adminButtonsVisibility
                    },
                    new CategoryItem
                    {
                        Number = 4,
                        Category = "Смокинг",
                        Id = clothingGromm != null ? clothingGromm.Name : "Не выбран",
                        Price = clothingPrice,
                        Date = clothingBooking,
                        CancelButtonVisibility = clothingGromm != null ?
                            Visibility.Visible : Visibility.Collapsed,
                        AdminButtonsVisibility = adminButtonsVisibility
                    },
                    new CategoryItem
                    {
                        Number = 5,
                        Category = "Ведущий",
                        Id = host != null ? host.Surname + " " + host.Name + " " + host.Patronymic : "Не выбран",
                        Price = hostPrice,
                        Date = hostDate,
                        CancelButtonVisibility = (host != null || hostBooking != null) ?
                            Visibility.Visible : Visibility.Collapsed,
                        AdminButtonsVisibility = adminButtonsVisibility
                    },
                    new CategoryItem
                    {
                        Number = 6,
                        Category = "Фото и видео",
                        Id = PhotographerVideographer != null ? PhotographerVideographer.TeamName : "Не выбран",
                        Price = photographerPrice,
                        Date = PhotographerVideographerDate,
                        CancelButtonVisibility = (PhotographerVideographer != null || PhotographerVideographerBooking != null) ?
                            Visibility.Visible : Visibility.Collapsed,
                        AdminButtonsVisibility = adminButtonsVisibility
                    },
                    new CategoryItem
                    {
                        Number = 7,
                        Category = "Букет невесты",
                        Id = Bouquet != null ? Bouquet.Name : "Не выбран",
                        Price = bouquetPrice,
                        Date = BouquetBooking,
                        CancelButtonVisibility = Bouquet != null ?
                            Visibility.Visible : Visibility.Collapsed,
                        AdminButtonsVisibility = adminButtonsVisibility
                    },
                    new CategoryItem
                    {
                        Number = 8,
                        Category = "Стилист",
                        Id = Stylist != null ? Stylist.TeamName : "Не выбран",
                        Price = stylistPrice,
                        Date = StylistDate,
                        CancelButtonVisibility = (Stylist != null || StylistBooking != null) ?
                            Visibility.Visible : Visibility.Collapsed,
                        AdminButtonsVisibility = adminButtonsVisibility
                    },
                    new CategoryItem
                    {
                        Number = 9,
                        Category = "Музыкальная группа",
                        Id = Musician != null ? Musician.TeamName : "Не выбран",
                        Price = musicianPrice,
                        Date = MusicianDate,
                        CancelButtonVisibility = (Musician != null || MusicianBooking != null) ?
                            Visibility.Visible : Visibility.Collapsed,
                        AdminButtonsVisibility = adminButtonsVisibility
                    },
                    new CategoryItem
                    {
                        Number = 10,
                        Category = "Трансфер",
                        Id = Transfer != null ? Transfer.Name : "Не выбран",
                        Price = transferPrice,
                        Date = TransferDate,
                        CancelButtonVisibility = (Transfer != null || TransferBooking != null) ?
                            Visibility.Visible : Visibility.Collapsed,
                        AdminButtonsVisibility = adminButtonsVisibility
                    },
                    new CategoryItem
                    {
                        Number = 11,
                        Category = "Торт",
                        Id = Cake != null ? Cake.Name : "Не выбран",
                        Price = cakePrice,
                        Date = CakeBooking,
                        CancelButtonVisibility = Cake != null ?
                            Visibility.Visible : Visibility.Collapsed,
                        AdminButtonsVisibility = adminButtonsVisibility
                    },
                    new CategoryItem
                    {
                        Number = 12,
                        Category = "Ювелирные украшения",
                        Id = Accessory != null ? Accessory.Name : "Не выбран",
                        Price = accessoryPrice,
                        Date = AccessoryBooking,
                        CancelButtonVisibility = Accessory != null ?
                            Visibility.Visible : Visibility.Collapsed,
                        AdminButtonsVisibility = adminButtonsVisibility
                    },
                };

                FavoritClientLv.ItemsSource = items;
                int totalPrice = items.Sum(item => item.Price);
                FinalPriceTb.Text = totalPrice.ToString("N0") + " руб.";
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
                                    itemToUpdate.Price = 0;
                                    itemToUpdate.CancelButtonVisibility = Visibility.Collapsed;
                                }
                                break;

                            case 2: // Декорации
                                if (CoupleFavorites.DecorationId != null)
                                {
                                    CoupleFavorites.DecorationId = null;
                                    itemToUpdate.Id = "Не выбран";
                                    itemToUpdate.Date = "Не забронирован";
                                    itemToUpdate.Price = 0;
                                    itemToUpdate.CancelButtonVisibility = Visibility.Collapsed;
                                }
                                break;

                            case 3: // Свадебное платье
                                if (CoupleFavorites.DressBriedId != null)
                                {
                                    CoupleFavorites.DressBriedId = null;
                                    itemToUpdate.Id = "Не выбран";
                                    itemToUpdate.Date = "Не забронирован";
                                    itemToUpdate.Price = 0;
                                    itemToUpdate.CancelButtonVisibility = Visibility.Collapsed;
                                }
                                break;

                            case 4: // Смокинг
                                if (CoupleFavorites.ClothingGrommId != null)
                                {
                                    CoupleFavorites.ClothingGrommId = null;
                                    itemToUpdate.Id = "Не выбран";
                                    itemToUpdate.Date = "Не забронирован";
                                    itemToUpdate.Price = 0;
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
                                    itemToUpdate.Price = 0;
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
                                    itemToUpdate.Price = 0;
                                    itemToUpdate.CancelButtonVisibility = Visibility.Collapsed;
                                }
                                break;

                            case 7: // Букет невесты
                                if (CoupleFavorites.BouquetId != null)
                                {
                                    CoupleFavorites.BouquetId = null;
                                    itemToUpdate.Id = "Не выбран";
                                    itemToUpdate.Date = "Не забронирован";
                                    itemToUpdate.Price = 0;
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
                                    itemToUpdate.Price = 0;
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
                                    itemToUpdate.Price = 0;
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
                                    itemToUpdate.Price = 0;
                                    itemToUpdate.CancelButtonVisibility = Visibility.Collapsed;
                                }
                                break;

                            case 11: // Торт
                                if (CoupleFavorites.CakeId != null)
                                {
                                    CoupleFavorites.CakeId = null;
                                    itemToUpdate.Id = "Не выбран";
                                    itemToUpdate.Date = "Не забронирован";
                                    itemToUpdate.Price = 0;
                                    itemToUpdate.CancelButtonVisibility = Visibility.Collapsed;
                                }
                                break;

                            case 12: // Ювелирные украшения
                                if (CoupleFavorites.AccessoryId != null)
                                {
                                    CoupleFavorites.AccessoryId = null;
                                    itemToUpdate.Id = "Не выбран";
                                    itemToUpdate.Date = "Не забронирован";
                                    itemToUpdate.Price = 0;
                                    itemToUpdate.CancelButtonVisibility = Visibility.Collapsed;
                                }
                                break;
                        }

                        // Сохраняем изменения в базе данных
                        DbConnection.MarryMe.SaveChanges();

                        // Обновляем отображение ListView
                        FavoritClientLv.Items.Refresh();
                        int totalPrice = items.Sum(item => item.Price);
                        FinalPriceTb.Text = totalPrice.ToString("N0") + " руб.";

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
                var prise = FinalPriceTb.Text;

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

                FillField(document, "фио_подписи_жениха", groomSignatureName);
                FillField(document, "фио_подписи_невесты", briedSignatureName);

                FillField(document, "итого_сумма", FinalPriceTb.Text);
                FillField(document, "итоговая_сумма_договора", FinalPriceTb.Text);
                if (decimal.TryParse(FinalPriceTb.Text, out decimal totalPrice))
                {
                    decimal advanceAmount = totalPrice * 0.5m; // Вычисляем 50% от суммы
                    FillField(document, "аванс_договора", advanceAmount.ToString("N2")); // Форматируем с 2 знаками после запятой
                }
                else
                {
                    FillField(document, "аванс_договора", "0,00"); // Значение по умолчанию при ошибке
                }

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
                            row.Cells[4].Range.Text = Convert.ToString(item.Price);
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
