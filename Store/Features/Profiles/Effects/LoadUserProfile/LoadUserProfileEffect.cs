﻿using BlazorConduit.Models.Common;
using BlazorConduit.Models.Profile;
using BlazorConduit.Services;
using BlazorConduit.Store.Features.Profiles.Actions.LoadUserProfile;
using BlazorConduit.Store.State;
using Fluxor;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace BlazorConduit.Store.Features.Profiles.Effects.LoadUserProfile
{
    public class LoadUserProfileEffect : Effect<LoadUserProfileAction>
    {
        private readonly ConduitApiService _apiService;
        private readonly ILogger<LoadUserProfileEffect> _logger;
        private readonly IState<ProfileState> _state;

        public LoadUserProfileEffect(ConduitApiService apiService, ILogger<LoadUserProfileEffect> logger, IState<ProfileState> state) =>
            (_apiService, _logger, _state) = (apiService, logger, state);

        protected override async Task HandleAsync(LoadUserProfileAction action, IDispatcher dispatcher)
        {
            try
            {
                // Call the profile user endpoint with the username
                var profileResponse = await _apiService.GetAsync<UserProfileResponse>($"profiles/{action.Username}");

                if (profileResponse is null || profileResponse.Profile is null)
                {
                    // Throw the exception to issue the failure action
                    throw new ConduitApiException($"Could not retrieve user profile {action.Username}", HttpStatusCode.NotFound);
                }

                dispatcher.Dispatch(new LoadUserProfileSuccessAction(profileResponse.Profile));
            }
            catch (ConduitApiException e)
            {
                _logger.LogError($"Validation error during profile retrieval for user with email {action.Username}");
                dispatcher.Dispatch(new LoadUserProfileFailureAction(e.Message, e.ApiErrors));
            }
            catch (Exception e)
            {
                _logger.LogError($"Error retrieving profile for user {action.Username}");
                dispatcher.Dispatch(new LoadUserProfileFailureAction(e.Message));
            }
        }
    }
}
