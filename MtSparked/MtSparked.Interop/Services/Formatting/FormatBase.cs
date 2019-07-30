using System.Collections.Generic;
using MtSparked.Interop.Utils;

namespace MtSparked.Interop.Services.Formatting {
    public abstract class FormatBase : IFormat {

        protected FormatBase(string name, string description, IDictionary<string, object> formatOptions, string version) {
            this.Name = name;
            this.Description = description;
            this.FormatOptions = formatOptions;
            this.Version = version;
        }

        public string Name { get; }
        public string Description { get; }
        public IDictionary<string, object> FormatOptions { get; }
        public string Version { get; }

    }

    public class ReversedFormat<Format1, T, FormatResult> : FormatBase, IFormat<FormatResult, T>
            where Format1 : FormatBase<T, FormatResult> {

        internal Format1 ReversedInstance { get; }

        internal ReversedFormat(string name, string description, IDictionary<string, object> formatOptions, string version)
                : base(name, description, formatOptions, version) {
            this.ReversedInstance = this as Format1;
        }

        public T Format(FormatResult model) => this.ReversedInstance.Parse(model);
        public FormatResult Parse(T formattedModel) => this.ReversedInstance.Format(formattedModel);
        // We coudl implement conversion operators but since this has all Formats reverse themselves there's
        // no need for conversions or alternate ways to construct

    }

    public abstract class FormatBase<T, FormatResult> : ReversedFormat<FormatBase<T, FormatResult>, T, FormatResult>, IFormat<T, FormatResult> {

        public FormatBase(string name, string description, IDictionary<string, object> formatOptions, string version)
                : base(name, description, formatOptions, version) { }

        public abstract FormatResult Format(T model);
        public abstract T Parse(FormatResult formattedModel);
    }

    public abstract class FormatBase<T, FormatResult, FormattedResult>
            : FormatBase<T, FormattedResult>, IFormat<T, FormatResult, FormattedResult>
            where FormattedResult : Formatted<FormatResult> {
        public FormatBase(string name, string description, IDictionary<string, object> formatOptions, string version)
                : base(name, description, formatOptions, version) { }
    }

    public sealed class AnyFormat<T> : FormatBase, IOutFormat<T, Formatted<T>.Any> {

        public AnyFormat()
                : base(typeof(T).Name + " to Any<" + typeof(T).Name + ">", "Add Any qualification",
                       new Dictionary<string, object>(), "v1.0") { }

        // Implicit conversions on Any make this work.
        public Formatted<T>.Any Format(T model) => model;
    }

    public sealed class AnyFromMarkedFormat<T, Marker> : FormatBase, IOutFormat<Formatted<T>.For<Marker>, Formatted<T>.Any> {

        public AnyFromMarkedFormat()
                : base("For<" + typeof(T).Name + ", " + typeof(Marker).Name + "> to Any<" + typeof(T).Name + ">", "Add Any qualification",
                       new Dictionary<string, object>(), "v1.0") { }

        // Implicit conversions on Any and For make this work.
        public Formatted<T>.Any Format(Formatted<T>.For<Marker> model) => model;

    }

    public class ChainFormatOut<T, Mid, FormatResult>
            : FormatBase, IOutFormat<T, FormatResult> {

        private IOutFormat<T, Mid> FormatInstance1 { get; }
        private IOutFormat<Mid, FormatResult> FormatInstance2 { get; }

        public ChainFormatOut(IOutFormat<T, Mid> format1, IOutFormat<Mid, FormatResult> format2)
                : base(format1.Name + " / " + format2.Name, format1.Description + "\n/\n" + format2.Description,
                       GenericExtensions.CombineDicts(format1.FormatOptions, format2.FormatOptions),
                       format1.Version + "/" + format2.Version) {
            this.FormatInstance1 = format1;
            this.FormatInstance2 = format2;
        }

        public FormatResult Format(T model) => this.FormatInstance2.Format(this.FormatInstance1.Format(model));
    }


    public class ChainFormatOut<T, Mid, FormattedMid, FormatResult, FormattedResult>
            : ChainFormatOut<T, FormattedMid, FormattedResult>,
              IOutFormat<T, FormatResult, FormattedResult>
            where FormattedMid : Formatted<Mid>
            where FormattedResult : Formatted<FormatResult> {

        public ChainFormatOut(IOutFormat<T, Mid, FormattedMid> format1, IOutFormat<FormattedMid, FormatResult, FormattedResult> format2)
               : base(format1, format2) { }


        // Doesn't compile because ChainFormat is not one of the arguments.
        // Putting it on FormatBase doesn't work eitheer because it only has the type info for Format1(or Format2)
        // but you need both and you can't have generics on types. Maybe a subclass with implicit conversions could work?
        // public static ChainFormat<Format1, T, Mid, FormattedMid, Format2, FormatResult, FormattedResult> operator|(Format1 format1, Format2 format2)
        //    => new ChainFormat<Format1, T, Mid, FormattedMid, Format2, FormatResult, FormattedResult>(format1, format2);
    }
}
