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
        private const string CardPaymentQueueName = "CardPaymentDirectRouting_Queue";

        static void Main()
        {

            _factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" };
            using IConnection _connection = _factory.CreateConnection();
            using var channel = _connection.CreateModel();

            try
            {// Try-catch após as conexões faz com que o consumidor continue buscando mensagens.
                // Declarando exchange e a fila de pagamento
                channel.ExchangeDeclare(ExchangeName, "direct");
                channel.QueueDeclare(CardPaymentQueueName, true, false, false, null);
                channel.QueueBind(CardPaymentQueueName, ExchangeName, "CardPayment");

                channel.BasicQos(0, 1, false);

                var consumer = new EventingBasicConsumer(channel);

                System.Console.WriteLine("Recebendo mensagens");
                consumer.Received += (model, ea) => // suporta async
                {
                    var body = ea.Body.ToArray();
                    System.Console.WriteLine("DeSerializando a mensagem");
                    var message = (Payment)body.DeSerialize(typeof(Payment));
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine($"----- Payment - Routing Key<{routingKey}> :{message.CardNumber} : {message.AmountToPay}");
                    channel.BasicAck(ea.DeliveryTag, false);
                };

                channel.BasicConsume(CardPaymentQueueName, autoAck: false, consumer);
            }
            catch (System.Exception)
            {

                throw;
            }
            Console.ReadLine();
        }
    }
}
