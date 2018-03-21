using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Robmikh.CompositionSurfaceFactory;

namespace CustomMayd.Uwp.Controls
{
    public class VisualControl : UserControl
    {
        public static readonly DependencyProperty ImageUriProperty = DependencyProperty.Register(
            "ImageUri", typeof(Uri), typeof(VisualControl), new PropertyMetadata(default(Uri), ImageUriChanged));

        private static SurfaceFactory _surfaceFactoryInstance;


        private static void ImageUriChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var instance = (VisualControl) dependencyObject;
            var unused = instance.LoadImageAsync(dependencyPropertyChangedEventArgs.NewValue as Uri);
        }

        public Uri ImageUri
        {
            get => (Uri) GetValue(ImageUriProperty);
            set => SetValue(ImageUriProperty, value);
        }

        private readonly Compositor _compositor;
        private UriSurface _uriSurface;

        public SpriteVisual Visual { get; }

        public VisualControl()
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            Visual = _compositor.CreateSpriteVisual();

            if (_surfaceFactoryInstance == null)
            {
                _surfaceFactoryInstance = SurfaceFactory.CreateFromCompositor(_compositor);
            }

            ElementCompositionPreview.SetElementChildVisual(this, Visual);
            SizeChanged +=
                (sender, args) =>
                {
                    Visual.Size = new Vector2((float) args.NewSize.Width, (float) args.NewSize.Height);
                };

            Unloaded += VisualControl_Unloaded;
        }

        public async Task LoadImageAsync(Uri imageUri)
        {
            if (_uriSurface == null)
            {
                _uriSurface = await _surfaceFactoryInstance.CreateUriSurfaceAsync(imageUri);
            }
            else
            {
                await _uriSurface.RedrawSurfaceAsync(imageUri);
            }

            var brush = _compositor.CreateSurfaceBrush(_uriSurface.Surface);
            brush.Stretch = CompositionStretch.Uniform;
            Visual.Brush = brush;
            Visual.Size = new Vector2((float) ActualWidth, (float) ActualHeight);
        }

        private void VisualControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _uriSurface?.Dispose();
        }
    }
}