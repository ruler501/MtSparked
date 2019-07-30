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

    public interface IOutFormat<T, FormatResult, FormattedResult>
            : IOutFormat<T, FormattedResult>
            where FormattedResult : Formatted<FormatResult> { }

    public interface IInFormat<out T, in FormatResult> : IFormat {

        T Parse(FormatResult formattedModel);

    }

    public interface IInFormat<T, FormatResult, FormattedResult>
            : IInFormat<T, FormattedResult>
            where FormattedResult : Formatted<FormatResult> { }

    public interface IFormat<T, FormatResult>
            : IOutFormat<T, FormatResult>,
              IInFormat<T, FormatResult> { }

    public interface IFormat<T, FormatResult, FormattedResult>
            : IFormat<T, FormattedResult>,
              IOutFormat<T, FormatResult, FormattedResult>,
              IInFormat<T, FormatResult, FormattedResult>
            where FormattedResult : Formatted<FormatResult> { }

    // Helper classes so you don't have to reference Formatted directly.
    // Should not be used for reflection.
    public interface IFormatToAny<T, FormatResult>
            : IFormat<T, FormatResult, Formatted<FormatResult>.Any> { }

    public interface IFormatMarked<T, FormatResult, Marker>
            : IFormat<T, FormatResult, Formatted<FormatResult>.For<Marker>> { }
}
