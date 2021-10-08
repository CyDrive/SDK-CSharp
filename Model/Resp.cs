using CyDrive.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace CyDrive.Model
{
	public class Resp
	{
		[JsonPropertyName("status")]
		public int Status { get; set; }
		[JsonPropertyName("message")]
		public string Message { get; set; }
		[JsonPropertyName("data")]
		public string Data { get; set; }

		public bool IsStatusOK()
		{
			return Status == (int)StatusCode.StatusOk;
		}
	}
}
