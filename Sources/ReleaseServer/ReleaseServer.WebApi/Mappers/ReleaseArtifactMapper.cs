using System;
using System.IO.Compression;
using Microsoft.AspNetCore.Http;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Mappers
{
    public static class ReleaseArtifactMapper
    {
        public static ReleaseArtifact ConvertToReleaseArtifact(string product, string os, string architecture,
            string version, ZipArchive payload)
        {
            return new ReleaseArtifact
            {
                ProductInformation = new ProductInformation
                {
                    Identifier = product,
                    Os = os,
                    Architecture = architecture,
                    Version = new ProductVersion(version)
                },
                Payload = payload
            };
        }
    }
}