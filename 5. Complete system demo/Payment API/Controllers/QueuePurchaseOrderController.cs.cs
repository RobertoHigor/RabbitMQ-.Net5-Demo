using System;
using Microsoft.AspNetCore.Mvc;
using Payments.Models;
using Payments.RabbitMQ;

namespace Payment_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QueuePurchaseOrderController : ControllerBase
    {
       
        [HttpPost]
        public IActionResult MakePayment([FromBody] PurchaseOrder purchaseOrder)
        {  
            try
            {
                RabbitMQClient client = new();
                client.SendPurchaseOrder(purchaseOrder);
                client.Close();
            }
            catch (Exception)
            {                
                return BadRequest();
            }

            return Ok(purchaseOrder);
        }
    }
}
