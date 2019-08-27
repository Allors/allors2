// <copyright file="LoginForm.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace BaseExcelAddIn.Base
{
    using System;
    using System.Windows.Forms;
    using Allors.Workspace.Client;
    using Nito.AsyncEx;

    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            this.InitializeComponent();

            this.button1.Enabled = false;
        }

        public Uri Uri { get; set; }

        public ClientDatabase Database { get; set; }

        public bool IsLoggedIn { get; set; }

        public string UserName { get; set; }

        private async void Button1_Click(object sender, EventArgs e)
        {
            this.HideError();

            if (string.IsNullOrEmpty(this.textBoxUser.Text))
            {
                this.ShowError("Enter Username.");
            }

            if (string.IsNullOrEmpty(this.textBoxPassword.Text))
            {
                this.ShowError("Enter Password.");
            }

            AsyncContext.Run(
                async () =>
                {
                    this.IsLoggedIn = await this.Database.Login(this.Uri, this.textBoxUser.Text, this.textBoxPassword.Text);
                });

            if (this.IsLoggedIn)
            {
                this.UserName = this.textBoxUser.Text;
                // Close the dialog
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                this.ShowError("Login failed.");
            }
        }

        private void ShowError(string message)
        {
            this.labelErrorMessage.Text = message;
            this.labelErrorMessage.Visible = true;
        }

        private void HideError()
        {
            this.UserName = null;
            this.labelErrorMessage.Text = "";
            this.labelErrorMessage.Visible = false;
        }

        private void TextBoxUser_TextChanged(object sender, EventArgs e) => this.button1.Enabled = this.textBoxUser.Text.Length > 0 && this.textBoxPassword.Text.Length > 0;

        private void TextBoxPassword_TextChanged(object sender, EventArgs e) => this.button1.Enabled = this.textBoxUser.Text.Length > 0 && this.textBoxPassword.Text.Length > 0;

        private void ButtonTogglePassword_Click(object sender, EventArgs e)
        {
            if (this.textBoxPassword.PasswordChar != '\0')
            {
                this.textBoxPassword.PasswordChar = '\0';
            }
            else
            {
                this.textBoxPassword.PasswordChar = '*';
            }
        }
    }
}
