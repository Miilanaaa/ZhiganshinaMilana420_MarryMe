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
        private static readonly SolidColorBrush GuestColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E4C8BF"));
        private static readonly SolidColorBrush RectangleColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EDB69E"));
        private static readonly SolidColorBrush TableColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#909478"));
        private class TableInfo
        {
            public Grid TableContainer { get; set; }
            public Shape TableShape { get; set; }
            public TextBlock TableText { get; set; }
            public List<Ellipse> Seats { get; } = new List<Ellipse>();
            public int SeatsCount { get; set; } = 8;
        }

        private List<TableInfo> _tables = new List<TableInfo>();
        private FrameworkElement _selectedElement;
        private Point _offset;
        private bool _isDragging;
        private const int DefaultTableSize = 150;
        private const int SeatSize = 55;
        private const int SeatMargin = 40;

        public SeatingPage()
        {
            InitializeComponent();
        }

        private void AddRectangle_Click(object sender, RoutedEventArgs e)
        {
            var rectangle = new Rectangle
            {
                Width = 100,
                Height = 60,
                Fill = RectangleColor,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            // Создаем Grid для объединения прямоугольника и текста
            var contentGrid = new Grid();
            contentGrid.Children.Add(rectangle);

            var textBlock = new TextBlock
            {
                Text = $"Прямоугольник {DrawingCanvas.Children.Count + 1}", // Цифра в названии
                Style = (Style)FindResource("CenterTextStyle")
            };
            contentGrid.Children.Add(textBlock);

            // Основной контейнер
            var grid = new Grid
            {
                Style = (Style)FindResource("ElementContainerStyle")
            };
            grid.Children.Add(contentGrid);
            grid.Children.Add(CreateDeleteButton(grid));
            grid.Children.Add(CreateShapeEditButton(rectangle, textBlock));

            SetupDrag(grid);
            Panel.SetZIndex(grid, 1);

            Canvas.SetLeft(grid, 20 + (DrawingCanvas.Children.Count * 20));
            Canvas.SetTop(grid, 20 + (DrawingCanvas.Children.Count * 20));
            DrawingCanvas.Children.Add(grid);
        }

        private void AddEllipse_Click(object sender, RoutedEventArgs e)
        {
            var ellipse = new Ellipse
            {
                Width = 55,
                Height = 55,
                Fill = GuestColor,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Cursor = Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Top
            };

            var textBlock = new TextBlock
            {
                Text = $"Гость\n{DrawingCanvas.Children.Count + 1}",
                Style = (Style)FindResource("BottomTextStyle") // Используем стиль для текста внизу
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            Grid.SetRow(ellipse, 0);
            Grid.SetRow(textBlock, 1);

            grid.Children.Add(ellipse);
            grid.Children.Add(textBlock);
            grid.Children.Add(CreateDeleteButton(grid));
            grid.Children.Add(CreateShapeEditButton(ellipse, textBlock));

            SetupDrag(grid);
            Panel.SetZIndex(grid, 1);

            Canvas.SetLeft(grid, 20 + (DrawingCanvas.Children.Count * 20));
            Canvas.SetTop(grid, 20 + (DrawingCanvas.Children.Count * 20));
            DrawingCanvas.Children.Add(grid);
        }

        private void AddTable_Click(object sender, RoutedEventArgs e)
        {
            var table = new Ellipse
            {
                Width = DefaultTableSize,
                Height = DefaultTableSize,
                Fill = TableColor,
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                VerticalAlignment = VerticalAlignment.Top
            };

            var textBlock = new TextBlock
            {
                Text = $"Стол {DrawingCanvas.Children.Count + 1}", // Цифра в названии
                Style = (Style)FindResource("CenterTextStyle"),
                FontWeight = FontWeights.Bold
            };

            var grid = new Grid
            {
                Style = (Style)FindResource("ElementContainerStyle")
            };
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            Grid.SetRow(table, 0);
            Grid.SetRow(textBlock, 1);

            var tableInfo = new TableInfo
            {
                TableContainer = grid,
                TableShape = table,
                TableText = textBlock,
                SeatsCount = 6
            };
            _tables.Add(tableInfo);

            grid.Children.Add(table);
            AddSeatsAroundTable(grid, tableInfo, tableInfo.SeatsCount);
            grid.Children.Add(textBlock);
            grid.Children.Add(CreateDeleteButton(grid));
            grid.Children.Add(CreateTableEditButton(tableInfo));

            SetupDrag(grid);
            Panel.SetZIndex(grid, 0);

            Canvas.SetLeft(grid, 20 + (DrawingCanvas.Children.Count * 10));
            Canvas.SetTop(grid, 20 + (DrawingCanvas.Children.Count * 10));
            DrawingCanvas.Children.Add(grid);
        }

        private void AddSeatsAroundTable(Grid tableContainer, TableInfo tableInfo, int seatsCount)
        {
            // Удаляем старые места
            foreach (var seat in tableInfo.Seats)
            {
                tableContainer.Children.Remove(seat);
            }
            tableInfo.Seats.Clear();

            double tableRadius = Math.Max(tableInfo.TableShape.Width, tableInfo.TableShape.Height) / 2;
            double seatRadius = tableRadius + SeatSize / 2 + SeatMargin;
            double angleStep = 360.0 / seatsCount;

            for (int i = 0; i < seatsCount; i++)
            {
                double angle = angleStep * i;
                double radians = angle * Math.PI / 180;
                double x = seatRadius * Math.Cos(radians);
                double y = seatRadius * Math.Sin(radians);

                var seat = new Ellipse
                {
                    Width = SeatSize,
                    Height = SeatSize,
                    Fill = Brushes.Transparent,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 1,
                    Margin = new Thickness(x - SeatSize / 2, y - SeatSize / 20, -20, 0),
                    Tag = "seat"
                };

                tableInfo.Seats.Add(seat);
                tableContainer.Children.Add(seat);
            }
        }

        private Button CreateTableEditButton(TableInfo tableInfo)
        {
            var editButton = new Button
            {
                Content = "✏️",
                Width = 30,
                Height = 30,
                Margin = new Thickness(5, -35, 30, 0),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(0),
                Tag = tableInfo
            };

            editButton.Click += (sender, e) =>
            {
                e.Handled = true;
                if (sender is Button button && button.Tag is TableInfo info)
                {
                    var editWindow = new EditTableWindow(
                        info.TableText.Text, // Передаем полный текст, включая цифру
                        info.TableShape.Width,
                        info.TableShape.Height,
                        info.SeatsCount);

                    if (editWindow.ShowDialog() == true)
                    {
                        info.TableText.Text = editWindow.ShapeName; // Сохраняем полный текст
                        info.TableShape.Width = editWindow.ShapeWidth;
                        info.TableShape.Height = editWindow.ShapeHeight;
                        info.TableShape.Fill = TableColor;
                        info.SeatsCount = editWindow.SeatsCount;
                        AddSeatsAroundTable(info.TableContainer, info, info.SeatsCount);
                    }
                }
            };
            return editButton;
        }

        private Button CreateShapeEditButton(Shape shape, TextBlock textBlock)
        {
            var editButton = new Button
            {
                Content = "✏️",
                Width = 30,
                Height = 30,
                Margin = new Thickness(0, -35, 30, 0),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(0),
                Tag = new { Shape = shape, Text = textBlock, OriginalBrush = shape.Fill } // Сохраняем оригинальную заливку
            };

            editButton.Click += (sender, e) =>
            {
                e.Handled = true;
                if (sender is Button button && button.Tag != null)
                {
                    dynamic tag = button.Tag;
                    var editWindow = new EditShapeWindow(
                        tag.Text.Text, // Передаем полный текст, включая цифру
                        tag.Shape.Width,
                        tag.Shape.Height);

                    if (editWindow.ShowDialog() == true)
                    {
                        tag.Text.Text = editWindow.ShapeName; // Сохраняем полный текст
                        tag.Shape.Width = editWindow.ShapeWidth;
                        tag.Shape.Height = editWindow.ShapeHeight;
                        tag.Shape.Fill = tag.OriginalBrush;
                    }
                }
            };

            return editButton;
        }

        private Button CreateDeleteButton(FrameworkElement elementToRemove)
        {
            var deleteButton = new Button
            {
                Content = "🗑",
                Width = 30,
                Height = 30,
                Margin = new Thickness(60, -35, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Background = Brushes.Transparent,
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand
            };

            deleteButton.Click += (sender, e) =>
            {
                if (elementToRemove != null && elementToRemove.Parent is Panel parent)
                {
                    if (elementToRemove is Grid grid && _tables.Any(t => t.TableContainer == grid))
                    {
                        var tableInfo = _tables.First(t => t.TableContainer == grid);
                        _tables.Remove(tableInfo);
                    }
                    parent.Children.Remove(elementToRemove);
                }
                e.Handled = true;
            };

            return deleteButton;
        }

        private void ClearCanvas_Click(object sender, RoutedEventArgs e)
        {
            DrawingCanvas.Children.Clear();
            _tables.Clear();
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    printDialog.PrintVisual(DrawingCanvas, "Макет рассадки");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при печати: {ex.Message}");
            }
        }

        private void SetupDrag(FrameworkElement element)
        {
            element.MouseLeftButtonDown += (sender, e) =>
            {
                if (e.OriginalSource is Button) return;

                _selectedElement = element;
                _offset = e.GetPosition(element);
                _isDragging = true;
                element.CaptureMouse();
                element.Opacity = 0.7;
                e.Handled = true;
            };

            element.MouseMove += (sender, e) =>
            {
                if (_isDragging && _selectedElement == element)
                {
                    var position = e.GetPosition(DrawingCanvas);
                    Canvas.SetLeft(element, position.X - _offset.X);
                    Canvas.SetTop(element, position.Y - _offset.Y);
                }
            };

            element.MouseLeftButtonUp += (sender, e) =>
            {
                if (_isDragging)
                {
                    element.ReleaseMouseCapture();
                    element.Opacity = 1;
                    _isDragging = false;
                    _selectedElement = null;
                }
            };
        }

        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && _selectedElement != null)
            {
                var position = e.GetPosition(DrawingCanvas);
                Canvas.SetLeft(_selectedElement, position.X - _offset.X);
                Canvas.SetTop(_selectedElement, position.Y - _offset.Y);
            }
        }

        private void DrawingCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _selectedElement?.ReleaseMouseCapture();
                if (_selectedElement != null)
                    _selectedElement.Opacity = 1;
                _isDragging = false;
                _selectedElement = null;
            }
        }

        private void DrawingCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                _selectedElement?.ReleaseMouseCapture();
                if (_selectedElement != null)
                    _selectedElement.Opacity = 1;
                _isDragging = false;
                _selectedElement = null;
            }
        }

        private void ExitBtt_Clik(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        [Serializable]
        public class CanvasElement
        {
            public string Type { get; set; }
            public double Left { get; set; }
            public double Top { get; set; }
            public double Width { get; set; }
            public double Height { get; set; }
            public string Text { get; set; }
            public int SeatsCount { get; set; }
            public string Color { get; set; }
        }

        [Serializable]
        public class CanvasData
        {
            public List<CanvasElement> Elements { get; set; } = new List<CanvasElement>();
        }


        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                DefaultExt = ".json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var canvasData = new CanvasData();

                    foreach (UIElement element in DrawingCanvas.Children)
                    {
                        if (element is Grid grid)
                        {
                            var canvasElement = new CanvasElement
                            {
                                Left = Canvas.GetLeft(grid),
                                Top = Canvas.GetTop(grid)
                            };

                            // Check if it's a table
                            var tableInfo = _tables.FirstOrDefault(t => t.TableContainer == grid);
                            if (tableInfo != null)
                            {
                                canvasElement.Type = "Table";
                                canvasElement.Width = tableInfo.TableShape.Width;
                                canvasElement.Height = tableInfo.TableShape.Height;
                                canvasElement.Text = tableInfo.TableText.Text;
                                canvasElement.SeatsCount = tableInfo.SeatsCount;
                                canvasElement.Color = (tableInfo.TableShape.Fill as SolidColorBrush)?.Color.ToString();
                            }
                            else
                            {
                                // Check for rectangle
                                var contentGrid = grid.Children.OfType<Grid>().FirstOrDefault();
                                if (contentGrid != null)
                                {
                                    var rectangle = contentGrid.Children.OfType<Rectangle>().FirstOrDefault();
                                    if (rectangle != null)
                                    {
                                        canvasElement.Type = "Rectangle";
                                        canvasElement.Width = rectangle.Width;
                                        canvasElement.Height = rectangle.Height;
                                        canvasElement.Color = (rectangle.Fill as SolidColorBrush)?.Color.ToString();
                                        canvasElement.Text = contentGrid.Children.OfType<TextBlock>().FirstOrDefault()?.Text;
                                    }
                                }

                                // Check for guest if not rectangle
                                if (canvasElement.Type == null)
                                {
                                    var ellipse = grid.Children.OfType<Ellipse>().FirstOrDefault(i => i.Tag as string != "seat");
                                    if (ellipse != null)
                                    {
                                        canvasElement.Type = "Guest";
                                        canvasElement.Width = ellipse.Width;
                                        canvasElement.Height = ellipse.Height;
                                        canvasElement.Color = (ellipse.Fill as SolidColorBrush)?.Color.ToString();
                                        canvasElement.Text = grid.Children.OfType<TextBlock>().FirstOrDefault()?.Text;
                                    }
                                }
                            }

                            if (canvasElement.Type != null)
                            {
                                canvasData.Elements.Add(canvasElement);
                            }
                        }
                    }

                    string json = JsonConvert.SerializeObject(canvasData, Formatting.Indented);
                    File.WriteAllText(saveFileDialog.FileName, json);
                    MessageBox.Show("Файл успешно сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string json = File.ReadAllText(openFileDialog.FileName);
                    var canvasData = JsonConvert.DeserializeObject<CanvasData>(json);

                    if (canvasData != null)
                    {
                        ClearCanvas_Click(null, null); // Очищаем холст перед загрузкой

                        foreach (var element in canvasData.Elements)
                        {
                            try
                            {
                                switch (element.Type)
                                {
                                    case "Rectangle":
                                        AddRectangleFromData(element);
                                        break;
                                    case "Guest":
                                        AddGuestFromData(element);
                                        break;
                                    case "Table":
                                        AddTableFromData(element);
                                        break;
                                    default:
                                        MessageBox.Show($"Неизвестный тип элемента: {element.Type}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Ошибка при загрузке элемента {element.Type}: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        MessageBox.Show("Файл успешно загружен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при открытии файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddRectangleFromData(CanvasElement element)
        {
            try
            {
                var rectangle = new Rectangle
                {
                    Width = element.Width,
                    Height = element.Height,
                    Fill = !string.IsNullOrEmpty(element.Color)
                           ? new SolidColorBrush((Color)ColorConverter.ConvertFromString(element.Color))
                           : RectangleColor,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };

                // Создаем Grid для объединения прямоугольника и текста
                var contentGrid = new Grid();
                contentGrid.Children.Add(rectangle);

                var textBlock = new TextBlock
                {
                    Text = element.Text ?? $"Прямоугольник {DrawingCanvas.Children.Count + 1}",
                    Style = (Style)FindResource("CenterTextStyle")
                };
                contentGrid.Children.Add(textBlock);

                // Основной контейнер
                var grid = new Grid
                {
                    Style = (Style)FindResource("ElementContainerStyle")
                };
                grid.Children.Add(contentGrid);
                grid.Children.Add(CreateDeleteButton(grid));
                grid.Children.Add(CreateShapeEditButton(rectangle, textBlock));

                SetupDrag(grid);
                Panel.SetZIndex(grid, 1);

                Canvas.SetLeft(grid, element.Left);
                Canvas.SetTop(grid, element.Top);
                DrawingCanvas.Children.Add(grid);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании прямоугольника: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddGuestFromData(CanvasElement element)
        {
            var ellipse = new Ellipse
            {
                Width = element.Width,
                Height = element.Height,
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(element.Color)),
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Cursor = Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Top
            };

            var textBlock = new TextBlock
            {
                Text = element.Text,
                Style = (Style)FindResource("BottomTextStyle")
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            Grid.SetRow(ellipse, 0);
            Grid.SetRow(textBlock, 1);

            grid.Children.Add(ellipse);
            grid.Children.Add(textBlock);
            grid.Children.Add(CreateDeleteButton(grid));
            grid.Children.Add(CreateShapeEditButton(ellipse, textBlock));

            SetupDrag(grid);
            Panel.SetZIndex(grid, 1);

            Canvas.SetLeft(grid, element.Left);
            Canvas.SetTop(grid, element.Top);
            DrawingCanvas.Children.Add(grid);
        }

        private void AddTableFromData(CanvasElement element)
        {
            var table = new Ellipse
            {
                Width = element.Width,
                Height = element.Height,
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(element.Color)),
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                VerticalAlignment = VerticalAlignment.Top
            };

            var textBlock = new TextBlock
            {
                Text = element.Text,
                Style = (Style)FindResource("CenterTextStyle"),
                FontWeight = FontWeights.Bold
            };

            var grid = new Grid
            {
                Style = (Style)FindResource("ElementContainerStyle")
            };
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            Grid.SetRow(table, 0);
            Grid.SetRow(textBlock, 1);

            var tableInfo = new TableInfo
            {
                TableContainer = grid,
                TableShape = table,
                TableText = textBlock,
                SeatsCount = element.SeatsCount
            };
            _tables.Add(tableInfo);

            grid.Children.Add(table);
            AddSeatsAroundTable(grid, tableInfo, tableInfo.SeatsCount);
            grid.Children.Add(textBlock);
            grid.Children.Add(CreateDeleteButton(grid));
            grid.Children.Add(CreateTableEditButton(tableInfo));

            SetupDrag(grid);
            Panel.SetZIndex(grid, 0);

            Canvas.SetLeft(grid, element.Left);
            Canvas.SetTop(grid, element.Top);
            DrawingCanvas.Children.Add(grid);
        }
    }
}
