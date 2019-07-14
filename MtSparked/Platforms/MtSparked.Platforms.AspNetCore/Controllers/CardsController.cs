using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MtSparked.Core.Services;
using MtSparked.Interop.Models;

namespace MtSparked.Platforms.AspNetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardsController : ControllerBase
    {
        [HttpGet]
        public IList<Card> ListCards([FromQuery]string query) {
            return CardDataStore.CardsQuery.FromString(query).ToList();
        }
    }
}