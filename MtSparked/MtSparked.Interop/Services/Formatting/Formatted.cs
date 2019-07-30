namespace MtSparked.Interop.Services.Formatting {
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
}
