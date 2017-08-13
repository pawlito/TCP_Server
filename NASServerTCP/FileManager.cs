using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NASServerTCP
{
    class FileManager
    {
        private string sourcePath = @"C:\Users\paweł\Documents\Visual Studio 2015\Projects\NASServerTCP\NASServerTCP\bin\Debug";
        private string destinationPath = @"C:\Users\paweł\Documents\Visual Studio 2015\Projects\NASServerTCP\NASServerTCP\bin\Debug\raid1";
        public FileManager()
        {

        }

        public Boolean CopyFile(string fileName)
        {
            Boolean returnCode = true;

            try
            {
                if (!Directory.Exists(destinationPath))
                {
                    Directory.CreateDirectory(destinationPath);
                }
                string sourceFile = Path.Combine(sourcePath, fileName);
                string destFile = Path.Combine(destinationPath, fileName);

                File.Copy(sourceFile, destFile, true);

            }
            catch (IOException e)
            {
                returnCode = false;
            }

            return returnCode;
        }

        public Boolean DeleteFile(string fileName)
        {
            Boolean returnCode = true;
  
            string sourceFile = Path.Combine(sourcePath, fileName);
            if (File.Exists(sourceFile))
            {
                try
                {
                    FileInfo fi = new FileInfo(sourceFile);
                    fi.Delete();

                }
                catch (IOException e)
                {
                    returnCode = false;
                }
            }
                    
            

            return returnCode;
        }
        public Boolean RenameFile(string oldName, string newName)
        {
            Boolean returnCode = true;

            try
            {
                string sourceFile = Path.Combine(destinationPath, oldName);
                string destFile = Path.Combine(destinationPath, newName);

                File.Move(sourceFile, destFile);

            }
            catch(IOException e)
            {
                returnCode = false;
            }

            return returnCode;
        }

    }
}
