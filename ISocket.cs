using NSCommonUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CY_SysLog
{
    public class ISocket
    {
        public int Port { get; private set; }
        public long Ip { get; private set; }
        public bool _writable = false;
        private bool _enable = false;
        private readonly string _lockFile = null;
        private readonly object _locker = new object();
        public bool IsConnect { get; private set; } = false;
        private Socket _socketService = null;
        private Socket _socket = null;
        public StringBuilder sb = new StringBuilder();
        public ISocket(long ip, int port)
        {

            Port = port;
            Ip = ip;
        }
        private void StartListen()
        {
            try
            {
                _socketService = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                EndPoint point = new IPEndPoint(Ip, Port);
                _socketService.Bind(point);
                _socketService.Listen(4);
                Log.Write(ELogLevel.Info, " waite conn");

                Task.Factory.StartNew(Accept);

            }
            catch (Exception er)
            {
                Log.Write(ELogLevel.Error, $" error:{er}");
            }
        }

        private void Accept()
        {
            while (true)
            {
                _socket = _socketService.Accept();
                Log.Write(ELogLevel.Info, " conn ok");
                IsConnect = true;
                _writable = true;
                Task.Factory.StartNew(Receive);
            }
        }


        public void Start()
        {
            lock (_locker)
            {
                if (!_enable)
                {
                    _enable = true;
                    Task.Factory.StartNew(StartListen);
                }
            }
        }
        public List<string> Output()
        {
            lock (_locker)
            {
                List<string> data = null;
                if (sb.Length < 1)
                    return null;
                string ret = sb.ToString();
                if (ret.Contains("\r\n"))
                {
                    data = ret.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    sb = new StringBuilder();
                    sb.Append(data.LastOrDefault());
                }
                return data;
            }
        }

        public void Stop()
        {
            lock (_locker)
            {
                if (!_enable)
                    return;

                IsConnect = false;
                _enable = false;
                Close();
            }
        }
        private void Receive()
        {
            try
            {
                byte[] vs = new byte[1024];
                while (true)
                {
                    if (_socket == null)
                        break;
                    //while (!_writable)
                    //{
                    //    Thread.Sleep(1000);
                    //}
                    int i = _socket.Receive(vs);
                    if (i > 0)
                        lock (_locker)
                        {
                            sb.Append(Encoding.UTF8.GetString(vs, 0, i));
                        }
                }
            }
            catch (Exception er)
            {
                Log.Write(ELogLevel.Error, $" Receive() error:{er}");
            }
        }
        private void Close()
        {
            if (_socket != null)
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                _socket = null;
            }
            if (_socketService != null)
            {
                _socketService.Shutdown(SocketShutdown.Both);
                _socketService.Close();
                _socketService = null;
            }
        }
    }

}
