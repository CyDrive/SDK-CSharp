// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: consts/enums.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace CyDrive {

  /// <summary>Holder for reflection information generated from consts/enums.proto</summary>
  public static partial class EnumsReflection {

    #region Descriptor
    /// <summary>File descriptor for consts/enums.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static EnumsReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChJjb25zdHMvZW51bXMucHJvdG8SBmNvbnN0cypUCgtNZXNzYWdlVHlwZRII",
            "CgRUZXh0EAASCQoFSW1hZ2UQARIJCgVBdWRpbxACEgkKBVZpZGVvEAMSCAoE",
            "RmlsZRAEEhAKDE5vdGlmaWNhdGlvbhAFKpIBCgpTdGF0dXNDb2RlEgYKAk9r",
            "EAASDQoJQXV0aEVycm9yEAESEgoOTmVlZFBhcmFtZXRlcnMQAhIVChFJbnZh",
            "bGlkUGFyYW1ldGVycxAEEhAKDEZpbGVUb29MYXJnZRAIEgsKB0lvRXJyb3IQ",
            "EBIRCg1JbnRlcm5hbEVycm9yECASEAoMU2Vzc2lvbkVycm9yEEBCJVoZZ2l0",
            "aHViLmNvbS9DeURyaXZlL2NvbnN0c6oCB0N5RHJpdmViBnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::CyDrive.MessageType), typeof(global::CyDrive.StatusCode), }, null));
    }
    #endregion

  }
  #region Enums
  public enum MessageType {
    [pbr::OriginalName("Text")] Text = 0,
    [pbr::OriginalName("Image")] Image = 1,
    [pbr::OriginalName("Audio")] Audio = 2,
    [pbr::OriginalName("Video")] Video = 3,
    [pbr::OriginalName("File")] File = 4,
    [pbr::OriginalName("Notification")] Notification = 5,
  }

  public enum StatusCode {
    [pbr::OriginalName("Ok")] Ok = 0,
    [pbr::OriginalName("AuthError")] AuthError = 1,
    [pbr::OriginalName("NeedParameters")] NeedParameters = 2,
    [pbr::OriginalName("InvalidParameters")] InvalidParameters = 4,
    [pbr::OriginalName("FileTooLarge")] FileTooLarge = 8,
    [pbr::OriginalName("IoError")] IoError = 16,
    [pbr::OriginalName("InternalError")] InternalError = 32,
    [pbr::OriginalName("SessionError")] SessionError = 64,
  }

  #endregion

}

#endregion Designer generated code
