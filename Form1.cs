using System;
using System.Drawing;
using System.Windows.Forms;

public class SudokuForm : Form
{
    #region Fields
    private const int gridSize = 9;
    private const int textBoxSize = 40;
    private const int spacing = 2;
    private TextBox[,] grid = new TextBox[gridSize, gridSize];
    private Random rand = new Random();
    private int N = 9;
    #endregion

    #region Constructor
    public SudokuForm()
    {
        InitializeComponent();
        InitializeGame();
    }
    #endregion

    #region UI Initialization
    private void InitializeComponent()
    {
        this.Text = "🧩 Sudoku Game";
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(245, 245, 245);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;

        int boardWidth = gridSize * (textBoxSize + spacing);
        this.ClientSize = new Size(boardWidth + 20, boardWidth + 80);

        int offsetX = 10;
        int offsetY = 10;

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                TextBox textBox = new TextBox();
                textBox.Location = new Point(offsetX + j * (textBoxSize + spacing), offsetY + i * (textBoxSize + spacing));
                textBox.Size = new Size(textBoxSize, textBoxSize);
                textBox.MaxLength = 1;
                textBox.TextAlign = HorizontalAlignment.Center;
                textBox.Font = new Font("Segoe UI", 16, FontStyle.Bold);
                textBox.BorderStyle = BorderStyle.FixedSingle;
                textBox.BackColor = ((i / 3 + j / 3) % 2 == 0) ? Color.White : Color.FromArgb(240, 240, 240);

                textBox.KeyPress += new KeyPressEventHandler(delegate (object sender, KeyPressEventArgs e)
                {
                    if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
                        e.Handled = true;
                });

                textBox.GotFocus += new EventHandler(delegate (object s, EventArgs e)
                {
                    ((TextBox)s).BackColor = Color.LightYellow;
                });

                textBox.LostFocus += new EventHandler(delegate (object s, EventArgs e)
                {
                    if (((i / 3 + j / 3) % 2 == 0))
                        ((TextBox)s).BackColor = Color.White;
                    else
                        ((TextBox)s).BackColor = Color.FromArgb(240, 240, 240);
                });

                this.Controls.Add(textBox);
                grid[i, j] = textBox;
            }
        }

        int btnY = offsetY + gridSize * (textBoxSize + spacing) + 10;

        Button checkButton = CreateStyledButton("✔ Check", new Point(10, btnY));
        checkButton.Click += new EventHandler(CheckButton_Click);
        this.Controls.Add(checkButton);

        Button resetButton = CreateStyledButton("🔄 Reset", new Point(140, btnY));
        resetButton.Click += new EventHandler(ResetButton_Click);
        this.Controls.Add(resetButton);

        Button solveButton = CreateStyledButton("💡 Solution", new Point(270, btnY));
        solveButton.Click += new EventHandler(SolutionButton_Click);
        this.Controls.Add(solveButton);
    }

    private Button CreateStyledButton(string text, Point location)
    {
        Button button = new Button();
        button.Text = text;
        button.Location = location;
        button.Size = new Size(120, 35);
        button.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        button.BackColor = Color.FromArgb(50, 150, 250);
        button.ForeColor = Color.White;
        button.FlatStyle = FlatStyle.Flat;
        return button;
    }
    #endregion

    #region Game Initialization
    private void InitializeGame()
    {
        foreach (TextBox textBox in grid)
        {
            textBox.Text = "";
            textBox.Enabled = true;

            int row = textBox.Location.Y / (textBoxSize + spacing);
            int col = textBox.Location.X / (textBoxSize + spacing);
            if (((row / 3 + col / 3) % 2 == 0))
                textBox.BackColor = Color.White;
            else
                textBox.BackColor = Color.FromArgb(240, 240, 240);

            textBox.ForeColor = Color.Black;
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
                        grid[row, col].BackColor = Color.LightGray;
                        grid[row, col].ForeColor = Color.DarkBlue;
                        cluesAdded++;
                    }
                }
            }
        }
    }
    #endregion

    #region Validation Methods
    private bool IsValidPlacement(TextBox[,] grid, int row, int col, int number)
    {
        string numStr = number.ToString();

        for (int c = 0; c < gridSize; c++)
        {
            if (c != col && grid[row, c].Text == numStr)
                return false;
        }

        for (int r = 0; r < gridSize; r++)
        {
            if (r != row && grid[r, col].Text == numStr)
                return false;
        }

        int subGridRowStart = (row / 3) * 3;
        int subGridColStart = (col / 3) * 3;

        for (int i = subGridRowStart; i < subGridRowStart + 3; i++)
        {
            for (int j = subGridColStart; j < subGridColStart + 3; j++)
            {
                if ((i != row || j != col) && grid[i, j].Text == numStr)
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
                if (string.IsNullOrEmpty(grid[i, j].Text))
                    return false;

                int num;
                if (!int.TryParse(grid[i, j].Text, out num))
                    return false;

                if (!IsValidPlacement(grid, i, j, num))
                    return false;
            }
        }
        return true;
    }
    #endregion

    #region Backtracking Solver
    private bool BackTracking(int[,] grid, int row, int col)
    {
        if (row == N - 1 && col == N)
            return true;

        if (col == N)
        {
            row++;
            col = 0;
        }

        if (grid[row, col] != 0)
            return BackTracking(grid, row, col + 1);

        for (int num = 1; num <= 9; num++)
        {
            if (IsSafe(grid, row, col, num))
            {
                grid[row, col] = num;
                if (BackTracking(grid, row, col + 1))
                    return true;
                grid[row, col] = 0;
            }
        }

        return false;
    }

    private static bool IsSafe(int[,] grid, int row, int col, int num)
    {
        for (int x = 0; x < 9; x++)
        {
            if (grid[row, x] == num || grid[x, col] == num)
                return false;
        }

        int startRow = row - row % 3;
        int startCol = col - col % 3;

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (grid[i + startRow, j + startCol] == num)
                    return false;
            }
        }

        return true;
    }
    #endregion

    #region Event Handlers
    private void CheckButton_Click(object sender, EventArgs e)
    {
        if (VerifWining(grid))
        {
            MessageBox.Show("🎉 Congratulations! You solved the puzzle!");
        }
        else
        {
            MessageBox.Show("❌ There are mistakes in your solution.");
        }
    }

    private void ResetButton_Click(object sender, EventArgs e)
    {
        InitializeGame();
    }

    private void SolutionButton_Click(object sender, EventArgs e)
    {
        int[,] intGrid = new int[gridSize, gridSize];

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                int value;
                if (int.TryParse(grid[i, j].Text, out value) && !grid[i, j].Enabled)
                    intGrid[i, j] = value;
                else
                    intGrid[i, j] = 0;
            }
        }

        if (BackTracking(intGrid, 0, 0))
        {
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    grid[i, j].Text = intGrid[i, j].ToString();
                    grid[i, j].BackColor = Color.LightGreen;
                }
            }
        }
        else
        {
            MessageBox.Show("No solution found.");
        }
    }
    #endregion

    #region Entry Point
    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new SudokuForm());
    }
    #endregion
}
