using System;
using ReleaseServer.WebApi.Config;
using ReleaseServer.WebApi.Extensions;
using Xunit;

namespace ReleaseServer.WebApi.Test.Extensions
{
    public class DeploymentMetaInfoExtensiontest
    {
        [Fact]
        public void ValidateDeploymentMeta_Valid()
        {
            var testDeploymentMetaInfo = new DeploymentMetaInfoModel
            {
                ArtifactFileName = "testArtifact.zip",
                ReleaseNotesFileName = "releaseNotes.txt",
                ReleaseDate = new DateTime(2020, 02, 01)
            };

            Assert.True(testDeploymentMetaInfo.IsValid());
        }
        
        [Fact]
        public void ValidateDeploymentMeta_Invalid()
        {
            
            //Without release notes
            var testDeploymentMetaInfo1 = new DeploymentMetaInfoModel
            {
                ArtifactFileName = "testArtifact.zip",
                ReleaseDate = new DateTime(2020, 02, 01)
            };
            
            //Without ReleaseDate
            var testDeploymentMetaInfo2 = new DeploymentMetaInfoModel
            {
                ArtifactFileName = "testArtifact.zip",
                ReleaseNotesFileName = "releaseNotes.txt",
            };
            
            //Without ArtifactFileName
            var testDeploymentMetaInfo3 = new DeploymentMetaInfoModel
            {
                ReleaseNotesFileName = "releaseNotes.txt",
                ReleaseDate = new DateTime(2020, 02, 01)
            };
            
            //Empty
            var testDeploymentMetaInfo4 = new DeploymentMetaInfoModel();
            
            Assert.False(testDeploymentMetaInfo1.IsValid());
            Assert.False(testDeploymentMetaInfo2.IsValid());
            Assert.False(testDeploymentMetaInfo3.IsValid());
            Assert.False(testDeploymentMetaInfo4.IsValid());
        }
    }
}