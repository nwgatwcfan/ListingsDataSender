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

namespace LineupBuilder
{
    public class Zap2ItLineup
    {
        public void ParseXMLforLineup(string folderpath, DataTable lineup)
        {
            lineup.Clear();
            XDocument lineupdata = XDocument.Load(folderpath + @"\data.xml");
            foreach (XElement chan in lineupdata.Descendants("channel"))
            {
                string channumber = ""; //initialize channel number in case it does not exist
                string call_letters = ""; //initialize channel call letters in case they do not exist
                int index = 1; //initialize the index value for the array below

                string[] chid = chan.Attribute("id").Value.Split('.');

                string source = chid[1]; // first element is a source id value - up to 6 chars and will always exist
                foreach (XElement chanid in chan.Descendants())
                {  //loops through all of the values found in each source
                    switch (index)
                    {
                        case 2:    // 2nd element is the channel number on the lineup
                            channumber = chanid.Value;
                            break;
                        case 3:    // 3rd element is the call letters for the channel
                            call_letters = chanid.Value;
                            break;
                        default:  // default condition if index does not equal 2 or 3
                            break;
                    }
                    index++;
                }

                DataRow workRow;
                workRow = lineup.NewRow();
                workRow["id"] = source;
                workRow["channum"] = channumber;
                workRow["call_letters"] = call_letters;
                workRow["RedHiLt"] = "0";
                workRow["SBS"] = "0";
                workRow["PTagDisable"] = "0";
                workRow["PPVSrc"] = "0";
                workRow["DittoEnable"] = "0";
                workRow["LtBlueHiLt"] = "0";
                workRow["StereoSrc"] = "1";
                workRow["Daypt1"] = 255;
                workRow["Daypt2"] = 255;
                workRow["Daypt3"] = 255;
                workRow["Daypt4"] = 255;
                workRow["Daypt5"] = 255;
                workRow["Daypt6"] = 255;
                lineup.Rows.Add(workRow);
            }

        }

        







    }
}
