﻿namespace SampleApp.Mef2.Samples
{
    using TomsToolbox.Wpf.Composition.Mef2;

    /// <summary>
    /// Interaction logic for CommandViewChild2.xaml
    /// </summary>
    [DataTemplate(typeof(CompositeCommandChild2ViewModel))]
    public partial class CompositeCommandChild2View
    {
        public CompositeCommandChild2View()
        {
            InitializeComponent();
        }
    }
}
