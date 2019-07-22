using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MtSparked.Interop.Models;
using MtSparked.Services.AspNetCore.Results;

namespace MtSparked.Services.AspNetCore.Controllers {
    [Route("cards")]
    [ApiController]
    public class CardsController : ControllerBase {

        [HttpGet]
        // TODO: Implement CardsController.ListCards
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public IActionResult<IList<Card>> ListCards([FromQuery]string query) {
            if (query is null) {
                return new BadRequestResult<IList<Card>>();
            }
            //DataStore<Card>.IQuery.FromString(query).ToDataStore().Items.ToArray().FirstOrDefault();
            return null;
        }

    }
}