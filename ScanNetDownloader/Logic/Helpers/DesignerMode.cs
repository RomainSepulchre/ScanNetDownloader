using System.ComponentModel;
using System.Windows;

namespace ScanNetDownloader.Logic.Helpers
{
    static class DesignerMode
    {
        private static bool? _isInDesignerMode;

        public static bool IsInDesignerMode
        {
            get
            {
                if (_isInDesignerMode.HasValue == false)
                {
                    DependencyProperty isInDesignModeProperty = DesignerProperties.IsInDesignModeProperty;
                    _isInDesignerMode = (bool)DependencyPropertyDescriptor.FromProperty(isInDesignModeProperty, typeof(FrameworkElement)).Metadata.DefaultValue;
                }

                return _isInDesignerMode.Value;
            }
        }

    }
}
