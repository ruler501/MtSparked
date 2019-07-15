using System;
using System.Collections.Generic;
using System.Text;
using Equ;
using MtSparked.Interop.Databases;

namespace MtSparked.Interop.Models {
    public abstract class ModelWithId : Model, IEquatable<ModelWithId> {
        private string id;
        [PrimaryKey]
        [MemberwiseEqualityIgnore]
        public string Id {
            get { return this.id; }
            set { _ = this.SetProperty(ref this.id, value); }
        }

        public bool Equals(ModelWithId other) => this.GetType() == other?.GetType() && this.Id == other?.Id;

        public override int GetHashCode() => this.Id.GetHashCode();

        public bool ValueEquals(ModelWithId other) => base.Equals(other);
    }
}
