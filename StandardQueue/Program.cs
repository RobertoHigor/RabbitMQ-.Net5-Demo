using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQ.Examples
{
    class Program
    {
        private static ConnectionFactory _factory;
        private static IConnection _connection;
        private static IModel _model;

        private const string QueueName = "StandardQueue_ExampleQueue";

        public static void Main()
        {
            var payment1 = new Payment { AmountToPay = 25.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };
            var payment2 = new Payment { AmountToPay = 5.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };
            var payment3 = new Payment { AmountToPay = 2.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };
            var payment4 = new Payment { AmountToPay = 17.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };
            var payment5 = new Payment { AmountToPay = 300.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };
            var payment6 = new Payment { AmountToPay = 350.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };
            var payment7 = new Payment { AmountToPay = 295.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };
            var payment8 = new Payment { AmountToPay = 5625.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };
            var payment9 = new Payment { AmountToPay = 5.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };
            var payment10 = new Payment { AmountToPay = 12.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };

            CreateQueue();

            SendMessage(payment1);
            SendMessage(payment2);
            SendMessage(payment3);
            SendMessage(payment4);
            SendMessage(payment5);
            SendMessage(payment6);
            SendMessage(payment7);
            SendMessage(payment8);
            SendMessage(payment9);
            SendMessage(payment10);

            Recieve();

            Console.ReadLine();
        }

        // Inicializa a queue
        private static void CreateQueue()
        {
            _factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" };
            _connection = _factory.CreateConnection();
            _model = _connection.CreateModel();            
            _model.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        private static void SendMessage(Payment message)
        {
            // "" envia para o default exchange 
            _model.BasicPublish("", QueueName, null, message.Serialize()); // Envia objeto como bytearray
            Console.WriteLine(" [x] Payment Message Sent : {0} : {1} : {2}", message.CardNumber, message.AmountToPay, message.Name);
        }

        public static void Recieve()
        {
            // https://stackoverflow.com/questions/38746900/queueingbasicconsumer-is-deprecated-which-consumer-is-better-to-implement-rabbi
            var consumer = new EventingBasicConsumer(_model);
            var msgCount = GetMessageCount(_model, QueueName);         

            // Recebe mensagem como objeto do tipo                
            //var message = (Payment)consumer.Model.Queue.Dequeue().Body.DeSerialize(typeof(Payment));
            var utf8 = Encoding.UTF8;
            Payment message = new Payment();              

            // Dizendo para o servidor que queremos menasgens da fila
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();

                // Valor local, ver como tirar de arrow function
                message = (Payment)body.DeSerialize(typeof(Payment));
                Console.WriteLine(" [x] Done");
                Console.WriteLine($"----- Received {message.CardNumber} : {message.AmountToPay} : {message.Name}");
            };       

            // Registra o consumer para escutar uma fila especifica.
            _model.BasicConsume(queue: QueueName, autoAck: true, consumer: consumer);       
        }

        private static uint GetMessageCount(IModel channel, string queueName)
        {
            var results = channel.QueueDeclare(queueName, true, false, false, null);
            return results.MessageCount;
        }
    }
}
