using Microsoft.AspNetCore.Mvc;
using Payments.Models;
using Payments.RabbitMQ;

namespace Payment_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DirectCardPaymentController : ControllerBase
    {

        [HttpGet]
        public IActionResult MakePayment([FromBody] CardPayment payment)
        {
            string reply;

            try
            {
                RabbitMQDirectClient client = new();
                client.CreateConnection();
                reply = client.MakePayment(payment);

                client.Close();
            }
            catch (System.Exception)
            {
                return BadRequest();
            }

            return Ok(reply);
        }
    }
}
