using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

namespace CyDrive.Model
{
	public class FileInfo
	{
		private string filePath;

		[JsonPropertyName("file_mode")]
		public uint FileMode { get; set; }
		[JsonPropertyName("modify_time")]
		public long ModifyTime { get; set; }

		[JsonPropertyName("file_path")]
		public string FilePath 
		{
			get
			{
				return filePath;
			}
			set
			{
				filePath = value;
				Name = System.IO.Path.GetFileName(FilePath);
			}
		}

		[JsonPropertyName("size")]
		public long Size { get; set; }
		[JsonPropertyName("is_dir")]
		public bool IsDir { get; set; }
		[JsonPropertyName("is_compressed")]
		public bool IsCompressed { get; set; }

		public string Name { get; set; }

		public string FileSize
		{
			get
			{
				if(IsDir)
				{
					return "...";
				}
				if(Size >=(1<<30))
				{
					return string.Format("{0:#.#}GiB", Size / (float)(1 << 30));
				}
				else if(Size>=(1<<20))
				{
					return string.Format("{0:#.#}MiB", Size / (float)(1 << 20));
				}
				else if(Size>=(1<<10))
				{
					return string.Format("{0:#.#}KiB", Size / (float)(1 << 10));
				}

				return string.Format("{0:#.#}B", Size);
			}
		}

		public FileInfo() { }
		public FileInfo(User user,string path)
		{
			var absPath = Path.Combine(user.WorkDir, path);
			var osFileInfo = new System.IO.FileInfo(absPath);

			FileMode = 0x1ff;
			ModifyTime = osFileInfo.LastWriteTimeUtc.Ticks;
			FilePath = path;
			if(osFileInfo.Exists)
				Size = osFileInfo.Length;
			IsDir = System.IO.Directory.Exists(absPath);
			IsCompressed = false;
		}

		public override string ToString()
		{
			return System.IO.Path.GetFileName(FilePath);
		}
	}
}
