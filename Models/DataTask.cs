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
        private TcpClient tcpClient = new TcpClient();
        private Task innerTask;

        public int Id { get; set; }
        public DataTaskType Type { get; set; }
        public string LocalPath { get; set; }
        public long Offset { get; set; }
        public IPEndPoint ServerAddr { get; set; }
        public FileInfo FileInfo { get; set; }
        public DateTime StartAt { get; set; }

        public long DoneBytes { get; private set; }

        // Optional parameters
        public bool ShouldTruncate { get; set; }
        public int BufferSize { get; set; } = 4096;


        public DataTask(int id, DataTaskType type, string localPath, long offset, IPEndPoint serverAddr, FileInfo fileInfo)
        {
            Id = id;
            Type = type;
            LocalPath = localPath;
            Offset = offset;
            ServerAddr = serverAddr;
            FileInfo = fileInfo;

            StartAt = DateTime.Now;
            DoneBytes = 0;
        }

        public async void StartAsync()
        {
            try
            {
                tcpClient.Connect(ServerAddr);

                switch (Type)
                {
                    case DataTaskType.Download:
                        innerTask = DownloadData();
                        break;
                    case DataTaskType.Upload:
                        innerTask = UploadData();
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task DownloadData()
        {
            try
            {
                var stream = tcpClient.GetStream();
                var sendIdTask = SendIdAsync(stream);

                // Open/Create file
                Directory.CreateDirectory(Path.GetDirectoryName(LocalPath));
                var fileMode = FileMode.OpenOrCreate;
                var fs = File.Open(LocalPath, fileMode);
                fs.Seek(Offset, SeekOrigin.Begin);
                if (ShouldTruncate)
                {
                    fs.SetLength(Offset);
                }

                await sendIdTask;

                // Download file
                byte[] buf = new byte[BufferSize];
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

                fs.Close();
                tcpClient.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task UploadData()
        {
            var stream = tcpClient.GetStream();
            var sendIdTask = SendIdAsync(stream);

            var fs = File.Open(LocalPath, FileMode.Open, FileAccess.Read);
            fs.Seek(Offset, SeekOrigin.Begin);

            await sendIdTask;

            // Download file
            byte[] buf = new byte[BufferSize];
            while (true)
            {
                var readBytesCount = await fs.ReadAsync(buf, 0, buf.Length);
                if (readBytesCount == 0)
                {
                    break;
                }

                await stream.WriteAsync(buf, 0, readBytesCount);
                await stream.FlushAsync();
                Offset += readBytesCount;
            }

            fs.Close();
            tcpClient.Close();
        }

        public void Wait()
        {
            if (innerTask != null)
            {
                innerTask.Wait();
            }
        }

        private async Task SendIdAsync(NetworkStream stream)
        {
            var idBytes = BitConverter.GetBytes(Id);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(idBytes);
            }
            await stream.WriteAsync(idBytes, 0, idBytes.Length);
        }
    }
}
