using System;
using System.Collections.Generic;
using System.Globalization;
using ReleaseServer.WebApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace ReleaseServer.WebApi.SwaggerDocu
{
    public class ReleaseInformationResponseExample : IExamplesProvider<ReleaseInformation>
    {
        public ReleaseInformation GetExamples()
        {
            return new ReleaseInformation()
            {
                ReleaseNotes = new ReleaseNotes
                { Changes = new Dictionary<CultureInfo, List<ChangeSet>>
                    {
                        {new CultureInfo("de"), new List<ChangeSet>
                            {
                                new ChangeSet
                                {
                                    Platforms = new List<string>{"windows/any", "linux/rpi"},
                                    Added = new List<string>{"added de 1", "added de 2"},
                                    Fixed = new List<string>{"fix de 1", "fix de 2"},
                                    Breaking = new List<string>{"breaking de 1", "breaking de 2"},
                                    Deprecated = new List<string>{"deprecated de 1", "deprecated de 2"}
                                }
                            }
                        },
                        {new CultureInfo("en"), new List<ChangeSet>
                            {
                                new ChangeSet
                                {
                                    Platforms = new List<string>{"windows/any", "linux/rpi"},
                                    Added = new List<string>{"added en 1", "added en 2"},
                                    Fixed = new List<string>{"fix en 1", "fix en 2"},
                                    Breaking = new List<string>{"breaking en 1", "breaking en 2"},
                                    Deprecated = new List<string>{"deprecated en 1", "deprecated en 2"}
                                },
                                new ChangeSet
                                {
                                    Platforms = null,
                                    Added = new List<string>{"added en 3"},
                                    Fixed = new List<string>{"fix en 3"},
                                    Breaking = new List<string>{"breaking en 3"},
                                    Deprecated = new List<string>{"deprecated en 3"}
                                }
                            }
                        }
                    }
                },
                ReleaseDate = new DateTime(2020, 02, 01)
            };
        }
    }
}