using System;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Hosting;
using Microsoft.Xaml.Interactivity;

namespace CustomMayd.Uwp.Behaviors
{
    public class SizeAndOffsetImplicitAnimationBehavior : Behavior<DependencyObject>
    {
        private static Compositor _compositor;

        /// <summary>
        ///     The duration of the animations in MilliSeconds
        /// </summary>
        public static readonly DependencyProperty DurationMilliSecondsProperty = DependencyProperty.Register(
            "DurationMilliSeconds", typeof(double), typeof(SizeAndOffsetImplicitAnimationBehavior),
            new PropertyMetadata(200d));

        /// <summary>
        ///     The duration of the animations in MilliSeconds
        /// </summary>
        public double DurationMilliSeconds
        {
            get => (double) GetValue(DurationMilliSecondsProperty);
            set => SetValue(DurationMilliSecondsProperty, value);
        }

        private ImplicitAnimationCollection _implicitAnimations;

        protected override void OnAttached()
        {
            base.OnAttached();

            var element = AssociatedObject as UIElement;
            if (element != null)
            {
                EnsureImplicitAnimations(element);
                // Check to see if the element has a SpriteVisual - assumption being that is what is going to be sized
                var visual = ElementCompositionPreview.GetElementChildVisual(element) ??
                             ElementCompositionPreview.GetElementVisual(element);
                visual.ImplicitAnimations = _implicitAnimations;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            var element = AssociatedObject as UIElement;
            if (element != null)
            {
                var visual = ElementCompositionPreview.GetElementChildVisual(element) ??
                             ElementCompositionPreview.GetElementVisual(element);
                visual.ImplicitAnimations = null;
            }
        }

        private void EnsureImplicitAnimations(UIElement element)
        {
            if (_implicitAnimations == null)
            {
                if (_compositor == null)
                {
                    _compositor = ElementCompositionPreview.GetElementVisual(element).Compositor;
                }

                var offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
                offsetAnimation.Target = nameof(Visual.Offset);
                offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
                offsetAnimation.Duration = TimeSpan.FromMilliseconds(DurationMilliSeconds);

                var sizeAnimation = _compositor.CreateVector2KeyFrameAnimation();
                sizeAnimation.Target = nameof(Visual.Size);
                sizeAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
                sizeAnimation.Duration = TimeSpan.FromMilliseconds(DurationMilliSeconds);

                _implicitAnimations = _compositor.CreateImplicitAnimationCollection();
                _implicitAnimations[nameof(Visual.Offset)] = offsetAnimation;
                _implicitAnimations[nameof(Visual.Size)] = sizeAnimation;
            }
        }
    }
}