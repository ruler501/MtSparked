using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MtSparked.Interop.Models;
using MtSparked.Interop.Databases;

namespace MtSparked.Platforms.AspNetCore.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class CardsController : ControllerBase {
        [HttpGet]
        // TODO: Implement CardsController.ListCards
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public IList<Card> ListCards([FromQuery]string query) =>
            //DataStore<Card>.IQuery.FromString(query).ToDataStore().Items.ToArray().FirstOrDefault();
            null;
    }
}