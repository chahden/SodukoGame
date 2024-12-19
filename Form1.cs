using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public class SudokuForm : Form
{
    private const int gridSize = 9;
    private const int textBoxSize = 30;
    private const int spacing = 5;
    private TextBox[,] grid = new TextBox[gridSize, gridSize];
    private Random rand = new Random();

    public SudokuForm()
    {
        InitializeComponent();
        InitializeGame();
    }

    private void InitializeComponent()
    {
        this.Text = "Sudoku Game";
        this.ClientSize = new Size(10 + gridSize * (textBoxSize + spacing), 60 + gridSize * (textBoxSize + spacing));

        Button checkButton = new Button
        {
            Text = "Check Your Work",
            Location = new Point(30, 10 + gridSize * (textBoxSize + spacing)),
            Size = new Size(120, 30)
        };
        checkButton.Click += CheckButton_Click;
        this.Controls.Add(checkButton);

        Button resetButton = new Button
        {
            Text = "Reset",
            Location = new Point(170, 10 + gridSize * (textBoxSize + spacing)),
            Size = new Size(120, 30)
        };
        resetButton.Click += ResetButton_Click;
        this.Controls.Add(resetButton);

        Button solutionButton = new Button
        {
            Text = "Give Solution",
            Location = new Point(310, 10 + gridSize * (textBoxSize + spacing)),
            Size = new Size(120, 30)
        };
        solutionButton.Click += SolutionButton_Click;
        this.Controls.Add(solutionButton);

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                TextBox textBox = new TextBox
                {
                    Location = new Point(10 + j * (textBoxSize + spacing), 10 + i * (textBoxSize + spacing)),
                    Size = new Size(textBoxSize, textBoxSize),
                    MaxLength = 1,
                    TextAlign = HorizontalAlignment.Center,
                    Font = new Font("Arial", 16)
                };
                textBox.KeyPress += (sender, e) =>
                {
                    if (!char.IsDigit(e.KeyChar) || e.KeyChar == '0')
                        e.Handled = true;
                };
                this.Controls.Add(textBox);
                grid[i, j] = textBox;
            }
        }
    }

    private void InitializeGame()
    {
        foreach (TextBox textBox in grid)
        {
            textBox.Text = string.Empty;
            textBox.Enabled = true;
            textBox.BackColor = Color.White;
        }

        GenerateClues();
    }

    private void GenerateClues()
    {
        for (int i = 0; i < gridSize; i += 3)
        {
            for (int j = 0; j < gridSize; j += 3)
            {
                int cluesAdded = 0;
                while (cluesAdded < 2)
                {
                    int row = rand.Next(i, i + 3);
                    int col = rand.Next(j, j + 3);
                    int number = rand.Next(1, 10);

                    if (IsValidPlacement(grid, row, col, number))
                    {
                        grid[row, col].Text = number.ToString();
                        grid[row, col].Enabled = false;
                        cluesAdded++;
                    }
                }
            }
        }
    }

    private bool IsValidPlacement(TextBox[,] grid, int row, int col, int number)
    {
        string numStr = number.ToString();

        for (int c = 0; c < gridSize; c++)
        {
            if (grid[row, c].Text == numStr)
                return false;
        }

        for (int r = 0; r < gridSize; r++)
        {
            if (grid[r, col].Text == numStr)
                return false;
        }

        int subGridRowStart = (row / 3) * 3;
        int subGridColStart = (col / 3) * 3;
        for (int i = subGridRowStart; i < subGridRowStart + 3; i++)
        {
            for (int j = subGridColStart; j < subGridColStart + 3; j++)
            {
                if (grid[i, j].Text == numStr)
                    return false;
            }
        }

        return true;
    }

    private bool VerifWining(TextBox[,] grid)
    {
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                if (string.IsNullOrEmpty(grid[i, j].Text) || !IsValidPlacement(grid, i, j, int.Parse(grid[i, j].Text)))
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void SolutionButton_Click(object sender, EventArgs e)
    {
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                TextBox currentCell = grid[i, j];
                string currentText = currentCell.Text;

                if (!currentCell.Enabled)
                {
                    currentCell.BackColor = Color.Green;
                    currentCell.ForeColor = Color.Black;
                }
                else if (string.IsNullOrEmpty(currentText))
                {
                    currentCell.BackColor = Color.Red;
                    currentCell.ForeColor = Color.Black;
                    currentCell.Text = GetCorrectNumber(i, j).ToString();
                }
                else
                {
                    int number;
                    if (int.TryParse(currentText, out number) && IsValidPlacement(grid, i, j, number))
                    {
                        currentCell.BackColor = Color.Green;
                        currentCell.ForeColor = Color.Black;
                    }
                    else
                    {
                        currentCell.BackColor = Color.Red;
                        currentCell.ForeColor = Color.Black;
                        currentCell.Text = GetCorrectNumber(i, j).ToString();
                    }
                }
            }
        }
    }

    private int GetCorrectNumber(int row, int col)
    {
        for (int s = 1; s <= 9; s++)
        {
            if (IsValidPlacement(grid, row, col, s))
                return s;
        }
        throw new InvalidOperationException("No valid number found.");
    }

    private void CheckButton_Click(object sender, EventArgs e)
    {
        if (VerifWining(grid))
        {
            MessageBox.Show("Congratulations! You solved the puzzle!");
        }
        else
        {
            MessageBox.Show("There are mistakes in your solution. Keep trying!");
        }
    }

    private void ResetButton_Click(object sender, EventArgs e)
    {
        InitializeGame();
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new SudokuForm());
    }
}
