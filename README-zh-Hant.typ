#import "@preview/tsinswreng-auto-heading:0.1.0": auto-heading
#let H = auto-heading

//Avalonia Android Input Method Editor (IME) Demo
#H[Avalonia 安卓輸入法(IME)概念驗證][
	此項目只是概念驗證、驗證使用Avalonia開發安卓輸入法的可行性。
	構建產物只是簡單的示例、不是成熟可用的輸入法。

	#H[依賴環境][
		- NET 10
		- Avalonia 12
	]
]

#H[效果圖][
	#image("assets\2026-06-15-16-51-49.png")
	按下a鍵可上屏字符`a`、空格鍵 退格鍵 回車鍵均可正常工作
]


#H[在自己的設備上嘗試][

	#H[準備事項][
		+ #[已安裝.NET10 SDK]
		+ #[把adb連通]
	]


	#H[正式步驟][
		+ #[獲取倉庫源碼]

		+ #[
				編譯
				```bash
				cd AvlnImeDemo.Android
				dotnet run
				```
			]
		+ #[在手機輸入法管理中啓用
				#image("assets\2026-06-15-16-50-54.png")
			]
		+ #[選用輸入法]
		+ #[聚焦輸入框 即可見輸入法界面彈出]
	]

]
