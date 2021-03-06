using System.IO;
using System.IO.Compression;
using FluentAssertions;
using ReleaseServer.WebApi.Mappers;
using ReleaseServer.WebApi.Models;
using ReleaseServer.WebApi.Test.Utils;
using Xunit;

namespace ReleaseServer.WebApi.Test.Mappers
{
    public class ReleaseArtifactMapperTest
    {
        private readonly byte[] testFile;

        public ReleaseArtifactMapperTest()
        {
            var projectDirectory = TestUtils.GetProjectDirectory();
            testFile = File.ReadAllBytes(Path.Combine(projectDirectory, "TestData", "test_zip.zip"));
        }
        
        [Fact]
        public void ConvertToReleaseArtifactTest()
        {
            //Setup
            using var stream = new MemoryStream(testFile);
            var testZip = new ZipArchive(stream);
            
            var expectedArtifact = new ReleaseArtifact
            {
                ProductInformation = new ProductInformation
                {
                    Identifier = "product",
                    Version = new ProductVersion("1.1"),
                    Os = "ubuntu",
                    Architecture = "amd64"
                },
                Payload = testZip
            };
            
            var testArtifact = ReleaseArtifactMapper.ConvertToReleaseArtifact("product",  "ubuntu",
                "amd64", "1.1", testZip);
            
            testArtifact.Should().BeEquivalentTo(expectedArtifact);
        }
    }
}