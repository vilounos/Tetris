namespace Tetris
{

    // Pozice
    public class Position
    {
        //Řada
        public int Row { get; set; }

        //Sloupec
        public int Column { get; set; }


        // Pozice
        public Position(int row, int column)
        {
            Row = row;
            Column = column;
        }
    }
}
