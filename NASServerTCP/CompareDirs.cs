using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NASServerTCP
{
    class CompareDirs
    {
        private string sourcePath = @"C:\Users\paweł\Documents\Visual Studio 2015\Projects\NASServerTCP\NASServerTCP\bin\Debug";
        private string destinationPath = @"C:\Users\paweł\Documents\Visual Studio 2015\Projects\NASServerTCP\NASServerTCP\bin\Debug\raid1";
        
        //A custom file comparer defined below  
        private FileCompare myFileCompare = new FileCompare();

        public CompareDirs() { }

        public void CompareDirectories()
        {
            System.IO.DirectoryInfo dir1 = new System.IO.DirectoryInfo(sourcePath);
            System.IO.DirectoryInfo dir2 = new System.IO.DirectoryInfo(destinationPath);

            // Take a snapshot of the file system.  
            IEnumerable<System.IO.FileInfo> list1 = dir1.GetFiles("*.*", System.IO.SearchOption.AllDirectories);
            IEnumerable<System.IO.FileInfo> list2 = dir2.GetFiles("*.*", System.IO.SearchOption.AllDirectories);

            if (AreIdentical(list1, list2, myFileCompare))
            {
                Console.WriteLine("the two folders are the same");
            }
            else
            {
                Console.WriteLine("The two folders are not the same");
                ReturnCommonFiles(list1, list2, myFileCompare);
                ReturnExeptFiles(list1, list2, myFileCompare);
            }

        }

        private bool AreIdentical(IEnumerable<System.IO.FileInfo> l1, 
            IEnumerable<System.IO.FileInfo> l2, FileCompare myFileCompare)
        {
            return (l1.SequenceEqual(l2, myFileCompare)) ? true : false; 
        }

        private IEnumerable<System.IO.FileInfo> ReturnCommonFiles(IEnumerable<System.IO.FileInfo> list1, 
            IEnumerable<System.IO.FileInfo> list2, FileCompare myFileCompare)
        {
            var queryCommonFiles = list1.Intersect(list2, myFileCompare);

            if (queryCommonFiles.Count() > 0)
            {
                Console.WriteLine("The following files are in both folders:");
                foreach (var v in queryCommonFiles)
                {
                    Console.WriteLine(v.FullName); //shows which items end up in result list  
                }
            }
            else
            {
                Console.WriteLine("There are no common files in the two folders.");
            }

            return queryCommonFiles; 
        }

        private IEnumerable<System.IO.FileInfo> ReturnExeptFiles(IEnumerable<System.IO.FileInfo> list1,
            IEnumerable<System.IO.FileInfo> list2, FileCompare myFileCompare)
        {
            var queryList1Only = (from file in list1
                                  select file).Except(list2, myFileCompare);

            Console.WriteLine("The following files are in list1 but not list2:");
            foreach (var v in queryList1Only)
            {
                Console.WriteLine(v.FullName);
            }

            return queryList1Only;
        }
    }
}
