// <copyright file="LoginComponent.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace libs.angular.material.custom.src.auth
{
    using Components;
    using libs.angular.material.custom.src.dashboard;

    public partial class LoginComponent
    {
        public DashboardComponent Login(string userName = "administrator")
        {
            this.Username.Value = userName;
            this.SignIn.Click();

            this.Driver.WaitForAngular();

            return new DashboardComponent(this.Driver);
        }
    }
}
