using Microsoft.AspNetCore.Mvc;
using Payments.Models;
using Payments.RabbitMQ;

namespace Payment_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QueueCardPaymentController : ControllerBase
    {
       
        [HttpGet]
        public IActionResult MakePayment([FromBody] CardPayment payment)
        {     
            try
            {
                RabbitMQClient client = new RabbitMQClient();
                client.SendPayment(payment);
                client.Close();
            }
            catch (System.Exception)
            {                
                return BadRequest();
            }

            return Ok(payment);
        }
    }
}
