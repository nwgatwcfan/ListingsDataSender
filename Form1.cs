using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Data;
using System.Net;
using System.Windows.Forms;
using System.Globalization;
using System.Data.SqlClient;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using DataFunctions;
using Calculator;
using Weather;
using FileIO;
using SerialIO;
using LineupBuilder;
using ListingsBuilder;

namespace WindowsFormsApplication1
{
    

    public partial class Form1 : Form
    {
        public class DaypartData
        {
            public int DaypartSelected { get; set; }
            public string DaypartValue { get; set; }
        }

        // Global Variables throughout program:

        readonly string firstrunfileName = String.Format(@"{0}\settings.txt", Application.StartupPath);

        public string FolderPath;
        char FirstRun;

        int wu_minutes;
        int wu_seconds;

        //User Set Variables
        public int timeslotselection;
        public int adjustedJulianDateselection;
        public int channeladjustedJulianDateselection;
        public int listingattrselection;
        public char[] empty = new char[] { };

        


    // Functions Section: Functions needed for program

    private void Timer_Tick(object sender, EventArgs e)
        {
            // Clock Timer
            DateTime curday = DateTime.Now;
            CurDateTime.Text = "Current Date and Time: " + DateTime.Now.ToString();
            CurrentDayofYear.Text = "Current Day of the Year: " + curday.DayOfYear.ToString();
        }



        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Time Slot selection box
            if (BlackoutMask.Checked == true)
            {
                timeslotselection = comboBox1.SelectedIndex + 33;
            }
            else
            {
                timeslotselection = comboBox1.SelectedIndex + 1;
            }
        }

        private void ComboBox2_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {
                serialPort1.Close();
                Properties.Settings.Default.COMPort = comboBox2.SelectedItem.ToString();
                Properties.Settings.Default.Save();
                serialPort1.PortName = Properties.Settings.Default.COMPort;
                serialPort1.Open();
            }
            catch
            {
                MessageBox.Show("Error setting serial port: ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation,
    MessageBoxDefaultButton.Button1);
            }
        }

        public void Calculate_JDate(object sender, EventArgs e)
        {
            //Time String - Input the Julian Date Julian Date is in Decimal Form
            //Calculate Julian Date that Prevue will understand - anything over 256 starts over at zero
            // Julian Date selection box

            DateTime dateselection = dateTimePicker1.Value;
            Calc c = new Calc();
            adjustedJulianDateselection = c.CalcAdjustedJulianDate(dateselection);
            textBox1.Text = adjustedJulianDateselection.ToString();
        }

        public void CalculateChannelJDate(object sender, EventArgs e)
        {
            //Time String - Input the Julian Date Julian Date is in Decimal Form
            //Calculate Julian Date that Prevue will understand - anything over 256 starts over at zero
            // Julian Date selection box

            DateTime dateselection = dateTimePicker2.Value;
            Calc c = new Calc();
            channeladjustedJulianDateselection = c.CalcAdjustedJulianDate(dateselection);
            Jdate.Text = channeladjustedJulianDateselection.ToString();
        }


        private void None_CheckedChanged(object sender, EventArgs e)
        {
            Calc c = new Calc();
            listingattrselection = c.ValueDeterminationFunction(None.Checked, listingattrselection, 1);
        }
        private void Movie_CheckedChanged(object sender, EventArgs e)
        {
            Calc c = new Calc();
            listingattrselection = c.ValueDeterminationFunction(Movie.Checked, listingattrselection, 2);
        }
        private void AltHiliteProg_CheckedChanged(object sender, EventArgs e)
        {
            Calc c = new Calc();
            listingattrselection = c.ValueDeterminationFunction(AltHiliteProg.Checked, listingattrselection, 4);
        }
        private void TagProg_CheckedChanged(object sender, EventArgs e)
        {
            Calc c = new Calc();
            listingattrselection = c.ValueDeterminationFunction(TagProg.Checked, listingattrselection, 8);
        }
        private void SportsProg_CheckedChanged(object sender, EventArgs e)
        {
            Calc c = new Calc();
            listingattrselection = c.ValueDeterminationFunction(SportsProg.Checked, listingattrselection, 16);
        }
        private void DViewUsed_CheckedChanged(object sender, EventArgs e)
        {
            Calc c = new Calc();
            listingattrselection = c.ValueDeterminationFunction(DViewUsed.Checked, listingattrselection, 32);
        }
        private void RepeatProg_CheckedChanged(object sender, EventArgs e)
        {
            Calc c = new Calc();
            listingattrselection = c.ValueDeterminationFunction(RepeatProg.Checked, listingattrselection, 64);
        }
        private void PrevDayData_CheckedChanged(object sender, EventArgs e)
        {
            Calc c = new Calc();
            listingattrselection = c.ValueDeterminationFunction(PrevDayData.Checked, listingattrselection, 128);
        }

        public Form1()
        {
            InitializeComponent();
            RunAtStartup();
        }

        private void SelectFolder_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                // The user selects a folder and pressed the OK button.

                FolderPath = folderBrowserDialog1.SelectedPath;
                WriteFolderInfoFile(firstrunfileName, "N", FolderPath);
                RunAtStartup();
                MessageBox.Show("Selected folder:" + FolderPath, "Message");
            }
        }

        private void ChangeSaveFolder_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                // The user selects a folder and pressed the OK button.
                serialPort1.Close();
                FolderPath = folderBrowserDialog1.SelectedPath;
                WriteFolderInfoFile(firstrunfileName, "N", FolderPath);
                RunAtStartup();
                MessageBox.Show("Selected folder:" + FolderPath, "Message");
            }
        }

        private void ExitProgram_Click(object sender, EventArgs e)
        {
            try
            {
                Thread currentThread = Thread.CurrentThread;
                if (currentThread.IsAlive)
                    currentThread.Abort();
            }
            catch (ThreadAbortException ex)
            {
                Thread.ResetAbort();
            }
            Properties.Settings.Default.Save();
            System.Windows.Forms.Application.Exit();
        }

        private void RunAtStartup()
        {

            if (File.Exists(firstrunfileName))
            {
                string line;
                StreamReader firstrunfile = new StreamReader(firstrunfileName);

                while ((line = firstrunfile.ReadLine()) != null)
                {
                    char[] delimiterChars = { '\x00' };
                    string text = line;
                    string[] parseinfo = text.Split(delimiterChars);
                    FirstRun = Convert.ToChar(parseinfo[0]);
                    FolderPath = parseinfo[1];

                }
                firstrunfile.Close();

                if (FirstRun == 'Y')
                {
                    DialogResult result = folderBrowserDialog1.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        // The user selects a folder
                        FolderPath = folderBrowserDialog1.SelectedPath;
                        WriteFolderInfoFile(firstrunfileName, "N", FolderPath);
                        UpdateDisplayFields();
                        LineupFile lineupfile = new LineupFile();
                        lineupfile.ReadLineupFile(FolderPath, LineupDataSet.Tables["Lineup"]);
                        // Read the saved channel lineup into memory during startup
                        ListingsFile list = new ListingsFile();
                        list.ReadListingsFile(FolderPath, ListingDataSet.Tables["Listings"]);
                        ReadConfigurationFile();
                        WeatherFile wthr = new WeatherFile();
                        wthr.ReadWeatherFile(FolderPath, WeatherDataSet.Tables["Weather"]);
                        QTableFile qtable = new QTableFile();
                        qtable.ReadQTableFile(FolderPath, QTableDataSet.Tables["QTable"]);
                        BindAllData();
                    }
                }
                else
                {
                    UpdateDisplayFields();
                    LineupFile lineupfile = new LineupFile();
                    lineupfile.ReadLineupFile(FolderPath, LineupDataSet.Tables["Lineup"]);
                    ListingsFile list = new ListingsFile();
                    list.ReadListingsFile(FolderPath, ListingDataSet.Tables["Listings"]);
                    ReadConfigurationFile();
                    WeatherFile wthr = new WeatherFile();
                    wthr.ReadWeatherFile(FolderPath, WeatherDataSet.Tables["Weather"]);
                    QTableFile qtable = new QTableFile();
                    qtable.ReadQTableFile(FolderPath, QTableDataSet.Tables["QTable"]);
                    BindAllData();
                }
            }
            else
            {
                WriteFolderInfoFile(firstrunfileName, "Y", firstrunfileName);
                RunAtStartup();
            }
        }
        private void WriteFolderInfoFile(string filepath, string value, string settingsfilename)
        {
            StreamWriter setfile = new StreamWriter(filepath);

            setfile.WriteLine(value + "\x00" + settingsfilename);
            setfile.Close();
        }

        private void UpdateDisplayFields()
        {
            comboBox2.SelectedIndex = comboBox2.Items.IndexOf(Properties.Settings.Default.COMPort);
            textBox87.Text = Properties.Settings.Default.Select_Code;
            textBox10.Text = Properties.Settings.Default.Current_Version;
            WeatherIDTextBox.Text = Properties.Settings.Default.WeatherID;
            WeatherCityTextBox.Text = Properties.Settings.Default.WeatherCity;
            WeatherFreqComboBox.Text = Properties.Settings.Default.WeatherFrequency;
            WeatherParseCycleTextBox.Text = Properties.Settings.Default.WeatherParseCycle;
            textBox19.Text = Properties.Settings.Default.TopLine;
            textBox20.Text = Properties.Settings.Default.BottomLine;
            textBox100.Text = Properties.Settings.Default.DlNumberOfDays;
            textBox101.Text = Properties.Settings.Default.DlUsername;
            textBox102.Text = Properties.Settings.Default.DlPassword;



        }

        private void dataGridView1_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        { 
            //Properties.Settings.Default.DGV1Col0Width = dataGridView1.Columns[0].Width;
            //Properties.Settings.Default.DGV1Col1Width = dataGridView1.Columns[1].Width;
            //Properties.Settings.Default.DGV1Col2Width = dataGridView1.Columns[2].Width;
            //Properties.Settings.Default.DGV1Col3Width = dataGridView1.Columns[3].Width;
            //Properties.Settings.Default.DGV1Col4Width = dataGridView1.Columns[4].Width;
            //Properties.Settings.Default.DGV1Col5Width = dataGridView1.Columns[5].Width;
            //Properties.Settings.Default.DGV1Col6Width = dataGridView1.Columns[6].Width;
            //Properties.Settings.Default.DGV1Col7Width = dataGridView1.Columns[7].Width;
            //Properties.Settings.Default.DGV1Col8Width = dataGridView1.Columns[8].Width;
            //Properties.Settings.Default.DGV1Col9Width = dataGridView1.Columns[9].Width;
            //Properties.Settings.Default.Save();
        }


        private void BindAllData()
        {
            // Create a BindingSource  
            BindingSource lineupbindsource = new BindingSource
            {
                DataSource = LineupDataSet.Tables["Lineup"]
            };

            BindingSource listingbindsource = new BindingSource
            {
                DataSource = ListingDataSet.Tables["Listings"]
            };

            BindingSource configbindsource = new BindingSource
            {
                DataSource = ConfigDataSet.Tables["Config"]
            };

            BindingSource weatherbindsource = new BindingSource
            {
                DataSource = WeatherDataSet.Tables["Weather"]
            };

            BindingSource qtablebindsource = new BindingSource
            {
                DataSource = QTableDataSet.Tables["QTable"]
            };

            BindingSource downloadLineupbindsource = new BindingSource
            {
                DataSource = downloadLineupDataSet.Tables["dllineup"]
            };

            dataGridView6.DataSource = downloadLineupbindsource;

            BindingSource downloadListingbindsource = new BindingSource
            {
                DataSource = downloadListingDataSet.Tables["dllisting"]
            };
            dataGridView7.DataSource = downloadListingbindsource;

            // Bind data to DataGridView.DataSource  
            dataGridView1.DataSource = lineupbindsource;
            dataGridView1.Columns[0].Width = Properties.Settings.Default.DGV1Col0Width;
            dataGridView1.Columns[1].Width = Properties.Settings.Default.DGV1Col1Width;
            dataGridView1.Columns[2].Width = Properties.Settings.Default.DGV1Col2Width;
            dataGridView1.Columns[3].Width = Properties.Settings.Default.DGV1Col3Width;
            dataGridView1.Columns[4].Width = Properties.Settings.Default.DGV1Col4Width;
            dataGridView1.Columns[5].Width = Properties.Settings.Default.DGV1Col5Width;
            dataGridView1.Columns[6].Width = Properties.Settings.Default.DGV1Col6Width;
            dataGridView1.Columns[7].Width = Properties.Settings.Default.DGV1Col7Width;
            dataGridView1.Columns[8].Width = Properties.Settings.Default.DGV1Col8Width;
            dataGridView1.Columns[9].Width = Properties.Settings.Default.DGV1Col9Width;


            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            foreach (DataGridViewColumn dc in dataGridView1.Columns)
            {
                dc.Width = 50;
                dc.SortMode = DataGridViewColumnSortMode.NotSortable;
                dc.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }


            dataGridView2.DataSource = listingbindsource;
            dataGridView2.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView2.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView2.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView2.Columns[0].Width = 60;
            dataGridView2.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView2.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView2.Columns[1].Width = 80;
            dataGridView2.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView2.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView2.Columns[2].Width = 50;
            dataGridView2.Columns[3].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView2.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGridView2.Columns[3].Width = 50;
            dataGridView2.Columns[4].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView2.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridView2.Columns[4].Width = dataGridView2.Width - dataGridView2.Columns[0].Width - dataGridView2.Columns[1].Width - dataGridView2.Columns[2].Width - dataGridView2.Columns[3].Width - 70;
            dataGridView3.DataSource = configbindsource;
            foreach (DataGridViewColumn dc in dataGridView3.Columns)
            {
                dc.Width = 30;
            }
            dataGridView4.DataSource = weatherbindsource;
            dataGridView5.DataSource = qtablebindsource;
        }



        // Channel Listings Section::
        //The Program Data is transmitted as such:
        //Channel Mode - Time Slot - Julian Date - Source ID - Flag 1 - Flag 2 - Title of Program - null char - checksum of entire string 

        private void AddtoList_Click(object sender, EventArgs e)
        {
            //Getting Text Needed from Form and assigning it to their respective lists:  
            DataTable listing = ListingDataSet.Tables["Listings"];

            DataRow workRow;
            workRow = listing.NewRow();
            workRow["JulianDay"] = adjustedJulianDateselection;
            workRow["SourceID"] = SourceID.Text;
            workRow["TimeSlot"] = timeslotselection;
            workRow["Attr"] = listingattrselection;
            workRow["Title"] = Title.Text;
            listing.Rows.Add(workRow);
            ListingsFile list = new ListingsFile();
            list.WriteListingsFile(FolderPath, ListingDataSet.Tables["Listings"]);
        }

        private void Update_Listing_Entry_Click(object sender, EventArgs e)
        {
            DataTable listing = ListingDataSet.Tables["Listings"];
            listing.AcceptChanges();
            ListingsFile list = new ListingsFile();
            list.WriteListingsFile(FolderPath, ListingDataSet.Tables["Listings"]);

            toolStripStatusLabel1.Text = "Listings Updated";
        }

        private void Delete_Listing_Entry_Click(object sender, EventArgs e)
        {

            if (MessageBox.Show("Do you want to delete the selected items?", "My Application", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DataTable listing = ListingDataSet.Tables["Listings"];

                foreach (DataGridViewRow item in this.dataGridView2.SelectedRows)
                {
                    dataGridView2.Rows.RemoveAt(item.Index);
                }
                listing.AcceptChanges();
                ListingsFile list = new ListingsFile();
                list.WriteListingsFile(FolderPath, ListingDataSet.Tables["Listings"]);
                toolStripStatusLabel1.Text = "Selected listings have been deleted.";
            }
            else
            {
                toolStripStatusLabel1.Text = "No listings have been deleted.";
            }

        }

        private void SendSerialSingleListingData_Click(object sender, EventArgs e)
        {
            serial s = new serial();

            DataTable listing = ListingDataSet.Tables["Listings"];
            s.TransmitMessage(serialPort1, FolderPath, "box on", Properties.Settings.Default.Select_Code.ToCharArray());

            foreach (DataGridViewRow row in this.dataGridView2.SelectedRows)
            {
                char[] TimeSlot = new char[] { Convert.ToChar(listing.Rows[row.Index]["Timeslot"]) };
                char[] JDate = new char[] { Convert.ToChar(listing.Rows[row.Index]["JulianDay"]) };
                char[] Flg1 = new char[] { Convert.ToChar(18) };
                char[] attr = new char[] { Convert.ToChar(listing.Rows[row.Index]["Attr"]) };

                string src = listing.Rows[row.Index]["SourceID"].ToString();
                string ttl = listing.Rows[row.Index]["Title"].ToString();
                char[] source = src.ToCharArray();
                char[] title = ttl.ToCharArray();

                List<char> list = new List<char>();
                list.AddRange(TimeSlot);
                list.AddRange(JDate);
                list.AddRange(source);
                list.AddRange(Flg1);
                list.AddRange(attr);
                list.AddRange(title);

                char[] body = list.ToArray();

                s.TransmitMessage(serialPort1, FolderPath, "listing", body);
            }
            s.TransmitMessage(serialPort1, FolderPath, "box off", empty);
            toolStripStatusLabel1.Text = "Selected listings have been sent via serial.";

        }

        private void SendDrev3ListingData_Click(object sender, EventArgs e)
        {
            serial s = new serial();

            DataTable listing = ListingDataSet.Tables["Listings"];
            s.TransmitMessage(serialPort1, FolderPath, "box on", Properties.Settings.Default.Select_Code.ToCharArray());
            
            foreach (DataGridViewRow row in this.dataGridView2.SelectedRows)
            {
                char[] TimeSlot = new char[] { Convert.ToChar(listing.Rows[row.Index]["Timeslot"]) };
                char[] JDate = new char[] { Convert.ToChar(listing.Rows[row.Index]["JulianDay"]) };
                char[] Flg1 = new char[] { Convert.ToChar(18) };
                char[] attr = new char[] { Convert.ToChar(1) };

                string src = listing.Rows[row.Index]["SourceID"].ToString();
                string ttl = listing.Rows[row.Index]["Title"].ToString();
                char[] source = src.ToCharArray();
                char[] title = ttl.ToCharArray();

                List<char> list = new List<char>();
                list.AddRange(TimeSlot);
                list.AddRange(JDate);
                list.AddRange(source);
                list.AddRange(Flg1);
                list.AddRange(attr);
                list.AddRange(title);

                char[] body = list.ToArray();

                s.TransmitMessage(serialPort1, FolderPath, "listing", body);
            } 
            s.TransmitMessage(serialPort1, FolderPath, "box off", empty);
            toolStripStatusLabel1.Text = "Selected listings have been sent via serial.";

        }


        private void SendBatchListings()
        {
            serial s = new serial();

            DataTable listing = ListingDataSet.Tables["Listings"];
            s.TransmitMessage(serialPort1, FolderPath, "box on", Properties.Settings.Default.Select_Code.ToCharArray());

            for (int i = 0; i < listing.Rows.Count; i++)
            {
                char[] TimeSlot = new char[] { Convert.ToChar(listing.Rows[i]["Timeslot"]) };
                char[] JDate = new char[] { Convert.ToChar(listing.Rows[i]["JulianDay"]) };
                char[] Flg1 = new char[] { Convert.ToChar(18) };
                char[] attr = new char[] { Convert.ToChar(listing.Rows[i]["Attr"]) };

                string src = listing.Rows[i]["SourceID"].ToString();
                string ttl = listing.Rows[i]["Title"].ToString();
                char[] source = src.ToCharArray();
                char[] title = ttl.ToCharArray();

                List<char> list = new List<char>();
                list.AddRange(TimeSlot);
                list.AddRange(JDate);
                list.AddRange(source);
                list.AddRange(Flg1);
                list.AddRange(attr);
                list.AddRange(title);

                char[] body = list.ToArray();

                s.TransmitMessage(serialPort1, FolderPath, "listing", body);

            }
            s.TransmitMessage(serialPort1, FolderPath, "box off", empty);
        }
        private void SendListingsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            serial s = new serial();

            DataTable listing = ListingDataSet.Tables["Listings"];
            s.TransmitMessage(serialPort1, FolderPath, "box on", Properties.Settings.Default.Select_Code.ToCharArray());

            for (int i = 0; i < listing.Rows.Count; i++)
            {

                SendListingsWorker.ReportProgress(i);
                if (SendListingsWorker.CancellationPending)
                {
                    // Set the e.Cancel flag so that the WorkerCompleted event
                    // knows that the process was cancelled.
                    e.Cancel = true;
                    SendListingsWorker.ReportProgress(0);
                    break;
                }
                else
                {
                    char[] TimeSlot = new char[] { Convert.ToChar(listing.Rows[i]["Timeslot"]) };
                    char[] JDate = new char[] { Convert.ToChar(listing.Rows[i]["JulianDay"]) };
                    char[] Flg1 = new char[] { Convert.ToChar(18) };
                    char[] attr = new char[] { Convert.ToChar(listing.Rows[i]["Attr"]) };

                    string src = listing.Rows[i]["SourceID"].ToString();
                    string ttl = listing.Rows[i]["Title"].ToString();
                    char[] source = src.ToCharArray();
                    char[] title = ttl.ToCharArray();

                    List<char> list = new List<char>();
                    list.AddRange(TimeSlot);
                    list.AddRange(JDate);
                    list.AddRange(source);
                    list.AddRange(Flg1);
                    list.AddRange(attr);
                    list.AddRange(title);

                    char[] body = list.ToArray();

                    s.TransmitMessage(serialPort1, FolderPath, "listing", body);

                }
            }
            s.TransmitMessage(serialPort1, FolderPath, "box off", empty);

        }
        void SendListingsWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            toolStripStatusLabel1.Text = "Processing......" + e.ProgressPercentage.ToString() + "%";
        }

        void SendListingsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // The background process is complete. We need to inspect
            // our response to see if an error occurred, a cancel was
            // requested or if we completed successfully.  
            if (e.Cancelled)
            {
                toolStripStatusLabel1.Text = "Task Cancelled.";
                MessageBox.Show("Operation was canceled");
            }

            // Check to see if an error occurred in the background process.

            else if (e.Error != null)
            {
                toolStripStatusLabel1.Text = "Error while performing background operation.";
                MessageBox.Show(e.Error.Message);
            }
            else
            {
                // Everything completed normally.
                toolStripStatusLabel1.Text = "Task Completed...";
            }

            //Change the status of the buttons on the UI accordingly
            //btnStartAsyncOperation.Enabled = true;
            //btnCancel.Enabled = false;
        }
        private void SendSerialBatchListingData_Click(object sender, EventArgs e)
        {
            SendBatchListings();
            //SendListingsWorker.RunWorkerAsync();
            button30.Enabled = true;
            SendAllListings.Enabled = false;
        }

        private void StopSendBatchListingData_Click(object sender, EventArgs e)
        {
            //SendListingsWorker.CancelAsync();
            serialPort1.DiscardOutBuffer();

            try
            {
            }
            catch (ThreadAbortException ex)
            {
            }
            button30.Enabled = false;
            SendAllListings.Enabled = true;
        }

        // Channel Lineup Section:
        
        private void Update_TS_Entries(int daypart, DataGridViewCell item)
        {
            DaypartData entry = new DaypartData
            {
                DaypartSelected = daypart,
                DaypartValue = item.Value.ToString()
            };

            using (Form3 frm = new Form3() { TSInfo = entry })
               {
                   if (frm.ShowDialog() == DialogResult.OK)
                   {
                       item.Value = entry.DaypartValue;
                   }

               }

           toolStripStatusLabel1.Text = "Entries have been updated to lineup.";
        }
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCell item = dataGridView1.CurrentCell;

            switch (item.ColumnIndex)
            {
                case 10: { Update_TS_Entries(1, item); break; }
                case 11: { Update_TS_Entries(2, item); break; }
                case 12: { Update_TS_Entries(3, item); break; }
                case 13: { Update_TS_Entries(4, item); break; }
                case 14: { Update_TS_Entries(5, item); break; }
                case 15: { Update_TS_Entries(6, item); break; }
                default: { break; }
            }
        }

        private void Update_Lineup_Entries_Click(object sender, EventArgs e)
        {
            DataTable lineup = LineupDataSet.Tables["Lineup"];
            lineup.AcceptChanges();
            LineupFile lineupfile = new LineupFile();
            lineupfile.WriteLineupFile(FolderPath, lineup);

            toolStripStatusLabel1.Text = "Entries have been updated to lineup.";
        }

        private void Delete_Lineup_Entries_Click(object sender, EventArgs e)
        {

            if (MessageBox.Show("Would you like to to delete the selected entries?", "My Application", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                foreach (DataGridViewRow item in this.dataGridView1.SelectedRows)
                {
                    dataGridView1.Rows.RemoveAt(item.Index);
                }

                DataTable lineup = LineupDataSet.Tables["Lineup"];
                lineup.AcceptChanges();
                LineupFile lineupfile = new LineupFile();
                lineupfile.WriteLineupFile(FolderPath, lineup);
                toolStripStatusLabel1.Text = "Selected entries have been deleted.";
            }
            else
            {
                toolStripStatusLabel1.Text = "No lineup entries have been deleted.";
            }

        }

        private void SendCdayLineup_Click(object sender, EventArgs e)
        {
            DataTable lineup = LineupDataSet.Tables["Lineup"];

            char[] chadjjd = new char[] { Convert.ToChar(channeladjustedJulianDateselection) }; //constant
            List<char> list = new List<char>();
            list.AddRange(chadjjd);

            for (int counter = 0; counter < lineup.Rows.Count; counter++)
            {
                Calc c = new Calc();
                int attr = c.CalculateBitmaskValue("1",
                                                   lineup.Rows[counter]["RedHiLt"].ToString(),
                                                   lineup.Rows[counter]["SBS"].ToString(),
                                                   lineup.Rows[counter]["PTagDisable"].ToString(),
                                                   lineup.Rows[counter]["PPVSrc"].ToString(),
                                                   lineup.Rows[counter]["DittoEnable"].ToString(),
                                                   lineup.Rows[counter]["LtBlueHiLt"].ToString(),
                                                   lineup.Rows[counter]["StereoSrc"].ToString(), "1", "0");
                char[] Flg1 = new char[] { Convert.ToChar(18) };
                char[] Flg2 = new char[] { Convert.ToChar(attr) };
                char[] Flg3 = new char[] { Convert.ToChar(17) };
                char[] Flg4 = new char[] { Convert.ToChar(20) };
                char[] Dypt1 = new char[] { Convert.ToChar(lineup.Rows[counter]["Daypt1"]) };
                char[] Dypt2 = new char[] { Convert.ToChar(lineup.Rows[counter]["Daypt2"]) };
                char[] Dypt3 = new char[] { Convert.ToChar(lineup.Rows[counter]["Daypt3"]) };
                char[] Dypt4 = new char[] { Convert.ToChar(lineup.Rows[counter]["Daypt4"]) };
                char[] Dypt5 = new char[] { Convert.ToChar(lineup.Rows[counter]["Daypt5"]) };
                char[] Dypt6 = new char[] { Convert.ToChar(lineup.Rows[counter]["Daypt6"]) };
                char[] Flg5 = new char[] { Convert.ToChar(1) };

                string srrc = lineup.Rows[counter]["SourceID"].ToString();
                string num = lineup.Rows[counter]["Number"].ToString();
                string id = lineup.Rows[counter]["ID"].ToString();
                char[] Src = srrc.ToCharArray();
                char[] Chnum = num.ToCharArray();
                char[] ChId = id.ToCharArray();

                list.AddRange(Flg1);
                list.AddRange(Flg2);
                list.AddRange(Src);
                list.AddRange(Flg3);
                list.AddRange(Chnum);
                list.AddRange(Flg4);
                list.AddRange(Dypt1);
                list.AddRange(Dypt2);
                list.AddRange(Dypt3);
                list.AddRange(Dypt4);
                list.AddRange(Dypt5);
                list.AddRange(Dypt6);
                list.AddRange(Flg5);
                list.AddRange(ChId);

            }
            char[] body = list.ToArray();
            serial s = new serial();
            s.TransmitMessage(serialPort1, FolderPath, "box on", Properties.Settings.Default.Select_Code.ToCharArray());
            s.TransmitMessage(serialPort1, FolderPath, "channel", body);
            s.TransmitMessage(serialPort1, FolderPath, "box off", empty);
            toolStripStatusLabel1.Text = "Current day channel lineup sent over serial port";

        }
        private void SendCdayLineupDrev3_Click(object sender, EventArgs e)
        {//Channel Mode - Julian Date - (Flag 1 - Flag 2- Source ID - Flag 3 - Channel number - Flag 4 - Channel ID) - null char - checksum of entire string 

            DataTable lineup = LineupDataSet.Tables["Lineup"];


            char[] chadjjd = new char[] { Convert.ToChar(channeladjustedJulianDateselection) }; //constant
            //char[] chadjjd = new char[] { '\xD2' }; //constant

            List<char> list = new List<char>();
            list.AddRange(chadjjd);
            for (int counter = 0; counter < lineup.Rows.Count; counter++)
            {
                Calc c = new Calc();
                int attr = c.CalculateBitmaskValue("1",
                                                   lineup.Rows[counter]["RedHiLt"].ToString(),
                                                   lineup.Rows[counter]["SBS"].ToString(),
                                                   lineup.Rows[counter]["PTagDisable"].ToString(),
                                                   lineup.Rows[counter]["PPVSrc"].ToString(),
                                                   lineup.Rows[counter]["DittoEnable"].ToString(),
                                                   lineup.Rows[counter]["LtBlueHiLt"].ToString(),
                                                   lineup.Rows[counter]["StereoSrc"].ToString(), "1", "0");
                char[] Flg1 = new char[] { Convert.ToChar(18) };
                //char[] Flg2 = new char[] { Convert.ToChar(attr) };
                char[] Flg2 = new char[] { Convert.ToChar(1) };
                char[] Flg3 = new char[] { Convert.ToChar(17) };
                char[] Flg4 = new char[] { Convert.ToChar(20) };
                char[] Dypt1 = new char[] { Convert.ToChar(lineup.Rows[counter]["Daypt1"]) };
                char[] Dypt2 = new char[] { Convert.ToChar(lineup.Rows[counter]["Daypt2"]) };
                char[] Dypt3 = new char[] { Convert.ToChar(lineup.Rows[counter]["Daypt3"]) };
                char[] Dypt4 = new char[] { Convert.ToChar(lineup.Rows[counter]["Daypt4"]) };
                char[] Dypt5 = new char[] { Convert.ToChar(lineup.Rows[counter]["Daypt5"]) };
                char[] Dypt6 = new char[] { Convert.ToChar(lineup.Rows[counter]["Daypt6"]) };
                char[] Flg5 = new char[] { Convert.ToChar(1) };

                string srrc = lineup.Rows[counter]["SourceID"].ToString();
                string num = lineup.Rows[counter]["Number"].ToString();
                string id = lineup.Rows[counter]["ID"].ToString();
                char[] Src = srrc.ToCharArray();
                char[] Chnum = num.ToCharArray();
                char[] ChId = id.ToCharArray();



                list.AddRange(Flg1);
                list.AddRange(Flg2);
                list.AddRange(Src);
                list.AddRange(Flg3);
                list.AddRange(Chnum);
                list.AddRange(Flg5);
                list.AddRange(ChId);

                //list.AddRange(Flg4);
                //list.AddRange(Dypt1);
                //list.AddRange(Dypt2);
                //list.AddRange(Dypt3);
                //list.AddRange(Dypt4);
                //list.AddRange(Dypt5);
                //list.AddRange(Dypt6);


            }
            char[] body = list.ToArray();
            serial s = new serial();
            s.TransmitMessage(serialPort1, FolderPath, "box on", Properties.Settings.Default.Select_Code.ToCharArray());
            s.TransmitMessage(serialPort1, FolderPath, "channel", body);
            s.TransmitMessage(serialPort1, FolderPath, "box off", empty);
            toolStripStatusLabel1.Text = "Drev3 channel lineup sent over serial port";

        }
        private void SendNextDayLineup_Click(object sender, EventArgs e)
        {//Channel Mode - Julian Date - (Flag 1 - Flag 2- Source ID - Flag 3 - Channel number - Flag 4 - Channel ID) - null char - checksum of entire string 

            serial s = new serial();

            DataTable lineup = LineupDataSet.Tables["Lineup"];

            s.TransmitMessage(serialPort1, FolderPath, "box on", Properties.Settings.Default.Select_Code.ToCharArray());

            char[] chadjjd = new char[] { Convert.ToChar(channeladjustedJulianDateselection + 1) }; //constant
            List<char> list = new List<char>();
            list.AddRange(chadjjd);

            for (int counter = 0; counter < lineup.Rows.Count; counter++)
            {
                Calc c = new Calc();
                int attr = c.CalculateBitmaskValue("1",
                                                   lineup.Rows[counter]["RedHiLt"].ToString(),
                                                   lineup.Rows[counter]["SBS"].ToString(),
                                                   lineup.Rows[counter]["PTagDisable"].ToString(),
                                                   lineup.Rows[counter]["PPVSrc"].ToString(),
                                                   lineup.Rows[counter]["DittoEnable"].ToString(),
                                                   lineup.Rows[counter]["LtBlueHiLt"].ToString(),
                                                   lineup.Rows[counter]["StereoSrc"].ToString(), "1", "0");
                char[] Flg1 = new char[] { Convert.ToChar(18) };
                char[] Flg2 = new char[] { Convert.ToChar(attr) };
                char[] Flg3 = new char[] { Convert.ToChar(17) };
                char[] Flg4 = new char[] { Convert.ToChar(20) };
                char[] Dypt1 = new char[] { Convert.ToChar(lineup.Rows[counter]["Daypt1"]) };
                char[] Dypt2 = new char[] { Convert.ToChar(lineup.Rows[counter]["Daypt2"]) };
                char[] Dypt3 = new char[] { Convert.ToChar(lineup.Rows[counter]["Daypt3"]) };
                char[] Dypt4 = new char[] { Convert.ToChar(lineup.Rows[counter]["Daypt4"]) };
                char[] Dypt5 = new char[] { Convert.ToChar(lineup.Rows[counter]["Daypt5"]) };
                char[] Dypt6 = new char[] { Convert.ToChar(lineup.Rows[counter]["Daypt6"]) };
                char[] Flg5 = new char[] { Convert.ToChar(1) };

                string srrc = lineup.Rows[counter]["SourceID"].ToString();
                string num = lineup.Rows[counter]["Number"].ToString();
                string id = lineup.Rows[counter]["ID"].ToString();
                char[] Src = srrc.ToCharArray();
                char[] Chnum = num.ToCharArray();
                char[] ChId = id.ToCharArray();

                list.AddRange(Flg1);
                list.AddRange(Flg2);
                list.AddRange(Src);
                list.AddRange(Flg3);
                list.AddRange(Chnum);
                list.AddRange(Flg4);
                list.AddRange(Dypt1);
                list.AddRange(Dypt2);
                list.AddRange(Dypt3);
                list.AddRange(Dypt4);
                list.AddRange(Dypt5);
                list.AddRange(Dypt6);
                list.AddRange(Flg5);
                list.AddRange(ChId);

            }
            char[] body = list.ToArray();

            s.TransmitMessage(serialPort1, FolderPath, "channel", body);
            s.TransmitMessage(serialPort1, FolderPath, "box off", empty);
            toolStripStatusLabel1.Text = "Current day channel lineup sent over serial port";

        }


        //Commands Tab

        private void SaveDataButton_Click(object sender, EventArgs e)
        {
            serial s = new serial();

            char[] body = new char[] { };

            s.TransmitMessage(serialPort1, FolderPath, "box on", Properties.Settings.Default.Select_Code.ToCharArray());
            s.TransmitMessage(serialPort1, FolderPath, "save data", empty);
            s.TransmitMessage(serialPort1, FolderPath, "box off", empty);
            toolStripStatusLabel1.Text = "Save Command sent via serial";
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            serial s = new serial();
            char[] body = new char[] { };

            s.TransmitMessage(serialPort1, FolderPath, "box on", Properties.Settings.Default.Select_Code.ToCharArray());
            s.TransmitMessage(serialPort1, FolderPath, "reset", body);
            s.TransmitMessage(serialPort1, FolderPath, "box off", empty);
            toolStripStatusLabel1.Text = "Reset Command sent via serial";
        }

        private void SelectCode_Set_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Select_Code = textBox87.Text;
            Properties.Settings.Default.Save();
            toolStripStatusLabel1.Text = "Select code updated.";
        }


        private void Version_Set_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Current_Version = textBox10.Text;
            Properties.Settings.Default.Save();
            toolStripStatusLabel1.Text = "Version updated.";
        }
        private void SendManualBoxOn_Click(object sender, EventArgs e)
        {
            serial s = new serial();
            s.TransmitMessage(serialPort1, FolderPath, "box on", Properties.Settings.Default.Select_Code.ToCharArray());

            toolStripStatusLabel1.Text = "Manually sent box off command via serial";
        }

        private void SendManualBoxOff_Click(object sender, EventArgs e)
        {
            serial s = new serial();
            s.TransmitMessage(serialPort1, FolderPath, "box off", empty);

            toolStripStatusLabel1.Text = "Manually sent box off command via serial";
        }

        private void Version_Check_Click(object sender, EventArgs e)
        {
            string messagebody = '\x01' + Properties.Settings.Default.Current_Version;
            char[] body = messagebody.ToCharArray();

            serial s = new serial();
            s.TransmitMessage(serialPort1, FolderPath, "box on", Properties.Settings.Default.Select_Code.ToCharArray());
            s.TransmitMessage(serialPort1, FolderPath, "version", body);
            s.TransmitMessage(serialPort1, FolderPath, "box off", empty);

            toolStripStatusLabel1.Text = "Version check sent via serial";
        }

        private void SendClockData_Click(object sender, EventArgs e)
        {
            serial s = new serial();
            int day = Convert.ToInt16(comboBox4.SelectedIndex);
            int month = Convert.ToInt16(comboBox5.SelectedIndex);
            int date = Convert.ToInt16(textBox23.Text) - 1;
            int year = Convert.ToInt16(textBox24.Text) - 1900;
            int hour = Convert.ToInt16(textBox25.Text) + 1;
            int minute = Convert.ToInt16(textBox26.Text);
            int second = Convert.ToInt16(textBox27.Text);
            int DST = Convert.ToInt16(comboBox6.SelectedIndex);

            textBox21.Text = "DOW " + day.ToString() + " Month " + month.ToString() + " Day " + date.ToString() + " Year " + year.ToString() + "  Time " +
                hour.ToString() + ":" + minute.ToString() + ":" + second.ToString() + " DST " + DST.ToString();

            char[] dy = new char[] { Convert.ToChar(day) };
            char[] mon = new char[] { Convert.ToChar(month) };
            char[] dt = new char[] { Convert.ToChar(date) };
            char[] yr = new char[] { Convert.ToChar(year) };
            char[] hr = new char[] { Convert.ToChar(hour) };
            char[] min = new char[] { Convert.ToChar(minute) };
            char[] sc = new char[] { Convert.ToChar(second) };
            char[] daylight = new char[] { Convert.ToChar(DST) };

            List<char> list = new List<char>();
            list.AddRange(dy);
            list.AddRange(mon);
            list.AddRange(dt);
            list.AddRange(yr);
            list.AddRange(hr);
            list.AddRange(min);
            list.AddRange(sc);
            list.AddRange(daylight);
            char[] body = list.ToArray();

            s.TransmitMessage(serialPort1, FolderPath, "box on", Properties.Settings.Default.Select_Code.ToCharArray());
            s.TransmitMessage(serialPort1, FolderPath, "clock", body);
            s.TransmitMessage(serialPort1, FolderPath, "box off", empty);

            toolStripStatusLabel1.Text = "Clock Data sent via serial";
        }

        private void ResetClockDataPage_Click(object sender, EventArgs e)
        {
            comboBox4.SelectedIndex = -1;
            comboBox5.SelectedIndex = -1;
            textBox23.Text = "";
            textBox24.Text = "";
            textBox25.Text = "";
            textBox26.Text = "";
            textBox27.Text = "";
            comboBox6.SelectedIndex = -1;
        }

        private void SendDSTData(string option)
        {
            serial s = new serial();

            string commandlength = "27";
            string Enter_Year = textBox11.Text;
            string Enter_JulianDay = textBox12.Text;
            string Enter_Time = textBox16.Text + ':' + textBox88.Text;
            string Exit_Year = textBox89.Text;
            string Exit_JulianDay = textBox90.Text;
            string Exit_Time = textBox91.Text + ':' + textBox92.Text;

            char[] dst1 = new char[] { '\x04' };
            char[] dst2 = new char[] { '\x13' };

            List<char> list = new List<char>();
            list.AddRange(option);
            list.AddRange(commandlength);
            list.AddRange(dst1);
            list.AddRange(Enter_Year);
            list.AddRange(Enter_JulianDay);
            list.AddRange(Enter_Time);
            list.AddRange(dst2);
            list.AddRange(Exit_Year);
            list.AddRange(Exit_JulianDay);
            list.AddRange(Exit_Time);

            char[] body = list.ToArray();

            s.TransmitMessage(serialPort1, FolderPath, "box on", Properties.Settings.Default.Select_Code.ToCharArray());
            //s.TransmitMessage.("DST", body);
            s.TransmitMessage(serialPort1, FolderPath, "box off", empty);

            toolStripStatusLabel1.Text = "Clock Data sent via serial";
        }

        private void Send_DST_G2_Click(object sender, EventArgs e)
        {
            SendDSTData("2");
            toolStripStatusLabel1.Text = "DST 2 Data sent via serial";
        }

        private void Send_DST_G3_Click(object sender, EventArgs e)
        {
            SendDSTData("3");
            toolStripStatusLabel1.Text = "DST 3 Data sent via serial";
        }


        private void TopLineSaveBtn_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.TopLine = textBox19.Text;
            Properties.Settings.Default.Save();
            toolStripStatusLabel1.Text = "EPG Top line information saved.";
        }

        private void TopLineSendBtn_Click(object sender, EventArgs e)
        {
            serial s = new serial();
            s.TransmitMessage(serialPort1, FolderPath, "box on", Properties.Settings.Default.Select_Code.ToCharArray());
            s.TransmitMessage(serialPort1, FolderPath, "top line", Properties.Settings.Default.TopLine.ToCharArray());
            s.TransmitMessage(serialPort1, FolderPath, "box off", empty);
            toolStripStatusLabel1.Text = "EPG Top line information sent via serial.";
        }

        private void BottomLineSaveBtn_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.BottomLine = textBox20.Text;
            Properties.Settings.Default.Save();
            toolStripStatusLabel1.Text = "EPG Bottom line information saved.";
        }

        private void BottomLineSendBtn_Click(object sender, EventArgs e)
        {
            serial s = new serial();
            s.TransmitMessage(serialPort1, FolderPath, "box on", Properties.Settings.Default.Select_Code.ToCharArray());
            s.TransmitMessage(serialPort1, FolderPath, "bottom line", Properties.Settings.Default.BottomLine.ToCharArray());
            s.TransmitMessage(serialPort1, FolderPath, "box off", empty);
            toolStripStatusLabel1.Text = "EPG Bottom line information sent via serial.";
        }

        //Weather Tab

        private void WeatherID_Setup_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.WeatherFrequency = WeatherFreqComboBox.Text;
            Properties.Settings.Default.WeatherID = WeatherIDTextBox.Text;
            Properties.Settings.Default.WeatherCity = WeatherCityTextBox.Text;
            Properties.Settings.Default.WeatherParseCycle = WeatherParseCycleTextBox.Text;
            Properties.Settings.Default.Save();
            toolStripStatusLabel1.Text = "Weather ID info saved.";
        }

        private void WeatherID_Send_Click(object sender, EventArgs e)
        {
            serial s = new serial();
            string messagebody = Properties.Settings.Default.WeatherFrequency +
                                 Properties.Settings.Default.WeatherID + '\x12' +
                                 Properties.Settings.Default.WeatherCity;

            char[] body = messagebody.ToCharArray();

            s.TransmitMessage(serialPort1, FolderPath, "box on", Properties.Settings.Default.Select_Code.ToCharArray());
            s.TransmitMessage(serialPort1, FolderPath, "weather id", body);
            s.TransmitMessage(serialPort1, FolderPath, "box off", empty);

            toolStripStatusLabel1.Text = "Weather ID sent via serial";

        }

        private void AddWeatherData()
        {
            string url = Properties.Settings.Default.WeatherDataPath + Properties.Settings.Default.WeatherID + ".TXT";
            string weatherdatafile = FolderPath + @"\" + Properties.Settings.Default.WeatherID + ".TXT";

            WeatherFile wfile = new WeatherFile();
            FormatWthr wthr = new FormatWthr();
            wthr.ParseWeatherData(wfile.DownloadWeatherData(url, weatherdatafile));

            DataTable weather = WeatherDataSet.Tables["Weather"];

            DataRow workRow;
            workRow = weather.NewRow();

            workRow["display_length"] = Convert.ToInt32(Properties.Settings.Default.WeatherParseCycle);
            workRow["ColorID"] = "1";
            workRow["IDString"] = Properties.Settings.Default.WeatherID;
            workRow["Expansion"] = "1";
            workRow["CurSky"] = wthr.FormatWeatherData("sky");
            if (wthr.CurWeather != "")
            {
                workRow["CurWeather"] = wthr.FormatWeatherData("weather");
            }
            workRow["IconID"] = wthr.SelectWeatherIcon();
            workRow["CurTemp"] = wthr.FormatWeatherData("temp");
            workRow["CurWind"] = wthr.FormatWeatherData("wind");
            workRow["CurPressure"] = wthr.FormatWeatherData("pressure");
            workRow["CurHumidity"] = wthr.FormatWeatherData("humidity");
            workRow["CurDewPoint"] = wthr.FormatWeatherData("dewpoint");
            workRow["CurVisibility"] = wthr.FormatWeatherData("visibility");
            weather.Rows.Add(workRow);

            WeatherFile weatherfile = new WeatherFile();
            weatherfile.WriteWeatherFile(FolderPath, WeatherDataSet.Tables["Weather"]);

            WeatherParseCycleTextBox.Text = Properties.Settings.Default.WeatherParseCycle;
            textBox13.Text = "1";
            textBox14.Text = wthr.SelectWeatherIcon(); //iconid
            textBox15.Text = "1";
            WeatherIDTextBox.Text = Properties.Settings.Default.WeatherID;
            textBox17.Text = wthr.FormatWeatherData("sky");
            textBox86.Text = wthr.FormatWeatherData("weather");
            textBox18.Text = wthr.FormatWeatherData("temp");
            textBox84.Text = wthr.FormatWeatherData("wind");
            textBox83.Text = wthr.FormatWeatherData("pressure");
            textBox82.Text = wthr.FormatWeatherData("humidity");
            textBox6.Text = wthr.FormatWeatherData("dewpoint");
            textBox85.Text = wthr.FormatWeatherData("visibility");
            toolStripStatusLabel1.Text = "Weather data added.";

        }
        private void SendWeatherData()
        {
            serial s = new serial();
            try
            {
                DataTable weather = WeatherDataSet.Tables["Weather"];
                s.TransmitMessage(serialPort1, FolderPath, "box on", Properties.Settings.Default.Select_Code.ToCharArray());

                int i = weather.Rows.Count - 1;
                string id = weather.Rows[i]["IDString"].ToString();
                string CSky = weather.Rows[i]["CurSky"].ToString();
                string CWeather = weather.Rows[i]["CurWeather"].ToString();
                string CTemp = weather.Rows[i]["CurTemp"].ToString();
                string CWind = weather.Rows[i]["CurWind"].ToString();
                string CPressure = weather.Rows[i]["CurPressure"].ToString();
                string CHumidity = weather.Rows[i]["CurHumidity"].ToString();
                string CDewPoint = weather.Rows[i]["CurDewPoint"].ToString();
                string CVisibility = weather.Rows[i]["CurVisibility"].ToString();

                char[] displen = new char[] { Convert.ToChar(weather.Rows[i]["display_length"]) };
                char[] ClrID = new char[] { Convert.ToChar(weather.Rows[i]["ColorID"]) };
                char[] IcID = new char[] { Convert.ToChar(weather.Rows[i]["IconID"]) };
                char[] Exp = new char[] { Convert.ToChar(weather.Rows[i]["Expansion"]) };
                char[] WthrID = id.ToCharArray();
                char[] flag1 = new char[] { Convert.ToChar(18) };
                char[] CurSky = CSky.ToCharArray();
                char[] CurWeather = CWeather.ToCharArray();
                char[] CurTemp = CTemp.ToCharArray();
                char[] CurWind = CWind.ToCharArray();
                char[] CurPressure = CPressure.ToCharArray();
                char[] CurHumidity = CHumidity.ToCharArray();
                char[] CurDewPoint = CDewPoint.ToCharArray();
                char[] CurVisibility = CVisibility.ToCharArray();

                List<char> list = new List<char>();
                list.AddRange(displen);
                list.AddRange(ClrID);
                list.AddRange(IcID);
                list.AddRange(Exp);
                list.AddRange(WthrID);
                list.AddRange(flag1);
                list.AddRange(CurSky);
                list.AddRange(CurWeather);
                list.AddRange(CurTemp);
                list.AddRange(CurWind);
                list.AddRange(CurPressure);
                list.AddRange(CurHumidity);
                list.AddRange(CurDewPoint);
                list.AddRange(CurVisibility);
                char[] body = list.ToArray();
                s.TransmitMessage(serialPort1, FolderPath, "weather data", body);
                s.TransmitMessage(serialPort1, FolderPath, "box off", empty);
                toolStripStatusLabel1.Text = "Weather data sent over Serial Port";
            }
            catch (Exception)
            {
                MessageBox.Show("Cannot send data", "My Application", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                timer2.Enabled = false;
                button16.Enabled = true;
                button17.Enabled = false;
            }
        }
        private void AddWeatherData_Click(object sender, EventArgs e)
        {
            AddWeatherData();
        }

        private void SendWeatherData_Click(object sender, EventArgs e)
        {
            SendWeatherData();
        }

        private void WeatherUpdateTimer_Tick(object sender, EventArgs e)
        {
            // Verify if the time didn't pass.
            if ((wu_minutes == 0) && (wu_seconds == 0))
            {
                // If the time is over, clear all settings and fields.
                button16.Enabled = false;
                button17.Enabled = true;
                label91.Text = "Downloading Update";
                AddWeatherData();
                SendWeatherData();
                DataTable weather = WeatherDataSet.Tables["Weather"];
                int i = weather.Rows.Count - 1;
                wu_minutes = Convert.ToInt32(weather.Rows[i]["display_length"]);
            }
            else
            {
                // Else continue counting.
                if (wu_seconds < 1)
                {
                    wu_seconds = 59;
                    wu_minutes -= 1;
                }
                else
                    wu_seconds -= 1;

                if (wu_seconds < 10) { label91.Text = wu_minutes.ToString() + ":0" + wu_seconds.ToString(); }
                else label91.Text = wu_minutes.ToString() + ":" + wu_seconds.ToString();
            }
        }

        private void StartWeatherDataTimer(object sender, EventArgs e)
        {
            timer2.Enabled = true;
            button16.Enabled = false;
            button17.Enabled = true;
        }

        private void StopWeatherDataTimer(object sender, EventArgs e)
        {
            timer2.Enabled = false;
            button16.Enabled = true;
            button17.Enabled = false;
        }

        //Config Tabs
        private void ReadConfigurationFile()
        {
            string configfilename = FolderPath + @"\Config.txt";
            string data;

            if (File.Exists(configfilename))
            {
                StreamReader lineupfile = new StreamReader(configfilename);
                DataTable config = ConfigDataSet.Tables["Config"];

                while ((data = lineupfile.ReadLine()) != null)
                {
                    char[] delimiterChars = { '\x00' };
                    string text = data;
                    string[] parseinfo = text.Split(delimiterChars);

                    DataRow workRow;
                    workRow = config.NewRow();
                    //Old Config 
                    workRow["Column1"] = parseinfo[0];
                    workRow["Column2"] = parseinfo[1];
                    workRow["Column3"] = Int32.Parse(parseinfo[2]);
                    workRow["Column4"] = Int32.Parse(parseinfo[3]);
                    workRow["Column5"] = Int32.Parse(parseinfo[4]);
                    workRow["Column6"] = parseinfo[5];
                    workRow["Column7"] = Int32.Parse(parseinfo[6]);
                    workRow["Column8"] = Int32.Parse(parseinfo[7]);
                    workRow["Column9"] = Int32.Parse(parseinfo[8]);
                    workRow["Column10"] = parseinfo[9];
                    workRow["Column11"] = parseinfo[10];
                    workRow["Column12"] = parseinfo[11];
                    workRow["Column13"] = parseinfo[12];
                    workRow["Column14"] = parseinfo[13];
                    workRow["Column15"] = parseinfo[14];
                    workRow["Column16"] = parseinfo[15];
                    workRow["Column17"] = parseinfo[16];
                    workRow["Column18"] = parseinfo[17];
                    workRow["Column19"] = Int32.Parse(parseinfo[18]);

                    //new config values below
                    workRow["Column20"] = Int32.Parse(parseinfo[19]);
                    workRow["Column21"] = parseinfo[20];
                    workRow["Column22"] = Int32.Parse(parseinfo[21]);
                    workRow["Column23"] = Int32.Parse(parseinfo[22]);
                    workRow["Column24"] = Int32.Parse(parseinfo[23]);
                    workRow["Column25"] = Int32.Parse(parseinfo[24]);
                    workRow["Column26"] = parseinfo[25];
                    workRow["Column27"] = parseinfo[26];
                    workRow["Column28"] = parseinfo[27];
                    workRow["Column29"] = parseinfo[28];
                    workRow["Column30"] = Int32.Parse(parseinfo[29]);
                    workRow["Column31"] = Int32.Parse(parseinfo[30]);
                    workRow["Column32"] = parseinfo[31];
                    workRow["Column33"] = parseinfo[32];
                    workRow["Column34"] = parseinfo[33];
                    workRow["Column35"] = parseinfo[34];
                    workRow["Column36"] = parseinfo[35];
                    workRow["Column37"] = parseinfo[36];
                    workRow["Column38"] = parseinfo[37];
                    workRow["Column39"] = Int32.Parse(parseinfo[38]);
                    workRow["Column40"] = Int32.Parse(parseinfo[39]);
                    workRow["Column41"] = parseinfo[40];
                    workRow["Column42"] = parseinfo[41];
                    workRow["Column43"] = parseinfo[42];
                    workRow["Column44"] = Int32.Parse(parseinfo[43]);
                    workRow["Column45"] = Int32.Parse(parseinfo[44]);
                    workRow["Column46"] = Int32.Parse(parseinfo[45]);
                    workRow["Column47"] = Int32.Parse(parseinfo[46]);
                    workRow["Column48"] = Int32.Parse(parseinfo[47]);
                    workRow["Column49"] = parseinfo[48];
                    workRow["Column50"] = parseinfo[49];
                    workRow["Column51"] = parseinfo[50];
                    workRow["Column52"] = parseinfo[51];
                    workRow["Column53"] = parseinfo[52];
                    workRow["Column54"] = Int32.Parse(parseinfo[53]);
                    workRow["Column55"] = Int32.Parse(parseinfo[54]);
                    workRow["Column56"] = Int32.Parse(parseinfo[55]);
                    workRow["Column57"] = parseinfo[56];
                    workRow["Column58"] = parseinfo[57];
                    workRow["Column59"] = parseinfo[58];
                    workRow["Column60"] = parseinfo[59];
                    workRow["Column61"] = parseinfo[60];
                    workRow["Column62"] = parseinfo[61];

                    config.Rows.Add(workRow);

                    textBox2.Text = config.Rows[0][0].ToString();
                    textBox3.Text = config.Rows[0][1].ToString();
                    textBox4.Text = config.Rows[0][2].ToString();
                    textBox5.Text = config.Rows[0][3].ToString();
                    textBox7.Text = config.Rows[0][4].ToString();
                    textBox8.Text = config.Rows[0][5].ToString();
                    textBox9.Text = config.Rows[0][6].ToString();
                    textBox22.Text = config.Rows[0][7].ToString();
                    textBox28.Text = config.Rows[0][8].ToString();
                    textBox29.Text = config.Rows[0][9].ToString();
                    textBox30.Text = config.Rows[0][10].ToString();
                    textBox31.Text = config.Rows[0][11].ToString();
                    textBox32.Text = config.Rows[0][12].ToString();
                    textBox33.Text = config.Rows[0][13].ToString();
                    textBox34.Text = config.Rows[0][14].ToString();
                    textBox35.Text = config.Rows[0][15].ToString();
                    textBox36.Text = config.Rows[0][16].ToString();
                    textBox37.Text = config.Rows[0][17].ToString();
                    textBox38.Text = config.Rows[0][18].ToString();

                    textBox39.Text = config.Rows[0][19].ToString();
                    textBox40.Text = config.Rows[0][20].ToString();
                    textBox41.Text = config.Rows[0][21].ToString();
                    textBox42.Text = config.Rows[0][22].ToString();
                    textBox43.Text = config.Rows[0][23].ToString();
                    textBox44.Text = config.Rows[0][24].ToString();
                    textBox45.Text = config.Rows[0][25].ToString();
                    textBox46.Text = config.Rows[0][26].ToString();
                    textBox47.Text = config.Rows[0][27].ToString();
                    textBox48.Text = config.Rows[0][28].ToString();
                    textBox49.Text = config.Rows[0][29].ToString();

                    textBox50.Text = config.Rows[0][30].ToString();
                    textBox51.Text = config.Rows[0][31].ToString();
                    textBox52.Text = config.Rows[0][32].ToString();
                    textBox53.Text = config.Rows[0][33].ToString();
                    textBox54.Text = config.Rows[0][34].ToString();
                    textBox55.Text = config.Rows[0][35].ToString();
                    textBox56.Text = config.Rows[0][36].ToString();
                    textBox57.Text = config.Rows[0][37].ToString();
                    textBox58.Text = config.Rows[0][38].ToString();
                    textBox59.Text = config.Rows[0][39].ToString();

                    textBox60.Text = config.Rows[0][40].ToString();
                    textBox61.Text = config.Rows[0][41].ToString();
                    textBox62.Text = config.Rows[0][42].ToString();
                    textBox63.Text = config.Rows[0][43].ToString();
                    textBox64.Text = config.Rows[0][44].ToString();
                    textBox65.Text = config.Rows[0][45].ToString();
                    textBox66.Text = config.Rows[0][46].ToString();
                    textBox67.Text = config.Rows[0][47].ToString();
                    textBox68.Text = config.Rows[0][48].ToString();
                    textBox69.Text = config.Rows[0][49].ToString();

                    textBox70.Text = config.Rows[0][50].ToString();
                    textBox71.Text = config.Rows[0][51].ToString();
                    textBox72.Text = config.Rows[0][52].ToString();
                    textBox73.Text = config.Rows[0][53].ToString();
                    textBox74.Text = config.Rows[0][54].ToString();
                    textBox75.Text = config.Rows[0][55].ToString();
                    textBox76.Text = config.Rows[0][56].ToString();
                    textBox77.Text = config.Rows[0][57].ToString();
                    textBox78.Text = config.Rows[0][58].ToString();
                    textBox79.Text = config.Rows[0][59].ToString();

                    textBox80.Text = config.Rows[0][60].ToString();
                    textBox81.Text = config.Rows[0][61].ToString();
                }
                lineupfile.Close();
            }

            else
            {
                // Create the file.
                File.Create(configfilename);
            }
        }

        private void WriteConfigurationFile()
        {

            string configfilename = FolderPath + @"\Config.txt";
            try
            {
                if (File.Exists(configfilename))
                {
                    StreamWriter configfile = new StreamWriter(configfilename);

                    DataTable config = ConfigDataSet.Tables["Config"];

                    for (int i = 0; i < 61; i++)
                    {
                        configfile.Write(config.Rows[0][i] + "\x00");

                    }
                    configfile.WriteLine(config.Rows[0][61]);
                    configfile.Close();
                    toolStripStatusLabel1.Text = "File Save Completed: " + config.Rows.Count + " channels saved";
                }
                else
                {
                    // Create the file.
                    File.Create(configfilename);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Cannot save configuration", "My Application", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }

        }


        private void SendOldConfig_Click(object sender, EventArgs e)
        {
            serial s = new serial();
            WriteConfigurationFile();
            DataTable config = ConfigDataSet.Tables["Config"];

            string confg3 = config.Rows[0][2].ToString();
            string confg4 = config.Rows[0][3].ToString();
            string confg5 = config.Rows[0][4].ToString();
            string confg9 = config.Rows[0][8].ToString();

            char[] cfg1 = new char[] { Convert.ToChar(config.Rows[0][0]) };
            char[] cfg2 = new char[] { Convert.ToChar(config.Rows[0][1]) };
            char[] cfg3 = confg3.ToCharArray();
            char[] cfg4 = confg4.ToCharArray();
            char[] cfg5 = confg5.ToCharArray();
            char[] cfg6 = new char[] { Convert.ToChar(config.Rows[0][5]) };
            char[] cfg7 = new char[] { Convert.ToChar(config.Rows[0][6]) };
            char[] cfg8 = new char[] { Convert.ToChar(config.Rows[0][7]) };
            char[] cfg9 = confg9.ToCharArray();
            char[] cfg10 = new char[] { Convert.ToChar(config.Rows[0][9]) };
            char[] cfg11 = new char[] { Convert.ToChar(config.Rows[0][10]) };
            char[] cfg12 = new char[] { Convert.ToChar(config.Rows[0][11]) };
            char[] cfg13 = new char[] { Convert.ToChar(config.Rows[0][12]) };
            char[] cfg14 = new char[] { Convert.ToChar(config.Rows[0][13]) };
            char[] cfg15 = new char[] { Convert.ToChar(config.Rows[0][14]) };
            char[] cfg16 = new char[] { Convert.ToChar(config.Rows[0][15]) };
            char[] cfg17 = new char[] { Convert.ToChar(config.Rows[0][16]) };
            char[] cfg18 = new char[] { Convert.ToChar(config.Rows[0][17]) };
            char[] cfg19 = new char[] { Convert.ToChar(config.Rows[0][18]) };

            List<char> list = new List<char>();
            list.AddRange(cfg1);
            list.AddRange(cfg2);
            list.AddRange(cfg3);
            list.AddRange(cfg4);
            list.AddRange(cfg5);
            list.AddRange(cfg6);
            list.AddRange(cfg7);
            list.AddRange(cfg8);
            list.AddRange(cfg9);
            list.AddRange(cfg10);
            list.AddRange(cfg11);
            list.AddRange(cfg12);
            list.AddRange(cfg13);
            list.AddRange(cfg14);
            list.AddRange(cfg15);
            list.AddRange(cfg16);
            list.AddRange(cfg17);
            list.AddRange(cfg18);
            list.AddRange(cfg19);

            char[] body = list.ToArray();

            s.TransmitMessage(serialPort1, FolderPath, "box on", Properties.Settings.Default.Select_Code.ToCharArray());
            s.TransmitMessage(serialPort1, FolderPath, "config", body);
            s.TransmitMessage(serialPort1, FolderPath, "box off", empty);

            toolStripStatusLabel1.Text = "Old config Information sent via serial";
        }
        public int ind = 35;
        private void SendNewConfig_Click(object sender, EventArgs e)
        {
            serial s = new serial();

            WriteConfigurationFile();
            /*
                        19  % 01l decimal 2  (Roll and Hold time)
                        20  % 01l char    C  (Grid Source/Channel Order)
                        21  % 01l decimal 0  (Movie Recap Review in F Setting - 0 for No, 1 for Yes, Frequency)
                        22  % 01l decimal 1  (SumbySource SBS in F Setting - 0 for No, 1 for Yes Frequency) 3
                        23  % 02l decimal 08  (Grid SummbySource Forward Window - defaults to 24)
                        24  % 02l decimal 08  (Scroll SummbySource Forward Window - defaults to 24)
                        25  % 01l char G  (Display mode - g or s)
                        26  % 01l char N  (Graphic Ad Insertion for Advantage Systems)
                        27  % 01l char A  (Pie Format - Default is A)
                        28  % 01l char E  (Unknown Effect)
                        29  % 01l decimal 0  (Unknown effect)
                        30  % 01l decimal 1  (Sports Summary in F Setting - 0 for No, 1 for Yes Frequency)
                        31  % 01l char N  (Unknown Effect)
                        32  % 01l char N  (Unknown Effect)
                        33  % 01l char N  (Unknown Effect)
                        34  % 01l char N  (Unknown Effect)
                        35  % 01l char N  Turns on the Pause Every Three Listings..(Y is the Default)
                        36  % 01l char N  (Unknown Effect)
                        37  % 01l char L  
                        38  % 02l decimal 29 
                        39  % 02l decimal 06 
                        40  % 01l char Y Cycle in F Setting(Y for Yes and N for No, Defaults to Y)
                        41  % 01l char Y (Unknown Effect)
                        42  % 01l char Y Locking the F Setting(N locks the setting / Y shows the setting) 
                        43  % 02l decimal 23
                        44  % 02l decimal 36
                        45  % 02l decimal 06
                        46  % 03l decimal 015 (AftrOrdr - F Setting(3 digits))[Confirmed]
                        47  % 01l decimal 1 (CycleFrequency - F Setting cannot be 0, defaults to 1)[Confirmed]
                        48  % 2.2 string 00 (Defaults to DT)
                        49  % 01l char Y (Unknown Effect)
                        50  % 01l char N (if Y then Army Time - also updates listings with Army Time as well) [Confirmed]
                        51  % 01l char Y (Unknown Effect)
                        52  % 01  char C (Unknown Effect)
                        53  % 01  char Ž [HEX CODE = 8E, Decimal 142] (Unknown Effect)
                        54  % 01  decimal 8 (Unknown Effect)
                        55  % 01  char N  (Unknown effect)
                        56  % 01  char N  (Unknown effect)
                        57  % 01  char N  (Unknown effect)
                        58  % 01  char N   Unlocks the Scroll Bar UP and Scroll Bar Down with + and - keys(Y is the default)
                        59  % 01  char N  (Unknown Effect)
                        60  % 01  char 1  (ClockCmd in F Setting) [Confirmed] (can't be 0 or can't exceed 2, will default to 1)

                */
                char[] commandfam = new char[] { '1' };
                char[] msglength = new char[] { Convert.ToChar(ind) };
                
                DataTable config = ConfigDataSet.Tables["Config"];
                //command family = 1 for ASCII
                //length of command including checksum


                string confg20 = config.Rows[0][19].ToString();
                string confg22 = config.Rows[0][21].ToString();
                string confg23 = config.Rows[0][22].ToString();
                string confg24 = config.Rows[0][23].ToString().PadLeft(2, '0');
                string confg25 = config.Rows[0][24].ToString().PadLeft(2, '0');
                string confg30 = config.Rows[0][29].ToString();
                string confg31 = config.Rows[0][30].ToString();
                string confg39 = config.Rows[0][38].ToString().PadLeft(2, '0');
                string confg40 = config.Rows[0][39].ToString().PadLeft(2, '0');
                string confg44 = config.Rows[0][43].ToString().PadLeft(2, '0');
                string confg45 = config.Rows[0][44].ToString().PadLeft(2, '0');
                string confg46 = config.Rows[0][45].ToString().PadLeft(2, '0');
                string confg47 = config.Rows[0][46].ToString().PadLeft(3, '0');
                string confg48 = config.Rows[0][47].ToString();
                string confg49 = config.Rows[0][48].ToString();
                //string confg54 = config.Rows[0][53].ToString();//dec to char
                //string confg55 = config.Rows[0][54].ToString();//dec to char
                string confg56 = config.Rows[0][55].ToString();

                char[] cfg20 = confg20.ToCharArray();
                char[] cfg21 = new char[] { Convert.ToChar(config.Rows[0][20]) };
                char[] cfg22 = confg22.ToCharArray();
                char[] cfg23 = confg23.ToCharArray();
                char[] cfg24 = confg24.ToCharArray();
                char[] cfg25 = confg25.ToCharArray();
                char[] cfg26 = new char[] { Convert.ToChar(config.Rows[0][25]) };
                char[] cfg27 = new char[] { Convert.ToChar(config.Rows[0][26]) };
                char[] cfg28 = new char[] { Convert.ToChar(config.Rows[0][27]) };
                char[] cfg29 = new char[] { Convert.ToChar(config.Rows[0][28]) };
                char[] cfg30 = confg30.ToCharArray();
                char[] cfg31 = confg31.ToCharArray();
                char[] cfg32 = new char[] { Convert.ToChar(config.Rows[0][31]) };
                char[] cfg33 = new char[] { Convert.ToChar(config.Rows[0][32]) };
                char[] cfg34 = new char[] { Convert.ToChar(config.Rows[0][33]) };
                char[] cfg35 = new char[] { Convert.ToChar(config.Rows[0][34]) };
                char[] cfg36 = new char[] { Convert.ToChar(config.Rows[0][35]) };
                char[] cfg37 = new char[] { Convert.ToChar(config.Rows[0][36]) };
                char[] cfg38 = new char[] { Convert.ToChar(config.Rows[0][37]) };
                char[] cfg39 = confg39.ToCharArray();
                char[] cfg40 = confg40.ToCharArray();
                char[] cfg41 = new char[] { Convert.ToChar(config.Rows[0][40]) };
                char[] cfg42 = new char[] { Convert.ToChar(config.Rows[0][41]) };
                char[] cfg43 = new char[] { Convert.ToChar(config.Rows[0][42]) };
                char[] cfg44 = confg44.ToCharArray();
                char[] cfg45 = confg45.ToCharArray();
                char[] cfg46 = confg46.ToCharArray();
                char[] cfg47 = confg47.ToCharArray();
                char[] cfg48 = confg48.ToCharArray();
                char[] cfg49 = confg49.ToCharArray();
                char[] cfg50 = new char[] { Convert.ToChar(config.Rows[0][49]) };
                char[] cfg51 = new char[] { Convert.ToChar(config.Rows[0][50]) };
                char[] cfg52 = new char[] { Convert.ToChar(config.Rows[0][51]) };
                char[] cfg53 = new char[] { Convert.ToChar(config.Rows[0][52]) };
                char[] cfg54 = new char[] { Convert.ToChar('\x8E') }; //confg54.ToCharArray();
              //char[] cfg55 = new char[] { Convert.ToChar(config.Rows[0][54]) };
                char[] cfg56 = confg56.ToCharArray();
                char[] cfg57 = new char[] { Convert.ToChar(config.Rows[0][56]) };
                char[] cfg58 = new char[] { Convert.ToChar(config.Rows[0][57]) };
                char[] cfg59 = new char[] { Convert.ToChar(config.Rows[0][58]) };
                char[] cfg60 = new char[] { Convert.ToChar(config.Rows[0][59]) };
                char[] cfg61 = new char[] { Convert.ToChar(config.Rows[0][60]) };
                char[] cfg62 = new char[] { Convert.ToChar(config.Rows[0][61]) };

                List<char> list = new List<char>();
                list.AddRange(commandfam);
                list.AddRange(msglength);
                list.AddRange(cfg20);
                list.AddRange(cfg21);
                list.AddRange(cfg22);
                list.AddRange(cfg23);
                list.AddRange(cfg24);
                list.AddRange(cfg25);
                list.AddRange(cfg26);
                list.AddRange(cfg27);
                list.AddRange(cfg28);
                list.AddRange(cfg29);
                list.AddRange(cfg30);
                list.AddRange(cfg31);
                list.AddRange(cfg32);
                list.AddRange(cfg33);
                list.AddRange(cfg34);
                list.AddRange(cfg35);
                list.AddRange(cfg36);
                list.AddRange(cfg37);
                list.AddRange(cfg38);
                list.AddRange(cfg39);
                list.AddRange(cfg40);
                list.AddRange(cfg41);
                list.AddRange(cfg42);
                list.AddRange(cfg43);
                list.AddRange(cfg44);
                list.AddRange(cfg45);
                list.AddRange(cfg46);
                list.AddRange(cfg47);
                list.AddRange(cfg48);
                list.AddRange(cfg49);
                list.AddRange(cfg50);
                list.AddRange(cfg51);
                list.AddRange(cfg52);
                list.AddRange(cfg53);
                list.AddRange(cfg54);
                //list.AddRange(cfg55);
                list.AddRange(cfg56);
                list.AddRange(cfg57);
                list.AddRange(cfg58);
                list.AddRange(cfg59);
                list.AddRange(cfg60);
                list.AddRange(cfg61);
                list.AddRange(cfg62);

                char[] body = list.ToArray();

                s.TransmitMessage(serialPort1, FolderPath, "box on", Properties.Settings.Default.Select_Code.ToCharArray());
                s.TransmitMessage(serialPort1, FolderPath, "new config", body);
                s.TransmitMessage(serialPort1, FolderPath, "box off", empty);
                label116.Text = ind.ToString();
                ind++;
                toolStripStatusLabel1.Text = "New Config Information sent via serial";
 
        }





        private void Update_QTable_Entry_Click(object sender, EventArgs e)
        {
            DataTable QTable = QTableDataSet.Tables["QTable"];
            QTable.AcceptChanges();
            QTableFile qtable = new QTableFile();
            qtable.WriteQTableFile(FolderPath, QTable);

            toolStripStatusLabel1.Text = "Entries have been updated to Q Table.";
        }

        private void Delete_QTable_Entry_Click(object sender, EventArgs e)
        {

            if (MessageBox.Show("Would you like to to delete the selected entries?", "My Application", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DataTable QTable = QTableDataSet.Tables["QTable"];

                foreach (DataGridViewRow item in this.dataGridView5.SelectedRows)
                {
                    dataGridView5.Rows.RemoveAt(item.Index);
                }
                QTable.AcceptChanges();
                QTableFile qtable = new QTableFile();
                qtable.WriteQTableFile(FolderPath, QTable);
                toolStripStatusLabel1.Text = "Selected Q Table entries have been deleted.";
            }
            else
            {
                toolStripStatusLabel1.Text = "No Q Table entries have been deleted.";
            }

        }

        private void Send_QTable_Click(object sender, EventArgs e)
        {
            serial s = new serial();
            string QTablefilename = FolderPath + @"\qtable.txt";

            if (File.Exists(QTablefilename))
            {
                s.TransmitMessage(serialPort1, FolderPath, "box on", Properties.Settings.Default.Select_Code.ToCharArray());
                DataTable QTable = QTableDataSet.Tables["QTable"];
                List<char> list = new List<char>();

                for (int counter = 0; counter < QTable.Rows.Count; counter++)
                {
                    string srrc = QTable.Rows[counter]["QTSource"].ToString();
                    string name = QTable.Rows[counter]["QTName"].ToString();
                    char[] Source = srrc.ToCharArray();
                    char[] ChName = name.ToCharArray();

                    list.AddRange(Source);
                    list.AddRange("\x09");
                    list.AddRange("\x18");
                    list.AddRange(ChName);
                    list.AddRange("\x0d");
                    list.AddRange("\x0a");

                }
                list.AddRange("\x1a");
                char[] body = list.ToArray();

                s.TransmitMessage(serialPort1, FolderPath, "qtable", body);
                s.TransmitMessage(serialPort1, FolderPath, "box off", empty);
                toolStripStatusLabel1.Text = "Q Table sent over serial port";
            }

        }

        private void button26_Click(object sender, EventArgs e)
        {
            serial s = new serial();
            int jdate = 90; // Convert.ToInt16(textBox93.Text);

            char[] msglength = new char[] { Convert.ToChar(14) };  //length of command including checksum

            string sourceid = "PREV02"; //textBox94.Text;
            int telvue = 35; // Convert.ToInt16(textBox95.Text);
            int sportsum = 44; // Convert.ToInt16(textBox96.Text);
            int gridbckgclr = 255;  //Convert.ToInt16(textBox97.Text);
            int gridforegclr = 255; // Convert.ToInt16(textBox98.Text);
            string brushid = "DT"; // textBox99.Text;

            char[] jdte = new char[] { Convert.ToChar(jdate) };
            char[] srcid = sourceid.ToCharArray();
            char[] flag2 = new char[] { Convert.ToChar(telvue) };
            char[] sports = new char[] { Convert.ToChar(sportsum) };
            char[] bgcolor = new char[] { Convert.ToChar(gridbckgclr) };
            char[] fgcolor = new char[] { Convert.ToChar(gridforegclr) };
            char[] brshid = brushid.ToCharArray();

            List<char> list = new List<char>();
            list.AddRange(jdte);
            list.AddRange(msglength);
            list.AddRange(srcid);
            list.AddRange(flag2);
            list.AddRange(sports);
            list.AddRange(bgcolor);
            list.AddRange(fgcolor);
            list.AddRange(brshid);

            char[] body = list.ToArray();

            s.TransmitMessage(serialPort1, FolderPath, "box on", Properties.Settings.Default.Select_Code.ToCharArray());
            s.TransmitMessage(serialPort1, FolderPath, "chan attr", body);
            s.TransmitMessage(serialPort1, FolderPath, "box off", empty);

            toolStripStatusLabel1.Text = "Channel Lineup Attributes sent via serial";
        }


        private void Zap2ItUserSettings_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.DlNumberOfDays = textBox100.Text;
            Properties.Settings.Default.DlUsername = textBox101.Text;
            Properties.Settings.Default.DlPassword = textBox102.Text;
            Properties.Settings.Default.Save();
        }

        private void Download_Click(object sender, EventArgs e)
        {
            Zap2ItXMLFile listingxml = new Zap2ItXMLFile();
            listingxml.DownloadZap2ItXMLInfo(FolderPath,
                 Properties.Settings.Default.DlNumberOfDays,
                 @Properties.Settings.Default.DlUsername,
                 @Properties.Settings.Default.DlPassword);

            Zap2ItLineup zaplineup = new Zap2ItLineup();
            zaplineup.ParseXMLforLineup(FolderPath, downloadLineupDataSet.Tables["dllineup"]);

            Zap2ItListings zaplistings = new Zap2ItListings();
            zaplistings.ParseXMLforListings(FolderPath, downloadListingDataSet.Tables["dllisting"]);


            toolStripStatusLabel1.Text = "Zap2Xml Information has downloaded.";
        }

        private void MergeLineup_Click(object sender, EventArgs e)
        {
            DataTable dwnldlineup = downloadLineupDataSet.Tables["dllineup"];
            DataTable lineup = LineupDataSet.Tables["Lineup"];
            Form2 fm2 = new Form2();
            lineup.Clear();
            fm2.Show();
            foreach (DataRow readRow in dwnldlineup.Rows)
            {
                fm2.ProgressBarBuild(lineup.Rows.Count, 0, dwnldlineup.Rows.Count);
                DataRow workRow = lineup.NewRow();
                workRow["SourceID"] = readRow["id"];
                workRow["Number"] = readRow["channum"];
                workRow["ID"] = readRow["call_letters"];
                workRow["RedHiLt"] = readRow["RedHiLt"];
                workRow["SBS"] = readRow["SBS"];
                workRow["PTagDisable"] = readRow["PTagDisable"];
                workRow["PPVSrc"] = readRow["PPVSrc"];
                workRow["DittoEnable"] = readRow["DittoEnable"];
                workRow["LtBlueHiLt"] = readRow["LtBlueHiLt"];
                workRow["StereoSrc"] = readRow["StereoSrc"];
                workRow["Daypt1"] = readRow["Daypt1"];
                workRow["Daypt2"] = readRow["Daypt2"];
                workRow["Daypt3"] = readRow["Daypt3"];
                workRow["Daypt4"] = readRow["Daypt4"];
                workRow["Daypt5"] = readRow["Daypt5"];
                workRow["Daypt6"] = readRow["Daypt6"];

                lineup.Rows.Add(workRow);

            }
            fm2.Close();
        }

        private void MergeListing()
        {
            DataTable dwnldlistings = downloadListingDataSet.Tables["dllisting"];
            DataTable listings = ListingDataSet.Tables["Listings"];
            listings.Clear();
            Form2 fm2 = new Form2();
            fm2.Show();
            foreach (DataRow readRow in dwnldlistings.Rows)
            {
                fm2.ProgressBarBuild(listings.Rows.Count, 0, dwnldlistings.Rows.Count);
                DataRow workRow = listings.NewRow();

                workRow["JulianDay"] = readRow["jdate"];
                workRow["SourceID"] = readRow["id"];
                workRow["TimeSlot"] = readRow["timeslot"];
                workRow["Attr"] = "1";
                workRow["Title"] = readRow["title"];
                listings.Rows.Add(workRow);

            }
            fm2.Close();


        }

        private void MergeListing_Click(object sender, EventArgs e)
        {

            Thread thr1 = new Thread(MergeListing);
            thr1.Start();
        }




        //Experimental Work

        private void BuildQTable_Click(object sender, EventArgs e)
        {
            serial s = new serial();

            string string1 = "XYZ	I'm am the greatestsinceCassiou Clay ";
            string string2 = " XY1	I'm am the greatestsinceCassiou Clay1 ";

            List<char> list = new List<char>();
            list.AddRange(string1.ToCharArray());
            list.AddRange(string2.ToCharArray());
            char[] body = list.ToArray();

            s.TransmitMessage(serialPort1, FolderPath, "box on", Properties.Settings.Default.Select_Code.ToCharArray());
            s.TransmitMessage(serialPort1, FolderPath, "qtable", body);
            s.TransmitMessage(serialPort1, FolderPath, "box off", empty);

            toolStripStatusLabel1.Text = "QTable build command sent via serial";
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            serial s = new serial();

            //char[] info = new char[] { '\x04','\u0095','N','0','1','0','0','2','\x12','\x01','\x18','W','A','T','C','H','\x19','T','H','E',' ','P','R','E','V','U','E',' ','G','U','I','D','E','\x18','1','-','9','9','9','-','9','9','9','-','9','9','9','9' };

            char[] jdate = new char[] { Convert.ToChar(1) };
            string title = "YOU ARE WATCHING PREVUE GUIDE";
            char[] titlearray = title.ToCharArray();
            char[] flag1 = new char[] { Convert.ToChar(18) };
            string source = "PRE001";
            char[] sourceid = source.ToCharArray();
            char[] flag2 = new char[] { Convert.ToChar(1) };
            char[] value1 = new char[] { Convert.ToChar(168) };
            char[] flag3 = new char[] { Convert.ToChar(1) };
            char[] value2 = new char[] { Convert.ToChar(169) };
            char[] flag4 = new char[] { Convert.ToChar(1) };
            char[] value3 = new char[] { Convert.ToChar(170) };

            List<char> list = new List<char>();
            list.AddRange(jdate);
            list.AddRange(titlearray);
            list.AddRange(flag1);
            list.AddRange(sourceid);
            list.AddRange(flag2);
            list.AddRange(value1);
            list.AddRange(flag3);
            list.AddRange(value2);
            list.AddRange(flag4);
            list.AddRange(value3);

            char[] body = list.ToArray();

            s.TransmitMessage(serialPort1, FolderPath, "box on", Properties.Settings.Default.Select_Code.ToCharArray());
            s.TransmitMessage(serialPort1, FolderPath, "new listing", body);
            s.TransmitMessage(serialPort1, FolderPath, "box off", empty);

            toolStripStatusLabel1.Text = "test sent via serial";
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            serial s = new serial();
            char[] body = new char[] { '\x1b' };

            s.TransmitMessage(serialPort1, FolderPath, "box on", Properties.Settings.Default.Select_Code.ToCharArray());
            //s.TransmitMessage(body, empty);
            s.TransmitMessage(serialPort1, FolderPath, "box off", empty);

            toolStripStatusLabel1.Text = "test sent via serial";
        }

        private void UtilityCommand_Click(object sender, EventArgs e)
        {
            serial s = new serial();

            char[] commandfamily = new char[] { Convert.ToChar(textBox114.Text) };
            string member = textBox115.Text;
            string submember = textBox116.Text;
            
            List<char> list = new List<char>();
            list.AddRange(commandfamily);
            list.AddRange(member.ToCharArray());
            list.AddRange(submember.ToCharArray());

            char[] body = list.ToArray();

            s.TransmitMessage(serialPort1, FolderPath, "box on", Properties.Settings.Default.Select_Code.ToCharArray());
            s.TransmitMessage(serialPort1, FolderPath, "utility", body);
            s.TransmitMessage(serialPort1, FolderPath, "box off", empty);
        }
    }
}

