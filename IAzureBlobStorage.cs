using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MicroServiceApp.Azure_Blob_storage
{
    public interface IAzureBlobStorage
    {
        Task<string> UploadAsync(string blobName, Stream stream, string contenType);
        Task<string> GetSASToken();
    }
}
