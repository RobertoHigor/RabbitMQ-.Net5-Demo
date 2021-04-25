using System;
using System.Text;
using Payments.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Payments.RabbitMQ
{
    public class RabbitMQDirectClient
    {
        private IConnection _connection;
        private IModel _channel;
        private string _replyQueueName;
        private EventingBasicConsumer _consumer;

        public void CreateConnection()
        {
            var factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _replyQueueName = _channel.QueueDeclare("rpc_reply", true, false, false, null);           

            _consumer = new EventingBasicConsumer(_channel);
            _channel.BasicConsume(_replyQueueName, true, _consumer);
        }

        public void Close()
        {
            _connection.Close();
        }

        public string MakePayment(CardPayment payment)
        {
            var corrId = Guid.NewGuid().ToString();
            var props = _channel.CreateBasicProperties();
            props.ReplyTo = _replyQueueName;
            props.CorrelationId = corrId;

            _channel.BasicPublish("", "rpc_queue", props, payment.Serialize()); 
            string authCode = "";

            _consumer.Received += (model, ea) => // suporta async
            {
                if (ea.BasicProperties.CorrelationId != corrId) 
                    Console.WriteLine($"If de corrid: {corrId}");              
                var body = ea.Body.ToArray();
                authCode = Encoding.UTF8.GetString(body);                
            };  

            return authCode;
        }
    }
}
