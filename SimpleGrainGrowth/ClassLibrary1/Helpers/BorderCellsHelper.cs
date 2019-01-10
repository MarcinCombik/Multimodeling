using GrainGrowth.Lib.Extensions;
using GrainGrowth.Lib.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grains.Library.Extensions.Helpers
{
    public class BorderCellsHelpers
    {
        public IList<Cell> GetBorderCells(Grid grid)
        {
            var borderCells = new ConcurrentBag<Cell>();

            Parallel.For(0, grid.Width, (i) =>
            {
                Parallel.For(0, grid.Height, (j) =>
                {
                    var currentCell = new Cell(i, j, grid.Cells[i, j]);
                    if (CheckForDifference(currentCell, grid))
                    {
                        borderCells.Add(currentCell);
                    }
                });
            });

            return new List<Cell>(borderCells);
        }

        private bool CheckForDifference(Cell currentCell, Grid grid)
        {
            if (currentCell.Id == 1)
            {
                return false;
            }

            foreach (var point in Unites.Unites.MooreCoordinates)
            {
                var tempCell = currentCell.Get(point.X, point.Y).ReworkeCell(grid);
                tempCell.Id = grid.Cells[tempCell.X, tempCell.Y];

                if (tempCell.Id != currentCell.Id && tempCell.Id != 1)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
