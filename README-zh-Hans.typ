#import "@preview/tsinswreng-auto-heading:0.1.0": auto-heading
#let H = auto-heading

//Avalonia Android Input Method Editor (IME) Demo
#H[Avalonia 安卓输入法(IME)概念验证][
	此项目只是概念验证、验证使用Avalonia开发安卓输入法的可行性。
	构建产物只是简单的示例、不是成熟可用的输入法。

	#H[依赖环境][
		- NET 10
		- Avalonia 12
	]
]

#H[效果图][
	#image("assets\2026-06-15-16-51-49.png")
	按下a键可上屏字符`a`、空格键 退格键 回车键均可正常工作
]


#H[在自己的设备上尝试][

	#H[准备事项][
		+ #[已安装.NET10 SDK]
		+ #[把adb连通]
	]


	#H[正式步骤][
		+ #[获取仓库源码]

		+ #[
				编译
				```bash
				cd AvlnImeDemo.Android
				dotnet run
				```
			]
		+ #[在手机输入法管理中启用
				#image("assets\2026-06-15-16-50-54.png")
			]
		+ #[选用输入法]
		+ #[聚焦输入框 即可见输入法界面弹出]
	]

]
