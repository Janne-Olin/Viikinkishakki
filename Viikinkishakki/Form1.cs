using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Viikinkishakki
{
    public partial class Form1 : Form
    {
        Game game;
        PictureBox pboxBoard = new PictureBox();
        PictureBox[,] boxGrid = PBoxGrid.Grid;
        public Form1()
        {
            InitializeComponent();
            CreateBoard();
            CreateGrid();
            game = new Game();
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {
            // Lähetetään pelille klikatun ruudun koordinaatit
            PictureBox pbox = (PictureBox)sender;
            int x = 0, y = 0;

            for (int i = 0; i < boxGrid.GetLength(0); i++)
            {
                for (int j = 0; j < boxGrid.GetLength(1); j++)
                {
                    if (boxGrid[i, j].Equals(pbox))
                    {
                        x = i;
                        y = j;

                        i = int.MaxValue - 1;
                        break;
                    }
                }
            }

            game.BoardClicked(x, y);
        }

        private void CreateBoard()
        {
            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            string mainPath = path.Replace("\\bin\\Debug\\netcoreapp3.1", "");
            mainPath = mainPath.Replace("file:\\", "");

            pboxBoard.Location = new Point(0, 30);
            pboxBoard.Dock = DockStyle.Fill;


            pboxBoard.SizeMode = PictureBoxSizeMode.StretchImage;
            pboxBoard.Image = Image.FromFile(mainPath + "\\icons\\board.png");
            pboxBoard.BringToFront();
            pboxBoard.Visible = true;

            splitContainer1.Panel2.Controls.Add(pboxBoard);
        }

        private void CreateGrid()
        {
            // Luodaan ruudukko
            for (int i = 0; i < boxGrid.GetLength(0); i++)
            {
                for (int j = 0; j < boxGrid.GetLength(1); j++)
                {
                    PictureBox pbox = new PictureBox();
                    pbox.Location = new Point(i * 70 + 9, j * 70 + 9);
                    pbox.Size = new Size(63, 63);
                    pbox.BackColor = Color.Transparent;
                    pbox.SizeMode = PictureBoxSizeMode.StretchImage;
                    pbox.Tag = "empty";

                    pboxBoard.Controls.Add(pbox);
                    boxGrid[i, j] = pbox;

                    pbox.Click += new EventHandler(pictureBox_Click);
                }
            }
        }

        

        private void toolStripMenuItemNewGame_Click(object sender, EventArgs e)
        {
            game.NewGame();
        }

        private void toolStripMenuItemExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
