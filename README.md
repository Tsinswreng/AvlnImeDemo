[繁體中文](./README-zh-Hant.md) [简体中文](./README-zh-Hans.md)

## Avalonia Android IME Proof of Concept

This project is a proof of concept, demonstrating the feasibility of building an Android input method using Avalonia. The build output is a minimal demo — not a production-ready input method.

### Dependencies

- NET 10
- Avalonia 12

## Screenshots

![](assets\2026-06-15-16-51-49.png)Pressing the `a` key commits the character `a`. The spacebar, backspace, and enter keys all function correctly.

## Try It on Your Own Device

### Prerequisites

1. .NET 10 SDK installed
2. ADB connected to your device

### Steps

1. Clone the repository

1. Build and run
   ```bash
   cd AvlnImeDemo.Android
   dotnet run
   ```
2. Enable the input method in your phone’s input method settings
   ![](assets\2026-06-15-16-50-54.png)
3. Select the input method
4. Focus on an input field — the IME UI will appear
