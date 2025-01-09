using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public abstract class UrgDevice : MonoBehaviour
{
    protected readonly SCIP.VersionInfo version = new SCIP.VersionInfo();
    public SCIP.VersionInfo Version { get { return version; } }

    protected readonly SCIP.ParameterInfo parameters = new SCIP.ParameterInfo();
    public SCIP.ParameterInfo Parameters { get { return parameters; } }

    protected readonly SCIP.StatusInfo status = new SCIP.StatusInfo();
    public SCIP.StatusInfo Status { get { return status; } }

    public bool HasMeasurementData { get; private set; }

    protected readonly List<int> distances = new List<int>();
    public List<int> Distances { get { return distances; } }

    protected readonly List<int> strengths = new List<int>();
    public List<int> Strengths { get { return strengths; } }

    public int TimeStamp { get; private set; }

    public bool IsLaserOn { get; private set; }

    protected Stream stream;

    protected enum EventType
    {
        Connect,
        Disconnect,
        GetVersion,
        GetParameters,
        GetStatus,
        ResetParameters,
        ReceiveData
    }

    protected Queue<EventType> eventQueue = new Queue<EventType>();

    public event Action OnConnect;
    public event Action OnDisconnect;
    public event Action OnGetVersion;
    public event Action OnGetParameters;
    public event Action OnGetStatus;
    public event Action OnResetParameters;
    public event Action OnReceiveData;

    public abstract bool Connected { get; }

    public virtual void Connect()
    {
        IsLaserOn = false;
        TimeStamp = 0;
    }

    public virtual void Disconnect()
    {
        HasMeasurementData = false;
        distances.Clear();
        strengths.Clear();
        IsLaserOn = false;
    }

    private void Update()
    {
        if (eventQueue.Count > 0)
        {
            EventType actionEvent = eventQueue.Dequeue();
            switch (actionEvent)
            {
                case EventType.Connect:
                    if (OnConnect != null) OnConnect.Invoke();
                    break;
                case EventType.Disconnect:
                    if (OnDisconnect != null) OnDisconnect.Invoke();
                    break;
                case EventType.GetVersion:
                    if (OnGetVersion != null) OnGetVersion.Invoke();
                    break;
                case EventType.GetParameters:
                    if (OnGetParameters != null) OnGetParameters.Invoke();
                    break;
                case EventType.GetStatus:
                    if (OnGetStatus != null) OnGetStatus.Invoke();
                    break;
                case EventType.ResetParameters:
                    if (OnResetParameters != null) OnResetParameters.Invoke();
                    break;
                case EventType.ReceiveData:
                    if (OnReceiveData != null) OnReceiveData.Invoke();
                    break;
            }
        }
    }

    /// <summary>
    /// Read command and data from connected device
    /// </summary>
    /// <returns>command and data</returns>
    protected string[] ReadLines()
    {
        if (stream != null && stream.CanRead)
        {
            StreamReader reader = new StreamReader(stream, Encoding.ASCII);
            try
            {
                List<string> result = new List<string>();
                string line;
                while (true)
                {
                    line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        break;
                    }
                    result.Add(line);
                }
                return result.ToArray();
            }
            catch (Exception)
            {
            }
        }
        return null;
    }

    /// <summary>
    /// Write command to connected device
    /// </summary>
    /// <param name="command">SCIP command</param>
    protected void WriteLine(string command)
    {
        if (stream != null && stream.CanWrite)
        {
            StreamWriter writer = new StreamWriter(stream, Encoding.ASCII);
            try
            {
                writer.WriteLine(command);
                writer.Flush();
            }
            catch (Exception)
            {
            }
        }
    }

    /// <summary>
    /// Switch protocol SCIP 1.x to SCIP 2.x (only for classic-URG)
    /// </summary>
    public void SwitchToSCIP2()
    {
        WriteLine(SCIP.MODE_SCIP2);
    }

    /// <summary>
    /// Request version information
    /// </summary>
    public void RequestVersion()
    {
        WriteLine(SCIP.CMD_VV);
    }

    /// <summary>
    /// Request parameter information
    /// </summary>
    public void RequestParameters()
    {
        WriteLine(SCIP.CMD_PP);
    }

    /// <summary>
    /// Request status information
    /// </summary>
    public void RequestStatus()
    {
        WriteLine(SCIP.CMD_II);
    }

    /// <summary>
    /// Turn on laser, measurement begins
    /// </summary>
    public void BeginMeasurement()
    {
        WriteLine(SCIP.CMD_BM);
    }

    /// <summary>
    /// Turn off laser, measurement suspends
    /// </summary>
    public void SuspendMeasurement()
    {
        WriteLine(SCIP.CMD_QT);
    }

    /// <summary>
    /// Resets parameters, measurement suspends (rotation speed of the motor and baudrate will be reset)
    /// </summary>
    public void ResetParameters()
    {
        WriteLine(SCIP.CMD_RS);
    }

    /// <summary>
    /// Change the baudrate when connected with RS232C
    /// </summary>
    /// <param name="rate">(19200, 38400, 57600, 115200, 250000, 500000, 750000)</param>
    public void ChangeBoudRate(int rate)
    {
        WriteLine(string.Format("{0}{1:D6}", SCIP.CMD_SS, rate));
    }

    /// <summary>
    /// Change the rotation speed of the motor
    /// </summary>
    /// <param name="speed">(0 = Default speed, 1 ~ 10 = Speed of levels, 99 = Reset to initial speed)</param>
    public void ChangeMotorSpeed(int speed)
    {
        WriteLine(string.Format("{0}{1:D2}", SCIP.CMD_CR, speed));
    }

    /// <summary>
    /// Change timestamp mode
    /// </summary>
    /// <param name="mode">(0 = Adjust mode on, 1 = Time request, 2 = Adjust mode off)</param>
    public void ChangeTimestampMode(int mode)
    {
        WriteLine(string.Format("{0}{1:D1}", SCIP.CMD_TM, mode));
    }

    /// <summary>
    /// Change sensitivity mode
    /// </summary>
    /// <param name="mode">(0 = Normal mode, 1 = High sensitivity mode)</param>
    public void ChangeSensivityMode(int mode)
    {
        WriteLine(string.Format("{0}{1:D1}", SCIP.CMD_HS, mode));
    }

    /// <summary>
    /// Request measurement data one at a time
    /// </summary>
    /// <param name="data_type">measurement data type (0 = short distance, 1 = distance, 2 = distance and strength)</param>
    /// <param name="start">measurement start step</param>
    /// <param name="end">measurement end step</param>
    /// <param name="grouping">grouping step number</param>
    public void RequestMeasurementData(int data_type, int start, int end, int grouping = 1)
    {
        if (data_type == 1)
        {
            WriteLine(string.Format("{0}{1:D4}{2:D4}{3:D2}", SCIP.CMD_GD, start, end, grouping));
        }
        else if (data_type == 2)
        {
            WriteLine(string.Format("{0}{1:D4}{2:D4}{3:D2}", SCIP.CMD_GE, start, end, grouping));
        }
        else
        {
            WriteLine(string.Format("{0}{1:D4}{2:D4}{3:D2}", SCIP.CMD_GS, start, end, grouping));
        }
    }

    /// <summary>
    /// Request measurement data continuously (measurement begins)
    /// </summary>
    /// <param name="data_type">measurement data type (0 = short distance, 1 = distance, 2 = distance and strength)</param>
    /// <param name="start">measurement start step</param>
    /// <param name="end">measurement end step</param>
    /// <param name="grouping">grouping step number</param>
    /// <param name="skips">skip scan interval</param>
    /// <param name="scans">number of scans (0 = Indefinite)</param>
    public void RequestContinuousMeasurementData(int data_type, int start, int end, int grouping = 1, int skips = 0, int scans = 0)
    {
        if (data_type == 1)
        {
            WriteLine(string.Format("{0}{1:D4}{2:D4}{3:D2}{4:D1}{5:D2}", SCIP.CMD_MD, start, end, grouping, skips, scans));
        }
        else if (data_type == 2)
        {
            WriteLine(string.Format("{0}{1:D4}{2:D4}{3:D2}{4:D1}{5:D2}", SCIP.CMD_ME, start, end, grouping, skips, scans));
        }
        else
        {
            WriteLine(string.Format("{0}{1:D4}{2:D4}{3:D2}{4:D1}{5:D2}", SCIP.CMD_MS, start, end, grouping, skips, scans));
        }
    }

    /// <summary>
    /// Process read from connected device (must be called in worker thread)
    /// </summary>
    public void ProcessRead()
    {
        string[] commands = ReadLines();
        if (commands == null || commands.Length == 0)
        {
            //Debug.Log("URG Device : No data received");
            return;
        }
        if (commands[0].StartsWith(SCIP.CMD_BM))
        {
            IsLaserOn = true;
        }
        else if (commands[0].StartsWith(SCIP.CMD_QT))
        {
            IsLaserOn = false;
        }
        else if (commands[0].StartsWith(SCIP.CMD_RS))
        {
            IsLaserOn = false;
            eventQueue.Enqueue(EventType.ResetParameters);
        }
        else if (commands[0].StartsWith(SCIP.CMD_VV))
        {
            if (commands.Length > 2 && commands[1].StartsWith(SCIP.RES_00))
            {
                version.DecodeFrom(commands, 2);
                eventQueue.Enqueue(EventType.GetVersion);
            }
        }
        else if (commands[0].StartsWith(SCIP.CMD_PP))
        {
            if (commands.Length > 2 && commands[1].StartsWith(SCIP.RES_00))
            {
                parameters.DecodeFrom(commands, 2);
                eventQueue.Enqueue(EventType.GetParameters);
            }
        }
        else if (commands[0].StartsWith(SCIP.CMD_II))
        {
            if (commands.Length > 2 && commands[1].StartsWith(SCIP.RES_00))
            {
                status.DecodeFrom(commands, 2);
                eventQueue.Enqueue(EventType.GetStatus);
            }
        }
        else if (commands[0].StartsWith(SCIP.CMD_GS))
        {
            HasMeasurementData = false;
            distances.Clear();
            strengths.Clear();
            if (commands.Length > 2 && commands[1].StartsWith(SCIP.RES_00))
            {
                TimeStamp = SCIP.Decode(commands[2], 4);
                SCIP.Decode(commands, 3, 2, distances);
                HasMeasurementData = true;
                eventQueue.Enqueue(EventType.ReceiveData);
            }
        }
        else if (commands[0].StartsWith(SCIP.CMD_MS))
        {
            IsLaserOn = true;
            HasMeasurementData = false;
            distances.Clear();
            strengths.Clear();
            if (commands.Length > 2 && commands[1].StartsWith(SCIP.RES_99))
            {
                TimeStamp = SCIP.Decode(commands[2], 4);
                SCIP.Decode(commands, 3, 2, distances);
                HasMeasurementData = true;
                eventQueue.Enqueue(EventType.ReceiveData);
            }
        }
        else if (commands[0].StartsWith(SCIP.CMD_GD))
        {
            HasMeasurementData = false;
            distances.Clear();
            strengths.Clear();
            if (commands.Length > 2 && commands[1].StartsWith(SCIP.RES_00))
            {
                TimeStamp = SCIP.Decode(commands[2], 4);
                SCIP.Decode(commands, 3, 3, distances);
                HasMeasurementData = true;
                eventQueue.Enqueue(EventType.ReceiveData);
            }
        }
        else if (commands[0].StartsWith(SCIP.CMD_MD))
        {
            IsLaserOn = true;
            HasMeasurementData = false;
            distances.Clear();
            strengths.Clear();
            if (commands.Length > 2 && commands[1].StartsWith(SCIP.RES_99))
            {
                TimeStamp = SCIP.Decode(commands[2], 4);
                SCIP.Decode(commands, 3, 3, distances);
                HasMeasurementData = true;
                eventQueue.Enqueue(EventType.ReceiveData);
            }
        }
        else if (commands[0].StartsWith(SCIP.CMD_GE))
        {
            HasMeasurementData = false;
            distances.Clear();
            strengths.Clear();
            if (commands.Length > 2 && commands[1].StartsWith(SCIP.RES_00))
            {
                TimeStamp = SCIP.Decode(commands[2], 4);
                SCIP.Decode(commands, 3, 3, distances, strengths);
                HasMeasurementData = true;
                eventQueue.Enqueue(EventType.ReceiveData);
            }
        }
        else if (commands[0].StartsWith(SCIP.CMD_ME))
        {
            IsLaserOn = true;
            HasMeasurementData = false;
            distances.Clear();
            strengths.Clear();
            if (commands.Length > 2 && commands[1].StartsWith(SCIP.RES_99))
            {
                TimeStamp = SCIP.Decode(commands[2], 4);
                SCIP.Decode(commands, 3, 3, distances, strengths);
                HasMeasurementData = true;
                eventQueue.Enqueue(EventType.ReceiveData);
            }
        }
    }
}
