using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ZhiganshinaMilana420_MarryMe.DB;
using ZhiganshinaMilana420_MarryMe.Windows;
using Path = System.IO.Path;



namespace ZhiganshinaMilana420_MarryMe.Pages
{

    /// <summary>
    /// Логика взаимодействия для HomePage.xaml
    /// </summary>
    /// 
    public class TaskDateDecorator : Decorator
    {


        private static readonly HashSet<DateTime> _datesWithTasks = new HashSet<DateTime>();

        public static void UpdateDatesWithTasks(IEnumerable<DateTime> dates)
        {
            _datesWithTasks.Clear();
            foreach (var date in dates)
            {
                _datesWithTasks.Add(date.Date);
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (Parent is CalendarDayButton button && button.DataContext is DateTime date)
            {
                var hasTasks = _datesWithTasks.Contains(date.Date);
                button.SetValue(HasTasksProperty, hasTasks);
            }
        }

        public static readonly DependencyProperty HasTasksProperty =
            DependencyProperty.RegisterAttached(
                "HasTasks",
                typeof(bool),
                typeof(CalendarDayButton),
                new PropertyMetadata(false));

        public static bool GetHasTasks(CalendarDayButton button)
        {
            return (bool)button.GetValue(HasTasksProperty);
        }

        public static void SetHasTasks(CalendarDayButton button, bool value)
        {
            button.SetValue(HasTasksProperty, value);
        }
    }
    public partial class HomePage : Page
    {
        public static List<Users> admin { get; set; }
        public static List<Couple> couples { get; set; }
        public static List<Gromm> gromms { get; set; }
        public static List<Bride> brides { get; set; }
        public static List<TaskUsers> taskUsers { get; set; }
        Users contextUsers;

        private int currentCouplePage = 1;
        private const int couplesPerPage = 4; // Количество пар на странице
        private int totalCouplePages;
        private List<Couple> allCouples = new List<Couple>();
        public HomePage(Users users)
        {
            InitializeComponent();
            contextUsers = users;

            DateTime today = DateTime.Today;

            // Загружаем все пары
            allCouples = DbConnection.MarryMe.Couple
                .Where(c => c.WeddingStatusId == 1)
                .OrderBy(c => c.WeddingDate)
                .ToList();

            // Инициализируем пагинацию
            InitializeCouplePagination();

            gromms = DbConnection.MarryMe.Gromm.ToList();
            taskUsers = DbConnection.MarryMe.TaskUsers
                .Where(i => i.UserId == users.Id && i.DateEnd == today).ToList();
            TaskUserLV.ItemsSource = taskUsers;
            DateTaskDp.Text = today.ToString("dd.MM.yyyy");

            brides = new List<Bride>(DbConnection.MarryMe.Bride.ToList());
            this.DataContext = this;

            if (users.RoleId == 2)
            {
                AddClientBtt.Visibility = Visibility.Visible;
            }
            else
            {
                AddClientBtt.Visibility = Visibility.Hidden;
            }
            Loaded += (s, e) => UpdateEmptyTaskImageVisibility();
        }


        private void DateTaskDp_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DateTaskDp.SelectedDate == null)
            {
                return;
            }
            DateTime selectedDate = DateTaskDp.SelectedDate ?? DateTime.MinValue;

            var filteredItems = DbConnection.MarryMe.TaskUsers
                .Where(i => i.DateEnd == selectedDate && i.UserId == contextUsers.Id).ToList();
            TaskUserLV.ItemsSource = filteredItems;

            UpdateEmptyTaskImageVisibility();
        }
        private void SearchTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            Refresh();
        }
        private void CoupleLV_Loaded(object sender, RoutedEventArgs e)
        {
            // Для каждого элемента в ListView
            foreach (var item in CoupleLV.Items)
            {
                // Находим контейнер элемента
                var container = CoupleLV.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
                if (container != null)
                {
                    // Находим кнопку в шаблоне
                    var button = FindVisualChild<Button>(container, "OpenContractBtn");
                    if (button != null)
                    {
                        // Устанавливаем видимость в зависимости от наличия пути к договору
                        var couple = item as Couple;
                        button.Visibility = string.IsNullOrEmpty(couple?.ContractPath)
                            ? Visibility.Collapsed
                            : Visibility.Visible;
                    }
                }
            }
        }

        // Вспомогательный метод для поиска дочерних элементов
        private T FindVisualChild<T>(DependencyObject parent, string childName)
            where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result && (result as FrameworkElement)?.Name == childName)
                    return result;

                var descendant = FindVisualChild<T>(child, childName);
                if (descendant != null)
                    return descendant;
            }
            return null;
        }
        private void OpenContract_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Couple couple)
            {
                if (string.IsNullOrEmpty(couple.ContractPath))
                {
                    MessageBox.Show("Для этой пары договор еще не создан.\n" +
                                  "Перейдите в карточку пары для создания договора.",
                                  "Договор отсутствует",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                    return;
                }

                try
                {
                    if (File.Exists(couple.ContractPath))
                    {
                        Process.Start(new ProcessStartInfo(couple.ContractPath)
                        {
                            UseShellExecute = true
                        });
                    }
                    else
                    {
                        MessageBox.Show("Файл договора не найден по указанному пути.\n" +
                                      $"Путь: {couple.ContractPath}",
                                      "Ошибка",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при открытии договора:\n{ex.Message}",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
        }
        public void Refresh()
        {
            // Загружаем все пары с фильтрацией и сортировкой
            allCouples = DbConnection.MarryMe.Couple
                .Include(c => c.Gromm)
                .Include(c => c.Bride)
                .Include(c => c.WeddingStatus)
                .Where(c => c.WeddingStatusId == 1)
                .OrderBy(c => c.WeddingDate)
                .ToList();

            string searchText = SearchTb?.Text?.Trim().ToLower() ?? "";

            if (!string.IsNullOrEmpty(searchText))
            {
                var searchTerms = searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                allCouples = allCouples.Where(c =>
                    c != null &&
                    c.Gromm != null &&
                    c.Bride != null &&
                    searchTerms.All(term =>
                        (c.Gromm.Surname != null && c.Gromm.Surname.ToLower().Contains(term)) ||
                        (c.Gromm.Name != null && c.Gromm.Name.ToLower().Contains(term)) ||
                        (c.Bride.Surname != null && c.Bride.Surname.ToLower().Contains(term)) ||
                        (c.Bride.Name != null && c.Bride.Name.ToLower().Contains(term))))
                    .ToList();
            }

            // Обновляем пагинацию
            InitializeCouplePagination();
        }

        private void InitializeCouplePagination()
{
    // Вычисляем общее количество страниц
    totalCouplePages = (int)Math.Ceiling((double)allCouples.Count / couplesPerPage);
    
    // Очищаем панель пагинации
    CouplePaginationPanel.Children.Clear();
    CouplePaginationPanel.Children.Add(CouplePrevPageBtn);

    // Создаем кнопки для каждой страницы
    for (int i = 1; i <= totalCouplePages; i++)
    {
        var pageBtn = new Button
        {
            Content = i.ToString(),
            Width = 30,
            Height = 30,
            FontSize = 12,
            Margin = new Thickness(2, 0, 2, 0),
            Tag = i
        };
        pageBtn.Click += CouplePageBtn_Click;

        if (i == currentCouplePage)
        {
            pageBtn.Background = Brushes.LightGray;
        }

        CouplePaginationPanel.Children.Add(pageBtn);
    }

    CouplePaginationPanel.Children.Add(CoupleNextPageBtn);

    // Загружаем данные для текущей страницы
    LoadCouplePageData();
}

private void LoadCouplePageData()
{
    // Получаем пары для текущей страницы
    var displayedCouples = allCouples
        .Skip((currentCouplePage - 1) * couplesPerPage)
        .Take(couplesPerPage)
        .ToList();

    CoupleLV.ItemsSource = displayedCouples;

    // Обновляем состояние кнопок
    UpdateCouplePaginationButtons();

    // Обновляем видимость кнопок договоров
    if (CoupleLV.IsLoaded)
    {
        CoupleLV_Loaded(null, null);
    }
}

private void UpdateCouplePaginationButtons()
{
    foreach (var child in CouplePaginationPanel.Children)
    {
        if (child is Button btn && btn.Tag is int pageNumber)
        {
            btn.Background = pageNumber == currentCouplePage ? Brushes.LightGray : Brushes.Transparent;
        }
    }

    CouplePrevPageBtn.IsEnabled = currentCouplePage > 1;
    CoupleNextPageBtn.IsEnabled = currentCouplePage < totalCouplePages;
}

private void CouplePageBtn_Click(object sender, RoutedEventArgs e)
{
    if (sender is Button btn && btn.Tag is int pageNumber)
    {
        currentCouplePage = pageNumber;
        LoadCouplePageData();
    }
}

private void CouplePrevPageBtn_Click(object sender, RoutedEventArgs e)
{
    if (currentCouplePage > 1)
    {
        currentCouplePage--;
        LoadCouplePageData();
    }
}

private void CoupleNextPageBtn_Click(object sender, RoutedEventArgs e)
{
    if (currentCouplePage < totalCouplePages)
    {
        currentCouplePage++;
        LoadCouplePageData();
    }
}

        private void CoupleLV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(CoupleLV.SelectedItem is Couple couple)
            {
                couple = CoupleLV.SelectedItem as Couple;
                NavigationService.Navigate(new СoupleСardPage(couple));
            }
        }

        private void TaskUserCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is CheckBox checkBox)) return;
            if (!(checkBox.Tag is TaskUsers task)) return;

            // Проверяем, что дата выполнения задачи еще не прошла
            if (task.DateEnd < DateTime.Today)
            {
                // Отменяем изменение состояния
                checkBox.IsChecked = !checkBox.IsChecked;

                MessageBox.Show("Нельзя отмечать выполнение задач с прошедшей датой.",
                              "Ограничение",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Обновляем поле Done в базе данных
                task.Done = checkBox.IsChecked ?? false;
                DbConnection.MarryMe.SaveChanges();

                // Опционально: обновляем список, если нужно
                RefreshTaskList();
            }
            catch (Exception ex)
            {
                // В случае ошибки возвращаем состояние CheckBox
                checkBox.IsChecked = !checkBox.IsChecked;

                MessageBox.Show($"Ошибка при обновлении задачи: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }
        private void AddEmployyeBt_Click(object sender, RoutedEventArgs e)
        {
            AddEmployeeWindow addEmployeeWindow = new AddEmployeeWindow();
            addEmployeeWindow.ShowDialog();
        }

        private void AddTaskUsersBt_Click(object sender, RoutedEventArgs e)
        {
            AddTaskUserWindow addTaskUserWindow = new AddTaskUserWindow(contextUsers);
            addTaskUserWindow.Closed += (s, args) =>
            {
                RefreshTaskList();
                UpdateEmptyTaskImageVisibility();
            };
            addTaskUserWindow.ShowDialog();
        }

        public void RefreshTaskList()
        {
            if (DateTaskDp.SelectedDate != null)
            {
                DateTime selectedDate = DateTaskDp.SelectedDate ?? DateTime.Today;
                taskUsers = new List<TaskUsers>(DbConnection.MarryMe.TaskUsers
                    .Where(i => i.DateEnd == selectedDate && i.UserId == contextUsers.Id).ToList());
                TaskUserLV.ItemsSource = taskUsers;
            }
            else
            {
                DateTime today = DateTime.Today;
                taskUsers = new List<TaskUsers>(DbConnection.MarryMe.TaskUsers
                    .Where(i => i.DateEnd == today && i.UserId == contextUsers.Id).ToList());
                TaskUserLV.ItemsSource = taskUsers;
            }

            // Обновляем видимость изображения
            UpdateEmptyTaskImageVisibility();
        }
        private void UpdateEmptyTaskImageVisibility()
        {
            if (TaskUserLV.ItemsSource == null || !TaskUserLV.ItemsSource.Cast<object>().Any())
            {
                EmptyTaskImage.Visibility = Visibility.Visible;
                TaskUserLV.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmptyTaskImage.Visibility = Visibility.Collapsed;
                TaskUserLV.Visibility = Visibility.Visible;
            }
        }

        private void DelateBt_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is TaskUsers taskToDelete)
            {
                // Проверяем, что текущий пользователь - администратор задачи
                if (taskToDelete.AdminId != UserInfo.User.Id)
                {
                    MessageBox.Show("Вы можете удалять только задачи, которые создали сами.",
                          "Ограничение прав",
                          MessageBoxButton.OK,
                          MessageBoxImage.Warning);
                    return;
                }

                // Проверяем, что дата задачи еще не прошла
                if (!taskToDelete.IsTaskActive) // Используем наше свойство
                {
                    MessageBox.Show("Удаление задач с прошедшей датой запрещено.",
                          "Ограничение",
                          MessageBoxButton.OK,
                          MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    MessageBoxResult result = MessageBox.Show("Вы точно хотите удалить эту задачу?",
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        DbConnection.MarryMe.TaskUsers.Remove(taskToDelete);
                        DbConnection.MarryMe.SaveChanges();
                        RefreshTaskList();
                        MessageBox.Show("Задача успешно удалена!",
                            "Информация",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        RefreshTaskList();
                        UpdateEmptyTaskImageVisibility();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось удалить задачу: {ex.Message}",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void TaskUserLV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Проверяем, что выбран элемент
            if (TaskUserLV.SelectedItem is TaskUsers selectedTask)
            {
                // Проверяем права на редактирование
                if (selectedTask.AdminId != UserInfo.User.Id)
                {
                    MessageBox.Show("Вы можете редактировать только задачи, которые создали сами.",
                          "Ограничение прав",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);
                }
                else
                {
                    // Проверяем, что дата задачи еще не прошла
                    if (selectedTask.DateEnd < DateTime.Today)
                    {
                        MessageBox.Show("Редактирование задач с прошедшей датой запрещено.",
                              "Ограничение",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                    }
                    else
                    {
                        // Создаем окно редактирования только если дата не прошла
                        EditTaskUsersWindow editTaskUsersWindow = new EditTaskUsersWindow(contextUsers, selectedTask);

                        // Подписываемся на событие закрытия окна
                        editTaskUsersWindow.Closed += (s, args) =>
                        {
                            RefreshTaskList(); // Обновляем список при закрытии окна
                        };

                        editTaskUsersWindow.ShowDialog();
                    }
                }

                // Сбрасываем выделение
                TaskUserLV.SelectedItem = null;
            }
        }
        private void MaterialsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Couple couple)
            {
                try
                {
                    // Путь к базовой папке на рабочем столе
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    string baseFolderPath = Path.Combine(desktopPath, "Материалы MerryMe");

                    // Создаем базовую папку, если ее нет
                    if (!Directory.Exists(baseFolderPath))
                    {
                        Directory.CreateDirectory(baseFolderPath);
                    }

                    // Формируем имя папки для пары
                    string coupleFolderName = $"{couple.Gromm.Surname} {couple.Gromm.Name.Substring(0, 1)}.{couple.Gromm.Patronymic.Substring(0, 1)}. и {couple.Bride.Surname} {couple.Bride.Name.Substring(0, 1)}.{couple.Bride.Patronymic.Substring(0, 1)}.";
                    string coupleFolderPath = Path.Combine(baseFolderPath, coupleFolderName);

                    // Создаем папку для пары, если ее нет
                    if (!Directory.Exists(coupleFolderPath))
                    {
                        Directory.CreateDirectory(coupleFolderPath);
                    }

                    // Получаем путь к исполняемой папке (bin/Debug или bin/Release)
                    string appDirectory = AppDomain.CurrentDomain.BaseDirectory;

                    // Добавим вывод в отладку для проверки пути
                    Debug.WriteLine($"Ищем файлы в: {appDirectory}");

                    // Список файлов для копирования (проверяем разные варианты расширений)
                    List<string> sourceFiles = new List<string>();
                    string[] possibleFiles = { "Заметки", "Заметки.txt", "Заметки.docx",
                                     "Сценарий", "Сценарий.txt", "Сценарий.docx" };

                    // Ищем файлы в папке приложения
                    foreach (var file in possibleFiles)
                    {
                        string fullPath = Path.Combine(appDirectory, file);
                        if (File.Exists(fullPath))
                        {
                            sourceFiles.Add(fullPath);
                            Debug.WriteLine($"Найден файл для копирования: {fullPath}");
                        }
                    }

                    // Копируем найденные файлы
                    if (sourceFiles.Any())
                    {
                        foreach (var sourceFile in sourceFiles)
                        {
                            string fileName = Path.GetFileName(sourceFile);
                            string destFile = Path.Combine(coupleFolderPath, fileName);

                            try
                            {
                                File.Copy(sourceFile, destFile, true);
                                Debug.WriteLine($"Скопирован файл: {sourceFile} -> {destFile}");
                            }
                            catch (Exception copyEx)
                            {
                                Debug.WriteLine($"Ошибка копирования {sourceFile}: {copyEx.Message}");
                            }
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Не найдены файлы для копирования!");
                        MessageBox.Show("Шаблонные документы не найдены в папке приложения.",
                                        "Информация",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Information);
                    }

                    // Обновляем путь в базе данных
                    couple.MaterialsPath = coupleFolderPath;
                    DbConnection.MarryMe.SaveChanges();

                    // Открываем папку в проводнике
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = coupleFolderPath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при работе с материалами: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
        }
        private void AddClientBtt_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddClientPage());
        }

        //private void EditUserBtt_Click(object sender, RoutedEventArgs e)
        //{
        //    NavigationService.Navigate(new EditUserPage(UserInfo.User));
        //}
    }
}
