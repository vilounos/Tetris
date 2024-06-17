using System;

namespace Tetris
{

    // Nastavování pořadí dílků
    public class BlockQueue
    {

        // Přidání všech dílků
        private Block[] blocks = new Block[]
        {
            new IBlock(),
            new JBlock(),
            new LBlock(),
            new OBlock(),
            new SBlock(),
            new TBlock(),
            new ZBlock()
        };

        // Random pro náhodnou řadu dílků
        private Random random = new Random();

        // Další dílek v řadě
        public Block NextBlock { get; private set; }


        // Nastavení náhodného dalšího dílku
        public BlockQueue()
        {
            NextBlock = RandomBlock();
        }

        // Nastavení random dalšího dílku
        private Block RandomBlock()
        {
            return blocks[random.Next(blocks.Length)];
        }

        // Další blok jde na scénu
        public Block GetAndUpdate()
        {
            Block block = NextBlock;

            do
            {
                NextBlock = RandomBlock();
            }
            while (block.Id == NextBlock.Id);

            return block;
        }
    }
}
