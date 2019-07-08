﻿namespace SampleApp.Mef2.Samples
{
    using JetBrains.Annotations;

    using PropertyChanged;

    using TomsToolbox.Wpf.Composition.Mef2;

    [VisualCompositionExport(RegionId.Main, Sequence = 4)]
    [ImplementPropertyChanged]
    class SharedWidthSampleViewModel
    {
        [CanBeNull]
        public string LongText { get; set; } = "This is a long text";

        public override string ToString()
        {
            return "Alignment";
        }
    }
}
