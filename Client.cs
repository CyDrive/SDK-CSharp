using CyDrive.Models;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Google.Protobuf;
using System.Threading;
using System.IO;
using WebSocketSharp;
using System.Net.Http;

namespace CyDrive
{
    public class CyDriveClient
    {
        private Uri baseAddr;
        private CookieContainer cookies;
        private WebSocket messageClient;
        private HttpClient client;

        public readonly string ServerAddr = "123.57.39.79:6454";
        public bool IsLogin { get; set; }
        public Account Account = new Account();
        public readonly int DeviceId;

        public event EventHandler OnOpen
        {
            add { messageClient.OnOpen += value; }
            remove { messageClient.OnOpen -= value; }
        }

        public event EventHandler<MessageEventArgs> OnMessage
        {
            add { messageClient.OnMessage += value; }
            remove { messageClient.OnMessage -= value; }
        }

        public event EventHandler<WebSocketSharp.ErrorEventArgs> OnError
        {
            add { messageClient.OnError += value; }
            remove { messageClient.OnError -= value; }
        }

        public event EventHandler<CloseEventArgs> OnClose
        {
            add { messageClient.OnClose += value; }
            remove { messageClient.OnClose -= value; }
        }

        public CyDriveClient(string serverAddr, int deviceId, Account account = null)
        {
            // Set up http client
            baseAddr = new Uri(string.Format("http://{0}", serverAddr));
            cookies = new CookieContainer();
            var handler = new HttpClientHandler() { CookieContainer = cookies };
            client = new HttpClient(handler) { BaseAddress = baseAddr };
            messageClient = new WebSocket($"ws://{serverAddr}/message_service?device_id={deviceId}");
            DeviceId = deviceId;

            if (account != null)
            {
                Account = account;
            }
        }

        // Account Service API
        public async Task<bool> RegisterAsync(Account account = null)
        {
            if (account != null)
            {
                if (account.Email is null || account.Password is null)
                {
                    throw new InvalidParameterException("email or password is null");
                }

                Account = account;
            }

            var req = new RegisterRequest()
            {
                Email = Account.Email,
                Password = Account.Password,
                Name = Account.Name,
                Cap = Account.Cap,
            };
            var res = await client.PostAsync("/register",
                new StringContent(JsonFormatter.Default.Format(req)));
            if (!res.IsSuccessStatusCode)
            {
                return false;
            }

            var resBody = await res.Content.ReadAsStringAsync();
            Console.Error.WriteLine(resBody);
            var resp = JsonParser.Default.Parse<Response>(resBody);

            return resp.StatusCode == StatusCode.Ok;
        }

        public async Task<bool> LoginAsync(Account account = null)
        {
            if (account != null)
            {
                if (account.Email is null || account.Password is null)
                {
                    throw new InvalidParameterException("email or password is null");
                }

                Account = account;
            }

            var req = new LoginRequest()
            {
                Email = Account.Email,
                Password = Account.Password,
            };
            // Post login request
            var res = await client.PostAsync("/login",
                new StringContent(JsonFormatter.Default.Format(req)));
            if (!res.IsSuccessStatusCode)
            {
                return false;
            }

            var resBody = await res.Content.ReadAsStringAsync();
            Console.Error.WriteLine(resBody);
            var resp = JsonParser.Default.Parse<Response>(resBody);

            IsLogin = resp.StatusCode == StatusCode.Ok;
            if (IsLogin)
            {
                foreach (Cookie cookie in cookies.GetCookies(baseAddr))
                {
                    messageClient.SetCookie(new WebSocketSharp.Net.Cookie(cookie.Name, cookie.Value));
                }
            }

            return IsLogin;
        }

        // Storage Service API
        public async Task<Models.FileInfo[]> ListDirAsync(string path = "")
        {
            path = path.Replace('\\', '/');

            var res = await client.GetAsync("/list" + string.Format("/{0}", Uri.EscapeUriString(path)));
            if (!res.IsSuccessStatusCode)
            {
                return null;
            }
            var resBody = await res.Content.ReadAsStringAsync();
            Console.Error.WriteLine(resBody);

            var resp = JsonParser.Default.Parse<Response>(resBody);
            var getFileListResponse = JsonParser.Default.Parse<GetFileListResponse>(resp.Data);

            var fileInfoList = new Models.FileInfo[getFileListResponse.FileInfoList.Count];
            for (var i = 0; i < fileInfoList.Length; i++)
            {
                fileInfoList[i] = getFileListResponse.FileInfoList[i];
            }
            return fileInfoList;
        }

        /*public async Task<FileInfo[]> ListLocalDirAsync()
		{
			List<string> fileNameList = new List<string>(System.IO.Directory.GetDirectories(User.WorkDir));
			fileNameList.AddRange(System.IO.Directory.GetFiles(User.WorkDir));
			List<FileInfo> fileInfoList = new List<FileInfo>();

			foreach(var fileName in fileNameList)
			{
				var path = System.IO.Path.GetRelativePath(User.WorkDir, fileName);
				FileInfo fileInfo = new FileInfo(User,path);
				fileInfoList.Add(fileInfo);
			}

			return fileInfoList.ToArray();
		}*/

        public async Task<Models.FileInfo> GetFileInfoAsync(string path)
        {
            var res = await client.GetAsync("/file_info" + string.Format("/{0}", Uri.EscapeUriString(path)));
            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            var resBody = await res.Content.ReadAsStringAsync();
            Console.Error.WriteLine(resBody);
            var resp = JsonParser.Default.Parse<Response>(resBody);


            if (resp.StatusCode != StatusCode.Ok)
            {
                return null;
            }

            var fileInfo = JsonParser.Default.Parse<Models.FileInfo>(resp.Data);

            return fileInfo;
        }

        public async Task<DataTask> DownloadAsync(string path, string savePath, bool autoStartTask = true, bool shouldTruncate = false)
        {
            path.Replace('\\', '/');
            var res = await client.GetAsync("/file" + string.Format("/{0}", Uri.EscapeUriString(path)));
            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            var resBody = await res.Content.ReadAsStringAsync();
            Console.Error.WriteLine(resBody);

            var resp = JsonParser.Default.Parse<Response>(resBody);
            var downloadResponse = JsonParser.Default.Parse<DownloadResponse>(resp.Data);

            Console.Error.WriteLine(resp.Data);
            long offset = 0;
            if (!shouldTruncate && File.Exists(savePath))
            {
                var osFileInfo = new System.IO.FileInfo(savePath);
                offset = osFileInfo.Length;
            }

            var task = new DataTask(downloadResponse.TaskId, DataTaskType.Download,
                savePath, offset, Utils.ParseIpAddr(downloadResponse.NodeAddr),
                downloadResponse.FileInfo)
            {
                ShouldTruncate = shouldTruncate,
            };

            if (autoStartTask)
            {
                task.StartAsync();
            }
            return task;
        }

        public async Task<DataTask> UploadAsync(string absPath, string savePath, bool autoStartTask = true, bool shouldTruncate = false)
        {
            var osFileInfo = new System.IO.FileInfo(absPath);
            Models.FileInfo fileInfo = new Models.FileInfo()
            {
                FilePath = savePath,
                Size = osFileInfo.Length,
                IsCompressed = false,
                IsDir = false,
                ModifyTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.SpecifyKind(osFileInfo.LastWriteTime, DateTimeKind.Utc)),
            };

            var req = new UploadRequest()
            {
                FileInfo = fileInfo,
                ShouldTruncate = shouldTruncate,
            };
            var res = await client.PutAsync("/file" + string.Format("/{0}", Uri.EscapeDataString(savePath)),
                new StringContent(JsonFormatter.Default.Format(req)));
            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            var resBody = await res.Content.ReadAsStringAsync();
            Console.Error.WriteLine(resBody);

            var resp = JsonParser.Default.Parse<Response>(resBody);
            var uploadResp = JsonParser.Default.Parse<UploadResponse>(resp.Data);

            var task = new DataTask(uploadResp.TaskId, DataTaskType.Upload,
                absPath, uploadResp.Offset, Utils.ParseIpAddr(uploadResp.NodeAddr),
                fileInfo);

            if (autoStartTask)
            {
                task.StartAsync();
            }

            return task;
        }

        public async Task<Models.FileInfo> DeleteFileAsync(string path)
        {
            var res = await client.DeleteAsync("/file" + string.Format("/{0}", Uri.EscapeUriString(path)));
            var resBody = await res.Content.ReadAsStringAsync();
            Console.WriteLine(resBody);

            var resp = JsonParser.Default.Parse<Response>(resBody);
            var deleteResp = JsonParser.Default.Parse<DeleteResponse>(resp.Data);

            return deleteResp.FileInfo;
        }

        // Message Service API
        public async Task<bool> ConnectMessageService()
        {
            messageClient.Connect();

            return true;
        }

        public async void SendMessage(Message message, Action<bool> OnCompleted)
        {
            string messageString = JsonFormatter.Default.Format(message);
            messageClient.SendAsync(messageString, OnCompleted);
        }
    }
}
