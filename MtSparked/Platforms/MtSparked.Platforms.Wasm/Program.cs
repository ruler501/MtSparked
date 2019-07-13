using System;
using MtSparked.Views;
using Ooui;
using Xamarin.Forms;

namespace MtSparked.Platforms.Wasm {
    class Program {
        static void Main(string[] args) {
            Forms.Init();

            MainPage mainPage = new MainPage();

            UI.Publish("/", mainPage.getOouiElement());
        }
    }
}
