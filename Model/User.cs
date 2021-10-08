using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CyDrive.Model
{
	public class User
	{
		[JsonPropertyName("id")]
		public Int64 Id { get; set; }
		[JsonPropertyName("username")]
		public String Username { get; set; }
		[JsonPropertyName("password")]
		public String Password { get; set; }
		[JsonPropertyName("usage")]
		public Int64 Usage { get; set; }

		[JsonPropertyName("cap")]
		public Int64 Cap { get; set; }
		[JsonPropertyName("root_dir")]
		public String RootDir { get; set; }
		[JsonPropertyName("work_dir")]
		public String WorkDir { get; set; }
		[JsonPropertyName("created_at")]
		public DateTime CreatedAt { get; set; }
		[JsonPropertyName("updated_at")]
		public DateTime UpdatedAt { get; set; }
	}
}
