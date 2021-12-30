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


namespace DataFunctions
{
    public class SportsListBuilder
    {
        public List<string> sportstitles = new List<string>();

        public void BuildSportsTitleList()
        {
            sportstitles.Add("MLB Baseball");
            sportstitles.Add("NFL Football");
            sportstitles.Add("NBA Basketball");
            sportstitles.Add("NHL Hockey");
            sportstitles.Add("MLS Soccer");
            sportstitles.Add("NWSL Soccer");
            sportstitles.Add("WNBA Basketball");
            sportstitles.Add("Tennis");
            sportstitles.Add("Baseball");
            sportstitles.Add("College Baseball");
            sportstitles.Add("College Football");
            sportstitles.Add("College Basketball");
            sportstitles.Add("College Hockey");
            sportstitles.Add("Women's College Soccer");
            sportstitles.Add("Women's College Volleyball");
            sportstitles.Add("Women's College Basketball");
            sportstitles.Add("European PGA Tour Golf");
            sportstitles.Add("Women's Golf");
            sportstitles.Add("LPGA Tour Golf");
            sportstitles.Add("PGA Tour Golf");
            sportstitles.Add("PGA Tour Champions Golf");
            sportstitles.Add("Horse Racing");
            sportstitles.Add(@"ATP/WTA Tennis");
            sportstitles.Add("FIFA World Cup 2022 Qualifying");
            sportstitles.Add("Boxing");
            sportstitles.Add("Formula 1 Racing");
            sportstitles.Add("Figure Skating");
            sportstitles.Add("IMSA Weathertech Sportscar Championship");
            sportstitles.Add("High School Basketball");
            sportstitles.Add("High School Football");
            sportstitles.Add("Center Court");
            sportstitles.Add("Fight Sports: Boxing");
            sportstitles.Add("Boat Racing");
            sportstitles.Add("Major League Fishing");
            sportstitles.Add("Lucas Oil Motorsports");
            sportstitles.Add("FIFA Eliminatorias Copa Mundial 2022");
            sportstitles.Add("Béisbol Liga Mexicana");
            sportstitles.Add("Fútbol Americano Colegial de México");
            sportstitles.Add("FIS Alpine Skiing");
            sportstitles.Add("Premier League Soccer");
        }

    }

    public class ConvertArray
    {
        public int XORByteArrayFunction(byte[] bytearray)
        {
            int sPos = 0;
            int XORvalue = 0;

            // recursively loop through each char in the character array
            while (sPos < bytearray.Length)
            {
                XORvalue ^= bytearray[sPos];
                ++sPos;
            }
            return (XORvalue);
        }

        public byte[] CharArrayToByteArrayFunction(char[] messagetosend)
        {
            // create a byte arry to hold chars...
            byte[] data = new byte[messagetosend.Length];

            // loop thru chars and convert to byte...
            for (int i = 0; i < messagetosend.Length; i++)
            {
                // convert char to byte and place in byte array...
                data[i] = Convert.ToByte(messagetosend[i]);
            }
            return (data);

        }

    }

    public class XMLParseData
    { 
        public string FormatStartTime(int hr, int min)
        {
            string starttime = "";
            int adjhr;

            if (min != 0 && min != 30)
            {
                if (hr == 0) { adjhr = 12; }
                else if (hr > 12) { adjhr = hr - 12; }
                else { adjhr = hr; }
                starttime = "(" + adjhr.ToString().PadLeft(2, ' ') + ":" + min.ToString().PadLeft(2, '0') + ")";
                return starttime;
            }
            else { return starttime; }
        }

        public string FormatLength(int lengthinminutes)
        {
            string length;
            
            int hr = lengthinminutes / 60;
            int min = lengthinminutes % 60;
            
            length = "(" + hr.ToString() + ":" + min.ToString().PadLeft(2, '0') + ")";
            return length;
        }

        public string FormatRating(string rating)
        {
            string formattedrating = "";
            if (rating != "")
            {
                formattedrating = "(" + rating + ")";
                return formattedrating;
            }
            else { return formattedrating; }
        }

        public string FormatCC(string subtitle)
        {
            string progcc = "";
            if (subtitle == "teletext")
            {
                progcc = "(CC)";
                return progcc;
            }
            else { return progcc; }
        }

        public string FormatTitle(string category, string starttime, string rating, string title, string year,
                                  string subtitle, string desc, string cc, string length)
        {
            SportsListBuilder sports = new SportsListBuilder();
            sports.BuildSportsTitleList();

            string formattedtitle;

            if (category == "Movie")
            {
                if (starttime != "")
                {
                    formattedtitle = starttime + " \"" + title + "\" " + rating + " (" + year + ") " + desc + " " + cc + " " + length;
                    return formattedtitle;
                }
                else
                {
                    formattedtitle = "\"" + title + "\" " + rating + " (" + year + ") " + desc + " " + cc + " " + length;
                    return formattedtitle;
                }
            }
            else if (sports.sportstitles.Contains(title))
            {
                if (starttime != "")
                {
                    string content = subtitle == null ? "" : subtitle;
                    if (content != "")
                    {
                        formattedtitle = starttime + " " + title + ": " + subtitle + " " + cc;
                        return formattedtitle;

                    }
                    else
                    {
                        formattedtitle = starttime + " " + title + " " + cc;
                        return formattedtitle;
                    }
                }
                else
                {
                    string content = subtitle == null ? "" : subtitle;

                    if (content != "")
                    {
                        formattedtitle = title + ": " + subtitle + " " + cc;
                        return formattedtitle;
                    }
                    else
                    {
                        formattedtitle = title + " " + cc;
                        return formattedtitle;
                    }
                }
            }
            else
            {
                if (starttime != "")
                {
                    formattedtitle = starttime + " " + title + " " + rating + " " + cc;
                    return formattedtitle;
                }
                else
                {
                    formattedtitle = title + " " + rating + " " + cc;
                    return formattedtitle;
                }
            }
        }

        public string ChildNodeValue(XElement parentNode, string ChildNode)
        {
            string value = "";
            XElement node = parentNode.Element(ChildNode);
            if (node != null)
            {
                value = parentNode.Element(ChildNode).Value;
                return value;
            }
            else
            {
                return value;
            }
        }
        public string ChildNodeAttrValue(XElement parentNode, string ChildNode, string attr)
        {
            string value = "";
            XElement node = parentNode.Element(ChildNode);
            if (node != null)
            {
                value = parentNode.Element(ChildNode).Attribute(attr).Value;
                return value;
            }
            else
            {
                return value;
            }
        }

    }


}

