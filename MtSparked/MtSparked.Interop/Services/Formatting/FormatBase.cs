using System.Collections.Generic;
using MtSparked.Interop.Utils;

namespace MtSparked.Interop.Services.Formatting {
    public abstract class IFormatBase : IFormat {

        protected IFormatBase(string name, string description, IDictionary<string, object> formatOptions, string version) {
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

    public class ReversedFormat<Format1, T, FormatResult> : IFormatBase, IFormat<FormatResult, T>
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

    public sealed class AnyFormat<T> : FormatBase<Formatted<T>.Any, T> {

        public AnyFormat()
                : base("Any<" + typeof(T).Name + "> to " + typeof(T).Name, "Add or remove Any qualification",
                       new Dictionary<string, object>(), "v1.0") { }

        // Implicit conversions on Any make this work.
        public override T Format(Formatted<T>.Any model) => model;
        public override Formatted<T>.Any Parse(T formattedModel) => formattedModel;

    }

    public abstract class ChainFormatPre<Format1, T, Mid, FormattedMid, Format2, FormatResult, FormattedResult>
              : FormatBase<T, Mid, FormattedMid>,
                IFormat<FormattedMid, FormatResult, FormattedResult>
            where FormattedMid : Formatted<Mid>
            where FormattedResult : Formatted<FormatResult>
            where Format1 : IFormat<T, Mid, FormattedMid>
            where Format2 : IFormat<FormattedMid, FormatResult, FormattedResult> {

        internal ChainFormatPre(Format1 format1, Format2 format2)
                : base(format1.Name + " / " + format2.Name, format1.Description + "\n/\n" + format2.Description,
                       GenericExtensions.CombineDicts(format1.FormatOptions, format2.FormatOptions),
                       format1.Version + "/" + format2.Version) {
            this.Format1Instance = format1;
            this.Format2Instance = format2;
        }

        public Format1 Format1Instance { get; }
        public Format2 Format2Instance { get; }

        // It has the same argument as the version from ReversedFormat.
        // Can still call the original by casting up to it.
        FormattedResult IOutFormat<FormattedMid, FormattedResult>.Format(FormattedMid model) => this.Format2Instance.Format(model);
        public FormattedMid Parse(FormattedResult formattedModel) => this.Format2Instance.Parse(formattedModel);
    }

    // We have to split ChainFormatPre off because otherwise it will fail to compile since it would
    // technically be possible that the parameters could be specified so that the IFormat interfaces
    // would be the same which is not allowed. Moving them to separate classes makes it work though.
    public sealed class ChainFormat<Format1, T, Mid, FormattedMid, Format2, FormatResult, FormattedResult>
              : ChainFormatPre<Format1, T, Mid, FormattedMid, Format2, FormatResult, FormattedResult>,
                IFormat<T, FormatResult, FormattedResult>
            where FormattedMid : Formatted<Mid>
            where FormattedResult : Formatted<FormatResult>
            where Format1 : IFormat<T, Mid, FormattedMid>
            where Format2 : IFormat<FormattedMid, FormatResult, FormattedResult> {

        public ChainFormat(Format1 format1, Format2 format2) : base(format1, format2) { }

        public override FormattedMid Format(T model) => this.Format1Instance.Format(model);
        public override T Parse(FormattedMid formattedModel) => this.Format1Instance.Parse(formattedModel);
        FormattedResult IOutFormat<T, FormattedResult>.Format(T model) => this.Format2Instance.Format(this.Format1Instance.Format(model));
        T IInFormat<T, FormattedResult>.Parse(FormattedResult formattedModel) => this.Format1Instance.Parse(this.Format2Instance.Parse(formattedModel));

        // Doesn't compile because ChainFormat is not one of the arguments.
        // Putting it on FormatBase doesn't work eitheer because it only has the type info for Format1(or Format2)
        // but you need both and you can't have generics on types. Maybe a subclass with implicit conversions could work?
        // public static ChainFormat<Format1, T, Mid, FormattedMid, Format2, FormatResult, FormattedResult> operator|(Format1 format1, Format2 format2)
        //    => new ChainFormat<Format1, T, Mid, FormattedMid, Format2, FormatResult, FormattedResult>(format1, format2);

    }
}
