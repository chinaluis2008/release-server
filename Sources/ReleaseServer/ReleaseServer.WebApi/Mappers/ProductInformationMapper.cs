using System;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Mappers
{
    public class ProductInformationMapper
    {
        public static ProductInformationModel PathToProductInfo(string rootPath, string path)
        {
         
            //Remove the rootPath
            var truncatedDir = path.Remove(0, rootPath.Length);
            truncatedDir = truncatedDir.TrimStart('/', '\\');
            
            var infos = truncatedDir.Split('/', '\\');

            //If the directory has a depth of 4 (actual our standard artifact)
            if (infos.Length == 4)
            {
                return new ProductInformationModel
                {
                    ProductIdentifier = infos[0],
                    Os = infos[1],
                    HwArchitecture = infos[2],
                    Version = new Version(infos[3])
                };
            }
            return null;
        }
    }
}

