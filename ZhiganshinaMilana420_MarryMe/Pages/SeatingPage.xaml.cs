using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Xml;
using ZhiganshinaMilana420_MarryMe.Windows;
using Formatting = Newtonsoft.Json.Formatting;

namespace ZhiganshinaMilana420_MarryMe.Pages
{
    /// <summary>
    /// Логика взаимодействия для SeatingPage.xaml
    /// </summary>
    public partial class SeatingPage : Page
    {
        // Текущая выбранная фигура для перетаскивания
        private Shape selectedShape;
        // Смещение курсора относительно фигуры при начале перетаскивания
        private Point offset;
        // Флаг, указывающий что идет процесс перетаскивания
        private bool isDragging;
        private FrameworkElement selectedElement; // Вместо private Shape selectedShape;
        private const int SeatSize = 60; // Увеличили размер в 2 раза (было 18)
        private const int SeatMargin = 2;
        // Создаем класс для хранения данных о фигуре
        // 1. Обновленный класс ShapeTag
        private class ShapeTag
        {
            public Shape Shape { get; set; }
            public TextBlock TextBlock { get; set; }
            public bool IsTable { get; set; }
            public int SeatsCount { get; set; } = 8;
        }



        // Конструктор страницы
        public SeatingPage()
        {
            InitializeComponent(); // Инициализация компонентов XAML
        }

        // Обработчик клика по кнопке "Добавить прямоугольник"
        private void AddRectangle_Click(object sender, RoutedEventArgs e)
        {
            // Создание нового прямоугольника
            var rectangle = new Rectangle
            {
                Width = 100,     // Ширина прямоугольника
                Height = 60,     // Высота прямоугольника
                Fill = Brushes.LightBlue,  // Цвет заливки
                Stroke = Brushes.Black,     // Цвет границы
                StrokeThickness = 1,        // Толщина границы
                Margin = new Thickness(10)  // Отступы
            };

            // Добавление прямоугольника на холст
            AddShapeToCanvas(rectangle);
        }

        // Обработчик клика по кнопке "Добавить эллипс"
        private void AddEllipse_Click(object sender, RoutedEventArgs e)
        {
            // Создание нового эллипса
            var ellipse = new Ellipse
            {
                Width = 60,      // Ширина эллипса
                Height = 60,     // Высота эллипса
                Fill = Brushes.LightGreen,  // Цвет заливки
                Stroke = Brushes.Black,    // Цвет границы
                StrokeThickness = 1,        // Толщина границы
                Margin = new Thickness(10)  // Отступы
            };

            // Добавление эллипса на холст
            AddShapeToCanvas(ellipse);
        }

        private void AddTable_Click(object sender, RoutedEventArgs e)
        {
            // Сначала создаем текстовый блок
            var textBlock = new TextBlock
            {
                Text = $"Стол {DrawingCanvas.Children.Count + 1}",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Bold
            };

            // Затем создаем стол
            var table = new Rectangle
            {
                Width = 250,
                Height = 250,
                Fill = Brushes.LightBlue,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                RadiusX = 5,
                RadiusY = 5
            };

            // Создаем контейнер
            var tableContainer = new Grid();

            // Добавляем элементы в правильном порядке:
            tableContainer.Children.Add(table);          // 1. Сам стол
            AddSeatCircles(tableContainer, table.Width, table.Height, 8); // 2. Сиденья
            tableContainer.Children.Add(textBlock);      // 3. Текст
            tableContainer.Children.Add(CreateDeleteButton(tableContainer)); // 4. Кнопка удаления
            tableContainer.Children.Add(CreateTableEditButton(table, textBlock)); // 5. Кнопка редактирования

            // Настраиваем перетаскивание
            SetupDrag(tableContainer);

            // Добавляем на холст
            Canvas.SetLeft(tableContainer, 20 + (DrawingCanvas.Children.Count * 10));
            Canvas.SetTop(tableContainer, 20 + (DrawingCanvas.Children.Count * 10));
            DrawingCanvas.Children.Add(tableContainer);
        }
        private Button CreateTableEditButton(Rectangle table, TextBlock textBlock)
        {
            var editButton = new Button
            {
                Content = "✏️",
                Width = 24,
                Height = 24,
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(0),
                Tag = new ShapeTag
                {
                    Shape = table,
                    TextBlock = textBlock,
                    IsTable = true,
                    SeatsCount = 8
                }
            };

            editButton.Click += (sender, e) =>
            {
                e.Handled = true;
                if (sender is Button button && button.Tag is ShapeTag tag)
                {
                    var editWindow = new EditTableWindow(
                        tag.TextBlock.Text,
                        tag.Shape.Width,
                        tag.Shape.Height,
                        tag.SeatsCount);

                    if (editWindow.ShowDialog() == true)
                    {
                        tag.TextBlock.Text = editWindow.ShapeName;
                        tag.Shape.Width = editWindow.ShapeWidth;
                        tag.Shape.Height = editWindow.ShapeHeight;
                        tag.SeatsCount = editWindow.SeatsCount;

                        if (button.Parent is Grid container)
                        {
                            AddSeatCircles(container, editWindow.ShapeWidth, editWindow.ShapeHeight, editWindow.SeatsCount);
                        }
                    }
                }
            };

            return editButton;
        }
        // 2. Исправленный метод AddSeatCircles (убрана рекурсия)
        private void AddSeatCircles(Grid container, double tableWidth, double tableHeight, int seatsCount)
        {
            // Удаляем старые сиденья если есть
            var oldSeats = container.Children.OfType<Ellipse>().ToList();
            foreach (var seat in oldSeats)
            {
                container.Children.Remove(seat);
            }

            int seatsPerSide = seatsCount / 4;

            for (int side = 0; side < 4; side++)
            {
                for (int i = 0; i < seatsPerSide; i++)
                {
                    double ratio = (i + 1) / (double)(seatsPerSide + 1);

                    var seat = new Ellipse
                    {
                        Width = SeatSize,
                        Height = SeatSize,
                        Fill = Brushes.Transparent,
                        Stroke = Brushes.Gray,
                        StrokeThickness = 1,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top
                    };

                    // Точное позиционирование сидений:
                    switch (side)
                    {
                        case 0: // Верхняя сторона
                                // Центр сиденья точно у верхнего края стола
                            seat.Margin = new Thickness(
                                tableWidth * ratio - SeatSize / 5, // X: равномерно по ширине
                                -SeatSize / 5,                    // Y: верхний край стола
                                0, 0);
                            break;

                        case 1: // Правая сторона
                                // Центр сиденья точно у правого края стола
                            seat.Margin = new Thickness(
                                tableWidth - SeatSize / 10,         // X: правый край стола
                                tableHeight * ratio - SeatSize / 10, // Y: равномерно по высоте
                                0, 0);
                            break;

                        case 2: // Нижняя сторона
                                // Центр сиденья точно у нижнего края стола
                            seat.Margin = new Thickness(
                                tableWidth * ratio - SeatSize / 11, // X: равномерно по ширине
                                tableHeight - SeatSize / 11,        // Y: нижний край стола
                                0, 0);
                            break;

                        case 3: // Левая сторона
                                // Центр сиденья точно у левого края стола
                            seat.Margin = new Thickness(
                                -SeatSize / 5,                     // X: левый край стола
                                tableHeight * ratio - SeatSize / 5, // Y: равномерно по высоте
                                0, 0);
                            break;
                    }

                    container.Children.Add(seat);
                }
            }
        }
        private Button CreateDeleteButton(FrameworkElement elementToRemove)
        {
            var deleteButton = new Button
            {
                Content = "🗑️",
                Width = 24,
                Height = 24,
                Margin = new Thickness(30, 5, 40, 0), // Правее кнопки редактирования
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Background = Brushes.Transparent,
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(0),
                Cursor = Cursors.Hand
            };

            deleteButton.Click += (sender, e) =>
            {
                if (elementToRemove != null && elementToRemove.Parent is Panel parent)
                {
                    parent.Children.Remove(elementToRemove);
                }
                e.Handled = true;
            };

            return deleteButton;
        }

        private Button CreateEditButton(Shape shape, TextBlock textBlock)
        {
            var editButton = new Button
            {
                Content = "✏️",
                Width = 20,
                Height = 20,
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(0),
                Tag = new { Shape = shape, TextBlock = textBlock } // Сохраняем оба элемента
            };
            // При создании кнопки редактирования:
            editButton.Tag = new ShapeTag { Shape = shape, TextBlock = textBlock };
            // Обработчик кнопки редактирования:
            editButton.Click += (sender, e) =>
            {
                e.Handled = true;
                if (sender is Button button && button.Tag is ShapeTag tag)
                {
                    if (tag.IsTable)
                    {
                        var editWindow = new EditTableWindow(
                            tag.TextBlock.Text,
                            tag.Shape.Width,
                            tag.Shape.Height,
                            tag.SeatsCount);

                        if (editWindow.ShowDialog() == true)
                        {
                            tag.TextBlock.Text = editWindow.ShapeName;
                            tag.Shape.Width = editWindow.ShapeWidth;
                            tag.Shape.Height = editWindow.ShapeHeight;
                            tag.SeatsCount = editWindow.SeatsCount;

                            if (button.Parent is Grid container)
                            {
                                var seats = container.Children.OfType<Ellipse>().ToList();
                                foreach (var seat in seats)
                                {
                                    container.Children.Remove(seat);
                                }

                                AddSeatCircles(container, editWindow.ShapeWidth, editWindow.ShapeHeight, editWindow.SeatsCount);
                            }
                        }
                    }
                    else
                    {
                        var editWindow = new EditShapeWindow(
                            tag.TextBlock.Text,
                            tag.Shape.Width,
                            tag.Shape.Height);

                        if (editWindow.ShowDialog() == true)
                        {
                            tag.TextBlock.Text = editWindow.ShapeName;
                            tag.Shape.Width = editWindow.ShapeWidth;
                            tag.Shape.Height = editWindow.ShapeHeight;
                        }
                    }
                }
            };

            return editButton;
        }

        private void SetupDrag(Grid element)
        {
            element.MouseLeftButtonDown += (sender, e) =>
            {
                if (e.OriginalSource is Button) return;

                selectedElement = element;
                offset = e.GetPosition(element);
                isDragging = true;
                element.CaptureMouse();
                element.Opacity = 0.7;
                e.Handled = true;
            };

            element.MouseLeftButtonUp += (sender, e) => EndDrag();
            element.MouseLeave += (sender, e) => EndDrag();
        }

        // Метод для добавления фигуры на холст
        private void AddShapeToCanvas(Shape shape)
        {
            // Создаем кнопку редактирования
            var editButton = new Button
            {
                Content = "✏️",
                Width = 20,
                Height = 20,
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(0),
                Tag = shape // Сохраняем ссылку на фигуру
            };

            // Обработчик кнопки редактирования
            editButton.Click += (sender, e) =>
            {
                e.Handled = true;
                if (sender is Button button && button.Tag is Shape shapeToEdit)
                {
                    var parentGrid = button.Parent as Grid;
                    // Изменили имя переменной с textBlock на shapeTextBlock
                    var shapeTextBlock = parentGrid?.Children.OfType<TextBlock>().FirstOrDefault();

                    if (shapeTextBlock != null)
                    {
                        var editWindow = new EditShapeWindow(
                            shapeTextBlock.Text,
                            shapeToEdit.Width,
                            shapeToEdit.Height);

                        if (editWindow.ShowDialog() == true)
                        {
                            shapeTextBlock.Text = editWindow.ShapeName;
                            shapeToEdit.Width = editWindow.ShapeWidth;
                            shapeToEdit.Height = editWindow.ShapeHeight;
                        }
                    }
                }
            };

            // Текстовый блок
            var textBlock = new TextBlock
            {
                Text = $"Фигура {DrawingCanvas.Children.Count + 1}",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Контейнер Grid
            var grid = new Grid();
            grid.Children.Add(shape);
            grid.Children.Add(textBlock);
            grid.Children.Add(editButton);

            // Добавляем кнопки (сначала удаление, потом редактирование)
            grid.Children.Add(CreateDeleteButton(grid));
            grid.Children.Add(CreateEditButton(shape, textBlock));
            // Обработчики для перетаскивания
            grid.MouseLeftButtonDown += (sender, e) =>
            {
                if (e.OriginalSource is Button) return;

                selectedElement = grid;
                offset = e.GetPosition(grid);
                isDragging = true;
                grid.CaptureMouse();
                grid.Opacity = 0.7;
                e.Handled = true;
            };

            grid.MouseLeftButtonUp += (sender, e) => EndDrag();
            grid.MouseLeave += (sender, e) => EndDrag();

            // Добавляем на холст
            Canvas.SetLeft(grid, 20 + (DrawingCanvas.Children.Count * 10));
            Canvas.SetTop(grid, 20 + (DrawingCanvas.Children.Count * 10));
            DrawingCanvas.Children.Add(grid);
        }

        // Обработчик нажатия левой кнопки мыши на фигуре
        private void Shape_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Проверяем, что отправитель события - FrameworkElement
            if (sender is FrameworkElement element)
            {
                // Получаем фигуру из родительского контейнера Grid
                selectedShape = element.Parent is Grid grid ? grid.Children[0] as Shape : null;

                if (selectedShape != null)
                {
                    // Запоминаем позицию курсора относительно фигуры
                    offset = e.GetPosition(selectedShape);

                    // Если есть родительский элемент, получаем позицию относительно него
                    var parent = VisualTreeHelper.GetParent(selectedShape) as UIElement;
                    if (parent != null)
                    {
                        offset = e.GetPosition(parent);
                    }

                    // Начинаем перетаскивание
                    isDragging = true;
                    // Захватываем мышь для фигуры
                    selectedShape.CaptureMouse();
                    // Устанавливаем прозрачность для визуального эффекта
                    selectedShape.Opacity = 0.7;
                    // Помечаем событие как обработанное
                    e.Handled = true;
                }
            }
        }

        // Обработчик движения мыши на холсте
        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && selectedElement != null)
            {
                var position = e.GetPosition(DrawingCanvas);
                Canvas.SetLeft(selectedElement, position.X - offset.X);
                Canvas.SetTop(selectedElement, position.Y - offset.Y);
            }
        }

        // Обработчик отпускания кнопки мыши на фигуре
        private void Shape_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            EndDrag(); // Завершаем перетаскивание
        }




        // Обработчик отпускания кнопки мыши на холсте
        private void DrawingCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            EndDrag();
        }

        // Обработчик выхода мыши за пределы холста
        private void DrawingCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            EndDrag();
        }

        // Метод для завершения перетаскивания
        private void EndDrag()
        {
            if (isDragging && selectedElement != null)
            {
                selectedElement.ReleaseMouseCapture();
                selectedElement.Opacity = 1;
                isDragging = false;
                selectedElement = null;
            }
        }

        // Обработчик двойного клика на фигуре (для удаления)
        private void Shape_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && !(element is Button))
            {
                var parent = element.Parent as Panel;
                if (parent != null)
                {
                    DrawingCanvas.Children.Remove(parent);
                }
            }
        }

        // Обработчик клика по кнопке "Очистить"
        private void ClearCanvas_Click(object sender, RoutedEventArgs e)
        {
            // Удаляем все фигуры с холста
            DrawingCanvas.Children.Clear();
        }

        private void ExitNavigate_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
