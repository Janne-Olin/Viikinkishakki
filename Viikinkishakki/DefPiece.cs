﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Viikinkishakki
{
    class DefPiece : Piece
    {
        public DefPiece(int x, int y)
        {
            XPos = x;
            YPos = y;
            IconPath = "\\icons\\defPawn.png";
            SelectedIconPath = "\\icons\\defPawnSelected.png";
            Tag = "defPiece";
        }
    }
}
