using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using FluentAssertions;
using ReleaseServer.WebApi.Common;
using ReleaseServer.WebApi.Models;
using ReleaseServer.WebApi.Test.Utils;
using Xunit;

namespace ReleaseServer.WebApi.Test.Common
{
    public class JsonSerializeTest
    {
        private readonly string ProjectDirectory;
        private readonly ReleaseNotes ExpectedReleaseNotes;
        private readonly DeploymentMetaInfo ExpectedMeta;

        public JsonSerializeTest()
        {
            ProjectDirectory = TestUtils.GetProjectDirectory();
            ExpectedReleaseNotes = new ReleaseNotes
            {
                Changes = new Dictionary<CultureInfo, List<ChangeSet>>
                {
                    {
                        new CultureInfo("de"), new List<ChangeSet>
                        {
                            new ChangeSet
                            {
                                Platforms = new List<string> {"windows/any", "linux/rpi"},
                                Added = new List<string> {"added de 1", "added de 2"},
                                Fixed = new List<string> {"fix de 1", "fix de 2"},
                                Breaking = new List<string> {"breaking de 1", "breaking de 2"},
                                Deprecated = new List<string> {"deprecated de 1", "deprecated de 2"}
                            }
                        }
                    },
                    {
                        new CultureInfo("en"), new List<ChangeSet>
                        {
                            new ChangeSet
                            {
                                Platforms = new List<string> {"windows/any", "linux/rpi"},
                                Added = new List<string> {"added en 1", "added en 2"},
                                Fixed = new List<string> {"fix en 1", "fix en 2"},
                                Breaking = new List<string> {"breaking en 1", "breaking en 2"},
                                Deprecated = new List<string> {"deprecated en 1", "deprecated en 2"}
                            },
                            new ChangeSet
                            {
                                Platforms = null,
                                Added = new List<string> {"added en 3"},
                                Fixed = new List<string> {"fix en 3"},
                                Breaking = new List<string> {"breaking en 3"},
                                Deprecated = new List<string> {"deprecated en 3"}
                            }
                        }
                    }
                }
            };
            ExpectedMeta =  new DeploymentMetaInfo
            {
                ReleaseNotesFileName = "releaseNotes.json",
                ArtifactFileName = "artifact.zip",
                ReleaseDate = new DateTime(2020, 02, 01)
            };
        }
        
        
        [Fact]
        public void DeserializeReleaseNotes()
        {
            var parsedReleaseNotes = ReleaseNotes.FromJsonFile(Path.Combine(ProjectDirectory, "TestData", "testReleaseNotes.json"));

            parsedReleaseNotes.Should().BeEquivalentTo(ExpectedReleaseNotes);
        }

        [Fact]
        public void DeserializeDeploymentMetaInfo_Success()
        {
            var parsedMeta = DeploymentMetaInfo.FromJsonFile(Path.Combine(ProjectDirectory, "TestData", "testDeployment.json"));
            
            parsedMeta.Should().BeEquivalentTo(ExpectedMeta);
        }
        
        [Fact]
        public void DeserializeDeploymentMetaInfo_Error_No_ReleaseDate()
        {
            var pathToInvalidMeta = Path.Combine(ProjectDirectory, "TestData", "validateUploadPayload",
                "invalid_meta_format", "no_release_date_deployment.json");
            
            var exception = Assert.Throws<Newtonsoft.Json.JsonSerializationException>(() => 
                DeploymentMetaInfo.FromJsonFile(pathToInvalidMeta));

            Assert.Equal("Required property 'ReleaseDate' not found in JSON. Path '', line 4, position 1.",
                exception.Message);
        }
        
        [Fact]
        public void DeserializeDeploymentMetaInfo_Error_Empty_ReleaseDate()
        {
            var pathToMetaWithEmptyDate = Path.Combine(ProjectDirectory, "TestData", "validateUploadPayload",
                "invalid_meta_format", "empty_release_date_deployment.json");
            
            var exception = Assert.Throws<Newtonsoft.Json.JsonSerializationException>(() => 
                DeploymentMetaInfo.FromJsonFile(pathToMetaWithEmptyDate));
            
            Assert.Equal("Error converting value {null} to type 'System.DateTime'. Path 'ReleaseDate', line 4, position 19.",
                exception.Message);
        }
    }
}