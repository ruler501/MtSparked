using System;
using System.Collections.Generic;
using System.Text;
using MtSparked.Interop.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MtSparked.Interop.Services.Formatting {
    public interface IFormat {

        string Name { get; }

        string Description { get; }

        IDictionary<string, object> FormatOptions { get; }

        string Version { get; }

    }

    public interface IOutFormat<in T, out FormatResult> : IFormat {

        FormatResult Format(T model);

    }

    public interface IInFormat<out T, in FormatResult> : IFormat {

        T Parse(FormatResult formattedModel);

    }


    public interface IFormat<T, FormatResult>
            : IOutFormat<T, FormatResult>,
              IInFormat<T, FormatResult> { }

    public interface IFormat<T, FormatResult, FormattedResult>
            : IFormat<T, FormattedResult>
            where FormattedResult : Formatted<FormatResult> { }

    public interface IFormatToAny<T, FormatResult>
            : IFormat<T, FormatResult, Formatted<FormatResult>.Any> { }

    public interface IFormatMarked<T, FormatResult, Marker>
            : IFormat<T, FormatResult, Formatted<FormatResult>.For<Marker>> { }

    public interface IFormatted<out FormatResult> {

        FormatResult Value { get; }

    }

    public interface IFormatted : IFormatted<object> { }

    public abstract class Formatted<FormatResult> : IFormatted<FormatResult> {

        public FormatResult Value { get; }

        public static implicit operator FormatResult(Formatted<FormatResult> self)
            => self.Value;

        protected Formatted(FormatResult value) {
            this.Value = value;
        }

        public class Any : Formatted<FormatResult> {

            public Any(FormatResult value) : base(value) { }

            public static implicit operator Any(FormatResult value)
                => new Any(value);

        }

        public class For<Marker> : Formatted<FormatResult> {

            public For(FormatResult value) : base(value) { }

            public static implicit operator Any(For<Marker> self)
                => new Any(self.Value);
    
        }

    }

    public interface IJsonFormat {

        JsonWriter CurrentWriter { get; set; }

        JsonSerializer CurrentSerializer { get; set; }

    }

    public interface IJsonFormat<T>
            : IFormat<T, JToken>,
              IJsonFormat { }

    public interface IJsonFormat<T, FormattedResult>
            : IFormat<T, JToken, FormattedResult>,
              IJsonFormat
            where FormattedResult : Formatted<JToken> { }

    public interface IJsonFormatToAny<T>
            : IJsonFormat<T, Formatted<JToken>.Any> { }

    public interface IJsonFormatMarked<T, Marker>
            : IJsonFormat<T, Formatted<JToken>.For<Marker>> { }
}
