//------------------------------------------------------------------------------------------------- 
// <copyright file="ClassMethod.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Person type.</summary>
//-------------------------------------------------------------------------------------------------

namespace Allors.Domain
{
    public partial class C1
    {
        public void CoreClassMethod(C1ClassMethod method)
        {
            method.Value += "C1Core";
        }
        public void CustomClassMethod(C1ClassMethod method)
        {
            method.Value += "C1Custom";
        }
    }

    public partial class C1ClassMethod
    {
        public string Value { get; set; }
    }
}
