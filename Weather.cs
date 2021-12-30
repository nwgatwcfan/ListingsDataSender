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

namespace Weather
{
    public class FormatWthr
    {

        public List<string> parsed_weather_lines = new List<string>();

        public string PrevPressure;
        public string PressureChg;
        public string CurWind;
        public string CurWindDirection;
        public string CurWindGust;
        public string CurVisibility;
        public string CurSky;
        public string CurTemp;
        public string CurDewPoint;
        public string CurHumidity;
        public string CurPressure;
        public string CurWeather;
        public string CurWindChill;
        public string CurHeatIndex;
        public string IconSelection;

        

        public void ParseWeatherData(List<string> parsed_weather_lines)
        {
            char[] delimiterChars = { ':' };
            char[] spacedelimiter = { ' ' };
            char[] charsToTrim = { '%' };
            string line;

            CurWeather = "";

            foreach (string linedata in parsed_weather_lines)
            {
                
                string[] parseinfo = linedata.Split(delimiterChars);
                switch (parseinfo[0])
                {
                    case "Wind":
                        line = parseinfo[1];
                        string[] parsewindline = line.Split(spacedelimiter);
                        if (parsewindline[1] == "Calm")
                        {
                            CurWind = "0";
                            CurWindDirection = "";
                            CurWindGust = "0";
                        }
                        else
                        {
                            if (Array.Exists(parsewindline, element => element == "gusting"))
                            {
                                CurWind = parsewindline[7];
                                CurWindDirection = parsewindline[3];
                                CurWindGust = parsewindline[13];
                            }
                            else
                            {
                                if (Array.Exists(parsewindline, element => element == "(direction variable)"))
                                {
                                    CurWind = parsewindline[7];
                                    CurWindDirection = "Variable";
                                    CurWindGust = "0";
                                }
                                else
                                {
                                    CurWind = parsewindline[7];
                                    CurWindDirection = parsewindline[3];
                                    CurWindGust = "0";
                                }
                            }
                        }
                        break;
                    case "Visibility":
                        line = parseinfo[1];
                        string[] parsevisibilityline = line.Split(spacedelimiter);
                        if (parsevisibilityline[1] == "less")
                        {
                            CurVisibility = parsevisibilityline[1] + " " + parsevisibilityline[2] + " " + parsevisibilityline[3];
                        }
                        else
                        {
                            CurVisibility = parsevisibilityline[1];
                        }
                        break;
                    case "Sky conditions":
                        CurSky = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(parseinfo[1]);
                        break;
                    case "Weather":
                        CurWeather = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(parseinfo[1]);
                        break;
                    case "Temperature":
                        line = parseinfo[1];
                        string[] parsetempline = line.Split(spacedelimiter);
                        CurTemp = parsetempline[1];
                        if (Convert.ToDecimal(CurTemp) >= 70)
                        {
                            Calc c = new Calc();
                            CurHeatIndex = c.CalcHeatIndex(Convert.ToDouble(CurTemp), Convert.ToDouble(CurHumidity)).ToString();
                            CurWindChill = "";
                        }
                        else
                        {
                            if ((Convert.ToDouble(CurTemp) <= 50) && (Convert.ToDouble(CurWind) > 3))
                            {
                                Calc c = new Calc();
                                CurWindChill = c.CalcWindChill(Convert.ToDouble(CurTemp), Convert.ToDouble(CurWind)).ToString();
                                CurHeatIndex = "";
                            }
                            else
                            {
                                CurHeatIndex = "";
                                CurWindChill = "";
                            }
                        }
                        break;
                    case "Dew Point":
                        line = parseinfo[1];
                        string[] parseDewptline = line.Split(spacedelimiter);
                        CurDewPoint = parseDewptline[1];
                        break;
                    case "Relative Humidity":
                        CurHumidity = parseinfo[1].Trim(charsToTrim);
                        break;
                    case "Pressure (altimeter)":
                        PrevPressure = CurPressure;
                        line = parseinfo[1];
                        string[] parsePressureline = line.Split(spacedelimiter);
                        decimal cp = Convert.ToDecimal(parsePressureline[1]);
                        CurPressure = String.Format("{0:0.00}", cp);
                        if (Convert.ToDecimal(PrevPressure) < cp)
                        { PressureChg = "R"; }
                        else
                        {
                            if (Convert.ToDecimal(PrevPressure) > cp)
                            { PressureChg = "F"; }
                            else
                            { PressureChg = "S"; }
                        }
                        break;
                    default:
                        break;
                }

            }
        }
        public string SelectWeatherIcon()
            {
                string iconselected = "1";
                string sky = CurSky;
                string weather = CurWeather;

                if (sky == " Partly Cloudy" && weather == "")
                {
                    iconselected = "2";
                    return iconselected;
                }
                else if (sky == " Overcast" && weather == "")
                {
                    iconselected = "4";
                    return iconselected;
                }
                else if (sky == " Clear" && weather == "")
                {
                    iconselected = "1";
                    return iconselected;
                }
                else if (sky == " Mostly Cloudy" && weather == "")
                {
                    iconselected = "4";
                    return iconselected;
                }
                else if (sky == "" && weather == " Fog")
                {
                    iconselected = "3";
                    return iconselected;
                }
                else
                {
                    iconselected = "1";
                    return iconselected;
                }
            }
        
        public string FormatWeatherData(string selection)
        {
            string displaystring = "";

            switch (selection)
            {
                case "sky":
                    displaystring = "\x18" + CurSky;
                    return displaystring;

                case "weather":
                    displaystring = "\x18" + CurWeather;
                    return displaystring;

                case "wind":
                    if (CurWind == "0")
                    {
                        displaystring = "\x18" + "Wind Calm";
                        return displaystring;
                    }
                    else if (CurWindGust != "0")
                    {
                        displaystring = "\x18" + "Wind " + CurWindDirection + " at " + CurWind + " MPH  Gusts to " + CurWindGust + " MPH";
                        return displaystring;
                    }
                    else
                    {
                        displaystring = "\x18" + "Wind " + CurWindDirection + " at " + CurWind + " MPH";
                        return displaystring;
                    }
                case "temp":
                    if (CurHeatIndex != "")
                    {
                        displaystring = "\x18" + "Temp " + Math.Floor(Convert.ToDouble(CurTemp)) + "^F  Heat Index " + CurHeatIndex + "^F";
                        return displaystring;
                    }
                    else if (CurWindChill != "")
                    {
                        displaystring = "\x18" + "Temp " + Math.Floor(Convert.ToDouble(CurTemp)) + "^F  Wind Chill " + CurWindChill + "^F";
                        return displaystring;
                    }
                    else
                    {
                        displaystring = "\x18" + "Temp " + Math.Floor(Convert.ToDouble(CurTemp)) + "^F";
                        return displaystring;
                    }

                case "humidity":
                    displaystring = "\x18" + "Humidity " + Convert.ToDouble(CurHumidity) + "%";
                    return displaystring;

                case "dewpoint":
                    displaystring = "\x18" + "DewPoint " + Math.Floor(Convert.ToDouble(CurDewPoint)) + "^F";
                    return displaystring;

                case "visibility":
                    displaystring = "\x18" + "Visibility " + CurVisibility + " mi.";
                    return displaystring;

                case "pressure":
                    displaystring = "\x18" + "Pressure " + CurPressure + " in. " + PressureChg;
                    return displaystring;
                default:
                    return displaystring;
            }
        }
    }
}