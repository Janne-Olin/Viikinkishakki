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
        PictureBox[,] boxGrid = PBoxGrid.Grid;
        public Form1()
        {
            InitializeComponent();
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

        private void CreateGrid()
        {
            // Luodaan ruudukko
            for (int i = 0; i < boxGrid.GetLength(0); i++)
            {
                for (int j = 0; j < boxGrid.GetLength(1); j++)
                {
                    PictureBox pbox = new PictureBox();
                    pbox.Location = new Point(i * 40 + 10, j * 40 + 10);
                    pbox.Size = new Size(40, 40);

                    pbox.BorderStyle = BorderStyle.Fixed3D;

                    this.Controls.Add(pbox);
                    boxGrid[i, j] = pbox;

                    pbox.Click += new EventHandler(pictureBox_Click);
                }
            }

        }

    }
}
