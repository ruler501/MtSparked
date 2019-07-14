using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MtSparked.Interop.Models {
    public abstract class Model : INotifyPropertyChanged {

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName]string propertyName = "",
            Action onChanged = null) {
            if (EqualityComparer<T>.Default.Equals(backingStore, value)) {
                return false;
            }

            backingStore = value;
            onChanged?.Invoke();
            this.OnPropertyChanged(propertyName);
            return true;
        }

        // TODO #75: Investigate Making Model.PropertyChanged a WeakEvent
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChangedEventHandler changed = this.PropertyChanged;
            if (!(this.PropertyChanged is null)) {
                this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
