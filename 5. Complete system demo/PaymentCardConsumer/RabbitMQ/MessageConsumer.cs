using System;
using System.Text;
using RabbitMQ.Client;

namespace PaymentCardConsumer.RabbitMQ
{
    public class MessageConsumer : DefaultBasicConsumer
    {
        private readonly IModel _channel;
        public MessageConsumer(IModel channel)
        {
            this._channel = channel;
        }

        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, ReadOnlyMemory<byte> body)
        {
            var message = (CardPayment)body.ToArray().DeSerialize(typeof(CardPayment));      
             Console.WriteLine($"----- Payment - Routing Key<{routingKey}> : {message.Name} : {message.CardNumber} : {message.Amount}");
            _channel.BasicAck(deliveryTag, false);
        }
    }
}