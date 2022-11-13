using MediatR;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroRabbit.Infra.Bus
{
    public sealed class RabbitMQBus : IEventBus
    {
        private readonly IMediator _mediator;

        private readonly IServiceScopeFactory _serviceScopeFactory;

        private List<Type> EventsPool { get; }
        private Dictionary<string, List<Type>> EventSubscribersPool { get; }

        public RabbitMQBus(IMediator mediator, IServiceScopeFactory serviceScopeFactory)
        {
            _mediator = mediator;
            _serviceScopeFactory = serviceScopeFactory;

            EventsPool = new List<Type>();
            EventSubscribersPool = new Dictionary<string, List<Type>>();

        }
        
        #region "Public Event Capabilities"

        public Task SendCommand<T>(T command) where T : Command
        {
            return _mediator.Send(command);
        }

        /// <summary>
        /// Create new RabbitMQ queue
        /// Queue name = Event Type Name
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="eventToPublish"></param>
        public void Publish<TEvent>(TEvent eventToPublish) where TEvent : Event
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            var eventName = eventToPublish.GetType().Name;

            channel.QueueDeclare(eventName, false, false, false, null);

            var message = JsonConvert.SerializeObject(eventToPublish);
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish("", eventName, null, body);
        }

        /// <summary>
        /// Consume a specific event from the RabbitMQ
        /// Queue name = Event Type Name
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        /// <exception cref="ArgumentException"></exception>
        public void Subscribe<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>
        {
            var eventName = typeof(TEvent).Name;
            var newSubscriberType = typeof(TEventHandler);

            //Add New Event to the EventPool
            if (!EventsPool.Contains(typeof(TEvent)))
                EventsPool.Add(typeof(TEvent));

            //Add New Event to the EventHandlersPool with no Handlers
            if (!EventSubscribersPool.ContainsKey(eventName))
                EventSubscribersPool.Add(eventName, new List<Type>());

            else if (EventSubscribersPool[eventName].Any(subscriber => subscriber.GetType() == newSubscriberType))
                throw new ArgumentException(
                    $"Handler Type {newSubscriberType.Name} already is registered for '{eventName}'", newSubscriberType.Name);

            EventSubscribersPool[eventName].Add(newSubscriberType);

            //Consumer Messages from the RabbitMQs
            StartBasicConsume<TEvent>();
        }

        /// <summary>
        /// UnSubscribe from the handlers Pool of a specific Event's 
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        public void UnSubscribe<TEvent, TEventHandler>() where TEvent : Event where TEventHandler : IEventHandler<TEvent>
        {
            var eventName = typeof(TEvent).Name;
            var subscriberType = typeof(TEventHandler);

            if (EventSubscribersPool.ContainsKey(eventName))
                if (EventSubscribersPool[eventName].Any(subscriber => subscriber.GetType() == subscriberType))
                {
                    EventSubscribersPool[eventName].Remove(subscriberType);

                    if (EventSubscribersPool[eventName].Count == 0)
                    {
                        EventSubscribersPool.Remove(eventName);
                        EventsPool.Remove(typeof(TEvent));
                    }
                }
        }

        #endregion

        #region "Private helper methods"

        /// <summary>
        /// Consume a specific event from the RabbitMQ
        /// Queue name = Event Type Name
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        private void StartBasicConsume<TEvent>() where TEvent : Event
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                DispatchConsumersAsync = true
            };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            var eventName = typeof(TEvent).Name;

            channel.QueueDeclare(eventName, false, false, false, null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += Consumer_Received;

            channel.BasicConsume(eventName, true, consumer);
        }

        /// <summary>
        /// Queue Consuming Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        /// <returns></returns>
        private async Task Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
        {
            try
            {
                //Get the event name from the eventArgs
                var eventName = eventArgs.RoutingKey;
                var jsonMessage = Encoding.UTF8.GetString(eventArgs.Body.ToArray());

                await ProcessEvent(eventName, jsonMessage).ConfigureAwait(false);
            }
            catch (Exception)
            {
                //todo
            }
        }

        /// <summary>
        /// Announce Subscribers by the received Events
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="jsonMessage"></param>
        /// <returns></returns>
        private async Task ProcessEvent(string eventName, string jsonMessage)
        {
            //Check if Events Pool have this Event
            if (EventSubscribersPool.ContainsKey(eventName))
            {
                using var scope = _serviceScopeFactory.CreateScope();

                //Get Concrete Event type from the Event Pool
                var eventToProcess = EventsPool.SingleOrDefault(currentEvent => currentEvent.Name == eventName);

                //Get Subscribers Pool associated with this Event
                var subscribersPool = EventSubscribersPool[eventName];

                foreach (var currentSubscriber in subscribersPool)
                {
                    //Get a new instance of the Subscriber Type
                    var handler = scope.ServiceProvider.GetService(currentSubscriber);
                    if (handler == null)
                        continue;

                    var receivedEventDetails = JsonConvert.DeserializeObject(jsonMessage, eventToProcess!);

                    var concreteSubscriber = typeof(IEventHandler<>).MakeGenericType(eventToProcess);
                    await ((Task)concreteSubscriber.GetMethod("Handle")!.Invoke(handler, new[] { receivedEventDetails }))!;
                }
            }
        }

        #endregion
    }
}
