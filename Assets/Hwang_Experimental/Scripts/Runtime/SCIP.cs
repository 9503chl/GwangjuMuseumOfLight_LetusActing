using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// SCIP is a protocol used to communicate with URG (LiDAR) sensors.
/// Please refer https://sourceforge.net/p/urgnetwork/wiki/scip_en/
/// </summary>
public class SCIP
{
    /// <summary>
    /// Switch protocol SCIP 1.x to SCIP 2.x  (only for classic-URG)
    /// </summary>
    public const string MODE_SCIP2 = "SCIP2.0";
    /// <summary>
    /// Acquisition of version information
    /// </summary>
    public const string CMD_VV = "VV";
    /// <summary>
    /// Acquisition of parameter information
    /// </summary>
    public const string CMD_PP = "PP";
    /// <summary>
    /// Acquisition of status information
    /// </summary>
    public const string CMD_II = "II";
    /// <summary>
    /// Turn on laser, measurement begins
    /// </summary>
    public const string CMD_BM = "BM";
    /// <summary>
    /// Turn off laser, measurement suspends
    /// </summary>
    public const string CMD_QT = "QT";
    /// <summary>
    /// Reset parameters, measurement suspends (rotation speed of the motor and baudrate will be reset)
    /// </summary>
    public const string CMD_RS = "RS";
    /// <summary>
    /// Change the baudrate when connected with RS232C ("019200", "038400", "057600", "115200", "250000", "500000", "750000")
    /// </summary>
    public const string CMD_SS = "SS";
    /// <summary>
    /// Change the rotation speed of the motor ("00" = Default speed, "01" ~ "10" = Speed of levels, "99" = Reset to initial speed)
    /// </summary>
    public const string CMD_CR = "CR";
    /// <summary>
    /// Change timestamp mode ("0" = Adjust mode on, "1" = Time request, "2" = Adjust mode off)
    /// </summary>
    public const string CMD_TM = "TM";
    /// <summary>
    /// Change sensitivity mode ("0" = Normal mode, "1" = High sensitivity mode)
    /// </summary>
    public const string CMD_HS = "HS";
    /// <summary>
    /// Acquisition of short distance data one at a time - Distance data is represented using 2 bytes (maximum 4095)
    /// </summary>
    public const string CMD_GS = "GS";
    /// <summary>
    /// Acquisition of short distance data continuously (measurement begins) - Distance data is represented using 2 bytes (maximum 4095)
    /// </summary>
    public const string CMD_MS = "MS";
    /// <summary>
    /// Acquisition of distance data one at a time
    /// </summary>
    public const string CMD_GD = "GD";
    /// <summary>
    /// Acquisition of distance data continuously (measurement begins)
    /// </summary>
    public const string CMD_MD = "MD";
    /// <summary>
    /// Acquisition of distance and strength data one at a time
    /// </summary>
    public const string CMD_GE = "GE";
    /// <summary>
    /// Acquisition of distance and strength data continuously (measurement begins)
    /// </summary>
    public const string CMD_ME = "ME";
    /// <summary>
    /// Status success
    /// </summary>
    public const string RES_00 = "00";
    /// <summary>
    /// Status get data success
    /// </summary>
    public const string RES_99 = "99";

    /// <summary>
    /// Version information
    /// </summary>
    public class VersionInfo
    {
        public string Vendor { get; private set; }
        public string Product { get; private set; }
        public string Firmware { get; private set; }
        public string Protocol { get; private set; }
        public string SerialNumber { get; private set; }

        internal void DecodeFrom(string[] lines, int start_line)
        {
            Vendor = FindInfoStr(lines, start_line, "VEND:");
            Product = FindInfoStr(lines, start_line, "PROD:");
            Firmware = FindInfoStr(lines, start_line, "FIRM:");
            Protocol = FindInfoStr(lines, start_line, "PROT:");
            SerialNumber = FindInfoStr(lines, start_line, "SERI:");
        }

        public override string ToString()
        {
            return string.Format("Vendor = {0}\nProduct = {1}\nFirmware = {2}\nProtocol = {3}\nSerialNumber = {4}",
                Vendor, Product, Firmware, Protocol, SerialNumber);
        }
    }

    /// <summary>
    /// Parameter information
    /// </summary>
    public class ParameterInfo
    {
        public string Model { get; private set; }
        public int MinDistance { get; private set; }
        public int MaxDistance { get; private set; }
        public int AreaTotal { get; private set; }
        public int AreaMin { get; private set; }
        public int AreaMax { get; private set; }
        public int AreaFront { get; private set; }
        public int MotorSpeed { get; private set; }

        internal void DecodeFrom(string[] lines, int start_line)
        {
            Model = FindInfoStr(lines, start_line, "MODL:");
            MinDistance = FindInfoInt(lines, start_line, "DMIN:");
            MaxDistance = FindInfoInt(lines, start_line, "DMAX:");
            AreaTotal = FindInfoInt(lines, start_line, "ARES:");
            AreaMin = FindInfoInt(lines, start_line, "AMIN:");
            AreaMax = FindInfoInt(lines, start_line, "AMAX:");
            AreaFront = FindInfoInt(lines, start_line, "AFRT:");
            MotorSpeed = FindInfoInt(lines, start_line, "SCAN:");
        }

        public override string ToString()
        {
            return string.Format("Model = {0}\nMinDistance = {1}\nMaxDistance = {2}\nAreaTotal = {3}\nAreaMin = {4}\nAreaMax = {5}\nAreaFront = {6}\nMotorSpeed = {7}",
                Model, MinDistance, MaxDistance, AreaTotal, AreaMin, AreaMax, AreaFront, MotorSpeed);
        }
    }

    /// <summary>
    /// Status information
    /// </summary>
    [Serializable]
    public class StatusInfo
    {
        public string Model { get; private set; }
        public string LaserStatus { get; private set; }
        public int MotorDesiredSpeed { get; private set; }
        public string MeasurementMode { get; private set; }
        public string CommunicationType { get; private set; }
        public int TimeStamp { get; private set; }
        public string SensorStatus { get; private set; }

        internal void DecodeFrom(string[] lines, int start_line)
        {
            Model = FindInfoStr(lines, start_line, "MODL:");
            LaserStatus = FindInfoStr(lines, start_line, "LASR:");
            MotorDesiredSpeed = FindInfoInt(lines, start_line, "SCSP:");
            MeasurementMode = FindInfoStr(lines, start_line, "MESM:");
            CommunicationType = FindInfoStr(lines, start_line, "SBPS:");
            TimeStamp = Decode(FindInfoStr(lines, start_line, "TIME:"), 4);
            SensorStatus = FindInfoStr(lines, start_line, "STAT:");
        }

        public override string ToString()
        {
            return string.Format("Model = {0}\nLaserStatus = {1}\nMotorDesiredSpeed = {2}\nMeasurementMode = {3}\nCommunicationType = {4}\nTimeStamp = {5}\nSensorStatus = {6}",
                Model, LaserStatus, MotorDesiredSpeed, MeasurementMode, CommunicationType, TimeStamp, SensorStatus);
        }
    }

    /// <summary>
    /// Find information string
    /// </summary>
    /// <param name="lines">lines of string</param>
    /// <param name="start_line">start line index</param>
    /// <param name="header">header</param>
    /// <returns>information string</returns>
    public static string FindInfoStr(string[] lines, int start_line, string header)
    {
        for (int i = start_line; i < lines.Length; i++)
        {
            string line = lines[i];
            if (line.StartsWith(header))
            {
                int pos = line.IndexOf(';');
                if (pos != -1)
                {
                    return line.Substring(header.Length, pos - header.Length);
                }
            }
        }
        return string.Empty;
    }

    /// <summary>
    /// Find information integer
    /// </summary>
    /// <param name="lines">lines of string</param>
    /// <param name="start_line">start line index</param>
    /// <param name="header">header</param>
    /// <returns>information integer</returns>
    public static int FindInfoInt(string[] lines, int start_line, string header)
    {
        try
        {
            return Convert.ToInt32(FindInfoStr(lines, start_line, header));
        }
        catch (Exception)
        {
            return 0;
        }
    }

    /// <summary>
    /// Decode integer value
    /// </summary>
    /// <param name="data">encoded string</param>
    /// <param name="block_size">encoded block size</param>
    /// <param name="offset">start position</param>
    /// <returns>decoded integer value</returns>
    public static int Decode(string data, int block_size, int offset = 0)
    {
        int result = 0;
        if (data.Length >= offset + block_size)
        {
            for (int i = 0; i < block_size; i++)
            {
                result <<= 6;
                result |= (byte)data[offset + i] - 0x30;
            }
        }
        return result;
    }

    /// <summary>
    /// Decode distance data
    /// </summary>
    /// <param name="lines">array of string</param>
    /// <param name="start_line">start line index</param>
    /// <param name="block_size">encoded block size</param>
    /// <param name="distances">distance data</param>
    public static void Decode(string[] lines, int start_line, int block_size, List<int> distances)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = start_line; i < lines.Length; i++)
        {
            sb.Append(lines[i].Remove(lines[i].Length - 1));
        }
        string data = sb.ToString();
        for (int offset = 0; offset <= data.Length - block_size; offset += block_size)
        {
            distances.Add(Decode(data, block_size, offset));
        }
    }

    /// <summary>
    /// Decode distance and strength data
    /// </summary>
    /// <param name="lines">array of string</param>
    /// <param name="start_line">start line index</param>
    /// <param name="block_size">encoded block size</param>
    /// <param name="distances">distance data</param>
    /// <param name="strengths">strength data</param>
    public static void Decode(string[] lines, int start_line, int block_size, List<int> distances, List<int> strengths)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = start_line; i < lines.Length; i++)
        {
            sb.Append(lines[i].Remove(lines[i].Length - 1));
        }
        string data = sb.ToString();
        for (int offset = 0; offset <= data.Length - block_size * 2; offset += block_size * 2)
        {
            distances.Add(Decode(data, block_size, offset));
            strengths.Add(Decode(data, block_size, offset + block_size));
        }
    }
}
