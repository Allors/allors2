// <copyright file="Reference.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All Rights Reserved.
// Licensed under the LGPL v3 license.
// </copyright>

using System;

namespace Autotest.Angular
{
    using System.Linq;
    using Newtonsoft.Json.Linq;

    public partial class Reference
    {
        public Reference(JToken json)
        {
            this.Name = json["name"]?.Value<string>();
            this.Path = json["path"]?.Value<string>();

            this.Id = $"{this.Name} | {this.Path}";
        }

        public string Id { get; }

        public bool IsLocal => !this.Path.Contains("node_modules/");

        public string Name { get; }

        public string Namespace
        {
            get
            {
                if (this.Path != null)
                {
                    var unescapedNamespace = this.Path.Substring(0, this.Path.LastIndexOf("/")).Replace("/", ".");
                    var escapedNamespace = string.Join(".", unescapedNamespace.Split('.').Select(v => v.EscapeReservedKeyword()));
                    return escapedNamespace;
                }

                return string.Empty;
            }
        }

        public string Path { get; }

        internal static string ParseId(JToken json)
        {
            return json != null ? new Reference(json).Id : null;
        }

        internal static string[] ParseIds(JToken json)
        {
            return json != null ? json.Select(v => new Reference(v).Id).ToArray() : new string[0];
        }
    }
}