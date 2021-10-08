using System;
using System.Collections.Generic;
using System.Text;

namespace CyDrive.Utils
{
	static class Consts
	{
		public const String UserFile = "user.json";
	}

	enum StatusCode:int
	{
		StatusOk = 0,
		StatusAuthError = 1,
		StatusNoParameterError = 2,
		StatusSessionError = 4,
		StatusFileTooLargeError = 8,
		StatusIoError = 16,
		StatusInternalError = 32
	}
}
