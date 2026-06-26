#import "@preview/tsinswreng-auto-heading:0.1.0": auto-heading
#let H = auto-heading

#H[隱藏鍵盤實驗記錄][
	本文記錄 `AvlnImeDemo` 這個 Avalonia Android 輸入法概念驗證項目中，圍繞「隱藏鍵盤後再次彈出」所做的實驗、現象、結論與尚未解決的問題。

	這份文檔的目的不是只記最終結論，而是把 #emph[試過哪些方法、每種方法的效果如何、爲甚麼不行] 都明確寫下來，供未來其他人用 Avalonia 開發 Android 輸入法時參考。

	#H[最初狀態][
		最初的核心問題是：#emph[是否可以複用同一個 `AvaloniaView` 作爲輸入法鍵盤的 Android 宿主 view]。

		#H[方案一：複用同一個 AvaloniaView 實例][
			最初的做法是複用同一個 `AvaloniaView`。

			- #[現象:
				如果輸入框始終保持聚焦，鍵盤隱藏後再次彈出，界面很容易變成黑屏。
			]
			- #[具體表現:
				界面是黑的，但裏面的按鈕仍然可以點擊。
			]
			- #[補充觀察:
				如果隱藏鍵盤後，先讓輸入框失焦，再重新聚焦彈出鍵盤，黑屏反而不容易出現。
			]
			- #[初步判斷:
				問題不只是「隱藏再顯示」，還和 Android IME 的 lifecycle、輸入框是否持續聚焦有關。
			]
		]

		#H[方案二：每次都重新創建 AvaloniaView][
			另一個基線方案是：每次顯示鍵盤時都重新創建新的 `AvaloniaView`。

			- #[現象:
				這樣做時，聚焦狀態下隱藏再彈出，不再出現黑屏。
			]
			- #[問題:
				性能開銷非常明顯。鍵盤彈出有卡頓，重建整個 view 樹成本較高。
			]
			- #[結論:
				這能避開黑屏，但代價過大，不適合作爲希望繼續往真實輸入法方向發展的方案。
			]
		]
	]

	#H[今天最終實際達成的效果][
		今天最重要的成果不是完全解決所有問題，而是確認了「哪種隱藏路徑穩定、哪種不穩定」。

		截至今天，已確認：

		- #[如果通過點擊頁面空白處來隱藏鍵盤，之後再彈出鍵盤，不會黑屏。]
		- #[如果通過點擊系統導航欄的返回/下箭頭來隱藏鍵盤，之後再彈出鍵盤，不會黑屏。]
		- #[鍵盤內部自定義的 `Hide` 按鈕，仍然無法實現和上述兩條穩定路徑一樣的效果。]
	]

	#H[關鍵觀察][
		今天最關鍵的觀察不是「鍵盤能不能隱藏」，而是：

		#emph[不同的隱藏路徑，會觸發不同的 Android IME lifecycle，結果差別非常大。]

		目前可以明確區分爲兩類：

		- #[穩定路徑:
			點擊頁面空白處、或點擊系統導航欄的返回/下箭頭。
		]
		- #[不穩定路徑:
			點擊鍵盤內部自定義的 `Hide` 按鈕。
		]

		因此，問題不是簡單的「隱藏後再顯示一定會出錯」。
		更準確地說，是：

		- #[某些 Android 原生隱藏路徑是穩定的；]
		- #[某些由 IME 自己主動發起的隱藏路徑會導致後續黑屏、空殼鍵盤、乃至崩潰。]
	]

	#H[今天做過的嘗試與結果][
		下面按試驗方向逐條記錄。

		#H[1. 強制使用 Software renderer][
			#H[動機][
				當時懷疑黑屏可能只是 GPU/EGL surface 丟失，於是先嘗試把 Avalonia Android 渲染模式固定爲 `Software`。
			]

			#H[做法][
				在 Android app builder 中顯式設置 `AndroidPlatformOptions.RenderingMode = Software`。
			]

			#H[結果][
				沒有解決問題。
			]

			#H[結論][
				問題不能簡單理解爲「只要不用 GPU 就能避免黑屏」。
			]
		]

		#H[2. 手動重掛同一個 AvaloniaView][
			#H[動機][
				既然直接複用同一個 `AvaloniaView` 會黑屏，就嘗試在每次顯示鍵盤時，把同一個 `AvaloniaView` 從舊父節點摘下，再重新掛回 IME 宿主。
			]

			#H[做法][
				在 `OnStartInputView` / `OnWindowShown` 等 lifecycle 裏：

				- #[拿到同一個 `AvaloniaView`;]
				- #[如果它已經有父節點，就先 `RemoveView`;]
				- #[再 `SetInputView(imeView)` 掛回去;]
				- #[然後 `RequestLayout()` / `Invalidate()`。]
			]

			#H[結果][
				這一類做法在某些情況下 #emph[確實能改善黑屏]。
				也就是說，不做重掛會黑，做某種形式的重掛反而能讓顯示恢復。
			]

			#H[但後續發現的更嚴重問題][
				雖然黑屏得到了一定程度的修復，但這條路引入了新的風險：

				- #[部分寫法會直接導致 Java 層崩潰；]
				- #[部分寫法則會導致 native render thread 在 `libSkiaSharp.so` 裏崩潰。]
			]

			#H[2.1 手動設置 LayoutParameters 的嘗試][
				爲了保證重掛後尺寸正常，曾嘗試在重掛時手動設置 `LayoutParameters`。

				#H[嘗試 A：使用 `ViewGroup.LayoutParams`][
					- #[結果:
						直接崩潰。
					]
					- #[錯誤:
						`ClassCastException`，因爲 IME 宿主測量過程中期望的參數類型不是這個。
					]
				]

				#H[嘗試 B：使用 `ViewGroup.MarginLayoutParams`][
					- #[結果:
						仍然崩潰。
					]
					- #[錯誤:
						IME 宿主後續又要求 `FrameLayout.LayoutParams`，類型仍然不匹配。
					]
				]

				#H[結論][
					在 IME 這個宿主環境裏，手動改 `LayoutParameters` 很危險，極易破壞宿主自身要求的類型約束。
				]
			]

			#H[2.2 不再手動改 LayoutParameters，只做重掛][
				後來把手動設置 `LayoutParameters` 的部分撤掉，只保留：

				- #[`RemoveView`;]
				- #[`SetInputView`;]
				- #[`RequestLayout`;]
				- #[`Invalidate`。]

				#H[結果][
					這樣至少不再因爲 `LayoutParams` 類型錯誤而立刻崩潰。
				]

				#H[但新的嚴重問題][
					ADB 日誌顯示，在反覆 hide/show 之後，會出現 native crash：

					- #[`SIGSEGV`;]
					- #[崩在線程 `Render Thread`;]
					- #[崩在 `libSkiaSharp.so`;]
					- #[堆棧中可見 `sk_surface_draw`。]
				]

				#H[解讀][
					這說明：

					- #[手動重掛 Android 宿主 view 雖然能幫助恢復顯示；]
					- #[但同時又可能把 Avalonia/Skia 底層 surface 狀態搞壞；]
					- #[最後導致 native render thread 直接段錯。]
				]
			]
		]

		#H[3. 檢查事件重複訂閱問題][
			#H[動機][
				在某些日誌裏看到多次 hide 行爲，因此懷疑 bridge 事件可能在每次 `OnStartInputView` 時被重複訂閱。
			]

			#H[做法][
				把 `ImeServiceBridge` 的事件改成：

				- #[只在 service 初始化時訂閱一次；]
				- #[在 service 銷燬時統一解綁。]
			]

			#H[結果][
				這確實是有價值的清理，能避免某些重複回調問題。
			]

			#H[但結論][
				它不是這次「Hide 按鈕不穩定 / 黑屏 / 崩潰」的根本解法。
			]
		]

		#H[4. 各種 Hide 按鈕實現方式的嘗試][
			今天最花時間、也最重要的一部分，就是圍繞鍵盤內 `Hide` 按鈕試不同隱藏路徑。

			#H[4.1 `RequestHideSelf(0)`][
				#H[動機][
					這是最直觀的 IME 自己請求 Android 收起輸入法的方式。
				]

				#H[結果][
					從功能上說，它確實會收起鍵盤。
					但在這個 Avalonia IME 複用場景裏，穩定性很差。
				]

				#H[觀察到的問題][
					- #[高概率導致後續黑屏；]
					- #[有時導致空殼鍵盤；]
					- #[有時導致再也彈不出；]
					- #[有時最終走到 native crash。]
				]

				#H[結論][
					它能隱藏，但在這個場景中不夠穩定。
				]
			]

			#H[4.2 `HideWindow()`][
				#H[動機][
					比起 `RequestHideSelf`，`HideWindow()` 更接近「只收 IME 窗口」這條路徑。
				]

				#H[結果][
					這條路比某些其他方案更接近系統實際行爲，但並沒有徹底解決問題。
				]

				#H[重要發現][
					後來通過加日誌確認：

					- #[當用戶點擊頁面空白處收起鍵盤時，系統實際出現的核心事件就是 `InputMethodService: CALL: hideWindow`。]
				]

				這說明 `HideWindow()` 本身不是胡亂猜的 API，而是 #emph[確實接近穩定路徑]。

				#H[但問題][
					即使如此，從鍵盤內部按鈕觸發這條路徑，仍然沒有達到「和點空白處完全等價」的穩定效果。
				]
			]

			#H[4.3 模擬返回鍵：`SendDownUpKeyEvents(Keycode.Back)`][
				#H[動機][
					既然系統導航欄的返回/下箭頭能穩定收起鍵盤，就嘗試從 IME 裏模擬一次 `Back`。
				]

				#H[結果][
					它的效果和系統導航欄那個返回/下箭頭 #emph[完全不一樣]。
				]

				#H[具體表現][
					點擊之後，先看到宿主 app 直接回到上一個視圖/頁面，然後鍵盤也消失。
				]

				#H[解讀][
					這說明這裏發送的 `Back` 其實是作用到了宿主 app 的頁面導航，而不是只做 IME dismiss。
					鍵盤之所以消失，只是因爲頁面切換後原先的輸入框失去焦點。
				]

				#H[結論][
					這不是系統導航欄 `↓` 的等價行爲，不能用。
				]
			]

			#H[4.4 `InputMethodManager.HideSoftInputFromInputMethod(...)`][
				#H[動機][
					既然 `RequestHideSelf` 太激進，就嘗試直接拿 IME 當前的 binding token，通過 `InputMethodManager` 請系統收起這個輸入法。
				]

				#H[結果][
					在實際測試設備上，點擊 `Hide` 按鈕後沒有任何反應。
				]

				#H[結論][
					這條路在本場景下等價於 no-op。
				]
			]

			#H[4.5 `InputMethodManager.HideSoftInputFromWindow(...)`][
				#H[動機][
					進一步嘗試更像宿主窗口請求隱藏軟鍵盤的方式。
				]

				#H[結果][
					實測仍然沒有任何反應。
				]

				#H[結論][
					這條路在本場景下也不生效。
				]
			]
		]

		#H[5. 回滾重掛邏輯的嘗試][
			#H[動機][
				當發現手動重掛雖然能修正黑屏，但也可能帶來崩潰時，曾嘗試把後面加的 `ReattachImeView()` 整套撤掉，回到最小基線，想看看是不是重掛本身就是罪魁禍首。
			]

			#H[結果][
				效果更差。
			]

			#H[具體現象][
				在這個回滾版本裏：

				- #[無論是點頁面空白處，還是點 `Hide` 按鈕；]
				- #[都更高概率地重新出現黑屏。]
			]

			#H[結論][
				這說明：

				- #[手動重掛這套邏輯雖然有風險；]
				- #[但它也確實在某種程度上修復了黑屏；]
				- #[不重掛則黑屏問題明顯更嚴重。]
			]
		]

		#H[6. 加 Android lifecycle 日誌，對比兩條隱藏路徑][
			#H[動機][
				由於多種 hide API 的實際效果都與預期不同，所以不再繼續盲猜，而是直接加日誌比較：

				- #[點空白處收起時，IME 真正走了哪些 lifecycle；]
				- #[點鍵盤 `Hide` 按鈕時，IME 又走了哪些 lifecycle。]
			]

			#H[記錄的事件][
				- #[`OnCreateInputView`;]
				- #[`OnStartInputView`;]
				- #[`OnWindowShown`;]
				- #[`OnFinishInputView`;]
				- #[`OnFinishInput`;]
				- #[`OnWindowHidden`;]
				- #[自定義 `Hide` 按鈕點擊事件。]
			]

			#H[對比結果][
				點頁面空白處收起時，會穩定看到：

				- #[`OnFinishInputView finishingInput=False`;]
				- #[`OnWindowHidden`;]
				- #[系統日誌中的 `InputMethodService: CALL: hideWindow`。]
			]

			而在某些 `Hide` 按鈕方案中：

			- #[雖然按鈕事件本身確實被調用了；]
			- #[但系統沒有緊接着走同樣的有效 hide lifecycle；]
			- #[或者即使也走到了某些 hide 行爲，後續穩定性仍然與點空白處不同。]
			]

			#H[結論][
				今天最清楚的結論之一就是：

				#emph[不是所有“隱藏輸入法”的 API 都等價。]
			]
		]
	]

	#H[今天實驗中出現過的重要錯誤][
		#H[Java 層錯誤][
			在手動重掛 `AvaloniaView` 並亂改 `LayoutParameters` 時，出現過：

			- #[`ClassCastException`;]
			- #[`ViewGroup.LayoutParams` 與宿主要求的類型不匹配；]
			- #[`MarginLayoutParams` 也不匹配 `FrameLayout.LayoutParams`。]
		]

		#H[Native 層錯誤][
			在多次 hide/show 並手動重掛 surface 的場景下，出現過：

			- #[`SIGSEGV`;]
			- #[崩在線程 `Render Thread`;]
			- #[崩在 `libSkiaSharp.so`;]
			- #[堆棧中可見 `sk_surface_draw`。]
		]

		#H[這些錯誤的價值][
			它們證明了：

			- #[這不是純粹的 UI 表面問題；]
			- #[而是可能牽涉到底層 Avalonia/Skia surface lifecycle 與 Android IME 宿主 lifecycle 的兼容性。]
		]
	]

	#H[目前可確認的結論][
		根據今天整天的實驗，現在可以相對明確地寫下如下結論：

		- #[直接複用同一個 `AvaloniaView`，如果不做額外 lifecycle 處理，在輸入框持續聚焦的 hide/show 場景下非常容易黑屏。]
		- #[每次重新創建新的 `AvaloniaView` 可以避開黑屏，但性能成本明顯，卡頓和開銷較大。]
		- #[手動重掛同一個 `AvaloniaView` 確實能在一定程度上改善黑屏，但也可能破壞 Skia surface 狀態，導致更嚴重的崩潰。]
		- #[目前最穩定的隱藏路徑不是自定義按鈕，而是用戶點擊頁面空白處或系統導航欄返回/下箭頭。]
		- #[自定義 `Hide` 按鈕目前仍無法實現與這兩條穩定路徑完全一致的行爲。]
	]

	#H[目前還做不到甚麼][
		截至今天，仍然做不到的是：

		- #[在保留 Avalonia 鍵盤內部自定義 `Hide` 按鈕的同時，讓它和「點空白處」或「點系統導航欄返回/下箭頭」一樣穩定；]
		- #[在完全複用同一個 `AvaloniaView` 實例的情況下，既不黑屏，也不崩潰，且性能還好。]
	]

	#H[對未來使用 Avalonia 做 Android 輸入法的參考價值][
		儘管 `Hide` 按鈕仍未解決，但這個概念驗證項目仍然非常有參考意義，因爲它已經證明了：

		- #[可以用 Avalonia 實現 Android 輸入法的基本 UI；]
		- #[可以正常提交字符、空格、退格、回車等按鍵操作；]
		- #[真正難的地方不是 IME 註冊，也不是普通按鍵輸入，而是 Android IME hide/show lifecycle 與 Avalonia/Skia surface 的協調。]
	]

	#H[一句話總結][
		今天最重要的總結不是「`Hide` 按鈕沒做好」，
		而是：

		#emph[在 Avalonia Android IME 場景裏，鍵盤怎樣被隱藏，會直接影響下一次彈出時是否黑屏、是否空殼、是否崩潰。]
	]
]
