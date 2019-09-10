// <copyright file="Objects.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Adapters.Schema
{
    using System.Xml.Serialization;

    public partial class Objects
    {
        [XmlArray("database")]
        [XmlArrayItem("ot")]
        public ObjectType[] ObjectTypes { get; set; }
    }
}