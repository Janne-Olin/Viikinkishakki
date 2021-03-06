using System;
using System.Collections.Generic;
using System.Text;

namespace Viikinkishakki
{
    class Special : Piece
    {
        // Kuninkaan linna ja kulmaruudut
        public Special(int x, int y)
        {
            XPos = x;
            YPos = y;
            Tag = "special";
        }
    }
}
