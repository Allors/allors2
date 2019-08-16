﻿// <copyright file="EnableAuthenticatorViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Identity.Models.ManageViewModels
{
    public class EnableAuthenticatorViewModel
    {
        [Required]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Verification Code")]
        public string Code { get; set; }

        [ReadOnly(true)]
        public string SharedKey { get; set; }

        public string AuthenticatorUri { get; set; }
    }
}
