namespace MtSparked.Platforms.UWP {
    public sealed partial class MainPage {

        public MainPage() {
            this.InitializeComponent();

            // TODO #102: Fix SfDataGridRenderer on UWP
            // SfDataGridRenderer.Init();

            this.LoadApplication(new UI.App());
        }

    }
}
