using System;
using System.IO;


namespace GrainGrowth.Lib.Handlers
{
    public static class TextHandler
    {
        public static void ExportToTextFile(int[,] array, int xDimension, int yDimension, string path)
        {
            using (var outputFile = new StreamWriter(path))
            {
                for (int i = 0; i < xDimension; i++)
                {
                    for (int j = 0; j < xDimension; j++)
                    {
                        outputFile.Write(array[i, j] + " ");
                    }
                    outputFile.WriteLine();
                }
            }
        }

        public static void ImportFromTextFile(int[,] array, int xDimension, int yDimension, string path)
        {
            using (var inputFile = new StreamReader(path))
            {
                for (int i = 0; i < xDimension; i++)
                {
                    var line = inputFile.ReadLine().Split();

                    for (int j = 0; j < xDimension; j++)
                    {
                        array[i, j] = Convert.ToInt32(line[j]);
                    }
                }
            }
        }
    }
}
