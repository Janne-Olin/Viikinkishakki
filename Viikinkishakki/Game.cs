using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Viikinkishakki
{
    class Game
    {
        public List<Piece> Pieces { get; set; }
        public Piece Selected { get; set; }
        public King King { get; set; }
        public String Path { get; set; }
        public String MainPath { get; set; }
        public Boolean Animating { get; set; } = false;

        public Game()
        {
            Selected = null;
            Path = "";
            MainPath = "";
            CreatePieces();
        }

        private void CreatePieces()
        {
            Pieces = new List<Piece>();

            // Luodaan nappulat
            Pieces.Add(new AttPiece(0, 3));
            Pieces.Add(new AttPiece(0, 4));
            Pieces.Add(new AttPiece(0, 5));
            Pieces.Add(new AttPiece(0, 6));
            Pieces.Add(new AttPiece(0, 7));
            Pieces.Add(new AttPiece(1, 5));
            Pieces.Add(new AttPiece(3, 0));
            Pieces.Add(new AttPiece(3, 10));
            Pieces.Add(new AttPiece(4, 0));
            Pieces.Add(new AttPiece(4, 10));
            Pieces.Add(new AttPiece(5, 0));
            Pieces.Add(new AttPiece(5, 1));
            Pieces.Add(new AttPiece(5, 9));
            Pieces.Add(new AttPiece(5, 10));
            Pieces.Add(new AttPiece(6, 0));
            Pieces.Add(new AttPiece(6, 10));
            Pieces.Add(new AttPiece(7, 0));
            Pieces.Add(new AttPiece(7, 10));
            Pieces.Add(new AttPiece(9, 5));
            Pieces.Add(new AttPiece(10, 3));
            Pieces.Add(new AttPiece(10, 4));
            Pieces.Add(new AttPiece(10, 5));
            Pieces.Add(new AttPiece(10, 6));
            Pieces.Add(new AttPiece(10, 7));

            Pieces.Add(new DefPiece(3, 5));
            Pieces.Add(new DefPiece(4, 4));
            Pieces.Add(new DefPiece(4, 5));
            Pieces.Add(new DefPiece(4, 6));
            Pieces.Add(new DefPiece(5, 3));
            Pieces.Add(new DefPiece(5, 4));
            Pieces.Add(new DefPiece(5, 6));
            Pieces.Add(new DefPiece(5, 7));
            Pieces.Add(new DefPiece(6, 4));
            Pieces.Add(new DefPiece(6, 5));
            Pieces.Add(new DefPiece(6, 6));
            Pieces.Add(new DefPiece(7, 5));

            Pieces.Add(new Special(0, 0));
            Pieces.Add(new Special(0, 10));
            Pieces.Add(new Special(10, 0));
            Pieces.Add(new Special(10, 10));
            Pieces.Add(new Special(5, 5));

            King = new King(5, 5);
            Pieces.Add(King);

            AddToBoard();
        }

        private void AddToBoard()
        {
            
            Path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            MainPath = Path.Replace("\\bin\\Debug\\netcoreapp3.1", "");
            MainPath = MainPath.Replace("file:\\", "");
            

            // Asetetaan nappulat paikoilleen ja asetetaan niille oikeat värit
            foreach (Piece piece in Pieces)
            {
                int x = piece.XPos;
                int y = piece.YPos;
                
                PBoxGrid.Grid[x, y].Tag = piece.Tag;

                if (!(piece is Special))
                {
                    PBoxGrid.Grid[x, y].Image = Image.FromFile(MainPath + piece.IconPath);
                }
            }

        }

        public void BoardClicked(int x, int y)
        {  
            // Ei tehdä mitään animaation aikana
            if (Animating)
            {
                return;
            }

            // Tarkastetaanko onko nappulaa valittuna
            if (Selected != null)
            {
                // Valinta poistetaan, jos valittua nappulaa klikkaa uudelleen
                if (Selected.XPos == x && Selected.YPos == y)
                {
                    PBoxGrid.Grid[x, y].Image = Image.FromFile(MainPath + Selected.IconPath);
                    Selected = null;
                    return;
                }                
            }

            // Tarkistetaan onko klikatussa ruudussa nappulaa
            foreach (Piece piece in Pieces)
            {
                if (piece is Special)
                {
                    // Kulmaruutuja tai linnaa ei voi valita
                    continue;
                }

                if (piece.XPos == x && piece.YPos == y)
                {
                    // Vaihdetaan valittu nappula
                    if (Selected != null)
                    {
                        PBoxGrid.Grid[Selected.XPos, Selected.YPos].Image = Image.FromFile(MainPath + Selected.IconPath);
                    }

                    Selected = piece;
                    PBoxGrid.Grid[x, y].Image = Image.FromFile(MainPath + piece.SelectedIconPath);
                    return;
                }
            }
            // Yritetään siirtoa, jos nappula on valittuna ja klikattiin ruutua josta ei löytynyt nappulaa
            if (Selected != null)
            {
                MakeMove(x, y);
            }
            
        }

        private void MakeMove(int x, int y)
        {
            if (MoveIsLegal(x, y))
            {                
                if (Selected.XPos == 5 && Selected.YPos == 5)
                {
                    // Ei anneta linnalle "empty"-tagia kuninkaan poistuessa
                    PBoxGrid.Grid[Selected.XPos, Selected.YPos].Tag = "special";
                }
                else
                {
                    PBoxGrid.Grid[Selected.XPos, Selected.YPos].Tag = "empty";
                }

                Animate(x, y);

                // Päivitetään valitun nappulan sijainti                
                Selected.XPos = x;
                Selected.YPos = y;
                PBoxGrid.Grid[Selected.XPos, Selected.YPos].Tag = Selected.Tag;

                CheckCaptures();

                if (Selected == King)
                {
                    // Tarkistaa onko kuningas kulmaruuduissa
                    if ((Selected.XPos == 0 && (Selected.YPos == 0 || Selected.YPos == 10)) || 
                        Selected.XPos == 10 && (Selected.YPos == 0 || Selected.YPos == 10))
                    {
                        GameEnd("Puolustaja");
                    }
                }
            }
        }

        private bool MoveIsLegal(int x, int y)
        {
            // Ainoastaan kuningas voi liikkua keski- ja kulmaruutuihin
            if ((x == 0 && (y == 0 || y == 10)) || (x == 10 && (y == 0 || y == 10)) || (x == 5 && y == 5))
            {
                if (Selected != King)
                {
                    return false;
                }
            }
            
            // Palautetaan false, jos nappulan x- ja y-akselit ovat molemmat eri kuin klikatulla ruudulla
            if (Selected.XPos != x && Selected.YPos != y)
            {
                return false;
            }
            
            // Tarkastetaan onko nappulan kulkulinjalla muita nappuloita
            if (Selected.XPos != x)
            {
                if (Selected.XPos > x)
                {
                    for (int i = Selected.XPos - 1; i >= x ; i--)
                    {
                        if (PBoxGrid.Grid[i, y].Image != null)
                        {
                            return false;
                        }
                    }
                }
                else if (Selected.XPos < x)
                {
                    for (int i = Selected.XPos + 1; i <= x; i++)
                    {
                        if (PBoxGrid.Grid[i, y].Image != null)
                        {
                            return false;
                        }
                    }
                }                    
            }

            else if (Selected.YPos != y)
            {
                if (Selected.YPos > y)
                {
                    for (int i = Selected.YPos - 1; i >= y; i--)
                    {
                        if (PBoxGrid.Grid[x, i].Image != null)
                        {
                           return false;
                        }
                    }
                }
                else if (Selected.YPos < y)
                {
                    for (int i = Selected.YPos + 1; i <= y; i++)
                    {
                        if (PBoxGrid.Grid[x, i].Image != null)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private void Animate(int x, int y)
        {
            Animating = true;
            PictureBox pbox = PBoxGrid.Grid[Selected.XPos, Selected.YPos];
            Point origPboxLoc = new Point(pbox.Location.X, pbox.Location.Y);
            pbox.BringToFront();            


            Timer timer = new Timer();
            timer.Enabled = true;
            timer.Interval = 20;
            timer.Tick += (object sender, EventArgs e) => timer_Tick(sender, e, x, y, pbox, origPboxLoc);
        }

        void timer_Tick(object sender, EventArgs e, int x, int y, PictureBox pbox, Point OrigPboxLoc)
        {
            int curX = pbox.Location.X;
            int curY = pbox.Location.Y;
            int targetX = PBoxGrid.Grid[x, y].Location.X;
            int targetY = PBoxGrid.Grid[x, y].Location.Y;

            if (targetX > curX)
            {
                pbox.Location = new Point(curX + 70, curY);
            }
            else if (targetX < curX)
            {
                pbox.Location = new Point(curX - 70, curY);
            }
            else if (targetY > curY)
            {
                pbox.Location = new Point(curX, curY + 70);
            }
            else if (targetY < curY)
            {
                pbox.Location = new Point(curX, curY - 70);
            }
            else
            {
                // Poistetaan valittu nappula alkuperäisestä sijainnista
                pbox.Image = null;

                pbox.Location = OrigPboxLoc;
                PBoxGrid.Grid[Selected.XPos, Selected.YPos].Image = Image.FromFile(MainPath + Selected.IconPath);
                Selected = null;
                ((Timer)sender).Stop();
                Animating = false;
            }
        }

        /// <summary>
        /// Tarkastetaan tuleeko siirron jälkeen mikään nappula syödyksi
        /// </summary>
        private void CheckCaptures()
        {
            // Luodaan lista johon syödyt nappulat lisätään
            List<Piece> ToBeRemoved = new List<Piece>();

            foreach (Piece enemyPiece in Pieces)
            {

                // Varmistetaan ettei syödä omia nappuloita
                if ((Selected is AttPiece && enemyPiece is AttPiece) || (Selected is DefPiece && enemyPiece is DefPiece))
                {
                    continue;
                }

                // Erikoisruutuja (linnaa) ei voi syödä
                if (enemyPiece is Special)
                {
                    continue;
                }

                if ((Selected.XPos == enemyPiece.XPos) && (Selected.YPos + 1 == enemyPiece.YPos || Selected.YPos - 1 == enemyPiece.YPos))
                {
                    // Jos kuningas on linnassa tai sen vieressä
                    if (enemyPiece == King && (King.XPos == 5 && King.YPos == 5 || King.XPos == 4 && King.YPos == 5 ||
                        King.XPos == 6 && King.YPos == 5 || King.XPos == 5 && King.YPos == 4 || King.XPos == 5 && King.YPos == 6))
                    {
                        CheckKingCenterCapture(ToBeRemoved);
                        continue;
                    }



                    foreach (Piece allyPiece in Pieces)
                    {
                        if ((Selected is AttPiece && allyPiece is DefPiece) || (Selected is DefPiece && allyPiece is AttPiece))
                        {
                            // Vastapuolen nappulat eivät voi olla liittolaisia
                            continue;
                        }

                        if (allyPiece.Equals(Selected))
                        {
                            continue;
                        }

                        if (allyPiece.XPos != Selected.XPos)
                        {
                            continue;
                        }                        

                        if (allyPiece.XPos == 5 && allyPiece.YPos == 5 && King.XPos == 5 && King.YPos == 5)
                        {
                            // Linna ei voi syödä puolustajan nappuloita, jos kuningas on linnassa
                            if (Selected is AttPiece)
                            {
                                continue;
                            }
                        }

                        if (Selected.YPos > enemyPiece.YPos)
                        {
                            if (Selected.YPos - 2 == allyPiece.YPos)
                            {
                                ToBeRemoved.Add(enemyPiece);
                            }
                        }

                        else if (Selected.YPos < enemyPiece.YPos)
                        {
                            if (Selected.YPos + 2 == allyPiece.YPos)
                            {
                                ToBeRemoved.Add(enemyPiece);
                            }
                        }
                    }
                }

                else if ((Selected.YPos == enemyPiece.YPos) && (Selected.XPos + 1 == enemyPiece.XPos || Selected.XPos - 1 == enemyPiece.XPos))
                {
                    // Jos kuningas on linnassa tai sen vieressä
                    if (enemyPiece == King && (King.XPos == 5 && King.YPos == 5 || King.XPos == 4 && King.YPos == 5 ||
                        King.XPos == 6 && King.YPos == 5 || King.XPos == 5 && King.YPos == 4 || King.XPos == 5 && King.YPos == 6))
                    {
                        CheckKingCenterCapture(ToBeRemoved);
                        continue;
                    }

                    foreach (Piece allyPiece in Pieces)
                    {

                        if ((Selected is AttPiece && allyPiece is DefPiece) || (Selected is DefPiece && allyPiece is AttPiece))
                        {
                            continue;
                        }

                        if (allyPiece.Equals(Selected))
                        {
                            continue;
                        }

                        if (allyPiece.YPos != Selected.YPos)
                        {
                            continue;
                        }

                        if (allyPiece.XPos == 5 && allyPiece.YPos == 5 && King.XPos == 5 && King.YPos == 5)
                        {
                            // Linna ei voi syödä puolustajan nappuloita, jos kuningas on linnassa
                            if (Selected is AttPiece)
                            {
                                continue;
                            }
                        }

                        if (Selected.XPos > enemyPiece.XPos)
                        {
                            if (Selected.XPos - 2 == allyPiece.XPos)
                            {
                                ToBeRemoved.Add(enemyPiece);
                            }
                        }

                        else if (Selected.XPos < enemyPiece.XPos)
                        {
                            if (Selected.XPos + 2 == allyPiece.XPos)
                            {
                                ToBeRemoved.Add(enemyPiece);
                            }
                        }
                    }
                }
            }            

            if (ToBeRemoved.Count > 0)
            {
                // Lähetetään syödyt nappulat poistettavaksi
                RemoveCaptured(ToBeRemoved);
            }
        }

        private void CheckKingCenterCapture(List<Piece> toBeRemoved)
        {
            int x = King.XPos;
            int y = King.YPos;

            // Tarkistetaan onko linnassa tai sen vieressä oleva kuningas ympäröity
            if (PBoxGrid.Grid[x + 1, y].Tag.ToString() == "empty" || PBoxGrid.Grid[x + 1, y].Tag.ToString() == "defPiece")
                return;

            if (PBoxGrid.Grid[x - 1, y].Tag.ToString() == "empty" || PBoxGrid.Grid[x - 1, y].Tag.ToString() == "defPiece")
                return;

            if (PBoxGrid.Grid[x, y + 1].Tag.ToString() == "empty" || PBoxGrid.Grid[x, y + 1].Tag.ToString() == "defPiece")
                return;

            if (PBoxGrid.Grid[x, y - 1].Tag.ToString() == "empty" || PBoxGrid.Grid[x, y - 1].Tag.ToString() == "defPiece")
                return;

            toBeRemoved.Add(King);            
        }

        private void RemoveCaptured(List<Piece> deleteThese)
        {
            foreach (Piece piece in deleteThese)
            {
                // Poistetaan kaikki syödyt nappulat
                //PBoxGrid.Grid[piece.XPos, piece.YPos].BackColor = Color.Transparent;
                PBoxGrid.Grid[piece.XPos, piece.YPos].Image = null;
                PBoxGrid.Grid[piece.XPos, piece.YPos].Tag = "empty";
                Pieces.Remove(piece);

                if (piece == King)
                {
                    // Lopetetaan peli, jos kuningas on syöty
                    GameEnd("Hyökkääjä");
                }
            }
        }

        private void GameEnd(string winner)
        {
            // Luodaan ilmoitus, jossa ilmoitetaan pelin päättymisestä
            string message = winner + " voitti pelin";
            string caption = "Peli loppui!";
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            DialogResult result;


            result = MessageBox.Show(message, caption, buttons);
            if (result == DialogResult.OK)
            {
                NewGame();
            }
        }

        public void NewGame()
        {
            // Aloittaa pelin alusta
            foreach (PictureBox box in PBoxGrid.Grid)
            {
                box.Image = null;
                box.Tag = "empty";
            }

            CreatePieces();
        }
    }
}
