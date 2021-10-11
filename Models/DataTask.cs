using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace CyDrive.Models
{
    public enum DataTaskType
    {
        Download = 0,
        Upload = 1,
    }

    public class DataTask
    {
        private TcpClient tcpClient = new TcpClient();

        public long Id { get; set; }
        public DataTaskType Type { get; set; }
        public string PeerPath { get; set; }
        public long Offset { get; set; }
        public IPEndPoint ServerAddr { get; set; }
        public FileInfo FileInfo { get; set; }

        public DateTime StartAt { get; set; }

        public DataTask(long id, DataTaskType type, string peerPath, long offset, IPEndPoint serverAddr, FileInfo fileInfo)
        {
            Id = id;
            Type = type;
            PeerPath = peerPath;
            Offset = offset;
            ServerAddr = serverAddr;
            FileInfo = fileInfo;

            StartAt = DateTime.Now;
        }

        public void Start()
        {
            try
            {
                tcpClient.Connect(ServerAddr);
                switch (Type)
                {
                    case DataTaskType.Download:
                        DownloadData();
                        break;
                    case DataTaskType.Upload:

                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async void DownloadData()
        {
            var fs = File.Open(PeerPath, FileMode.OpenOrCreate | FileMode.Append);
            ArraySegment<byte> buf = new ArraySegment<byte>(new byte[tcpClient.ReceiveBufferSize]);
            await tcpClient.Client.ReceiveAsync(buf, SocketFlags.None);
            await fs.WriteAsync(buf);
        }
    }
}
