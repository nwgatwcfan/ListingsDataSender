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
using WindowsFormsApplication1;
using System.IO.Ports;


namespace SerialIO

{
    public class serial
    {
       
        public void InitializeSerialPort(SerialPort port, string portselection) 
        {
            try
            {
                port.PortName = portselection;
                //port.DiscardInBuffer();
                port.Close();
                port.Open();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }
        public void DiscardInBuffer()
        {
            Form1 f = new Form1();
            try
            {
                f.serialPort1.DiscardInBuffer();
            }
            catch (Exception ex)
            {
                //throw new InvalidOperationException(CommPhrases.ClearDataStreamError + ": " + ex.Message, ex);
            }
        }
        char[] Get_Mode_Select(string selection)
        {
            char[] modeselected;
            switch (selection)
            {
                case "box off":
                    modeselected = new char[] { '\u00bb', '\u00bb' };
                    return modeselected;
                case "box on":
                    modeselected = new char[] { 'A' };
                    return modeselected;
                case "listing":
                    modeselected = new char[] { 'P' };
                    return modeselected;
                case "new listing":
                    modeselected = new char[] { 'p' };
                    return modeselected;
                case "clock":
                    modeselected = new char[] { 'K' };
                    return modeselected;
                case "DST":
                    modeselected = new char[] { 'g' };
                    return modeselected;
                case "channel":
                    modeselected = new char[] { 'C' };
                    return modeselected;
                case "chan attr":
                    modeselected = new char[] { 'c' };
                    return modeselected;
                case "reset":
                    modeselected = new char[] { 'R' };
                    return modeselected;
                case "weather id":
                    modeselected = new char[] { 'I' };
                    return modeselected;
                case "weather data":
                    modeselected = new char[] { 'W' };
                    return modeselected;
                case "version":
                    modeselected = new char[] { 'V' };
                    return modeselected;
                case "qtable":
                    modeselected = new char[] { 'Q' };
                    return modeselected;
                case "top line":
                    modeselected = new char[] { 'T' };
                    return modeselected;
                case "bottom line":
                    modeselected = new char[] { 'B' };
                    return modeselected;
                case "config":
                    modeselected = new char[] { 'F' };
                    return modeselected;
                case "new config":
                    modeselected = new char[] { 'f' };
                    return modeselected;
                case "save data":
                    modeselected = new char[] { '%' };
                    return modeselected;
                case "utility":
                    modeselected = new char[] { 'x' };
                    return modeselected;
                default:
                    modeselected = new char[] { '*' };
                    return modeselected;
            }
        }

        public void TransmitMessage(SerialPort port, string folderpath, string selectedmode, char[] messagebody)
        {        
            byte[] header = new byte[] { (byte)'\u0055', (byte)'\u00AA' };
            byte[] endcommand = new byte[] { (byte)'\u0000' };

            ConvertArray array = new ConvertArray();

            byte[] mode = array.CharArrayToByteArrayFunction(Get_Mode_Select(selectedmode));
            byte[] body = array.CharArrayToByteArrayFunction(messagebody);

            List<byte> list = new List<byte>();
            list.AddRange(header);
            list.AddRange(mode);
            list.AddRange(body);
            list.AddRange(endcommand);
            byte[] messagebuild = list.ToArray();
            byte[] checksum = new byte[] { (byte)array.XORByteArrayFunction(messagebuild) };
            List<byte> messagetosend = new List<byte>();
            messagetosend.AddRange(messagebuild);
            messagetosend.AddRange(checksum);
            byte[] messagetotransmit = messagetosend.ToArray();

            try
            {
                if (selectedmode == "box off")
                {
                    port.Write(messagetotransmit, 0, messagetotransmit.Length);
                    SerialFile ser = new SerialFile();
                    ser.WriteSerialLogFile(messagetotransmit, folderpath);
                    port.Close();
                }
                else
                {
                    if (port.IsOpen)
                    {
                        port.Write(messagetotransmit, 0, messagetotransmit.Length);
                        SerialFile ser = new SerialFile();
                        ser.WriteSerialLogFile(messagetotransmit, folderpath);
                    }
                    else 
                    {
                    port.Open();
                    port.Write(messagetotransmit, 0, messagetotransmit.Length);
                    SerialFile ser = new SerialFile();
                    ser.WriteSerialLogFile(messagetotransmit, folderpath);
                    }
                }
            }
            catch
            {

                MessageBox.Show("Error sending data. Check serial port setting.", "Error", MessageBoxButtons.OK);

            }
        }

        public void TransmitNewLookMessage(SerialPort port, string folderpath, string selectedmode, char[] messagebody)
        {
            char[] header = new char[] { '\u0055', '\u00AA' };
            char[] endcommand = new char[] { '\u0000' };
            
            ConvertArray array = new ConvertArray();

            byte[] hdr = array.CharArrayToByteArrayFunction(header); //constant
            byte[] endc = array.CharArrayToByteArrayFunction(endcommand); //constant
            byte[] mde = array.CharArrayToByteArrayFunction(Get_Mode_Select(selectedmode));
            byte[] body = array.CharArrayToByteArrayFunction(messagebody);

            List<byte> list = new List<byte>();
            list.AddRange(hdr);
            list.AddRange(mde);
            list.AddRange(body);
            list.AddRange(endc);
            byte[] messagebuild = list.ToArray();
            byte[] checksum = new byte[] { (byte)array.XORByteArrayFunction(messagebuild) };
            List<byte> messagetosend = new List<byte>();
            messagetosend.AddRange(messagebuild);
            messagetosend.AddRange(checksum);
            byte[] messagetotransmit = messagetosend.ToArray();

            try
            {
                if (selectedmode == "box off")
                {
                    port.Write(messagetotransmit, 0, messagetotransmit.Length);
                    SerialFile ser = new SerialFile();
                    ser.WriteSerialLogFile(messagetotransmit, folderpath);
                    port.Close();
                }
                else
                {
                    if (port.IsOpen)
                    {
                        port.Write(messagetotransmit, 0, messagetotransmit.Length);
                        SerialFile ser = new SerialFile();
                        ser.WriteSerialLogFile(messagetotransmit, folderpath);
                    }
                    else
                    {
                        port.Open();
                        port.Write(messagetotransmit, 0, messagetotransmit.Length);
                        SerialFile ser = new SerialFile();
                        ser.WriteSerialLogFile(messagetotransmit, folderpath);
                    }
                }
            }
            catch
            {

                MessageBox.Show("Error sending data. Check serial port setting.", "Error", MessageBoxButtons.OK);

            }
        }









    }

}

