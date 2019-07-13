﻿namespace SampleApp.Mef2
{
    using System;
    using System.Collections.Generic;
    using System.Composition;
    using System.Composition.Convention;
    using System.Composition.Hosting;
    using System.Diagnostics;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;

    using JetBrains.Annotations;

    using TomsToolbox.Essentials;
    using TomsToolbox.Wpf;
    using TomsToolbox.Wpf.Composition;
    using TomsToolbox.Wpf.Composition.Mef2;
    using TomsToolbox.Wpf.Composition.XamlExtensions;
    using TomsToolbox.Wpf.Converters;
    using TomsToolbox.Wpf.Styles;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public sealed partial class App : IDisposable
    {
        [CanBeNull]
        private CompositionHost _container;

        public static IExportProvider ExportProvider { get; set; }

        public App()
        {
            // Thread.CurrentThread.CurrentUICulture = new CultureInfo("de-DE");
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
        }

        protected override void OnStartup([CanBeNull] StartupEventArgs e)
        {
            base.OnStartup(e);

            VisualComposition.Trace += (sender, args) => Trace.WriteLine(args.Text);
            BindingErrorTracer.Start(BindingErrorCallback);

            var conventions = new ConventionBuilder();

            conventions.ForTypesDerivedFrom<IValueConverter>().Export();

            var configuration = new ContainerConfiguration()
                .WithAssembly(GetType().Assembly, conventions)
                .WithAssembly(typeof(CoordinatesToPointConverter).Assembly, conventions);

            var container = _container = configuration.CreateContainer();

            ExportProvider = new ExportProviderAdapter(container);

            Resources.MergedDictionaries.Insert(0, WpfStyles.GetDefaultStyles().RegisterDefaultWindowStyle());
            Resources.MergedDictionaries.Add(DataTemplateManager.CreateDynamicDataTemplates(ExportProvider));

            var mainWindow = MainWindow = container.GetExport<MainWindow>();
            mainWindow.Show();
        }

        private void BindingErrorCallback([CanBeNull] string msg)
        {
            Dispatcher?.BeginInvoke((Action)(() => MessageBox.Show(msg)));
        }

        protected override void OnExit([CanBeNull] ExitEventArgs e)
        {
            Dispose();

            base.OnExit(e);
        }

        public void Dispose()
        {
            _container?.Dispose();
        }

        [Export(typeof(IExportProvider)), Shared]
        [UsedImplicitly]
        private class ExportProviderInstanceAdapter : IExportProvider
        {
            public event EventHandler<EventArgs> ExportsChanged;

            [NotNull]
            T IExportProvider.GetExportedValue<T>([CanBeNull] string contractName)
            {
                return ExportProvider.GetExportedValue<T>(contractName);
            }

            [CanBeNull]
            T IExportProvider.GetExportedValueOrDefault<T>([CanBeNull] string contractName)
            {
                return ExportProvider.GetExportedValueOrDefault<T>(contractName);
            }

            [NotNull, ItemNotNull]
            IEnumerable<T> IExportProvider.GetExportedValues<T>([CanBeNull] string contractName)
            {
                return ExportProvider.GetExportedValues<T>(contractName);
            }

            [NotNull, ItemNotNull]
            IEnumerable<ILazy<object>> IExportProvider.GetExports(Type type, [CanBeNull] string contractName)
            {
                return ExportProvider.GetExports(type, contractName);
            }
        }
    }
}
