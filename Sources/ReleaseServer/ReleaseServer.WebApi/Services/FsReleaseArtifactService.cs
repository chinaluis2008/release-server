using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ReleaseServer.WebApi.Mappers;
using ReleaseServer.WebApi.Models;
using ReleaseServer.WebApi.Repositories;

namespace ReleaseServer.WebApi.Services
{
    public class FsReleaseArtifactService : IReleaseArtifactService
    {
        private IReleaseArtifactRepository FsReleaseArtifactRepository;
        private ILogger Logger;
        private static SemaphoreSlim DirectoryLock;

        public FsReleaseArtifactService(IReleaseArtifactRepository fsReleaseArtifactRepository,
            ILogger<FsReleaseArtifactService> logger)
        {
            FsReleaseArtifactRepository = fsReleaseArtifactRepository;
            Logger = logger;
            DirectoryLock = new SemaphoreSlim(1, 1);
        }

        public async Task StoreArtifact(string product, string os, string architecture, string version,
            IFormFile payload)
        {
            Logger.LogDebug("convert the uploaded payload to a ZIP archive");
            using var fileStream = payload.OpenReadStream();
            using var zipPayload = new ZipArchive(payload.OpenReadStream());
            
            var artifact =
                ReleaseArtifactMapper.ConvertToReleaseArtifact(product, os, architecture, version, zipPayload);

            await DirectoryLock.WaitAsync();

            //It's important to release the semaphore. try / finally block ensures a guaranteed release (also if the operation may crash) 
            try
            {
                await Task.Run(() => FsReleaseArtifactRepository.StoreArtifact(artifact));
            }
            finally
            {
                DirectoryLock.Release();
            }
            
        }

        public async Task<List<ProductInformation>> GetProductInfos(string productName)
        {
            return await Task.Run(() => FsReleaseArtifactRepository.GetInfosByProductName(productName));
        }

        public async Task<List<string>> GetPlatforms(string productName, string version)
        {
            return await Task.Run(() => FsReleaseArtifactRepository.GetPlatforms(productName, version));
        }

        public async Task<ReleaseInformation> GetReleaseInfo(string productName, string os, string architecture, string version)
        {
            return await Task.Run(() =>
                FsReleaseArtifactRepository.GetReleaseInfo(productName, os, architecture, version));
        }

        public async Task<List<ProductVersion>> GetVersions(string productName, string os, string architecture)
        {
            return await Task.Run(() => FsReleaseArtifactRepository.GetVersions(productName, os, architecture));
        }

        public async Task<ProductVersion> GetLatestVersion(string productName, string os, string architecture)
        {
            var versions = await Task.Run(() => FsReleaseArtifactRepository.GetVersions(productName, os, architecture));

            if (versions.IsNullOrEmpty())
                return null;

            return versions.First();
        }

        public async Task<ArtifactDownload> GetSpecificArtifact(string productName, string os, string architecture,
            string version)
        {
            return await Task.Run(() =>
                FsReleaseArtifactRepository.GetSpecificArtifact(productName, os, architecture, version));
        }

        public async Task<ArtifactDownload> GetLatestArtifact(string productName, string os, string architecture)
        {
            var latestVersion = await Task.Run(() => GetLatestVersion(productName, os, architecture));

            if (latestVersion == null)
                return null;

            return await Task.Run(() =>
                FsReleaseArtifactRepository.GetSpecificArtifact(productName, os, architecture, latestVersion.ToString()));
        }

        public async Task<bool> DeleteSpecificArtifactIfExists(string productName, string os, string architecture,
            string version)
        {
            await DirectoryLock.WaitAsync();

            //It's important to release the semaphore. try / finally block ensures a guaranteed release (also if the operation may crash) 
            try
            {
                return await Task.Run(() =>
                    FsReleaseArtifactRepository.DeleteSpecificArtifactIfExists(productName, os, architecture, version));
            }
            finally
            {
                DirectoryLock.Release();
            }
        }

        public async Task<bool> DeleteProductIfExists(string productName)
        {
            await DirectoryLock.WaitAsync();

            //It's important to release the semaphore. try / finally block ensures a guaranteed release (also if the operation may crash) 
            try
            {
                return await Task.Run(() => FsReleaseArtifactRepository.DeleteProductIfExists(productName));
            }
            finally
            {
                DirectoryLock.Release();
            }
        }

        public async Task<BackupInformation> RunBackup()
        {
            BackupInformation backup;

            await DirectoryLock.WaitAsync();

            //It's important to release the semaphore. try / finally block ensures a guaranteed release (also if the operation may crash) 
            try
            {
                backup = await Task.Run(() => FsReleaseArtifactRepository.RunBackup());
            }
            finally
            {
                DirectoryLock.Release();
            }

            return backup;
        }

        public async Task RestoreBackup(IFormFile payload)
        {
            
            Logger.LogDebug("convert the uploaded payload to a ZIP archive");
            using var fileStream = payload.OpenReadStream();
            using var zipPayload = new ZipArchive(payload.OpenReadStream());
            
            await DirectoryLock.WaitAsync();

            //It's important to release the semaphore. try / finally block ensures a guaranteed release (also if the operation may crash) 
            try
            {
                await Task.Run(() => FsReleaseArtifactRepository.RestoreBackup(zipPayload));
            }
            finally
            {
                DirectoryLock.Release();
            }
        
        }

        public ValidationResult ValidateUploadPayload(IFormFile payload)
        {
            DeploymentMetaInfo deploymentMetaInfo;
            
            Logger.LogDebug("convert the uploaded payload to a ZIP archive");
            using var fileStream = payload.OpenReadStream();
            using var payloadZipArchive = new ZipArchive(payload.OpenReadStream());
            
            //Try to get the deployment.json entry from the zip archive and check, if it exists
            var deploymentInfoEntry = payloadZipArchive.GetEntry("deployment.json");

            if (deploymentInfoEntry == null)
            {
                var validationError = "the deployment.json does not exist in the uploaded payload!";
                Logger.LogError(validationError);
                return new ValidationResult {IsValid = false, ValidationError = validationError};
            }

            //Open the deployment.json and extract the DeploymentMetaInfo of it
            using (StreamReader deploymentInfoFile = new StreamReader(deploymentInfoEntry.Open(), System.Text.Encoding.UTF8))
            {
                try
                {
                    JsonSerializer serializer = new JsonSerializer();
                    deploymentMetaInfo = (DeploymentMetaInfo) serializer.Deserialize(deploymentInfoFile, typeof(DeploymentMetaInfo));
                }
                catch (Exception e)
                {
                    var validationError = "the deployment meta information (deployment.json) is invalid! "
                                          + "Error: " + e.Message;
                    Logger.LogError(validationError);
                    return new ValidationResult { IsValid = false, ValidationError = validationError };
                }
            }

            //Check if the uploaded payload contains the the artifact file
            if (payloadZipArchive.GetEntry(deploymentMetaInfo.ArtifactFileName) == null)
            {
                var validationError = "the expected artifact" + " \"" + deploymentMetaInfo.ArtifactFileName +
                                      "\"" + " does not exist in the uploaded payload!";
                Logger.LogError(validationError);
                return new ValidationResult {IsValid = false, ValidationError = validationError};
            }

            if (payloadZipArchive.GetEntry(deploymentMetaInfo.ReleaseNotesFileName) == null)
            {
                var validationError = "the expected release notes file" + " \"" +  deploymentMetaInfo.ReleaseNotesFileName +
                                      "\"" +  " does not exist in the uploaded payload!";
                Logger.LogError(validationError);
                return new ValidationResult {IsValid = false, ValidationError = validationError};
            }
            
            //Check if the uploaded payload contains the release notes file
            var releaseNotesEntry = payloadZipArchive.GetEntry(deploymentMetaInfo.ReleaseNotesFileName);
            
            if (releaseNotesEntry == null)
            {
                var validationError = "the expected release notes file" + " \"" +  deploymentMetaInfo.ReleaseNotesFileName +
                                      "\"" +  " does not exist in the uploaded payload!";
                Logger.LogError(validationError);
                return new ValidationResult {IsValid = false, ValidationError = validationError};
            }
            
            //Try to Deserialize the release notes and check if it is valid
            using (StreamReader releaseNotesFile = new StreamReader(releaseNotesEntry.Open(), System.Text.Encoding.UTF8))
            {
                try
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Deserialize(releaseNotesFile, typeof(ReleaseNotes));
                }
                catch (Exception e)
                {
                    var validationError = "the release notes file" + " \"" +  deploymentMetaInfo.ReleaseNotesFileName +
                                          "\" is an invalid json file! " + "Error: " + e.Message;;
                    return new ValidationResult { IsValid = false, ValidationError = validationError };
                }
            }
            
            //The uploaded payload is valid
            return new ValidationResult {IsValid = true};
        }
    }
}
