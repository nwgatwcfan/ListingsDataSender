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
using FileIO;
using SerialIO;
using WindowsFormsApplication1;

namespace ListingsBuilder
{

    public class Zap2ItListings
    {
        public void ParseXMLforListings(string folderpath, DataTable listings)
        {


            listings.Clear();

            XDocument listingsdata = XDocument.Load(folderpath + @"\data.xml");
            foreach (XElement prog in listingsdata.Descendants("programme"))
            {
                XMLParseData xml = new XMLParseData();
                Calc calc = new Calc();

                string[] chanid = prog.Attribute("channel").Value.Split('.');

                string chid = chanid[1];

                int starthr = Convert.ToInt16(prog.Attribute("start").Value.Substring(8, 2));
                int startmin = Convert.ToInt16(prog.Attribute("start").Value.Substring(10, 2));
                string proglistdate = prog.Attribute("start").Value.Substring(0, 8);
                int year = Convert.ToInt16(proglistdate.Substring(0, 4));
                int month = Convert.ToInt16(proglistdate.Substring(4, 2));
                int day = Convert.ToInt16(proglistdate.Substring(6, 2));
                int length = Convert.ToInt16(prog.Element("length").Value);
                string progcategory = xml.ChildNodeValue(prog, "category");
                string progsubtitle = xml.ChildNodeValue(prog, "sub-title");
                string progdesc = xml.ChildNodeValue(prog, "desc");

                string progstart = xml.FormatStartTime(starthr, startmin);
                string progjdate = calc.CalcOrdinalDate(year, month, day, starthr, startmin,
                                                        WindowsFormsApplication1.Properties.Settings.Default.TimeZoneOffset).ToString();
                string progts = calc.CalcTimeSlot(starthr, startmin, 
                                                  WindowsFormsApplication1.Properties.Settings.Default.TimeZoneOffset).ToString();

                string proglength = xml.FormatLength(length);
                string prograting = xml.FormatRating(xml.ChildNodeValue(prog, "rating"));
                string progcc = xml.FormatCC(xml.ChildNodeAttrValue(prog, "subtitles", "type"));
                string progdate = xml.ChildNodeValue(prog, "date");

                string progtitle = xml.FormatTitle(progcategory, progstart, prograting, prog.Element("title").Value, progdate,
                          progsubtitle, progdesc, progcc, proglength);
              
 

                DataRow workRow;
                workRow = listings.NewRow();
                workRow["id"] = chid;
                workRow["jdate"] = progjdate;
                workRow["timeslot"] = progts;
                workRow["title"] = progtitle;
                workRow["category"] = progcategory;
                workRow["subtitle"] = progsubtitle;
                workRow["desc"] = progdesc;
                
                listings.Rows.Add(workRow);

            }

        }
    }
    

}
