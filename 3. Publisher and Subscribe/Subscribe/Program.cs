using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQ.Examples
{
    class Program
    {
        private static ConnectionFactory _factory;
        //private static IConnection _connection;
        private static EventingBasicConsumer _consumer;

        private const string ExchangeName = "PublishSubscribe_Exchange";

        static void Main()
        {
            _factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" };
            using IConnection _connection = _factory.CreateConnection();
            using var channel = _connection.CreateModel();

            try
            {
                var queueName = DeclareAndBindQueueToExchange(channel);
                channel.BasicConsume(queueName, true, _consumer);

                _consumer.Received += (model, ea) => // suporta async
                {
                    var body = ea.Body.ToArray();
                    var message = (Payment)body.DeSerialize(typeof(Payment));
                    Console.WriteLine($"----- Payment Processed {message.CardNumber} : {message.AmountToPay}");
                };
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
                throw;
            }

            Console.ReadLine();
        }

        private static string DeclareAndBindQueueToExchange(IModel channel)
        {
            channel.ExchangeDeclare(ExchangeName, "fanout");
            // Cada instância irá ter uma fila gerada
            // Padrão de nome é amq.gen-xxxxxx
            var queueName = channel.QueueDeclare().QueueName;
            // Consumidor criou a própria fila e se inscreveu no exchange.
            channel.QueueBind(queueName, ExchangeName, "");
            _consumer = new EventingBasicConsumer(channel);

            return queueName;
        }
    }
}
