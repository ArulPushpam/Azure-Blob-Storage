using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace MicroServiceApp.Azure_Blob_storage
{
    public class AzureBlobStorage : IAzureBlobStorage
    {
        #region " Public "

        public AzureBlobStorage(AzureBlobSettings settings)
        {
            this.settings = settings;
        }
        public async Task<string> UploadAsync(string blobName, Stream stream, string contenType)
        {
            try
            {
                //Blob
                CloudBlockBlob blockBlob = await GetBlockBlobAsync(blobName);
                //Upload
                stream.Position = 0;
                blockBlob.Properties.ContentType = contenType;
                //stream.ToArray();
                stream.Seek(0, SeekOrigin.Begin);
                 blockBlob.UploadFromStreamAsync(stream);
                return blockBlob.Uri.AbsoluteUri;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public async Task<string> GetSASToken()
        {
            try
            {
                CloudBlobContainer blobContainer = await GetContainerAsync();
                var token = await GetBlobSasUri(blobContainer);
                return token.ToString();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #region " Private "

        private readonly AzureBlobSettings settings;

        private async Task<CloudBlobContainer> GetContainerAsync()
        {
            try
            {
                //Account
                CloudStorageAccount storageAccount = new CloudStorageAccount(
                    new StorageCredentials(settings.StorageAccount, settings.StorageKey), false);

                //Client
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                //Container
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(settings.ContainerName);
                await blobContainer.CreateIfNotExistsAsync();

                return blobContainer;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private async Task<CloudBlockBlob> GetBlockBlobAsync(string blobName)
        {
            //Container
            CloudBlobContainer blobContainer = await GetContainerAsync();
            //Blob
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(blobName);

            return blockBlob;
        }
        private async Task<string> GetBlobSasUri(CloudBlobContainer container)
        {
            var policyName = "yourPolicyName";
            // create the stored policy we will use, with the relevant permissions and expiry time
            var storedPolicy = new SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddDays(10),
                Permissions = SharedAccessBlobPermissions.Read |
                              SharedAccessBlobPermissions.Write |
                              SharedAccessBlobPermissions.List
            };
            // get the existing permissions (alternatively create new BlobContainerPermissions())
            var permissions = await container.GetPermissionsAsync();
            // optionally clear out any existing policies on this container
            permissions.SharedAccessPolicies.Clear();
            // add in the new one
            permissions.SharedAccessPolicies.Add(policyName, storedPolicy);
            // save back to the container
            await container.SetPermissionsAsync(permissions);
            // Now we are ready to create a shared access signature based on the stored access policy
            var containerSignature = container.GetSharedAccessSignature(null, policyName);
            // create the URI a client can use to get access to just this container
            return containerSignature;
        }
        #endregion
        #endregion
    }
}
