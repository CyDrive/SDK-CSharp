using CyDrive.Model;
using CyDrive.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace CyDrive
{
	public class CyDriveClient
	{
		private HttpClient client;

		public readonly string ServerAddr = "123.57.39.79:6454";
		public bool IsLogin { get; set; }
		public User User;


		public CyDriveClient()
		{
			// Set up http client
			var baseAddr = new Uri(string.Format("http://{0}", ServerAddr));
			var cookieContainer = new CookieContainer();
			var handler = new HttpClientHandler() { CookieContainer = cookieContainer };
			client = new HttpClient(handler) { BaseAddress = baseAddr };

			// Read user info
			try
			{
				var userJson = System.IO.File.ReadAllText(Consts.UserFile);
				User = JsonSerializer.Deserialize<User>(userJson);
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		public async Task<bool> LoginAsync()
		{
			// Post login request
			var resp = await client.PostAsync("/login", new FormUrlEncodedContent(new[]
			{
				new KeyValuePair<string,string>("username","test"),
				new KeyValuePair<string, string>("password",Utils.Utils.PasswordHash("testCyDrive")),
			}));


			// Get the resp
			var res = await JsonSerializer.DeserializeAsync<Resp>(await resp.Content.ReadAsStreamAsync());

			if (!res.IsStatusOK())
			{
				throw new Exception(res.Message);
			}

			IsLogin = true;

			return res.IsStatusOK();
		}

		public async Task<FileInfo[]> ListRemoteDirAsync(string path = "")
		{
			path = path.Replace('\\', '/');

			var resp = await client.GetAsync("/list" + string.Format("/{0}", Uri.EscapeUriString(path)));

			var resJson = await resp.Content.ReadAsStringAsync();
			var res = JsonSerializer.Deserialize<Resp>(resJson);

			if (!res.IsStatusOK())
			{
				throw new Exception(res.Message);
			}

			List<FileInfo> list = JsonSerializer.Deserialize<List<FileInfo>>(res.Data);

			return list.ToArray();
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

		public async Task<FileInfo> GetFileInfoAsync(string path)
		{
			var resp = await client.GetAsync("/file_info" + string.Format("/{0}", Uri.EscapeUriString(path)));

			var resJson = await resp.Content.ReadAsStringAsync();
			var res = JsonSerializer.Deserialize<Resp>(resJson);

			if (!res.IsStatusOK())
			{
				throw new Exception(res.Message);
			}

			return JsonSerializer.Deserialize<FileInfo>(res.Data);
		}

		public async Task<Resp> PutFileInfoAsync(FileInfo fileInfo)
		{
			var fileInfoJson = JsonSerializer.Serialize(fileInfo);
			var resp = await client.PutAsync("/file_info" + string.Format("/{0}", Uri.EscapeUriString(fileInfo.FilePath)),
				new StringContent(fileInfoJson));

			var res = JsonSerializer.Deserialize<Resp>(await resp.Content.ReadAsStringAsync());

			if (!res.IsStatusOK())
			{
				throw new Exception(res.Message);
			}

			return res;
		}

		public async Task<bool> DownloadAsync(string path)
		{
			var fileInfo = await GetFileInfoAsync(path);
			if (fileInfo == null)
				return false;

			string localPath = System.IO.Path.Combine(User.RootDir, fileInfo.FilePath);
			var resp = await client.GetAsync("/file" + string.Format("/{0}", Uri.EscapeUriString(path)));
			await System.IO.File.WriteAllBytesAsync(localPath, await resp.Content.ReadAsByteArrayAsync());
			return true;
		}

		public async Task<Resp> UploadAsync(string path)
		{
			var fileData = await System.IO.File.ReadAllBytesAsync(
				System.IO.Path.Combine(User.WorkDir, path));

			FileInfo fileInfo = new FileInfo(User, path);

			string fileInfoJson = JsonSerializer.Serialize(fileInfo);
			var resp = await client.PutAsync("/file" + string.Format("/{0}", Uri.EscapeDataString(path)),
				new ByteArrayContent(fileData));
			await PutFileInfoAsync(fileInfo);

			var resJson = await resp.Content.ReadAsStringAsync();
			var res = JsonSerializer.Deserialize<Resp>(resJson);

			if (!res.IsStatusOK())
			{
				throw new Exception("upload failed");
			}

			return res;
		}

		public async Task<Resp> DeleteFileAsync(string path)
		{
			var resp = await client.DeleteAsync("/file" + string.Format("/{0}", Uri.EscapeUriString(path)));
			var resJson = await resp.Content.ReadAsStringAsync();
			var res = JsonSerializer.Deserialize<Resp>(resJson);

			if (!res.IsStatusOK())
			{
				throw new Exception("Delete failed");
			}

			return res;
		}
	}
}
