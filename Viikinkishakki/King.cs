using System;
using System.Collections.Generic;
using System.Text;

namespace Viikinkishakki
{
    class King : DefPiece
    {
        public King(int x, int y):base(x, y)
        {
            XPos = x;
            YPos = y;
            IconPath = "\\icons\\king.png";
            SelectedIconPath = "\\icons\\kingSelected.png";
            Tag = "king";
        }
    }
}
