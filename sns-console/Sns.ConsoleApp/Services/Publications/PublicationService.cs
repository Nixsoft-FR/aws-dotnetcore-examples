using System;
using System.Net;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Newtonsoft.Json;

namespace Sns.ConsoleApp.Services.Publications
{
    public sealed class PublicationService<T> : IPublicationService<T>
    {
        private readonly IAmazonSimpleNotificationService _sns;

        public PublicationService(IAmazonSimpleNotificationService sns)
        {
            _sns = sns ?? throw new ArgumentNullException(nameof(sns));
        }

        public Task<PublishConfirmation> PublishMessageAsync(string topicName, T message, string subject)
        {
            if (string.IsNullOrWhiteSpace(topicName))
                throw new ArgumentException($"A non-null/empty '{topicName}' is required.", nameof(topicName));

            if (message == null)
                throw new ArgumentNullException($"A non-null '{message}' is required.", nameof(message));

            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException($"A non-null/empty '{subject}' is required.", nameof(subject));

            return publishAsync();

            async Task<PublishConfirmation> publishAsync()
            {
                var topic = await _sns.FindTopicAsync(topicName);

                if (topic == null)
                    throw new ArgumentException($"The topic '{topicName}' does not exist.");

                var request = new PublishRequest
                {
                    Message = JsonConvert.SerializeObject(message),
                    Subject = subject,
                    TopicArn = topic.TopicArn
                };

                var response = await _sns.PublishAsync(request);

                if (response.HttpStatusCode != HttpStatusCode.OK)
                    throw new InvalidOperationException($"Publish failed for topic '{topicName}'");

                return new PublishConfirmation
                {
                    MessageId = response.MessageId
                };
            }
        }
    }
}