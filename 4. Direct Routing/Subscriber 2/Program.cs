using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQ.Examples
{
    class Program
    {
        private static ConnectionFactory _factory;
        //private static IConnection _connection;

        private const string ExchangeName = "DirectRouting_Exchange";
        private const string PurchaseOrderQueueName = "PurchaseOrderDirectRouting_Queue";

        static void Main()
        {

            try
            {// Try-catch após as conexões faz com que o consumidor continue buscando mensagens.
                _factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" };
                using IConnection _connection = _factory.CreateConnection();
                using var channel = _connection.CreateModel();

                // Com chaves vazias, o código fica dentro de um escopo
                // Nesse caso, ele continua esperando novas mensagens.
                {
                    channel.ExchangeDeclare(ExchangeName, "direct");
                    channel.QueueDeclare(PurchaseOrderQueueName, true, false, false, null);
                    channel.QueueBind(PurchaseOrderQueueName, ExchangeName, "PurchaseOrder");
                    channel.BasicQos(0, 1, false);

                    var consumer = new EventingBasicConsumer(channel);
                    channel.BasicConsume(PurchaseOrderQueueName, false, consumer);

                    consumer.Received += (model, ea) => // suporta async
                    {
                        var body = ea.Body.ToArray();
                        var message = (PurchaseOrder)body.DeSerialize(typeof(PurchaseOrder));
                        var routingKey = ea.RoutingKey;
                        channel.BasicAck(ea.DeliveryTag, false);
                        Console.WriteLine($"----- Payment - Routing Key<{routingKey}> :{message.CompanyName} : ${message.AmountToPay} : {message.PaymentDayTerms} : {message.PoNumber}");
                    };
                }
            }
            catch (System.Exception)
            {

                throw;
            }
            Console.ReadLine();
        }
    }
}
