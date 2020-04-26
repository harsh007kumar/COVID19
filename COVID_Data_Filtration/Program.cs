using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COVID_Data_Filtration
{
    public class Covid
    {
        static string tabSpace= "	";
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
        public static void FindNoOfInfectedStatesAndUniqueDates(string textFilePath, ref List<string> infectedStates,ref List<DateTime> uniqueDates, ref StringBuilder noOfColumns)
        {
            using (StreamReader file = new StreamReader(textFilePath))
            {
                bool readLine = false;
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    string[] fields = line.Split('\t');
                    if (readLine)
                    {
                        if (!infectedStates.Contains(fields[2]))
                            infectedStates.Add(fields[2]);
                        if (!uniqueDates.Contains(Convert.ToDateTime(fields[1])))
                        {
                            uniqueDates.Add(Convert.ToDateTime(fields[1]));
                            noOfColumns.Append(tabSpace).Append(fields[1]);         // Append unique dates to List of Columns
                        }
                        else
                            noOfColumns.Append(fields[2]);                              // Append existing columns from text file 1st line
                    }
                    readLine = true;
                }
                file.Close();
            }
        }

        /// <summary>
        /// AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
        /// </summary>
        /// <param name="textFilePath"></param>
        /// <param name="patientList"></param>
        /// <param name="noOfColumns"></param>
        /// <param name="infectedStates"></param>
        /// <param name="uniqueDates"></param>
        /// <returns></returns>
        public static Dictionary<string,string> ProcessTxtFile(string textFilePath, ref Patient[] patientList, ref List<string> infectedStates, ref List<DateTime> uniqueDates)
        {
            Dictionary<string, string> State_And_DateWiseCases = new Dictionary<string, string>();      // Final Dictonary which is being flushed in Output text file
            Dictionary<string, int> State_And_NoOfCasesInCurrentDate = new Dictionary<string, int>();   // Dictonary to hold values of no of cases per state for a particular date.
            using (StreamReader file = new StreamReader(textFilePath))
            {
                int lineNo = 0;
                string line, previousDate = "", id, currentDate, stateName, stateCode, patientStatus;
                while ((line = file.ReadLine()) != null)
                {
                    string[] fields = line.Split('\t');
                    id = fields[0];
                    currentDate = fields[1];
                    stateName = fields[2];
                    stateCode = fields[3];
                    patientStatus = fields[4];
                    if (lineNo > 0)
                    {
                        if (currentDate != previousDate)
                        {
                            previousDate = currentDate;
                            uniqueDates.Add(Convert.ToDateTime(currentDate));                        // Add NewDate to the list of UniqueDates
                            Update(ref State_And_DateWiseCases, State_And_NoOfCasesInCurrentDate, uniqueDates.Count);
                            State_And_NoOfCasesInCurrentDate.Add(stateName, 1); // Add State Name with counter 1 for current date.
                        }
                        else
                            Update(ref State_And_NoOfCasesInCurrentDate, stateName);
                        
                        // Collecting List of of all unique Patients
                        patientList[lineNo - 1] = new Patient { ID = id, DateAnnounced = currentDate, StateName = stateName,
                            StateCode = stateCode, PatientCurrentState = (CurrentStatus)Enum.Parse(typeof(CurrentStatus), patientStatus) };
                    }
                    lineNo++;
                }
                file.Close();
                Console.WriteLine($"File has {lineNo} lines.");
            }
            return State_And_DateWiseCases;
        }

        private static void Update(ref Dictionary<string, string> state_And_DateWiseCases, Dictionary<string, int> state_And_NoOfCasesOnCurrentDate, int noOfDatesWithZeroCases)
        {
            int len = state_And_NoOfCasesOnCurrentDate.Count;
            if(len>=1)
            {
                foreach(KeyValuePair<string,int> currentState in state_And_NoOfCasesOnCurrentDate)          // State-wise update to master list
                {
                    if (state_And_DateWiseCases.ContainsKey(currentState.Key))
                        state_And_DateWiseCases[currentState.Key] += (tabSpace + currentState.Value);       // Update State Data with new cases for current date
                    else
                    {
                        string str = fillPreviousDates(noOfDatesWithZeroCases);
                        state_And_DateWiseCases.Add(currentState.Key, str+currentState.Value);              // Adding New State to DataSet where there have been Zero cases so far.
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("========================== START {0} ========================", DateTime.UtcNow);
            string SourceFile = "..\\..\\SourceFile.txt";
            StringBuilder noOfColumn = new StringBuilder();
            int noOfLines = Covid.ReadNoOfLines(SourceFile);
            List<DateTime> uniqueDates = new List<DateTime>(noOfLines);   // Generic List of type DateTime
            List<string> infectedStates = new List<string>(noOfLines);   // Generic List of type string
            Patient[] plist = new Patient[noOfLines];                   // List of Patients
            FindNoOfInfectedStatesAndUniqueDates(SourceFile,ref infectedStates, ref uniqueDates, ref noOfColumn);
            var AllPatientList = Covid.ProcessTxtFile(SourceFile,ref plist, ref infectedStates, ref uniqueDates);
            Console.WriteLine("========================== END {0} ==========================", DateTime.UtcNow);
            Console.ReadKey();
        }


        // Used only when new state which hasn't shown up yet in dataSet shows up and we need to fill 0 value for cases found on previous dates.
        private static string fillPreviousDates(int noOfDatesWithZeroCases)
        {
            string blankSpaces = "";
            while (noOfDatesWithZeroCases-- > 0)
                blankSpaces += (tabSpace + 0);
            return blankSpaces + tabSpace;
        }

        private static void Update(ref Dictionary<string, int> state_And_NoOfCasesOnCurrentDate, string stateName)
        {
            if (state_And_NoOfCasesOnCurrentDate.ContainsKey(stateName))
                state_And_NoOfCasesOnCurrentDate[stateName] += 1;
            else
                state_And_NoOfCasesOnCurrentDate.Add(stateName, 1);
        }
    }

    // Class to represent COVID patient details
    public class Patient
    {
        int _id;
        public string ID { get { return _id.ToString(); } set { _id = Convert.ToInt32(value); } }
        DateTime _date;
        public string DateAnnounced { get { return _date.ToString(); } set { _date = Convert.ToDateTime(value); } }
        public string StateName { get; set; }
        public string StateCode { get; set; }
        public CurrentStatus PatientCurrentState { get; set; }
    }

    // States of COVID-19 patients
    public enum CurrentStatus
    {
        Recovered=1,
        Hospitalized,
        Deceased
    }

}
