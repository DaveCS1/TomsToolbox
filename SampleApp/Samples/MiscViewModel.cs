﻿namespace SampleApp.Samples
{
    using System;
    using System.Windows;
    using System.Windows.Input;

    using JetBrains.Annotations;

    using TomsToolbox.Desktop;
    using TomsToolbox.Wpf;
    using TomsToolbox.Wpf.Composition;

    [VisualCompositionExport(RegionId.Main, Sequence = 99)]
    internal class MiscViewModel : ObservableObject
    {
        private DateTime _operationStarted = DateTime.Now;
        private TimeSpan _minimumDuration = TimeSpan.FromMinutes(0.2);

        public override string ToString()
        {
            return "Misc.";
        }

        public DateTime OperationStarted
        {
            get
            {
                return _operationStarted;
            }
            set
            {
                SetProperty(ref _operationStarted, value, "OperationStarted");
            }
        }

        public TimeSpan MinimumDuration
        {
            get
            {
                return _minimumDuration;
            }
            set
            {
                SetProperty(ref _minimumDuration, value, "MinimumDuration");
            }
        }

        [NotNull]
        public ICommand ItemsControlDefaultCommand => new DelegateCommand<string>(item => MessageBox.Show(item + " clicked."));
    }
}