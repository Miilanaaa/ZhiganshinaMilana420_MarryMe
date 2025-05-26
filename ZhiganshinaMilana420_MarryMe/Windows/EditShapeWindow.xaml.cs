using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace ZhiganshinaMilana420_MarryMe.Windows
{
    /// <summary>
    /// Логика взаимодействия для EditShapeWindow.xaml
    /// </summary>
    public partial class EditShapeWindow : Window
    {
        public string ShapeName { get; private set; }
        public double ShapeWidth { get; private set; }
        public double ShapeHeight { get; private set; }

        public EditShapeWindow(string currentName, double currentWidth, double currentHeight)
        {
            InitializeComponent();
            NameTextBox.Text = currentName;
            WidthTextBox.Text = currentWidth.ToString();
            HeightTextBox.Text = currentHeight.ToString();

            // Добавьте подсказки
            WidthTextBox.ToolTip = "Ширина стола";
            HeightTextBox.ToolTip = "Высота стола";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(WidthTextBox.Text, out double width) &&
                double.TryParse(HeightTextBox.Text, out double height))
            {
                // Сохраняем весь текст, включая цифру
                ShapeName = NameTextBox.Text;
                ShapeWidth = width;
                ShapeHeight = height;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите корректные числовые значения для размеров.");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}
