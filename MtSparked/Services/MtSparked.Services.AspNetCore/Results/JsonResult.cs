using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MtSparked.Services.AspNetCore.Results {
    public class JsonResult<T> : JsonResult, IActionResult<T> {

        public JsonResult(T value)
                : base(value) {
        }

        public JsonResult(T value, JsonSerializerSettings serializerOptions)
                : base(value, serializerOptions) {
        }

    }
}
