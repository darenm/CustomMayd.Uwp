using System;
using System.Numerics;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using CustomMayd.Uwp.Composition;
using Microsoft.Graphics.Canvas.Effects;
using Robmikh.CompositionSurfaceFactory;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace CustomMayd.Uwp.Controls
{
    public sealed partial class CompositionImage : UserControl
    {
        private const int AnimationDuration = 500;

        private static Compositor _compositor;
        private static SurfaceFactory _surfaceFactory;

        private static CompositionBrush _blurBrush;

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof(ImageSource), typeof(CompositionImage), new PropertyMetadata(default(ImageSource)));

        public ImageSource Source
        {
            get => (ImageSource) GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty StretchProperty = DependencyProperty.Register(
            "Stretch", typeof(Stretch), typeof(CompositionImage), new PropertyMetadata(default(Stretch)));

        public Stretch Stretch
        {
            get => (Stretch) GetValue(StretchProperty);
            set => SetValue(StretchProperty, value);
        }

        private bool _isLoaded;

        public CompositionImage()
        {
            if (_compositor == null)
            {
                _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            }

            if (_surfaceFactory == null)
            {
                _surfaceFactory = SurfaceFactory.CreateFromCompositor(_compositor);
            }

            InitializeComponent();
            ImageControl.ImageOpened += ImageControlOnImageOpened;
            DataContextChanged += (sender, args) => Bindings.Update();
        }

        public event RoutedEventHandler ImageOpened;

        public void ItemEntered()
        {
            if (!_isLoaded)
            {
                return;
            }

            var visual = ElementCompositionPreview.GetElementVisual(ImageControl);
            visual.Scale = new Vector3(1.05f, 1.05f, 1.0f);

            // Create a Scoped batch to capture animation completion events
            var batch = _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

            var detailsVisual = ElementCompositionPreview.GetElementVisual(DetailPanel);

            var leftInsetEnterAnimation = _compositor.CreateScalarKeyFrameAnimation(
                nameof(InsetClip.LeftInset),
                0.0f,
                (float) ImageControl.ActualWidth * 0.023f,
                TimeSpan.FromMilliseconds(AnimationDuration));

            var rightInsetRightAnimation = _compositor.CreateScalarKeyFrameAnimation(
                nameof(InsetClip.RightInset),
                0.0f,
                (float) ImageControl.ActualWidth * 0.023f,
                TimeSpan.FromMilliseconds(AnimationDuration));

            if (visual.Clip == null)
            {
                visual.Clip = _compositor.CreateInsetClip(0, 0, 0, 0);
            }

            // Batch is ended and no objects can be added
            batch.End();

            // Method triggered when batch completion event fires
            batch.Completed += (o, args) =>
            {
                rightInsetRightAnimation.Dispose();
                leftInsetEnterAnimation.Dispose();
            };

            ((InsetClip) visual.Clip)?.StartAnimation(nameof(InsetClip.LeftInset), leftInsetEnterAnimation);
            ((InsetClip) visual.Clip)?.StartAnimation(nameof(InsetClip.RightInset), rightInsetRightAnimation);

            // Update implicit animations
            var finalOffset = new Vector3(0, (float) RootVisual.ActualHeight - 60, 0);
            detailsVisual.Offset = finalOffset;
            detailsVisual.Opacity = 1.0f;
        }

        public void ItemExited()
        {
            var visual = ElementCompositionPreview.GetElementVisual(ImageControl);
            visual.Scale = new Vector3(1.0f, 1.0f, 1.0f);

            // Create a Scoped batch to capture animation completion events
            var batch = _compositor.CreateScopedBatch(CompositionBatchTypes.Animation);

            var anim = _compositor.CreateExpressionAnimation();

            var leftInsetExitAnimation = _compositor.CreateScalarKeyFrameAnimation(
                nameof(InsetClip.LeftInset),
                (float) ImageControl.ActualWidth * 0.023f,
                0,
                TimeSpan.FromMilliseconds(AnimationDuration));

            var rightInsetExitAnimation = _compositor.CreateScalarKeyFrameAnimation(
                nameof(InsetClip.RightInset),
                (float) ImageControl.ActualWidth * 0.023f,
                0,
                TimeSpan.FromMilliseconds(AnimationDuration));

            var detailsVisual = ElementCompositionPreview.GetElementVisual(DetailPanel);

            var finalOffset = new Vector3(0, (float) RootVisual.ActualHeight, 0);

            if (visual.Clip == null)
            {
                visual.Clip = _compositor.CreateInsetClip(0, 0, 0, 0);
            }

            // Batch is ended and no objects can be added
            batch.End();

            // Method triggered when batch completion event fires
            batch.Completed += (o, args) =>
            {
                rightInsetExitAnimation.Dispose();
                leftInsetExitAnimation.Dispose();
            };

            ((InsetClip) visual.Clip)?.StartAnimation(nameof(InsetClip.LeftInset), leftInsetExitAnimation);
            ((InsetClip) visual.Clip)?.StartAnimation(nameof(InsetClip.RightInset), rightInsetExitAnimation);

            detailsVisual.Offset = finalOffset;
            detailsVisual.Opacity = 0.0f;
        }

        private static CompositionBrush CreateBlurBrush()
        {
            if (_blurBrush != null)
            {
                return _blurBrush;
            }

            var blurEffect = new GaussianBlurEffect
            {
                Name = "Blur",
                BlurAmount = 15.0f,
                BorderMode = EffectBorderMode.Hard,
                Optimization = EffectOptimization.Balanced,
                Source = new CompositionEffectSourceParameter("source")
            };

            var blendEffect = new BlendEffect
            {
                Background = blurEffect,
                Foreground = new ColorSourceEffect {Name = "Color", Color = Color.FromArgb(64, 0, 0, 0)},
                Mode = BlendEffectMode.SoftLight
            };

            var blurEffectFactory = _compositor.CreateEffectFactory(blendEffect);
            var blurBrush = blurEffectFactory.CreateBrush();

            var backdropBrush = _compositor.CreateBackdropBrush();
            blurBrush.SetSourceParameter("source", backdropBrush);

            _blurBrush = blurBrush;

            return _blurBrush;
        }

        private void DetailPanel_OnLoaded(object sender, RoutedEventArgs e)
        {
            var sprite = _compositor.CreateSpriteVisual();
            sprite.Brush = CreateBlurBrush();
            sprite.Size = new Vector2((float) DetailPanel.ActualWidth, (float) DetailPanel.ActualHeight);
            ElementCompositionPreview.SetElementChildVisual(Backdrop, sprite);

            var visual = ElementCompositionPreview.GetElementVisual(DetailPanel);
            visual.Opacity = 0;
            var finalOffset = new Vector3(0, (float) RootVisual.ActualHeight, 0);
            visual.Offset = finalOffset;

            var visualImplicitAnimations = _compositor.CreateImplicitAnimationCollection();

            var offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.Target = nameof(Visual.Offset);
            offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(AnimationDuration);
            visualImplicitAnimations[nameof(Visual.Offset)] = offsetAnimation;

            var opacityAnimation = _compositor.CreateScalarKeyFrameAnimation();
            opacityAnimation.Target = nameof(Visual.Opacity);
            opacityAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            opacityAnimation.Duration = TimeSpan.FromMilliseconds(AnimationDuration);
            visualImplicitAnimations[nameof(Visual.Opacity)] = offsetAnimation;

            visual.ImplicitAnimations = visualImplicitAnimations;
        }

        private void DetailPanel_OnUnloaded(object sender, RoutedEventArgs e)
        {
            var sprite = ElementCompositionPreview.GetElementChildVisual(Backdrop);
            sprite?.Dispose();
            ElementCompositionPreview.SetElementChildVisual(Backdrop, null);
        }

        private void ImageControlOnImageOpened(object sender, RoutedEventArgs routedEventArgs)
        {
            ImageOpened?.Invoke(this, routedEventArgs);

            var visual = ElementCompositionPreview.GetElementVisual((UIElement) sender);
            visual.Scale = new Vector3(0.9f, 0.9f, 1);
            visual.Opacity = 0;

            visual.CenterPoint = new Vector3((float) ImageControl.ActualWidth / 2,
                (float) ImageControl.ActualHeight / 2,
                0);

            var visualImplicitAnimations = _compositor.CreateImplicitAnimationCollection();
            var animation = _compositor.CreateVector3KeyFrameAnimation();
            animation.Target = nameof(Visual.Scale);
            animation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            animation.Duration = TimeSpan.FromMilliseconds(AnimationDuration);
            visualImplicitAnimations[nameof(Visual.Scale)] = animation;
            visual.ImplicitAnimations = visualImplicitAnimations;

            var insetClip = _compositor.CreateInsetClip(0, 0, 0, 0);
            visual.Clip = insetClip;

            var opacityAnimation = _compositor.CreateScalarKeyFrameAnimation(
                nameof(Visual.Opacity),
                0.0f,
                1.0f,
                TimeSpan.FromMilliseconds(500));

            visual.StartAnimation(nameof(Visual.Opacity), opacityAnimation);
            visual.Scale = new Vector3(1, 1, 1);

            var detailVisual = ElementCompositionPreview.GetElementVisual(DetailPanel);
            detailVisual.Opacity = 0;
            var finalOffset = new Vector3(0, (float) RootVisual.ActualHeight, 0);
            detailVisual.Offset = finalOffset;

            _isLoaded = true;
        }

        private void Photo_OnLoaded(object sender, RoutedEventArgs e)
        {
            // If the image has already loaded (in a back scenario say) we don't do entrancs animations
            if (ImageControl.Source != null && ((BitmapImage) ImageControl.Source).PixelHeight != 0)
            {
                return;
            }

            var visual = ElementCompositionPreview.GetElementVisual((UIElement) sender);
            visual.Scale = new Vector3(0.9f, 0.9f, 1);
            visual.Opacity = 0;
        }

        private void Photo_OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            ItemEntered();
        }

        private void Photo_OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            ItemExited();
        }

        private void Photo_UnLoaded(object sender, RoutedEventArgs e)
        {
            var visual = ElementCompositionPreview.GetElementVisual((UIElement) sender);
            var insetClip = visual.Clip as InsetClip;
            insetClip?.Dispose();
            visual.Clip = null;
        }

        private void RootVisual_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var visual = ElementCompositionPreview.GetElementVisual(DetailPanel);
            visual.Opacity = 0;
            var finalOffset = new Vector3(0, (float) RootVisual.ActualHeight, 0);
            visual.Offset = finalOffset;
        }
    }
}