using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace COVID_Data_Filtration
{
    class CommonUtility
    {
        /// <summary>
        /// Read no of line from text file and returns the count as int value
        /// </summary>
        /// <param name="textFilePath"></param>
        /// <returns></returns>
        public static int ReadNoOfLines(string textFilePath)
        {
            using (StreamReader file = new StreamReader(textFilePath))
            {
                int noOfLines = 0;
                while (file.ReadLine() != null)
                    noOfLines++;
                file.Close();
                Console.WriteLine($"File has {noOfLines} lines.");
                return noOfLines;
            }
        }

        /// <summary>
        /// Write the array of strings in specified output file, each string index is stored line by line
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="outputBuffer"></param>
        public static void WriteToOutputFile(string sourceFile, string[] outputBuffer)
        {
            File.WriteAllLines(sourceFile, outputBuffer);
            Console.WriteLine($"Output file is created succesfully at location : {sourceFile}");
        }

        public static string[] FetchContentsOfTextFile(string textFilePath)
        {
            using (StreamReader file = new StreamReader(textFilePath))
            {
                int noOfLines = 0;
                string line;
                List<string> lines = new List<string>();
                while ((line = file.ReadLine()) != null)
                {
                    //string[] fields = line.Split('\t');
                    lines.Add(line);
                    noOfLines++;
                }
                file.Close();
                Console.WriteLine($"File has {noOfLines} lines.");
                return lines.ToArray();
            }
        }

        /// <summary>
        /// Function which take string input and splits it using passed seperator and return int array of numbers present in string
        /// </summary>
        /// <param name="line"></param>
        /// <param name="splitby"></param>
        /// <returns></returns>
        public static int[] ConvertLineToIntArray(string line, char splitby='\t')
        {
            string[] fields = line.Split(splitby);
            int[] array = new int[fields.Length-1];
            for (int i = 1; i < fields.Length; i++)
                array[i-1] = Convert.ToInt32(fields[i]);
            return array;
        }

        /// <summary>
        /// Function which take int array of numbers snf returns string with values seperated by '\t'
        /// </summary>
        /// <param name="array"></param>
        /// <param name="splitby"></param>
        /// <returns></returns>
        public static string ConvertIntArrayToLine(int[] array, char splitby = '\t')
        {
            StringBuilder line = new StringBuilder();
            for (int i = 0; i < array.Length; i++)
                line.Append(array[i]).Append('\t');
            return line.ToString();
        }

        /// <summary>
        /// For Input 1,1,1,1,1 return fibonacci way of sum like 1,2,3,4,5 as Output
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static int[] SumInFibonaciSeries(int[] array)
        {
            for (int index = 1; index < array.Length; index++)
                array[index] += array[index - 1];
            return array;
        }

        public static string[] removeIndex(string[] array, int indexToBeRemoved = 0)
        {
            int len;
            if ((len = array.Length) == 1)
                return new string[0];
            else
            {
                string[] newArray = new string[len - 1];
                for (int i = 1; i < len; i++)
                    newArray[i - 1] = array[i];
                return newArray;
            }
        }

    }
}
