// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.11.1

using Bot.Models;
using Bot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Bots
{
	public class EchoBot : ActivityHandler
	{
		private readonly StateService stateService;

		public EchoBot(StateService stateService)
		{
			this.stateService = stateService ?? throw new ArgumentNullException($"{nameof(stateService)} in Greeting Bot Class");
		}

		private async Task GetName(ITurnContext turnContext, CancellationToken cancellationToken)
		{
			UserProfile userProfile = await stateService.UserProfileAccessor.GetAsync(turnContext, () => new UserProfile());
			ConversationData conversationData = await stateService.ConversationDataAccessor.GetAsync(turnContext, () => new ConversationData());

			if (!string.IsNullOrEmpty(userProfile.Name))
			{
				await turnContext.SendActivityAsync(MessageFactory.Text(String.Format($"Hi {userProfile.Name}. How can I Help you today?")), cancellationToken);
			}

			else
			{
				if (conversationData.PromptedUserForName)
				{
					// Set the name to what the user provided
					userProfile.Name = turnContext.Activity.Text?.Trim();

					// Acknowledge that we got their name
					await turnContext.SendActivityAsync(MessageFactory.Text(String.Format($"Thanks {userProfile.Name}")), cancellationToken);

					// Reset the flag to allow the bot to go through the cycle again
					conversationData.PromptedUserForName = false;
				}

				else
				{
					// Prompt the user for their name
					await turnContext.SendActivityAsync(MessageFactory.Text(String.Format("What is your name ?")), cancellationToken);

					// Set the flag to true, to don't prompt in the next trun
					conversationData.PromptedUserForName = true;
				}

				// Save any state changes that might have occured during the turn
				await stateService.UserProfileAccessor.SetAsync(turnContext, userProfile);
				await stateService.ConversationDataAccessor.SetAsync(turnContext, conversationData);

				await stateService.UserState.SaveChangesAsync(turnContext);
				await stateService.ConversationState.SaveChangesAsync(turnContext);
			}
		}

		protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
		{
			await GetName(turnContext, cancellationToken);
		}

		protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
		{
			foreach (var member in membersAdded)
			{
				if (member.Id != turnContext.Activity.Recipient.Id)
				{
					await GetName(turnContext, cancellationToken);
				}
			}
		}
	}
}
