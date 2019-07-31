using System.Collections.Generic;
using MtSparked.Interop.Utils;

namespace MtSparked.Interop.Services.Formatting {
    public abstract class FormatBase : IFormat {

        protected FormatBase(string name, string description,
                             IDictionary<string, object> formatOptions, string version) {
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

    public class ReversedFormat<T, FormatResult> : FormatBase, IFormat<FormatResult, T> { 

        private IFormat<T, FormatResult> ReversedInstance { get; }

        public ReversedFormat(IFormat<T, FormatResult> format)
                : base("Reversed: " + format.Name,
                       "Reversed of IFormat: " + format.Name,
                       new Dictionary<string, object>(), "v1.0") {
            this.ReversedInstance = format;
        }

        public T Format(FormatResult model) => this.ReversedInstance.Parse(model);
        public FormatResult Parse(T formattedModel) => this.ReversedInstance.Format(formattedModel);

    }

    public class ReversedFormat<T, FormattedT, FormatResult>
            : ReversedFormat<FormattedT, FormatResult>, IFormat<FormatResult, T, FormattedT>
            where FormattedT : Formatted<T> {

        public ReversedFormat(IFormat<FormattedT, FormatResult> format) : base(format) { }

    }

    public class ReversedOutFormat<T, FormatResult>
            : FormatBase, IInFormat<FormatResult, T> {

        private IOutFormat<T, FormatResult> ReversedInstance { get; }

        public ReversedOutFormat(IOutFormat<T, FormatResult> format)
                : base("ReversedOut: " + format.Name,
                       "Reversed of IOutFormat: " + format.Name,
                       new Dictionary<string, object>(), "v1.0") {
            this.ReversedInstance = format;
        }

        public FormatResult Parse(T formattedModel) => this.ReversedInstance.Format(formattedModel);

    }

    public class ReversedOutFormat<T, FormattedT, FormatResult>
            : ReversedOutFormat<FormattedT, FormatResult>, IInFormat<FormatResult, T, FormattedT>
            where FormattedT : Formatted<T> {

        public ReversedOutFormat(IOutFormat<FormattedT, FormatResult> format) : base(format) { }

    }

    public class ReversedInFormat<T, FormatResult>
            : FormatBase, IOutFormat<FormatResult, T> {

        private IInFormat<T, FormatResult> ReversedInstance { get; }

        public ReversedInFormat(IInFormat<T, FormatResult> format)
                : base("ReversedIn: " + format.Name,
                       "Reversed of IInFormat: " + format.Name,
                       new Dictionary<string, object>(), "v1.0") {
            this.ReversedInstance = format;
        }

        public T Format(FormatResult formattedModel) => this.ReversedInstance.Parse(formattedModel);

    }

    public class ReversedInFormat<T, FormattedT, FormatResult>
            : ReversedInFormat<FormattedT, FormatResult>, IOutFormat<FormatResult, T, FormattedT>
            where FormattedT : Formatted<T> {

        public ReversedInFormat(IInFormat<FormattedT, FormatResult> format) : base(format) { }

    }

    public class CombinedFormat<T, FormatResult>
            : FormatBase, IFormat<T, FormatResult> {

        private IOutFormat<T, FormatResult> OutFormat { get; }
        private IInFormat<T, FormatResult> InFormat { get; }

        public CombinedFormat(IOutFormat<T, FormatResult> outFormat, IInFormat<T, FormatResult> inFormat)
                : base("Combined: " + outFormat.Name + " / " + inFormat.Name,
                       "Combined In/Out Format From: " + outFormat.Name + " and " + inFormat.Name,
                       new Dictionary<string, object>(), "v1.0") { }

        public FormatResult Format(T model) => this.OutFormat.Format(model);
        public T Parse(FormatResult formattedModel) => this.InFormat.Parse(formattedModel);

    }

    public class CombinedFormat<T, FormatResult, FormattedResult>
            : CombinedFormat<T, FormattedResult>, IFormat<T, FormatResult, FormattedResult>
            where FormattedResult : Formatted<FormatResult> {

        public CombinedFormat(IOutFormat<T, FormatResult, FormattedResult> outFormat,
                              IInFormat<T, FormatResult, FormattedResult> inFormat)
                : base(outFormat, inFormat) { }
    }

    public class AnyFormat<T> : FormatBase, IOutFormat<T, Formatted<T>.Any> {

        public AnyFormat()
                : base(typeof(T).Name + " to Any<" + typeof(T).Name + ">", "Add Any qualification",
                       new Dictionary<string, object>(), "v1.0") { }

        // Implicit conversions on Any make this work.
        public Formatted<T>.Any Format(T model) => model;
    }

    public class AnyFromMarkedFormat<T, Marker>
            : FormatBase, IOutFormat<Formatted<T>.For<Marker>, Formatted<T>.Any> {

        public AnyFromMarkedFormat()
                : base("For<" + typeof(T).Name + ", " + typeof(Marker).Name + "> to Any<" + typeof(T).Name + ">",
                      "Add Any qualification", new Dictionary<string, object>(), "v1.0") { }

        // Implicit conversions on Any and For make this work.
        public Formatted<T>.Any Format(Formatted<T>.For<Marker> model) => model;

    }

    public class ChainOutFormat<T, Mid, FormatResult> : FormatBase, IOutFormat<T, FormatResult> {

        private IOutFormat<T, Mid> FormatInstance1 { get; }
        private IOutFormat<Mid, FormatResult> FormatInstance2 { get; }
         
        public ChainOutFormat(IOutFormat<T, Mid> format1, IOutFormat<Mid, FormatResult> format2)
                : base(format1.Name + " / " + format2.Name, format1.Description + "\n/\n" + format2.Description,
                       GenericExtensions.CombineDicts(format1.FormatOptions, format2.FormatOptions),
                       format1.Version + "/" + format2.Version) {
            this.FormatInstance1 = format1;
            this.FormatInstance2 = format2;
        }

        public FormatResult Format(T model) => this.FormatInstance2.Format(this.FormatInstance1.Format(model));
    }


    public class ChainOutFormat<T, Mid, FormattedMid, FormatResult, FormattedResult>
            : ChainOutFormat<T, FormattedMid, FormattedResult>,
              IOutFormat<T, FormatResult, FormattedResult>
            where FormattedMid : Formatted<Mid>
            where FormattedResult : Formatted<FormatResult> {

        public ChainOutFormat(IOutFormat<T, Mid, FormattedMid> format1, IOutFormat<FormattedMid, FormatResult, FormattedResult> format2)
               : base(format1, format2) { }

        // Doesn't compile because ChainFormat is not one of the arguments.
        // Putting it on FormatBase doesn't work eitheer because it only has the type info for Format1(or Format2)
        // but you need both and you can't have generics on types. Maybe a subclass with implicit conversions could work?
        // public static ChainFormat<T, Mid, FormattedMid, FormatResult, FormattedResult> operator|(IOutFormat<T, Mid, FormattedMid> format1,
        //                                                                                          IOutFormat<FormattedMid, FormatResult, FormattedResult> format2)
        //    => new ChainOutFormat<T, Mid, FormattedMid, FormatResult, FormattedResult>(format1, format2);
    }

    public class ChainInFormat<T, Mid, FormatResult> : ReversedOutFormat<FormatResult, T> {

        public ChainInFormat(IInFormat<T, Mid> format1, IInFormat<Mid, FormatResult> format2)
            : base(new ChainOutFormat<FormatResult, Mid, T>(new ReversedInFormat<Mid, FormatResult>(format2),
                                                            new ReversedInFormat<T, Mid>(format1))) { }

    }

    public class ChainInFormat<T, Mid, FormattedMid, FormatResult, FormattedResult>
            : ChainInFormat<T, FormattedMid, FormattedResult>,
              IInFormat<T, FormatResult, FormattedResult>
            where FormattedMid : Formatted<Mid>
            where FormattedResult : Formatted<FormatResult> {

        public ChainInFormat(IInFormat<T, Mid, FormattedMid> format1,
                             IInFormat<FormattedMid, FormatResult, FormattedResult> format2)
               : base(format1, format2) { }
    }

    public class ChainFormat<T, Mid, FormatResult>
            : CombinedFormat<T, FormatResult> {

        public ChainFormat(IFormat<T, Mid> format1, IFormat<Mid, FormatResult> format2)
                : base(new ChainOutFormat<T, Mid, FormatResult>(format1, format2),
                       new ChainInFormat<T, Mid, FormatResult>(format1, format2)) { }

    }

    public class ChainFormat<T, Mid, FormattedMid, FormatResult, FormattedResult>
            : ChainFormat<T, FormattedMid, FormattedResult>,
              IFormat<T, FormatResult, FormattedResult>
            where FormattedMid : Formatted<Mid>
            where FormattedResult : Formatted<FormatResult> {

        public ChainFormat(IFormat<T, Mid, FormattedMid> format1,
                           IFormat<FormattedMid, FormatResult, FormattedResult> format2)
               : base(format1, format2) { }
    }
}
