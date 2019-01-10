
using GrainGrowth.Lib.Handlers;
using Grains.Utilities.IOHandlers;
using Microsoft.Win32;
using System;
using System.IO;


namespace GrainGrowth.Lib.Handlers
{
    public class OpeningHandler : IOHandler
    {
        public OpeningHandler(string filter) : base(filter)
        {
        }

        public override string GetPath()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = this.filter;
            string path = string.Empty;

            if (openFileDialog.ShowDialog() == true)
            {
                path = openFileDialog.FileName;

                if (!File.Exists(path))
                {
                    throw new Exception("Specified file doesn't exist");
                }
            }

            return path;
        }
    }
}
