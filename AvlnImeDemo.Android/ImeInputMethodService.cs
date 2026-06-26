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
	/// <summary>
	/// 複用同一個 AvaloniaView，避免每次彈出鍵盤都重建整棵視圖樹。
	/// </summary>
	public AvaloniaView? ImeView { get; set; }

	/// <summary>
	/// 避免在多次 show/hide 过程中重复订阅桥接事件。
	/// 否则点一次 Hide 可能实际触发多次 RequestHideSelf，导致 IME 状态紊乱。
	/// </summary>
	private bool IsBridgeSubscribed { get; set; }

	/// <summary>
	/// 取得半屏高度，讓 IME 視圖與窗口尺寸保持一致。
	/// </summary>
	private int GetHalfScreenHeight() {
		var screenHeight = Resources?.DisplayMetrics?.HeightPixels ?? 0;
		return screenHeight > 0 ? screenHeight / 2 : ViewGroup.LayoutParams.WrapContent;
	}

	/// <summary>
	/// IME 視圖只創建一次，後續都複用同一個實例。
	/// </summary>
	private AvaloniaView EnsureImeView() {
		if (ImeView is not null) {
			return ImeView;
		}

		ImeView = new AvaloniaView(this) {
			Content = new ImeKeyboardView()
		};
		return ImeView;
	}

	/// <summary>
	/// 同一個輸入框持續保持焦點時，Android 可能只重新顯示 IME window，
	/// 但不會重新創建 input view。這裏主動把複用的 AvaloniaView 從舊父節點摘下，
	/// 再交回 InputMethodService 重新掛載，並觸發一次 layout/draw。
	/// </summary>
	private void ReattachImeView() {
		var imeView = EnsureImeView();
		if (imeView.Parent is ViewGroup parent) {
			parent.RemoveView(imeView);
		}
		SetInputView(imeView);
		imeView.RequestLayout();
		imeView.Invalidate();
	}

	public override bool OnEvaluateFullscreenMode() {
		return false;
	}

	/// <summary>
	/// 固定 IME 窗口高度，避免窗口尺寸變更與內容尺寸脫節。
	/// </summary>
	public override void OnConfigureWindow(Window? win, bool isFullscreen, bool isCandidatesOnly) {
		base.OnConfigureWindow(win, false, isCandidatesOnly);
		win?.SetLayout(ViewGroup.LayoutParams.MatchParent, GetHalfScreenHeight());
	}

	/// <summary>
	/// 首次需要 input view 時返回複用的 AvaloniaView。
	/// </summary>
	public override global::Android.Views.View OnCreateInputView() {
		return EnsureImeView();
	}

	/// <summary>
	/// 先設置 IME 專用主題，再走基類初始化。
	/// </summary>
	public override void OnCreate() {
		SetTheme(Resource.Style.MyTheme_Ime);
		base.OnCreate();
		SubscribeBridgeEvents();
	}

	/// <summary>
	/// 每次開始顯示輸入視圖時都主動重掛複用的 AvaloniaView。
	/// restarting=true 正是“同一個輸入框仍保持焦點”的場景。
	/// </summary>
	public override void OnStartInputView(EditorInfo? info, bool restarting) {
		base.OnStartInputView(info, restarting);
		ReattachImeView();
	}

	/// <summary>
	/// 窗口真正顯示前再補一次重掛與重繪。
	/// 這一層專門覆蓋“窗口回來了，但同一 editor 沒失焦”的場景。
	/// </summary>
	public override void OnWindowShown() {
		base.OnWindowShown();
		ReattachImeView();
	}

	/// <summary>
	/// 結束輸入視圖時只解除事件，不銷毀複用的 AvaloniaView。
	/// </summary>
	public override void OnFinishInputView(bool finishingInput) {
		base.OnFinishInputView(finishingInput);
	}

	/// <summary>
	/// Service 销毁时再统一解除订阅，避免悬挂引用。
	/// </summary>
	public override void OnDestroy() {
		UnsubscribeBridgeEvents();
		base.OnDestroy();
	}

	/// <summary>
	/// 只订阅一次桥接事件，避免重复回调。
	/// </summary>
	private void SubscribeBridgeEvents() {
		if (IsBridgeSubscribed) {
			return;
		}

		ImeServiceBridge.Instance.CommitTextRequested += OnCommitTextRequested;
		ImeServiceBridge.Instance.DeleteSurroundingTextRequested += OnDeleteSurroundingTextRequested;
		ImeServiceBridge.Instance.KeyEventRequested += OnKeyEventRequested;
		ImeServiceBridge.Instance.HideKeyboardRequested += OnHideKeyboardRequested;
		IsBridgeSubscribed = true;
	}

	/// <summary>
	/// 解除桥接事件订阅。
	/// </summary>
	private void UnsubscribeBridgeEvents() {
		if (!IsBridgeSubscribed) {
			return;
		}

		ImeServiceBridge.Instance.CommitTextRequested -= OnCommitTextRequested;
		ImeServiceBridge.Instance.DeleteSurroundingTextRequested -= OnDeleteSurroundingTextRequested;
		ImeServiceBridge.Instance.KeyEventRequested -= OnKeyEventRequested;
		ImeServiceBridge.Instance.HideKeyboardRequested -= OnHideKeyboardRequested;
		IsBridgeSubscribed = false;
	}

	/// <summary>
	/// 提交文本到當前聚焦的輸入框。
	/// </summary>
	private void OnCommitTextRequested(string text) {
		CurrentInputConnection?.CommitText(text, 1);
	}

	/// <summary>
	/// 刪除光標前方字符。
	/// </summary>
	private void OnDeleteSurroundingTextRequested(int beforeLength) {
		CurrentInputConnection?.DeleteSurroundingText(beforeLength, 0);
	}

	/// <summary>
	/// 把 Avalonia 內部按鍵映射成 Android key event。
	/// </summary>
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

	/// <summary>
	/// 請求 Android 隱藏當前輸入法窗口。
	/// </summary>
	private void OnHideKeyboardRequested() {
		RequestHideSelf(0);
	}
}
