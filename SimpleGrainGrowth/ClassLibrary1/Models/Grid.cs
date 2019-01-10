using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Grains.Library.Extensions;
using Grains.Library.Extensions.Helpers;
using GrainGrowth.Lib.Extensions;

namespace GrainGrowth.Lib.Models
{
    public class Grid
    {
        public int Width { get; set; }
        public int Height { get; set; }
    
        public bool[,] NotEmptyCells { get; set; }
        public int IdsNumber
        {
            get
            {
                return Cells.Max();
            }
        }
        public BorderStyle Border { get; set; }

        public List<int> ReservationId { get; set; }

        public int[,] Cells { get; set; }

        public int[,] Energy { get; set; }

        public List<Cell> CellsWhereBorderId { get; set; }
        public Grid(int size) : this(size, size)
        {
            this.ReservationId = new List<int>();
        }


        public IList<Cell> BorderCells
        {
            get
            {
               
                var borderCellsHelper = new BorderCellsHelpers();
                borderCells = borderCellsHelper.GetBorderCells(this);
               

                return borderCells;
            }
        }
        private IList<Cell> borderCells;

        public IList<Cell> ShuffledCells
        {
            get
            {
                
                shuffledCells = new List<Cell>(CellsWhereBorderId);
                var random = new Random();
                shuffledCells.Shuffle(random);
                wereCellsShuffled = true;
             

                return shuffledCells;
            }
        }

        private IList<Cell> shuffledCells;
        private bool wereCellsShuffled;

        public IList<Cell> ShuffledBorderCells
        {
            get
            {
               
                shuffledBorderCells = new List<Cell>(BorderCells);
                var random = new Random();
                shuffledBorderCells.Shuffle(random);
                wereBorderCellsShuffled = true;
               

                return shuffledBorderCells;
            }
        }

        private IList<Cell> shuffledBorderCells;
        private bool wereBorderCellsShuffled;
        public Grid(int width, int heigth)
        {
            this.Cells = new int[width, heigth];
            this.Energy = new int[width, heigth];
            this.Width = width;
            this.Height = heigth;
            this.ReservationId = new List<int>();
            this.NotEmptyCells = new bool[width, heigth];
            this.shuffledCells = new List<Cell>();
            this.shuffledBorderCells = new List<Cell>();
            this.borderCells = new List<Cell>();

            this.CellsWhereBorderId = new List<Cell>();
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    this.CellsWhereBorderId.Add(new Cell(i, j, Cells[i, j]));
                }
            }

        }

        public void Add(Cell cell)
        {
            this.Cells[cell.X, cell.Y] = cell.Id;
            this.NotEmptyCells[cell.X, cell.Y] = true;
        }
    }

    public enum BorderStyle { Transient, Closed }
}
