using System;
using System.Collections.Generic;
using System.Text;

namespace MtSparked.Interop.Services {
    public interface ISettings {

        string GetValueOrDefault(string key, string defaultValue);
        int GetValueOrDefault(string key, int defaultValue);
        bool GetValueOrDefault(string key, bool defaultValue);

        void AddOrUpdateValue(string key, string value);
        void AddOrUpdateValue(string key, int value);
        void AddOrUpdateValue(string key, bool value);

    }
}
