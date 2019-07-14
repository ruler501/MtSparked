using Xamarin.Forms;
using MtSparked.UI.Views;
using Ooui;

namespace MtSparked.Platforms.Wasm {
    public class Program {

#pragma warning disable IDE0060 // Remove unused parameter
        public static void Main(string[] _unusedArgs) {
#pragma warning restore IDE0060 // Remove unused parameter
            Forms.Init();

            MainPage mainPage = new MainPage();

            Ooui.UI.Publish("/", mainPage.GetOouiElement());
        }

    }
}
