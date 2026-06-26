using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;

namespace AvlnImeDemo.Android
{
    /// <summary>
    /// Android 端的 Avalonia 入口。
    /// IME 場景下先固定到 Software renderer 做概念驗證，
    /// 用來判斷黑屏是否由 Egl surface 在 hide/show 之間失效導致。
    /// </summary>
    [Application]
    public class Application : AvaloniaAndroidApplication<App>
    {
        protected Application(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        {
            return base.CustomizeAppBuilder(builder)
            .With(new AndroidPlatformOptions
            {
                RenderingMode = new[]
                {
                    Avalonia.AndroidRenderingMode.Software
                }
            })
            .WithInterFont();
        }
    }
}
