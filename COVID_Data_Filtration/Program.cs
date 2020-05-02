using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using COVID_Data_Filtration;

namespace COVID_Data_Filtration
{
    public class Covid
    {
        static string tabSpace= "	";
        public static void FindNoOfInfectedStatesAndUniqueDates(string textFilePath, ref List<string> infectedStates,ref List<DateTime> uniqueDates, ref StringBuilder noOfColumns)
        {
            using (StreamReader file = new StreamReader(textFilePath))
            {
                bool readLine = false;
                string line;
                DateTime dt;
                while ((line = file.ReadLine()) != null)
                {
                    string[] fields = line.Split('\t');
                    if (readLine)
                    {
                        if (!infectedStates.Contains(fields[2]))
                            infectedStates.Add(fields[2]);                                          // Append unique states to List
                        if (!uniqueDates.Contains(Convert.ToDateTime(fields[1])))
                        {
                            dt = Convert.ToDateTime(fields[1]);
                            uniqueDates.Add(dt);                                                    // Append unique dates to list
                            noOfColumns.Append(tabSpace).Append(dt.ToString("dd/MMM/yyyy"));        // Append unique dates to List of Columns
                        }
                    }
                    else
                        noOfColumns.Append(fields[1]);                                              // Append existing columns from text file 1st line
                    readLine = true;
                }
                file.Close();
            }
        }

        /// <summary>
        /// Processes SourceText file and return formatted data as Dictonary with key as states and value as count of cases per day as string value
        /// </summary>
        /// <param name="textFilePath">Path of source txt file</param>
        /// <param name="patientList">List which includes details of each unqiue patient</param>
        /// <param name="noOfColumns">Heading of output file containing all unique dates separated by tab i.e. \t </param>
        /// <param name="infectedStates">List which contains list of all infected states which have reported cases so far</param>
        /// <returns>FormattedData with key as States & count of patients on each days as it's value</returns>
        public static Dictionary<string,string> ProcessTxtFile(string textFilePath, ref Patient[] patientList, ref List<string> infectedStates)
        {
            Dictionary<string, string> MasterData_State = new Dictionary<string, string>();      // Final Dictonary which is being flushed in Output text file
            Dictionary<string, int> CurrentDate_Data = new Dictionary<string, int>();   // Dictonary to hold values of no of cases per state for a particular date.
            InitializeStates(ref CurrentDate_Data, infectedStates);               // Reset All States Data to Zero
            using (StreamReader file = new StreamReader(textFilePath))
            {
                int lineNo = 0;
                string line, previousDate = "", id, currentDate, stateName, stateCode, patientStatus;
                bool skipOnce = false;
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
                            if(skipOnce)
                                FlushDataTillLastDate(ref MasterData_State, CurrentDate_Data);  skipOnce = true;
                            previousDate = currentDate;
                        }
                        CurrentDate_Data[stateName] += 1;
                        
                        // Collecting List of of all unique Patients
                        patientList[lineNo - 1] = new Patient { ID = id, DateAnnounced = currentDate, StateName = stateName, StateCode = stateCode, PatientCurrentState = patientStatus };
                    }
                    lineNo++;
                }
                FlushDataTillLastDate(ref MasterData_State, CurrentDate_Data);          // add final date entry to master data.
                file.Close();
                Console.WriteLine($"File has {lineNo} lines.");
            }
            return MasterData_State;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("========================== START {0} ========================", DateTime.UtcNow);
            //string SourceFile = "..\\..\\SourceFile.txt";
            //string OutputFile = "..\\..\\OutputFile.txt";
            //StringBuilder noOfColumn = new StringBuilder();
            //int noOfLines = CommonUtility.ReadNoOfLines(SourceFile);
            //List<DateTime> uniqueDates = new List<DateTime>(noOfLines);             // Generic List of type DateTime
            //List<string> infectedStates = new List<string>(noOfLines);              // Generic List of type string
            //Patient[] plist = new Patient[noOfLines];                               // List of Patients
            //FindNoOfInfectedStatesAndUniqueDates(SourceFile,ref infectedStates, ref uniqueDates, ref noOfColumn);
            //// Processing Data
            //Dictionary<string,string> FormattedData = ProcessTxtFile(SourceFile,ref plist, ref infectedStates);
            //string[] outputBuffer = new string[infectedStates.Count + 1];           // array to hold each state data plus column fields
            //PrepareOutputBuffer(ref outputBuffer, noOfColumn, FormattedData);       // created Output buffer which is array of strings from FormattedData dictonary
            //CommonUtility.WriteToOutputFile(OutputFile, outputBuffer);                            // Flush data



            //string SourceFile2 = "..\\..\\SourceFile2.txt";
            //string OutputFile2 = "..\\..\\OutputFile2.txt";
            //string[] contents = CommonUtility.FetchContentsOfTextFile(SourceFile2);
            //string[] toProcess = CommonUtility.removeIndex(contents);
            //string[] outputBuffer = new string[contents.Length - 1];
            //for (int i = 1; i < contents.Length; i++)
            //{
            //    int[] lineArray = CommonUtility.ConvertLineToIntArray(contents[i]);
            //    lineArray = CommonUtility.SumInFibonaciSeries(lineArray);
            //    outputBuffer[i - 1] = CommonUtility.ConvertIntArrayToLine(lineArray);
            //}
            //CommonUtility.WriteToOutputFile(OutputFile2, outputBuffer);                            // Flush data
            //Console.WriteLine("========================== END {0} ==========================", DateTime.UtcNow);
            //Console.ReadKey();



            // ====================== IPL Data ============================= //
            string SourceFile = "..\\..\\IPL_Data.txt";
            string OutputFile = "..\\..\\IPL_OutPut.txt";
            StringBuilder FirstLine = new StringBuilder();
            int noOfLines = CommonUtility.ReadNoOfLines(SourceFile);
            List<DateTime> uniqueDates = new List<DateTime>();                      // Generic List of type DateTime
            List<string> teams = new List<string>();                                // Generic List to fetch 2nd primary key/unique key in input file
            Dictionary<string, string> teamsData = new Dictionary<string, string>();
            FetchNoOfUniqueKeysAndDates(SourceFile, ref teams, ref uniqueDates, ref FirstLine);
            // Processing Data
            Dictionary<string,string> FormattedData = SeasonStatsForTeams(SourceFile, ref teamsData, uniqueDates, teams);

            string[] outputBuffer = new string[teams.Count + 1];                    // array to hold each team data plus column fields
            PrepareOutputBuffer(ref outputBuffer, FirstLine, FormattedData);        // created Output buffer which is array of strings from FormattedData dictonary
            CommonUtility.WriteToOutputFile(OutputFile, outputBuffer);              // Flush data
        }

        private static void FetchNoOfUniqueKeysAndDates(string textFilePath, ref List<string> teams, ref List<DateTime> uniqueDates, ref StringBuilder firstLine)
        {
            using (StreamReader file = new StreamReader(textFilePath))
            {
                bool readLine = false;
                string line;
                DateTime dt;
                while ((line = file.ReadLine()) != null)
                {
                    string[] fields = line.Split('\t');
                    if (readLine)
                    {
                        if (!teams.Contains(fields[0]))
                            teams.Add(fields[0]);                                          // Append unique states to List
                        if (!uniqueDates.Contains(Convert.ToDateTime(fields[1])))
                        {
                            dt = Convert.ToDateTime(fields[1]);
                            uniqueDates.Add(dt);                                                    // Append unique dates to list
                            firstLine.Append(tabSpace).Append(dt.ToString("dd/MMM/yyyy"));        // Append unique dates to List of Columns
                        }
                    }
                    else
                        firstLine.Append(fields[1]);                                              // Append existing columns from text file 1st line
                    readLine = true;
                }
                file.Close();
            }
        }

        private static Dictionary<string, string> SeasonStatsForTeams(string sourceFile, ref Dictionary<string, string> teamsWinData, List<DateTime> uniqueDates, List<string> teams)
        {
            Dictionary<string, string> MasterData = new Dictionary<string, string>();       // Final Dictonary which is being flushed in Output text file
            Dictionary<string, int> CurrentDate_Data = new Dictionary<string, int>();       // Dictonary to hold values of team win on a particular date.
            InitializeTeams(ref CurrentDate_Data, teams);                                   // Reset All Teams Data to Zero
            using (StreamReader file = new StreamReader(sourceFile))
            {
                int lineNo = 0;
                string line, previousDate = "", teamName, currentDate;
                bool skipOnce = false;
                while ((line = file.ReadLine()) != null)
                {
                    string[] fields = line.Split('\t');
                    teamName = fields[0];
                    currentDate = fields[1];
                    if (lineNo > 0)
                    {
                        if (currentDate != previousDate)
                        {
                            if (skipOnce)
                                FlushDataTillLastDate(ref MasterData, CurrentDate_Data); skipOnce = true;
                            previousDate = currentDate;
                        }
                        CurrentDate_Data[teamName] += 1;
                    }
                    lineNo++;
                }
                FlushDataTillLastDate(ref MasterData, CurrentDate_Data);                    // add final date entry to master data.
                file.Close();
                Console.WriteLine($"File has {lineNo} lines.");
            }
            return MasterData;
        }

        private static void InitializeTeams(ref Dictionary<string, int> currentDate_Data, List<string> teams)
        {
            foreach (string team in teams)
                if (!currentDate_Data.ContainsKey(team))
                    currentDate_Data.Add(team, 0);                                                  // add team if not present
        }

        private static void PrepareOutputBuffer(ref string[] outputBuffer, StringBuilder noOfColumn, Dictionary<string, string>formattedData)
        {
            outputBuffer[0] = noOfColumn.ToString();
            int i = 1;
            foreach (KeyValuePair<string, string> line in formattedData)
                outputBuffer[i++] = line.Key + tabSpace + line.Value;
        }

        // update Data for each state till current date
        private static void FlushDataTillLastDate(ref Dictionary<string, string> masterData_State, Dictionary<string, int> currentDate_Data)
        {
            foreach (KeyValuePair<string, int> currentState in currentDate_Data)                // State-wise update to master list
            {
                if (masterData_State.ContainsKey(currentState.Key))
                    masterData_State[currentState.Key] += (tabSpace + currentState.Value);      // Update State Data with new cases for current date
                else
                {
                    masterData_State.Add(currentState.Key, ""+currentState.Value);                 // Adding New State to DataSet where there have been Zero cases so far.
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

        private static void InitializeStates(ref Dictionary<string, int> currentDate_Data, List<string> infectedStates, int initialValue = 0)
        {
            foreach (string state in infectedStates)
                if (!currentDate_Data.ContainsKey(state))
                    currentDate_Data.Add(state, initialValue);                                         // add state if not present
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
