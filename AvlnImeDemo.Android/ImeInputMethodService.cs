using Android.App;
using Android.Content;
using Android.InputMethodServices;
using Android.Views;
using Android.Views.InputMethods;
using Avalonia.Android;
using AvlnImeDemo.Ime;
using AvlnImeDemo.Views;

namespace AvlnImeDemo.Android;

[Service(
	Label = "AvlnImeDemo IME",
	Permission = global::Android.Manifest.Permission.BindInputMethod,
	Exported = true)]
[IntentFilter(new[] { "android.view.InputMethod" })]
[MetaData("android.view.im", Resource = "@xml/ime_method")]
public sealed class ImeInputMethodService : InputMethodService {
	public AvaloniaView? ImeView { get; set; }
	public bool ShouldRecreateImeView {
		get=>false;
		set{}
	}

	private int GetHalfScreenHeight() {
		var screenHeight = Resources?.DisplayMetrics?.HeightPixels ?? 0;
		return screenHeight > 0 ? screenHeight / 2 : ViewGroup.LayoutParams.WrapContent;
	}

	public override bool OnEvaluateFullscreenMode() {
		return false;
	}

	public override void OnConfigureWindow(Window? win, bool isFullscreen, bool isCandidatesOnly) {
		base.OnConfigureWindow(win, false, isCandidatesOnly);
		win?.SetLayout(ViewGroup.LayoutParams.MatchParent, GetHalfScreenHeight());
	}

	public override global::Android.Views.View OnCreateInputView() {
		if (ImeView is not null && !ShouldRecreateImeView) {
			return ImeView;
		}

		ImeView = new AvaloniaView(this) {
			Content = new ImeKeyboardView()
		};
		ImeView.LayoutParameters = new ViewGroup.LayoutParams(
			ViewGroup.LayoutParams.MatchParent,
			GetHalfScreenHeight()
		);
		ShouldRecreateImeView = false;
		return ImeView;
	}

	public override void OnCreate() {
		SetTheme(Resource.Style.MyTheme_Ime);
		base.OnCreate();
	}

	public override void OnStartInputView(EditorInfo? info, bool restarting) {
		base.OnStartInputView(info, restarting);
		if (ShouldRecreateImeView) {
			SetInputView(OnCreateInputView());
		}
		ImeServiceBridge.Instance.CommitTextRequested += OnCommitTextRequested;
		ImeServiceBridge.Instance.DeleteSurroundingTextRequested += OnDeleteSurroundingTextRequested;
		ImeServiceBridge.Instance.KeyEventRequested += OnKeyEventRequested;
		ImeServiceBridge.Instance.HideKeyboardRequested += OnHideKeyboardRequested;
	}

	public override void OnFinishInputView(bool finishingInput) {
		ImeServiceBridge.Instance.CommitTextRequested -= OnCommitTextRequested;
		ImeServiceBridge.Instance.DeleteSurroundingTextRequested -= OnDeleteSurroundingTextRequested;
		ImeServiceBridge.Instance.KeyEventRequested -= OnKeyEventRequested;
		ImeServiceBridge.Instance.HideKeyboardRequested -= OnHideKeyboardRequested;
		ShouldRecreateImeView = true;
		base.OnFinishInputView(finishingInput);
	}

	private void OnCommitTextRequested(string text) {
		CurrentInputConnection?.CommitText(text, 1);
	}

	private void OnDeleteSurroundingTextRequested(int beforeLength) {
		CurrentInputConnection?.DeleteSurroundingText(beforeLength, 0);
	}

	private void OnKeyEventRequested(string keyName) {
		var keyCode = keyName switch {
			"Enter" => global::Android.Views.Keycode.Enter,
			_ => global::Android.Views.Keycode.Unknown
		};

		if (keyCode == global::Android.Views.Keycode.Unknown) {
			return;
		}

		var down = new global::Android.Views.KeyEvent(global::Android.Views.KeyEventActions.Down, keyCode);
		var up = new global::Android.Views.KeyEvent(global::Android.Views.KeyEventActions.Up, keyCode);
		CurrentInputConnection?.SendKeyEvent(down);
		CurrentInputConnection?.SendKeyEvent(up);
	}

	private void OnHideKeyboardRequested() {
		RequestHideSelf(0);
	}
}
