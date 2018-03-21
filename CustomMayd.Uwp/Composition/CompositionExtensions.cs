using System;
using System.Numerics;
using Windows.UI.Composition;

namespace CustomMayd.Uwp.Composition
{
    public static class CompositionExtensions
    {
        public static ScalarKeyFrameAnimation CreateScalarKeyFrameAnimation(this Compositor compositor, string target,
            float from, float to, TimeSpan duration)
        {
            var scalarKeyFrameAnimation = compositor.CreateScalarKeyFrameAnimation();
            scalarKeyFrameAnimation.Target = target;
            scalarKeyFrameAnimation.InsertKeyFrame(0.0f, from);
            scalarKeyFrameAnimation.InsertKeyFrame(1.0f, to);
            scalarKeyFrameAnimation.Duration = duration;
            return scalarKeyFrameAnimation;
        }

        public static Vector2KeyFrameAnimation CreateVector2KeyFrameAnimation(this Compositor compositor, string target,
            Vector2 from, Vector2 to, TimeSpan duration)
        {
            var vector2KeyFrameAnimation = compositor.CreateVector2KeyFrameAnimation();
            vector2KeyFrameAnimation.Target = target;
            vector2KeyFrameAnimation.InsertKeyFrame(0.0f, from);
            vector2KeyFrameAnimation.InsertKeyFrame(1.0f, to);
            vector2KeyFrameAnimation.Duration = duration;
            return vector2KeyFrameAnimation;
        }

        public static Vector3KeyFrameAnimation CreateVector3KeyFrameAnimation(this Compositor compositor, string target,
            Vector3 from, Vector3 to, TimeSpan duration)
        {
            var vector3KeyFrameAnimation = compositor.CreateVector3KeyFrameAnimation();
            vector3KeyFrameAnimation.Target = target;
            vector3KeyFrameAnimation.InsertKeyFrame(0.0f, from);
            vector3KeyFrameAnimation.InsertKeyFrame(1.0f, to);
            vector3KeyFrameAnimation.Duration = duration;
            return vector3KeyFrameAnimation;
        }
    }
}