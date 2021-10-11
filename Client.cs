using CyDrive.Models;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Google.Protobuf;
using System.Threading;
using System.IO;

namespace CyDrive
{
    public class CyDriveClient
    {
        private HttpClient client;
        private ConcurrentDictionary<long, DataTask> taskMap = new ConcurrentDictionary<long, DataTask>();

        public readonly string ServerAddr = "123.57.39.79:6454";
        public bool IsLogin { get; set; }
        public Account Account = new Account();


        public CyDriveClient(string ServerAddr, Account account = null)
        {
            // Set up http client
            var baseAddr = new Uri(string.Format("http://{0}", ServerAddr));
            var cookieContainer = new CookieContainer();
            var handler = new HttpClientHandler() { CookieContainer = cookieContainer };
            client = new HttpClient(handler) { BaseAddress = baseAddr };

            if (account != null)
            {
                Account = account;
            }
        }

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

            var resp = JsonParser.Default.Parse<Response>(await res.Content.ReadAsStringAsync());

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

            var resp = JsonParser.Default.Parse<Response>(await res.Content.ReadAsStringAsync());

            IsLogin = resp.StatusCode == StatusCode.Ok;

            return IsLogin;
        }

        //public async Task<FileInfo[]> ListRemoteDirAsync(string path = "")
        //{
        //    path = path.Replace('\\', '/');

        //    var res = await client.GetAsync("/list" + string.Format("/{0}", Uri.EscapeUriString(path)));
        //    if (!res.IsSuccessStatusCode)
        //    {
        //        return null;
        //    }
        //    var resp = JsonParser.Default.Parse<Response>(await res.Content.ReadAsStringAsync());

        //    var getFileListResponse = JsonParser.Default.Parse<GetFileListResponse>(resp.Data);

        //    var fileInfoList = new FileInfo[getFileListResponse.FileInfoList.Count];
        //    for (var i = 0; i < fileInfoList.Length; i++)
        //    {
        //        fileInfoList[i] = getFileListResponse.FileInfoList[i];
        //    }
        //    return fileInfoList;
        //}

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
            var resp = JsonParser.Default.Parse<Response>(await res.Content.ReadAsStringAsync());


            if (resp.StatusCode != StatusCode.Ok)
            {
                return null;
            }

            var fileInfo = JsonParser.Default.Parse<Models.FileInfo>(resp.Data);

            return fileInfo;
        }

        public async Task<DataTask> DownloadAsync(string path, string savePath)
        {
            path.Replace('\\', '/');
            var res = await client.GetAsync("/file" + string.Format("/{0}", Uri.EscapeUriString(path)));
            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            var resp = JsonParser.Default.Parse<Response>(await res.Content.ReadAsStringAsync());
            var downloadResponse = JsonParser.Default.Parse<DownloadResponse>(resp.Data);


            var task = new DataTask(downloadResponse.TaskId, DataTaskType.Download,
                savePath, 0, Utils.ParseIpAddr(downloadResponse.NodeAddr),
                downloadResponse.FileInfo);

            AddTask(task);
            return task;
        }

        public async Task<DataTask> UploadAsync(string absPath, string savePath)
        {
            var osFileInfo = new System.IO.FileInfo(absPath);
            Models.FileInfo fileInfo = new Models.FileInfo()
            {
                FilePath = savePath,
                Size = osFileInfo.Length,
                IsCompressed = false,
                IsDir = false,
                ModifyTime = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(osFileInfo.LastWriteTime),
            };

            var req = new UploadRequest()
            {
                FileInfo = fileInfo,
            };
            var res = await client.PutAsync("/file" + string.Format("/{0}", Uri.EscapeDataString(savePath)),
                new StringContent(JsonFormatter.Default.Format(req)));

            var resp = JsonParser.Default.Parse<Response>(await res.Content.ReadAsStringAsync());
            var uploadResp = JsonParser.Default.Parse<UploadResponse>(resp.Data);

            var task = new DataTask(uploadResp.TaskId, DataTaskType.Upload,
                savePath, 0, Utils.ParseIpAddr(uploadResp.NodeAddr),
                fileInfo);

            AddTask(task);

            return task;
        }

        //public async Task<Resp> DeleteFileAsync(string path)
        //{
        //    var resp = await client.DeleteAsync("/file" + string.Format("/{0}", Uri.EscapeUriString(path)));
        //    var resJson = await resp.Content.ReadAsStringAsync();
        //    var res = JsonSerializer.Deserialize<Resp>(resJson);

        //    if (!res.IsStatusOK())
        //    {
        //        throw new Exception("Delete failed");
        //    }

        //    return res;
        //}

        public void AddTask(DataTask task)
        {
            taskMap.TryAdd(task.Id, task);
        }
    }
}
