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
	#image("assets/2026-06-15-16-51-49.png")
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
				#image("assets/2026-06-15-16-50-54.png")
			]
		+ #[选用输入法]
		+ #[聚焦输入框 即可见输入法界面弹出]
	]

]

#H[主要AI提示词][

本项目全由AI完成。主要提示词如下

````typ
看AvlnImeDemo.Android.csproj
这个项目是用 dotnet new avalonia.xplat 创建的。创建之后一行代码都没动过。
连adb之后 在安卓项目里面dotnet run 就能把app在我手机上跑起来。

现在我要做概念验证、看能不能用avalonia做安卓输入法。
你想办法把他注册成安卓的输入法。在输入框聚焦时弹出输入法界面。按下按键能上屏文字。

现在只是做概念验证。你的键盘界面随便弄几个按键就行 就弄一个字母示例键 回车键 退格键
或者还有别的特殊按键 你自己想值得试的

不会做就自己上网查资料。
不用原生View、界面要求跨平台。

**遇到任何疑问/不确定 立即停下手头的工作 来找我确认！** 疑问包括:

- 你不确定的地方
- 你觉得有设计问题的地方
  - 比如让你在当前框架下实现指定功能会很别扭或者难以实现、这时候你应该停下来和我确认、看看是不是我设计有误、不要硬着头皮做下去。

**建议多问 不要带着疑问干活**
````

````typst
avalonia android上应该能用完整的.net for android api。
有的API 你搜avalonia文档搜不到的话你就去.net for android搜
````


````typst
输入法弹出时候直接占满全屏了。高度给我调到半屏
````
]

#H[已知問題][
	鍵盤隱藏又彈出後使用GPU渲染有概率觸發黑屏
	#link("./HideKeyboard.typ")[記錄]
]
