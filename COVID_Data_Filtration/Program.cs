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
        public static Dictionary<string,string> ProcessTxtFile(string textFilePath, ref Patient[] patientList, ref List<string> infectedStates)
        {
            Dictionary<string, string> MasterData_State = new Dictionary<string, string>();      // Final Dictonary which is being flushed in Output text file
            Dictionary<string, int> CurrentDate_Data = new Dictionary<string, int>();   // Dictonary to hold values of no of cases per state for a particular date.
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
                            FlushDataTillLastDate(ref MasterData_State, CurrentDate_Data);
                            ResetStates(ref CurrentDate_Data, infectedStates);               // Reset All States Data to Zero
                            previousDate = currentDate;
                        }
                        CurrentDate_Data[stateName] += 1;
                        
                        // Collecting List of of all unique Patients
                        patientList[lineNo - 1] = new Patient { ID = id, DateAnnounced = currentDate, StateName = stateName, StateCode = stateCode, PatientCurrentState = patientStatus };
                    }
                    lineNo++;
                }
                file.Close();
                Console.WriteLine($"File has {lineNo} lines.");
            }
            return MasterData_State;
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
            var FormattedData = Covid.ProcessTxtFile(SourceFile,ref plist, ref infectedStates);
            Console.WriteLine("========================== END {0} ==========================", DateTime.UtcNow);
            Console.ReadKey();
        }

        // update Data for each state till current date
        private static void FlushDataTillLastDate(ref Dictionary<string, string> masterData_State, Dictionary<string, int> currentDate_Data)
        {
            foreach (KeyValuePair<string, int> currentState in currentDate_Data)          // State-wise update to master list
            {
                if (masterData_State.ContainsKey(currentState.Key))
                    masterData_State[currentState.Key] += (tabSpace + currentState.Value);       // Update State Data with new cases for current date
                else
                {
                    masterData_State.Add(currentState.Key, tabSpace + currentState.Value);              // Adding New State to DataSet where there have been Zero cases so far.
                }
            }
        }

        private static void ResetStates(ref Dictionary<string, int> currentDate_Data, List<string> infectedStates, int initialValue=0)
        {
            foreach (string state in infectedStates)
            {
                if (currentDate_Data.ContainsKey(state))
                    currentDate_Data[state] = initialValue;                                            // reset values to zero if preset
                else
                    currentDate_Data.Add(state, initialValue);                                         // add state if not present
            }
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
        CurrentStatus _currentState;
        public string PatientCurrentState { get { return Enum.GetName(typeof(CurrentStatus), _currentState).ToString(); } set { _currentState = (CurrentStatus)Enum.Parse(typeof(CurrentStatus), value); } }
    }

    // States of COVID-19 patients
    public enum CurrentStatus
    {
        Recovered=1,
        Hospitalized,
        Deceased
    }

}
