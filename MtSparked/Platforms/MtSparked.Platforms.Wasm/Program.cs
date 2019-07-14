using Xamarin.Forms;
using MtSparked.UI.Views;
using Ooui;
using Ooui.Forms;

namespace MtSparked.Platforms.Wasm {
    public class Program {
        public static void Main(string[] args) {
            Forms.Init();

            MainPage mainPage = new MainPage();

            Ooui.UI.Publish("/", mainPage.getOouiElement());
        }
    }
}
