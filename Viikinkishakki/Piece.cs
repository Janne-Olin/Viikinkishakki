using System;
using System.Collections.Generic;
using System.Text;

namespace Viikinkishakki
{
    abstract class Piece
    {
        public int XPos { get; set; }
        public int YPos { get; set; }
        public string IconPath { get; set; }
        public string SelectedIconPath { get; set; }
        public string Tag { get; set; }
    }
}
