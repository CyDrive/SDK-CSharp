using System;
using CyDrive.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Text.Json;

namespace CyDrive
{

    static class Utils
    {
        //public static async Task<FileInfo[]> ListLocalDirAsync(User User)
        //{
        //    List<string> fileNameList = new List<string>(System.IO.Directory.GetDirectories(User.WorkDir));
        //    fileNameList.AddRange(System.IO.Directory.GetFiles(User.WorkDir));
        //    List<FileInfo> fileInfoList = new List<FileInfo>();

        //    foreach (var fileName in fileNameList)
        //    {
        //        var path = System.IO.Path.GetRelativePath(User.WorkDir, fileName);
        //        FileInfo fileInfo = new FileInfo(User, path);
        //        fileInfoList.Add(fileInfo);
        //    }

        //    return fileInfoList.ToArray();
        //}

        public static string PasswordHash(string password)
        {
            byte[] hashRes = SHA256.Create().ComputeHash(
            MD5.Create().ComputeHash(ASCIIEncoding.ASCII.GetBytes(password)));

            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashRes)
            {
                sb.AppendFormat("{0}", b);
            }
            return sb.ToString();
        }

        public static IPEndPoint ParseIpAddr(string addr)
        {
            int index = addr.LastIndexOf(':');
            var ipAddr = IPAddress.Parse(addr.Substring(0, index));
            return new IPEndPoint(ipAddr, int.Parse(addr.Substring(index)));
        }
    }
}
