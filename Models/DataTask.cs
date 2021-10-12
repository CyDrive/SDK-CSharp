using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading.Tasks;

namespace CyDrive.Models
{
    public enum DataTaskType
    {
        Download = 0,
        Upload = 1,
    }

    public class DataTask
    {
        private CyDriveClient client;
        private TcpClient tcpClient = new TcpClient();

        public int Id { get; set; }
        public DataTaskType Type { get; set; }
        public string PeerPath { get; set; }
        public long Offset { get; set; }
        public IPEndPoint ServerAddr { get; set; }
        public FileInfo FileInfo { get; set; }

        public DateTime StartAt { get; set; }

        public DataTask(CyDriveClient client, int id, DataTaskType type, string peerPath, long offset, IPEndPoint serverAddr, FileInfo fileInfo)
        {
            this.client = client;
            Id = id;
            Type = type;
            PeerPath = peerPath;
            Offset = offset;
            ServerAddr = serverAddr;
            FileInfo = fileInfo;

            StartAt = DateTime.Now;
        }

        public Task Start()
        {
            try
            {
                tcpClient.Connect(ServerAddr);
                switch (Type)
                {
                    case DataTaskType.Download:
                        return DownloadData();
                    case DataTaskType.Upload:
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return null;
        }

        public async Task DownloadData()
        {
            using var fs = File.Open(PeerPath, FileMode.OpenOrCreate | FileMode.Append);
            var stream = tcpClient.GetStream();

            var idBytes = BitConverter.GetBytes(Id);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(idBytes);
            }
            await stream.WriteAsync(idBytes);

            byte[] buf = new byte[4096];
            while (true)
            {
                var readBytesCount = await stream.ReadAsync(buf, 0, buf.Length);
                if (readBytesCount == 0)
                {
                    break;
                }

                await fs.WriteAsync(buf, 0, readBytesCount);
                await fs.FlushAsync();
                Offset += readBytesCount;
            }

            tcpClient.Close();
            client.DropTask(Id);
        }
    }
}
