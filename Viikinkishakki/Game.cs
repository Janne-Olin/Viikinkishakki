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
        public DefPiece King { get; set; }

        public Game()
        {
            Selected = null;
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

            King = new DefPiece(5, 5);
            Pieces.Add(King);

            AddToBoard();
        }

        private void AddToBoard()
        {
            // Asetetaan nappulat paikoilleen ja asetetaan niille oikeat värit
            foreach (Piece piece in Pieces)
            {
                int x = piece.XPos;
                int y = piece.YPos;

                if (piece is AttPiece)
                {
                    //PBoxGrid.Grid[x, y].BackColor = Color.Brown;
                    PBoxGrid.Grid[x, y].Image = Image.FromFile("attPawn.png");

                }
                else if (piece is DefPiece)
                {
                    if (piece != King)
                    {
                        //PBoxGrid.Grid[x, y].BackColor = Color.Orange;
                        PBoxGrid.Grid[x, y].Image = Image.FromFile("defPawn.png");
                    }
                    else
                    {
                        //PBoxGrid.Grid[x, y].BackColor = Color.Gold;
                        PBoxGrid.Grid[x, y].Image = Image.FromFile("king.png");
                    }
                }

                else if (piece is Special)
                {
                    PBoxGrid.Grid[x, y].BackColor = Color.DarkSlateGray;
                }
            }
        }

        public void BoardClicked(int x, int y)
        {
            // Tarkastetaanko onko nappulaa valittuna
            if (Selected != null)
            {
                MakeMove(x, y);
                return;
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
                    Selected = piece;
                    PBoxGrid.Grid[x, y].BackColor = Color.LightBlue;
                    break;
                }
            }
        }

        private void MakeMove(int x, int y)
        {
            if (MoveIsLegal(x, y))
            {
                // Poistetaan valittu nappula alkuperäisestä sijainnista
                PBoxGrid.Grid[Selected.XPos, Selected.YPos].BackColor = Control.DefaultBackColor;
                PBoxGrid.Grid[Selected.XPos, Selected.YPos].Image = null;

                // Päivitetään valitun nappulan sijainti
                Selected.XPos = x;
                Selected.YPos = y;
                AddToBoard();
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

                Selected = null;
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
                        if (PBoxGrid.Grid[i, y].BackColor != Control.DefaultBackColor || PBoxGrid.Grid[i, y].Image != null)
                        {
                            if (PBoxGrid.Grid[i, y].BackColor == Color.DarkSlateGray)
                            {
                                // Kuningas saa liikkua erikoisruutuihin ja muut nappulat saavat kulkea linnan läpi
                                continue;
                            }

                            return false;
                        }
                    }
                }
                else if (Selected.XPos < x)
                {
                    for (int i = Selected.XPos + 1; i <= x; i++)
                    {
                        if (PBoxGrid.Grid[i, y].BackColor != Control.DefaultBackColor || PBoxGrid.Grid[i, y].Image != null)
                        {
                            if (PBoxGrid.Grid[i, y].BackColor == Color.DarkSlateGray)
                            {
                                // Kuningas saa liikkua erikoisruutuihin ja muut nappulat saavat kulkea linnan läpi
                                continue;
                            }

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
                        if (PBoxGrid.Grid[x, i].BackColor != Control.DefaultBackColor || PBoxGrid.Grid[x, i].Image != null)
                        {
                            if (PBoxGrid.Grid[x, i].BackColor == Color.DarkSlateGray)
                            {
                                // Kuningas saa liikkua erikoisruutuihin ja muut nappulat saavat kulkea linnan läpi
                                continue;
                            }

                            return false;
                        }
                    }
                }
                else if (Selected.YPos < y)
                {
                    for (int i = Selected.YPos + 1; i <= y; i++)
                    {
                        if (PBoxGrid.Grid[x, i].BackColor != Control.DefaultBackColor || PBoxGrid.Grid[x, i].Image != null)
                        {
                            if (PBoxGrid.Grid[x, i].BackColor == Color.DarkSlateGray)
                            {
                                // Kuningas saa liikkua erikoisruutuihin ja muut nappulat saavat kulkea linnan läpi
                                continue;
                            }

                            return false;
                        }
                    }
                }
            }

            return true;
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
                            // Linna ei voi syödä nappuloita, jos kuningas on linnassa
                            continue;
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
                            // Linna ei voi syödä nappuloita, jos kuningas on linnassa
                            continue;
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
            if (PBoxGrid.Grid[x + 1, y].BackColor != Color.Brown && PBoxGrid.Grid[x + 1, y].BackColor != Color.DarkSlateGray)
                return;

            if (PBoxGrid.Grid[x - 1, y].BackColor != Color.Brown && PBoxGrid.Grid[x - 1, y].BackColor != Color.DarkSlateGray)
                return;

            if (PBoxGrid.Grid[x, y + 1].BackColor != Color.Brown && PBoxGrid.Grid[x, y + 1].BackColor != Color.DarkSlateGray)
                return;

            if (PBoxGrid.Grid[x, y - 1].BackColor != Color.Brown && PBoxGrid.Grid[x, y - 1].BackColor != Color.DarkSlateGray)
                return;

            toBeRemoved.Add(King);            
        }

        private void RemoveCaptured(List<Piece> deleteThese)
        {
            foreach (Piece piece in deleteThese)
            {
                // Poistetaan kaikki syödyt nappulat
                //PBoxGrid.Grid[piece.XPos, piece.YPos].BackColor = Control.DefaultBackColor;
                PBoxGrid.Grid[piece.XPos, piece.YPos].Image = null;
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
                box.BackColor = Control.DefaultBackColor;
                box.Image = null;
            }

            CreatePieces();
        }
    }
}
