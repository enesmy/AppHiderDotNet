# App Hider .NET

**App Hider .NET** is a powerful, stealthy utility designed to give you instant privacy and control over your desktop environment. Built with **.NET 9.0** and **WPF**, it allows you to instantly hide any active window from the screen, taskbar, and Alt+Tab switcher with a simple hotkey.

![App Icon](app_icon.png)

## ✨ Key Features

*   **👻 Instant Stealth**: Press `Ctrl + Shift + 1` (or `NumPad 1`) to instantly vanish the active window.
*   **💎 Liquid Glass UI**: A stunning, modern interface featuring a semi-transparent, blur-effect design (Glassmorphism) with custom window chrome.
*   **🖱️ Drag Anywhere**: The Manager Window is fully draggable from any point on its surface for maximum convenience.
*   **⚡ Kill Switch**: Stuck application? Select it in the manager and hit **Kill** to force-terminate the process immediately.
*   **📂 System Tray Integration**: Runs silently in the background. Right-click the tray icon to quickly restore specific windows or exit the app.
*   **🔄 Auto-Sync**: The UI updates in real-time as you hide and restore windows.

## 🚀 How to Use

1.  **Start the App**: Run `AppHiderNet.exe`. The app will start quietly in the system tray.
2.  **Hide a Window**: Click on any window you want to hide and press **`Ctrl + Shift + 1`**. It will disappear instantly!
3.  **Manage Hidden Apps**:
    *   Double-click the **Tray Icon** (bottom right) to open the **Manager Window**.
    *   **Restore**: Select an app and click "Restore" to bring it back.
    *   **Kill**: Select an app and click "Kill" to close it permanently.
    *   **Restore All**: Bring back everything at once.

## 🛠️ Technologies Used

*   **C# / .NET 9.0**: High-performance core logic.
*   **WPF (Windows Presentation Foundation)**: For the beautiful, hardware-accelerated UI.
*   **Win32 APIs (P/Invoke)**: For deep system integration (window management, hotkeys, process control).
*   **Glassmorphism**: Custom styling using semi-transparent brushes and blur effects.

## 📦 Installation & Build

Requirements: **.NET 9.0 SDK**

```bash
# Clone the repository
git clone https://github.com/enesmy/AppHiderDotNet.git

# Navigate to the project folder
cd AppHiderDotNet

# Build the project
dotnet build

# Run the application
dotnet run
```

---
*Developed with ❤️ for privacy and productivity.*
