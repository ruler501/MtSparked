using Microsoft.AspNetCore.Mvc;

namespace MtSparked.Services.AspNetCore.Results {
    public class NotFoundResult<T> : NotFoundResult, IActionResult<T> {

    }
}
