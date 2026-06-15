#import "@preview/tsinswreng-auto-heading:0.1.0": auto-heading
#let H = auto-heading

//Avalonia Android Input Method Editor (IME) Demo
#link("./README-zh-Hant.md")[繁體中文]
#link("./README-zh-Hans.md")[简体中文]
#H[Avalonia Android IME Proof of Concept][
	This project is a proof of concept, demonstrating the feasibility of building an Android input method using Avalonia.
	The build output is a minimal demo — not a production-ready input method.

	#H[Dependencies][
		- NET 10
		- Avalonia 12
	]
]

#H[Screenshots][
	#image("assets\2026-06-15-16-51-49.png")
	Pressing the `a` key commits the character `a`. The spacebar, backspace, and enter keys all function correctly.
]

#H[Try It on Your Own Device][

	#H[Prerequisites][
		+ #[.NET 10 SDK installed]
		+ #[ADB connected to your device]
	]


	#H[Steps][
		+ #[Clone the repository]

		+ #[
				Build and run
				```bash
				cd AvlnImeDemo.Android
				dotnet run
				```
			]
		+ #[Enable the input method in your phone's input method settings
				#image("assets\2026-06-15-16-50-54.png")
			]
		+ #[Select the input method]
		+ #[Focus on an input field — the IME UI will appear]
	]

]
