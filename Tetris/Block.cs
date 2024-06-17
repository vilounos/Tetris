using System.Collections.Generic;

namespace Tetris
{
    public abstract class Block
    {

        // Pozice dílku
        protected abstract Position[][] Tiles { get; }

        // Počáteční bod
        protected abstract Position StartOffset { get; }

        // ID bloku
        public abstract int Id { get; }

        private int rotationState;
        private Position offset;

        public Block()
        {

            // offset nastavit na Počáteční bod
            offset = new Position(StartOffset.Row, StartOffset.Column);
        }

        public IEnumerable<Position> TilePositions()
        {
            foreach (Position p in Tiles[rotationState])
            {
                yield return new Position(p.Row + offset.Row, p.Column + offset.Column);
            }
        }


        // Otočení vpravo
        public void RotateCW()
        {
            rotationState = (rotationState + 1) % Tiles.Length;
        }


        // Otočení vlevo
        public void RotateCCW()
        {
            if (rotationState == 0)
            {
                rotationState = Tiles.Length - 1;
            }
            else
            {
                rotationState--;
            }
        }


        // Pohyb vlevo, vpravo, dolů
        public void Move(int rows, int columns)
        {
            offset.Row += rows;
            offset.Column += columns;
        }


        // Nastaví dílek na počáteční bod se základní rotací
        public void Reset()
        {
            rotationState = 0;
            offset.Row = StartOffset.Row;
            offset.Column = StartOffset.Column;
        }
    }
}
