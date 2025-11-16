using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Kursovik
{
    public partial class Form1 : Form
    {
        public class ColoringPage
        {
            public int[,] ColorNumbers { get; set; }  // Номера цветов для каждого квадрата
            public Dictionary<int, Color> Palette { get; set; }  // Палитра (номер = цвет)
            public int SquareSize { get; set; } = 20;  // Размер квадрата в пикселях
            public bool[,] FilledSquares { get; set; } // отслеживание закрашивания

            public ColoringPage()
            {
                Palette = new Dictionary<int, Color>();
            }
        }



        private ColoringPage coloringPage;
        private Color selectedColor;
        private int selectedColorNumber;


        public Form1()
        {
            InitializeComponent();
            coloringPage = new ColoringPage(); //раскраска
            SetupPalette();  // палитра
            picCanvas.Paint += PicCanvas_Paint;
            picCanvas.MouseClick += PicCanvas_MouseClick;
            btnLoad.Click += BtnLoad_Click;
        }
        private void SetupPalette()
        {
            // Заполняем ComboBox и палитру
            coloringPage.Palette = new Dictionary<int, Color> // коллекция(int - номер цвета, color - цвет)
    {
        { 0, Color.White },
        { 1, Color.Red },
        { 2, Color.Green },
        { 3, Color.Blue },
        { 4, Color.Yellow },
        { 5, Color.Purple },
        { 6, Color.Orange },
        { 7, Color.Pink },
        { 8, Color.Brown },
        {9, Color.Black }
    };

            cmbColors.Items.Clear(); //отчистка комбобокса
            foreach (var color in coloringPage.Palette) // перебор всех цветов палитры
            {
                cmbColors.Items.Add($"Цвет {color.Key} ({color.Value.Name})");
            }
            cmbColors.SelectedIndex = 0;
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {


            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt",
                Title = "Выберите файл с разметкой"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    //split - разделяет строку
                    //StringSplitOptions - Указывает, включает или исключает пустые строки из возвращаемого значения соответствующий метод
                    //RemoveEmptyEntries - Возвращаемое значение не содержит элементы массива, содержащие пустые строки.

                    string[] lines = File.ReadAllLines(openFileDialog.FileName);

                    int rows = lines.Length; //число строк - длина массива 
                    int cols = lines[0].Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).Length; //число столбцов = кол-во чисел в первой строке

                    coloringPage.ColorNumbers = new int[rows, cols]; //номера цветов
                    coloringPage.FilledSquares = new bool[rows, cols];

                    //заполнение массива числами из файла
                    for (int i = 0; i < rows; i++)
                    {
                        string[] numbers = lines[i].Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        for (int j = 0; j < cols; j++)
                        {
                            coloringPage.ColorNumbers[i, j] = int.Parse(numbers[j]);
                        }
                    }

                    picCanvas.Invalidate();  // обновление picturebox
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void PicCanvas_Paint(object sender, PaintEventArgs e)
        {
            if (coloringPage.ColorNumbers == null) return;

            Graphics g = e.Graphics;

            //определение размеров сетки
            int rows = coloringPage.ColorNumbers.GetLength(0);
            int cols = coloringPage.ColorNumbers.GetLength(1);


            //обработка всех ячеек
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    //создание квадрата
                    //Rectangle - задает область рисования
                    // coloringPage.SquareSize - размер 20х20 px
                    Rectangle rect = new Rectangle(
                        j * coloringPage.SquareSize,
                        i * coloringPage.SquareSize,
                        coloringPage.SquareSize,
                        coloringPage.SquareSize
                    );

                    // отрисовка границы
                    g.DrawRectangle(Pens.Black, rect);


                    string number = coloringPage.ColorNumbers[i, j].ToString(); //номер цвета
                    //настройки текста
                    SizeF textSize = g.MeasureString(number, Font);
                    float x = rect.Left + (rect.Width - textSize.Width) / 2;
                    float y = rect.Top + (rect.Height - textSize.Height) / 2;

                    //открисовка текста
                    g.DrawString(number, Font, Brushes.Black, x, y);
                }
            }
        }

        private void PicCanvas_MouseClick(object sender, MouseEventArgs e)
        {
            if (coloringPage.ColorNumbers == null || coloringPage.FilledSquares == null) return;

            int row = e.Y / coloringPage.SquareSize;
            int col = e.X / coloringPage.SquareSize;

            if (row >= 0 && row < coloringPage.ColorNumbers.GetLength(0) &&
                col >= 0 && col < coloringPage.ColorNumbers.GetLength(1))
            {
                int requiredColorNumber = coloringPage.ColorNumbers[row, col];
                int selectedNumber = coloringPage.Palette.Keys.ElementAt(cmbColors.SelectedIndex);

                if (selectedNumber == requiredColorNumber)
                {
                    // Помечаем квадрат как закрашенный
                    coloringPage.FilledSquares[row, col] = true;

                    // Закрашиваем квадрат
                    using (Graphics g = picCanvas.CreateGraphics())
                    {
                        Rectangle rect = new Rectangle(
                            col * coloringPage.SquareSize,
                            row * coloringPage.SquareSize,
                            coloringPage.SquareSize,
                            coloringPage.SquareSize
                        );

                        g.FillRectangle(
                            new SolidBrush(coloringPage.Palette[selectedNumber]),
                            rect
                        );
                        g.DrawRectangle(Pens.Black, rect);
                    }

                    // Проверяем, все ли квадраты закрашены
                    bool allFilled = true;
                    for (int i = 0; i < coloringPage.FilledSquares.GetLength(0); i++)
                    {
                        for (int j = 0; j < coloringPage.FilledSquares.GetLength(1); j++)
                        {
                            if (!coloringPage.FilledSquares[i, j])
                            {
                                allFilled = false;
                                break;
                            }
                        }
                        if (!allFilled) break;
                    }

                    if (allFilled)
                    {
                        MessageBox.Show("Вы закончили раскрашивать!", "Поздравляем!");
                    }
                }
                else
                {
                    MessageBox.Show(
                        $"Неверный цвет! Должен быть: {requiredColorNumber}",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            }
        }
    }
}
