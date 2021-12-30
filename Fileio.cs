using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
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
using WindowsFormsApplication1;

namespace FileIO
{
    class SerialFile 
    {
        public void WriteSerialLogFile(byte[] array, string folderpath)
        {
            string serial_log_filename = folderpath + @"\serial_log.txt";
            byte[] read_data = new byte[] { };
            byte[] line_end = new byte[] { (byte)'\x0D', (byte)'\x0A' };
            try
            {
                StreamReader serlogfile = new StreamReader(serial_log_filename);
                {
                    read_data = File.ReadAllBytes(serial_log_filename);
                    serlogfile.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            try
            {
                FileStream writeStream = new FileStream(serial_log_filename, FileMode.Create);

                BinaryWriter writeData = new BinaryWriter(writeStream);
                writeData.Write(read_data);
                writeData.Write(line_end);
                writeData.Write(array);
                writeData.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

    }

    public class Zap2ItXMLFile
    {
        public void DownloadZap2ItXMLInfo(string folderpath, string numofdays, string username, string password)
        {
            Process pProcess = new Process();
            //toolStripStatusLabel1.Text = "Zap2Xml Information downloading. Please wait...";
            pProcess.StartInfo.FileName = folderpath + @"\zap2xml.exe";
            pProcess.StartInfo.Arguments = @"-d " + numofdays +
                " -u " + @username +
                " -p " + @password +
                " -o " + folderpath + @"\data.xml"; //argument
            pProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            pProcess.Start();
            pProcess.WaitForExit();

        }

    }

    class LineupFile 
    {
        public void ReadLineupFile(string folderpath, DataTable lineup)
        {
            string data;
            string lineupfilename = folderpath + @"\Channel Lineup.txt";

            if (File.Exists(lineupfilename))
            {
                StreamReader lineupfile = new StreamReader(lineupfilename);

                while ((data = lineupfile.ReadLine()) != null)
                {
                    char[] delimiterChars = { '\x00' };
                    string text = data;
                    string[] parseinfo = text.Split(delimiterChars);

                    DataRow workRow;
                    workRow = lineup.NewRow();
                    workRow["SourceID"] = parseinfo[0];
                    workRow["Number"] = parseinfo[1];
                    workRow["ID"] = parseinfo[2];
                    workRow["RedHiLt"] = parseinfo[3];
                    workRow["SBS"] = parseinfo[4];
                    workRow["PTagDisable"] = parseinfo[5];
                    workRow["PPVSrc"] = parseinfo[6];
                    workRow["DittoEnable"] = parseinfo[7];
                    workRow["LtBlueHiLt"] = parseinfo[8];
                    workRow["StereoSrc"] = parseinfo[9];
                    workRow["Daypt1"] = Int32.Parse(parseinfo[10]);
                    workRow["Daypt2"] = Int32.Parse(parseinfo[11]);
                    workRow["Daypt3"] = Int32.Parse(parseinfo[12]);
                    workRow["Daypt4"] = Int32.Parse(parseinfo[13]);
                    workRow["Daypt5"] = Int32.Parse(parseinfo[14]);
                    workRow["Daypt6"] = Int32.Parse(parseinfo[15]);

                    lineup.Rows.Add(workRow);

                    //toolStripStatusLabel1.Text = "File Uploading";
                }
                lineupfile.Close();
                //toolStripStatusLabel1.Text = "File Reading Completed: " + lineup.Rows.Count + " channels found";
            }

            else
            {
                // Create the file.
                File.Create(lineupfilename);
            }
        }

        public void WriteLineupFile(string folderpath, DataTable lineup)
        {
            string lineupfilename = folderpath + @"\Channel Lineup.txt";
            try
            {
                if (File.Exists(lineupfilename))
                {
                    StreamWriter lineupfile = new StreamWriter(lineupfilename);

                    for (int i = 0; i < lineup.Rows.Count; i++)
                    {
                        lineupfile.WriteLine(lineup.Rows[i]["SourceID"] + "\x00" +
                                             lineup.Rows[i]["Number"] + "\x00" +
                                             lineup.Rows[i]["ID"] + "\x00" +
                                             lineup.Rows[i]["RedHiLt"] + "\x00" +
                                             lineup.Rows[i]["SBS"] + "\x00" +
                                             lineup.Rows[i]["PTagDisable"] + "\x00" +
                                             lineup.Rows[i]["PPVSrc"] + "\x00" +
                                             lineup.Rows[i]["DittoEnable"] + "\x00" +
                                             lineup.Rows[i]["LtBlueHiLt"] + "\x00" +
                                             lineup.Rows[i]["StereoSrc"] + "\x00" +
                                             lineup.Rows[i]["Daypt1"] + "\x00" +
                                             lineup.Rows[i]["Daypt2"] + "\x00" +
                                             lineup.Rows[i]["Daypt3"] + "\x00" +
                                             lineup.Rows[i]["Daypt4"] + "\x00" +
                                             lineup.Rows[i]["Daypt5"] + "\x00" +
                                             lineup.Rows[i]["Daypt6"]);

                        //toolStripStatusLabel1.Text = "File Saving:" + lineup.Rows.Count + " channels saved";
                    }

                    lineupfile.Close();
                    //toolStripStatusLabel1.Text = "File Save Completed: " + lineup.Rows.Count + " channels saved";
                }
                else
                {
                    // Create the file.
                    File.Create(lineupfilename);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Cannot save channel lineup", "My Application", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }

        }

    }
    class ListingsFile
    {
        public void WriteListingsFile(string folderpath, DataTable listing)
        {
            string listingsfilename = folderpath + @"\Listings.txt";
            try
            {
                if (File.Exists(listingsfilename))
                {
                    StreamWriter listingsfile = new StreamWriter(listingsfilename);

                    for (int i = 0; i < listing.Rows.Count; i++)
                    {
                        listingsfile.WriteLine(listing.Rows[i]["JulianDay"] + "\x00" +
                                               listing.Rows[i]["SourceID"] + "\x00" +
                                               listing.Rows[i]["TimeSlot"] + "\x00" +
                                               listing.Rows[i]["Title"] + "\x00" +
                                               listing.Rows[i]["Attr"]);

                        //f.toolStripStatusLabel1.Text = "File Saving:" + listing.Rows.Count + " listings saved";
                    }

                    listingsfile.Close();
                    //f.toolStripStatusLabel1.Text = "File Save Completed: " + listing.Rows.Count + " listings saved";
                }
                else
                {
                    // Create the file.
                    File.Create(listingsfilename);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Cannot save listings data", "My Application", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }

        }

        public void ReadListingsFile(string folderpath, DataTable listing)
        {
            string listingsfilename = folderpath + @"\Listings.txt";
            string data;
            try
            {
                if (File.Exists(listingsfilename))
                {
                    StreamReader listingsfile = new StreamReader(listingsfilename);

                    while ((data = listingsfile.ReadLine()) != null)
                    {
                        char[] delimiterChars = { '\x00' };
                        string text = data;
                        string[] parseinfo = text.Split(delimiterChars);

                        DataRow workRow;
                        workRow = listing.NewRow();
                        workRow["JulianDay"] = Int32.Parse(parseinfo[0]);
                        workRow["SourceID"] = parseinfo[1];
                        workRow["TimeSlot"] = Int32.Parse(parseinfo[2]);
                        workRow["Attr"] = Int32.Parse(parseinfo[4]);
                        workRow["Title"] = parseinfo[3];
                        listing.Rows.Add(workRow);

                        //f.toolStripStatusLabel1.Text = "File Uploading" + listing.Rows.Count + " listings found";

                    }

                    listingsfile.Close();
                    //f.toolStripStatusLabel1.Text = "File Reading Completed: " + listing.Rows.Count + " listings found";
                }
                else
                {
                    // Create the file.
                    File.Create(listingsfilename);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Cannot save listings data", "My Application", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }
    }
    class ConfigFile 
    { 
    

    }
    class WeatherFile
    {
        public List<string> DownloadWeatherData(string url, string weatherdatafile)
        {
            List<string> parsedlines = new List<string>();
            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
            request.Credentials = new NetworkCredential("anonymous", "janeDoe@contoso.com");
            request.Method = WebRequestMethods.Ftp.DownloadFile;
            try
            {
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    Stream responseStream = response.GetResponseStream();
                    
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        while (!reader.EndOfStream)
                        {
                           parsedlines.Add(reader.ReadLine());
                        }
                        response.Close();
                        reader.Close();
                    }
                }

                File.WriteAllLines(weatherdatafile, parsedlines);
                return parsedlines;
            }
            catch (Exception)
            {
                MessageBox.Show("Weather Data not available at this time.", "Prevue Weather", MessageBoxButtons.OK);
                return parsedlines;
            }
        }

        public void ReadWeatherFile(string folderpath, DataTable weather)
        {
            string weatherfilename = folderpath + @"\weatherdata.txt";
            string data;

            if (File.Exists(weatherfilename))
            {
                StreamReader weatherfile = new StreamReader(weatherfilename);

                while ((data = weatherfile.ReadLine()) != null)
                {
                    char[] delimiterChars = { '\x00' };
                    string text = data;
                    string[] parseinfo = text.Split(delimiterChars);

                    DataRow workRow;
                    workRow = weather.NewRow();

                    workRow["display_length"] = Int32.Parse(parseinfo[0]);
                    workRow["ColorID"] = Int32.Parse(parseinfo[1]);
                    workRow["IconID"] = Int32.Parse(parseinfo[2]);
                    workRow["Expansion"] = Int32.Parse(parseinfo[3]);
                    workRow["IDString"] = parseinfo[4];
                    workRow["CurSky"] = parseinfo[5];
                    workRow["CurWeather"] = parseinfo[6];
                    workRow["CurTemp"] = parseinfo[7];
                    workRow["CurWind"] = parseinfo[8];
                    workRow["CurPressure"] = parseinfo[9];
                    workRow["CurHumidity"] = parseinfo[10];
                    workRow["CurDewPoint"] = parseinfo[11];
                    workRow["CurVisibility"] = parseinfo[12];

                    weather.Rows.Add(workRow);

                    //f.toolStripStatusLabel1.Text = "File Uploading" + weather.Rows.Count + " records found";

                }

                weatherfile.Close();
                //f.toolStripStatusLabel1.Text = "File Reading Completed: " + weather.Rows.Count + " records found";
            }
            else
            {
                // Create the file.
                File.Create(weatherfilename);
            }
        }

        public void WriteWeatherFile(string folderpath, DataTable weather)
        {
            string weatherfilename = folderpath + @"\weatherdata.txt";
            try
            {
                if (File.Exists(weatherfilename))
                {
                    StreamWriter weatherfile = new StreamWriter(weatherfilename);

                    //DataTable weather = f.WeatherDataSet.Tables["Weather"];

                    for (int i = 0; i < weather.Rows.Count; i++)
                    {
                        weatherfile.WriteLine(weather.Rows[i]["display_length"] + "\x00" +
                                              weather.Rows[i]["ColorID"] + "\x00" +
                                              weather.Rows[i]["IconID"] + "\x00" +
                                              weather.Rows[i]["Expansion"] + "\x00" +
                                              weather.Rows[i]["IDString"] + "\x00" +
                                              weather.Rows[i]["CurSky"] + "\x00" +
                                              weather.Rows[i]["CurWeather"] + "\x00" +
                                              weather.Rows[i]["CurTemp"] + "\x00" +
                                              weather.Rows[i]["CurWind"] + "\x00" +
                                              weather.Rows[i]["CurPressure"] + "\x00" +
                                              weather.Rows[i]["CurHumidity"] + "\x00" +
                                              weather.Rows[i]["CurDewPoint"] + "\x00" +
                                              weather.Rows[i]["CurVisibility"]);

                        //f.toolStripStatusLabel1.Text = "File Saving:" + weather.Rows.Count + " records saved";
                    }

                    weatherfile.Close();
                    //f.toolStripStatusLabel1.Text = "File Save Completed: " + weather.Rows.Count + " records saved";
                }
                else
                {
                    // Create the file.
                    File.Create(weatherfilename);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Cannot save weather data", "My Application", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }
    }

    class QTableFile
    {
        public void ReadQTableFile(string folderpath, DataTable QTable)
        {
            string data;
            string QTablefilename = folderpath + @"\qtable.txt";

            if (File.Exists(QTablefilename))
            {
                StreamReader QTablefile = new StreamReader(QTablefilename);
                //DataTable QTable = QTableDataSet.Tables["QTable"];

                while ((data = QTablefile.ReadLine()) != null)
                {
                    char[] delimiterChars = { '\x09' };
                    string text = data;
                    string[] parseinfo = text.Split(delimiterChars);

                    DataRow workRow;
                    workRow = QTable.NewRow();
                    workRow["QTSource"] = parseinfo[0];
                    workRow["QTName"] = parseinfo[1];

                    QTable.Rows.Add(workRow);

                    //toolStripStatusLabel1.Text = "File Uploading";
                }
                QTablefile.Close();
                //toolStripStatusLabel1.Text = "File Reading Completed: " + QTable.Rows.Count + " channels found";
            }

            else
            {
                // Create the file.
                File.Create(QTablefilename);
            }
        }

        public void WriteQTableFile(string folderpath, DataTable QTable)
        {
            string QTablefilename = folderpath + @"\qtable.txt";
            try
            {
                if (File.Exists(QTablefilename))
                {
                    StreamWriter QTablefile = new StreamWriter(QTablefilename);
                    for (int i = 0; i < QTable.Rows.Count; i++)
                    {
                        QTablefile.WriteLine(QTable.Rows[i]["QTSource"] + "\x09" +
                                             QTable.Rows[i]["QTName"]);
                        //toolStripStatusLabel1.Text = "File Saving:" + QTable.Rows.Count + " channels saved";
                    }

                    QTablefile.Close();
                    //toolStripStatusLabel1.Text = "File Save Completed: " + QTable.Rows.Count + " channels saved";
                }
                else
                {
                    // Create the file.
                    File.Create(QTablefilename);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Cannot save Q Table", "My Application", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }

        }

    }

}