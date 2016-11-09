using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Serial_Monitor
{
    public static class FlowDocumentScrollViewerExtension
    {
        public static void Clear(this FlowDocumentScrollViewer flowDocumentScrollViewer)
        {
            flowDocumentScrollViewer.Document.Blocks.Clear();
        }

        public static void AppendText(this FlowDocumentScrollViewer flowDocumentScrollViewer, string data)
        {
            AppendText(flowDocumentScrollViewer, data, flowDocumentScrollViewer.FindResource(Microsoft.VisualStudio.PlatformUI.CommonControlsColors.TextBoxTextBrushKey) as SolidColorBrush);
        }

        public static void AppendText(this FlowDocumentScrollViewer flowDocumentScrollViewer, string data, SolidColorBrush brush)
        {
            TextRange range = new TextRange(flowDocumentScrollViewer.Document.ContentEnd.DocumentEnd, flowDocumentScrollViewer.Document.ContentEnd.DocumentEnd);
            range.Text = data.Replace(Environment.NewLine, "\r");
            range.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
            range.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Oblique);
        }

        public static void ScrollToEnd(this FlowDocumentScrollViewer flowDocumentScrollViewer)
        {
            if (VisualTreeHelper.GetChildrenCount(flowDocumentScrollViewer) == 0)
            {
                return;
            }

            DependencyObject firstChild = VisualTreeHelper.GetChild(flowDocumentScrollViewer, 0);
            if (firstChild == null)
            {
                return;
            }

            Decorator border = VisualTreeHelper.GetChild(firstChild, 0) as Decorator;

            if (border == null)
            {
                return;
            }

            (border.Child as ScrollViewer).ScrollToEnd();
        }
    }
}
