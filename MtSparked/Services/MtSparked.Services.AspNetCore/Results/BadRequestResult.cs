using Microsoft.AspNetCore.Mvc;

namespace MtSparked.Services.AspNetCore.Results {
    public class BadRequestResult<T> : BadRequestResult, IActionResult<T> {

    }
}
