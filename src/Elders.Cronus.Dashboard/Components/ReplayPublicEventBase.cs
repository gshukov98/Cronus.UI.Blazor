﻿using Elders.Cronus.Dashboard.Models;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Elders.Cronus.Dashboard.Components
{
    public class ReplayPublicEventBase : ComponentBase
    {
        [Inject]
        protected AppState App { get; set; }

        [Inject]
        protected TokenClient Token { get; set; }

        [Parameter]
        public bool? HasValidToken { get; set; }

        protected ReplayPublicEventValidator validationModel = new ReplayPublicEventValidator();

        protected override async Task OnInitializedAsync()
        {
            App.OnTenantChanged += OnTenantChange;
            HasValidToken = await IsTokenValid();
        }

        protected async Task OnTenantChange(oAuth oAuth)
        {
            HasValidToken = await IsTokenValid();
            StateHasChanged();
        }

        protected async Task Success()
        {

        }

        protected void Reset()
        {
            validationModel.Tenant = null;
            validationModel.RecipientBoundedContext = null;
            validationModel.RecipientHandlers = null;
            validationModel.SourceEventTypeId = null;
        }

        private async Task<bool> IsTokenValid()
        {
            string result = await Token.GetAccessTokenAsync(App.Connection);
            if (string.IsNullOrEmpty(result))
                return false;

            return true;
        }

        protected class ReplayPublicEventValidator
        {
            [Required]
            public string Tenant { get; set; }

            [Required]
            public string RecipientBoundedContext { get; set; }

            [Required]
            public string RecipientHandlers { get; set; }

            [Required]
            public string SourceEventTypeId { get; set; }
        }
    }
}
