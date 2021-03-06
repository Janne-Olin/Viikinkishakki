using System;
using System.Collections.Generic;
using System.Text;

namespace Viikinkishakki
{
    class AttPiece : Piece
    {
        public AttPiece(int x, int y)
        {
            XPos = x;
            YPos = y;
            IconPath = "\\icons\\attPawn.png";
            SelectedIconPath = "\\icons\\attPawnSelected.png";
            Tag = "attPiece";

        }
    }
}
