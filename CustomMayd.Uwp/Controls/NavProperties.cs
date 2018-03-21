using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CustomMayd.Uwp.Controls
{
    public class NavProperties : DependencyObject
    {
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.RegisterAttached("Header", typeof(string),
                typeof(NavProperties), new PropertyMetadata(null));

        public static readonly DependencyProperty IsStartPageProperty =
            DependencyProperty.RegisterAttached("IsStartPage", typeof(bool),
                typeof(NavProperties), new PropertyMetadata(false));

        public static readonly DependencyProperty PageTypeProperty =
            DependencyProperty.RegisterAttached("PageType", typeof(Type),
                typeof(NavProperties), new PropertyMetadata(null));

        public static string GetHeader(Page obj)
        {
            return (string) obj.GetValue(HeaderProperty);
        }

        public static bool GetIsStartPage(NavigationViewItem obj)
        {
            return (bool) obj.GetValue(IsStartPageProperty);
        }

        public static Type GetPageType(NavigationViewItem obj)
        {
            return (Type) obj.GetValue(PageTypeProperty);
        }

        public static void SetHeader(Page obj, string value)
        {
            obj.SetValue(HeaderProperty, value);
        }

        public static void SetIsStartPage(NavigationViewItem obj, bool value)
        {
            obj.SetValue(IsStartPageProperty, value);
        }

        public static void SetPageType(NavigationViewItem obj, Type value)
        {
            obj.SetValue(PageTypeProperty, value);
        }
    }
}