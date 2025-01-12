﻿using LogViewer.Enums;
using LogViewer.Helpers;
using LogViewer.Localization;
using LogViewer.MVVM.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Xml;

namespace LogViewer
{
    /// <summary>
    /// Класс-парсер логов по UDP
    /// </summary>
    public class UDPPacketsParser : IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private UdpClient udpClient;
        private IPEndPoint remoteIpEndPoint;
        private readonly XmlParserContext xmlContext;
        private readonly Encoding encoding;

        public int Port { get; }
        public bool IsInitialized { get; private set; }
        public List<IgnoredIPAddress> IgnoredIPs { get; set; }

        public UDPPacketsParser(Receiver receiver)
        {
            this.Port = receiver.Port;
            this.encoding = Encoding.GetEncoding(receiver.Encoding);
            xmlContext = CreateContext();
            IgnoredIPs = Settings.Instance.IgnoredIPs;
        }

        public bool Init()
        {
            try
            {
                udpClient = new UdpClient(Port);
                remoteIpEndPoint = new IPEndPoint(IPAddress.Any, Port);
                IsInitialized = true;
                return IsInitialized;
            }
            catch (SocketException socketException)
            {
                logger.Warn(socketException, "An error occurred in UDPPacketsParser ctor.");
                MessageBox.Show(string.Format(Locals.PortIsBusy, Port), Locals.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                IsInitialized = false;
                return IsInitialized;
            }
            catch (Exception e)
            {
                logger.Warn(e, "An error occured in UDPPacketsParser ctor.");
                MessageBox.Show(string.Format(Locals.ErrorToPortConnect, Port, e.Message), Locals.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                IsInitialized = false;
                return IsInitialized;
            }
        }

        public LogMessage GetLog()
        {
            LogMessage log = null;
            string incomingLog = string.Empty;
            try
            {
                // получаем байтики
                Byte[] receiveBytes = udpClient.Receive(ref remoteIpEndPoint);
                if (IgnoredIPs.Any(x => x.IsActive && x.IP.Contains(remoteIpEndPoint.Address.ToString())))
                    return null;

                // переводим их в строку
                incomingLog = encoding.GetString(receiveBytes);

                log = ReadXmlLog(incomingLog);
                log.Address = Settings.Instance.IsSeparateIpLoggersByPort ? $"{remoteIpEndPoint.Address.ToString()}:{Port}" : remoteIpEndPoint.Address.ToString();
                log.Receiver.Port = this.Port;
            }
            catch (SocketException)
            {
                return null;
            }
            catch (Exception e)
            {
                log = new LogMessage
                {
                    Logger = "UDP Logger",
                    Address = Settings.Instance.IsSeparateIpLoggersByPort ? $"{remoteIpEndPoint.Address.ToString()}:{Port}" : remoteIpEndPoint.Address.ToString(),
                    Thread = -1,
                    Message = $"An error occurred while parsing log: {incomingLog}. {Environment.NewLine} Exception: {e}",
                    Time = DateTime.Now,
                    Level = eLogLevel.Error,
                    ExecutableName = "LogViewer"
                };

                logger.Warn(e, "An error occurred while parsing log.");
            }
            return log;
        }

        private LogMessage ReadXmlLog(string xmlFragment)
        {
            LogMessage log = new LogMessage();

            using (XmlReader reader = new XmlTextReader(xmlFragment, XmlNodeType.Element, xmlContext))
            {
                reader.Read();
                if (reader.MoveToContent() != XmlNodeType.Element || reader.Name != "log4j:event")
                    throw new Exception("The Log Event is not a valid log4j Xml block.");

                log.Logger = reader.GetAttribute("logger");
                log.Level = (eLogLevel)Enum.Parse(typeof(eLogLevel), reader.GetAttribute("level").ToLower().FirstCharToUpper());
                log.Thread = int.Parse(reader.GetAttribute("thread"));

                long timeStamp;
                if (long.TryParse(reader.GetAttribute("timestamp"), out timeStamp))
                    log.Time = UnixTimeStampToDateTime(timeStamp).ToLocalTime();

                int eventDepth = reader.Depth;
                reader.Read();
                while (reader.Depth > eventDepth)
                {
                    if (reader.MoveToContent() == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "log4j:message":
                                log.Message = reader.ReadString();
                                break;

                            case "log4j:throwable":
                                log.Message += Environment.NewLine + reader.ReadString();
                                break;

                            case "log4j:properties":
                                reader.Read();
                                while (reader.MoveToContent() == XmlNodeType.Element
                                       && reader.Name == "log4j:data")
                                {
                                    string name = reader.GetAttribute("name");
                                    string value = reader.GetAttribute("value");
                                    if (!string.IsNullOrEmpty(name) && name == "log4japp" && !string.IsNullOrEmpty(value) && value.Contains(".exe"))
                                    {
                                        log.ExecutableName = value.Substring(0, value.IndexOf(".exe", StringComparison.Ordinal));
                                    }

                                    reader.Read();
                                }

                                break;
                        }
                    }
                    reader.Read();
                }
            }
            return log;
        }

        private XmlParserContext CreateContext()
        {
            var nt = new NameTable();
            var nsmanager = new XmlNamespaceManager(nt);
            nsmanager.AddNamespace("log4j", "http://jakarta.apache.org/log4j/");
            nsmanager.AddNamespace("nlog", "http://nlog-project.org");
            return new XmlParserContext(nt, nsmanager, "elem", XmlSpace.None, Encoding.UTF8);
        }

        private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc).AddMilliseconds(unixTimeStamp);
        }

        public void Dispose()
        {
            try
            {
                IsInitialized = false;
                udpClient?.Close();
                udpClient?.Dispose();
                udpClient = null;
            }
            catch (Exception e)
            {
                logger.Warn(e, "Dispose error");
            }
        }
    }
}