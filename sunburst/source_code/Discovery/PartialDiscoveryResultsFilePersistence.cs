// Decompiled with JetBrains decompiler
// Type: SolarWinds.Orion.Core.BusinessLayer.Discovery.PartialDiscoveryResultsFilePersistence
// Assembly: SolarWinds.Orion.Core.BusinessLayer, Version=2020.2.5300.12432, Culture=neutral, PublicKeyToken=null
// MVID: 8A00C947-7FE8-4638-AFC6-F6694E5CE56E
// Assembly location: Z:\samples\new\4572807326629888\sunburst.dll

using SolarWinds.Logging;
using SolarWinds.Orion.Discovery.Job;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace SolarWinds.Orion.Core.BusinessLayer.Discovery
{
  public class PartialDiscoveryResultsFilePersistence : IPartialDiscoveryResultsPersistence
  {
    private static readonly Log _log = new Log();
    private readonly string _persistenceFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SolarWinds/Discovery Engine/PartialResults");

    public PartialDiscoveryResultsFilePersistence()
    {
    }

    internal PartialDiscoveryResultsFilePersistence(string customPersistencePath)
    {
      this._persistenceFolderPath = customPersistencePath;
    }

    public bool SaveResult(Guid jobId, OrionDiscoveryJobResult result)
    {
      if (result == null)
        throw new ArgumentNullException(nameof (result));
      string path = (string) null;
      try
      {
        path = this.GetResultsTempFileName(jobId);
        DataContractSerializer contractSerializer = new DataContractSerializer(typeof (OrionDiscoveryJobResult));
        using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
        {
          aesAlg.Key = this.GetEncryptionKey(jobId);
          aesAlg.IV = this.GetEncryptionIV(aesAlg, jobId);
          aesAlg.Mode = CipherMode.CBC;
          aesAlg.Padding = PaddingMode.PKCS7;
          using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
          {
            using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
              using (CryptoStream cryptoStream = new CryptoStream((Stream) fileStream, encryptor, CryptoStreamMode.Write))
              {
                using (GZipStream gzipStream = new GZipStream((Stream) cryptoStream, CompressionMode.Compress))
                  contractSerializer.WriteObject((Stream) gzipStream, (object) result);
              }
            }
          }
        }
        return true;
      }
      catch (Exception ex)
      {
        PartialDiscoveryResultsFilePersistence._log.ErrorFormat("Error saving partial discovery result for job {0} to temporary file {1}. {2}", (object) jobId, (object) (path ?? "<unable to get filename>"), (object) ex);
      }
      return false;
    }

    public OrionDiscoveryJobResult LoadResult(Guid jobId)
    {
      string path = (string) null;
      try
      {
        path = this.GetResultsTempFileName(jobId);
        DataContractSerializer contractSerializer = new DataContractSerializer(typeof (OrionDiscoveryJobResult));
        using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
        {
          aesAlg.Key = this.GetEncryptionKey(jobId);
          aesAlg.IV = this.GetEncryptionIV(aesAlg, jobId);
          aesAlg.Mode = CipherMode.CBC;
          aesAlg.Padding = PaddingMode.PKCS7;
          using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
          {
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
              using (CryptoStream cryptoStream = new CryptoStream((Stream) fileStream, decryptor, CryptoStreamMode.Read))
              {
                using (GZipStream gzipStream = new GZipStream((Stream) cryptoStream, CompressionMode.Decompress))
                  return (OrionDiscoveryJobResult) contractSerializer.ReadObject((Stream) gzipStream);
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        PartialDiscoveryResultsFilePersistence._log.ErrorFormat("Error loading partial discovery result for job {0} from temporary file {1}. {2}", (object) jobId, (object) (path ?? "<unable to get filename>"), (object) ex);
      }
      return (OrionDiscoveryJobResult) null;
    }

    public void DeleteResult(Guid jobId)
    {
      string path = (string) null;
      try
      {
        path = this.GetResultsTempFileName(jobId);
        File.Delete(path);
      }
      catch (Exception ex)
      {
        PartialDiscoveryResultsFilePersistence._log.ErrorFormat("Error deleting partial discovery result for job {0} temporary file {1}. {2}", (object) jobId, (object) (path ?? "<unable to get filename>"), (object) ex);
      }
    }

    public void ClearStore()
    {
      try
      {
        if (!Directory.Exists(this._persistenceFolderPath))
          return;
        Directory.Delete(this._persistenceFolderPath, true);
      }
      catch (Exception ex)
      {
        PartialDiscoveryResultsFilePersistence._log.ErrorFormat("Error clearing partial discovery results persistence store '{0}'. {1}", (object) this._persistenceFolderPath, (object) ex);
      }
    }

    private byte[] GetEncryptionKey(Guid jobId)
    {
      return ((IEnumerable<byte>) ProtectedData.Protect(Encoding.UTF8.GetBytes(jobId.ToString()), (byte[]) null, DataProtectionScope.LocalMachine)).Take<byte>(32).ToArray<byte>();
    }

    private byte[] GetEncryptionIV(AesCryptoServiceProvider aesAlg, Guid jobId)
    {
      return ((IEnumerable<byte>) Encoding.UTF8.GetBytes(jobId.ToString())).Take<byte>(aesAlg.BlockSize / 8).ToArray<byte>();
    }

    private string GetResultsTempFileName(Guid jobId)
    {
      if (!Directory.Exists(this._persistenceFolderPath))
        Directory.CreateDirectory(this._persistenceFolderPath);
      return Path.Combine(this._persistenceFolderPath, jobId.ToString() + ".result");
    }
  }
}
