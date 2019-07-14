namespace MtSparked.UWP {
    public sealed partial class MainPage {

        public MainPage() {
            this.InitializeComponent();

            // TODO: Fix
            // SfDataGridRenderer.Init();

            this.LoadApplication(new UI.App());
        }

    }
}
