using GrainGrowth.Lib.Enums;
using GrainGrowth.Lib.Extensions;
using GrainGrowth.Lib.Models;
using System;
using System.Collections.Generic;

namespace Grains.Library.Extensions.Helpers
{
    public class InclusionsHelper
    {
        public void AddInclusions(Grid grid, int amount, int size, Inclusions type, IList<Cell> borderCells)
        {
            var rnd = new Random();

            for (int i = 0; i < amount; i++)
            {
                int inclusionX = 0;
                int inclusionY = 0;

                if (borderCells.Count == 0)
                {
                    inclusionX = rnd.Next(grid.Width);
                    inclusionY = rnd.Next(grid.Height);
                }
                else
                {
                    var borderCell = borderCells[rnd.Next(borderCells.Count)];

                    inclusionX = borderCell.X;
                    inclusionY = borderCell.Y;

                    borderCells.Remove(borderCell);
                }

                switch (type)
                {
                    case Inclusions.Square:
                        {
                            DrawSquareInclusion(grid, inclusionX, inclusionY, size);
                            break;
                        }
                    case Inclusions.Circular:
                        {
                            DrawCircularInclusion(grid, inclusionX, inclusionY, size);
                            break;
                        }
                }
            }
        }

        private void DrawSquareInclusion(Grid grid, int x, int y, int size)
        {
            x = x - size;
            y = y - size;

            for (int i = 0; i < size * 2; i++)
            {
                for (int j = 0; j < size * 2; j++)
                {
                    var cell = new Cell(x + i, y + j, 1).ReworkeCell(grid);
                    grid.Add(cell);
                }
            }
        }

        private void DrawCircularInclusion(Grid grid, int x, int y, int size)
        {
            int circleA = x;
            int circleB = y;

            x = x - size;
            y = y - size;

            for (int i = 0; i < size * 2; i++)
            {
                for (int j = 0; j < size * 2; j++)
                {
                    if ((x + i - circleA) * (x + i - circleA) + (y + j - circleB) * (y + j - circleB) <= size * size)
                    {
                        var cell = new Cell(x + i, y + j, 1).ReworkeCell(grid);
                        grid.Add(cell);
                    }
                }
            }
        }
    }
}
