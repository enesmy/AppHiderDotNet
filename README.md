# App Hider .NET (v1.2.0)

**App Hider .NET** is a powerful, stealthy utility designed to give you instant privacy and control over your desktop environment. Built with **.NET 9.0** and **WPF**, it allows you to instantly hide any active window from the screen, taskbar, and Alt+Tab switcher with a simple hotkey.

![App Icon](app_icon.png)

## ✨ Key Features

*   **👻 Instant Stealth**: Press `Ctrl + Shift + 1` to instantly vanish the active window.
*   **🛡️ Safe Mode**: Press `Ctrl + Shift + Space` to instantly hide a predefined list of sensitive applications.
*   **🌫️ Privacy Blur**: Press `Ctrl + Shift + 2` to apply a privacy blur overlay to any window - content becomes completely unreadable while staying visible.
*   **🎯 Area Privacy**: Press `Ctrl + Shift + 3` to select and obscure a specific area of your screen.
*   **🔒 Password Protection**: Secure your hidden windows or Safe Mode activation with passwords.
*   **💎 Liquid Glass UI**: A stunning, modern interface featuring a semi-transparent, blur-effect design (Glassmorphism) with custom window chrome.
*   **📂 System Tray Integration**: Runs silently in the background. Right-click the tray icon to quickly restore specific windows, manage Safe Mode, or exit the app.

## 🚀 How to Use

1.  **Start the App**: Run `AppHiderNet.exe`. The app will start quietly in the system tray.
2.  **Hide a Window**: Click on any window you want to hide and press **`Ctrl + Shift + 1`**.
3.  **Safe Mode**:
    *   **Setup**: Open the **Scan Apps** window, select apps, and click "Add to Safe Mode".
    *   **Activate**: Press **`Ctrl + Shift + Space`** to hide all apps in your Safe Mode list at once.
    *   **Restore**: Press the hotkey again (and enter your password if set) to restore them.
    *   **Manage**: Right-click the tray icon and select **Safe Mode Settings** to view/remove apps or set a password.
4.  **Blur a Window**: Click on any window you want to protect and press **`Ctrl + Shift + 2`**.
5.  **Secure an Area**: Press **`Ctrl + Shift + 3`**, select a region on your screen, and it will be blurred instantly.
6.  **Manage Hidden Apps**:
    *   Double-click the **Tray Icon** to open the **Manager Window**.
    *   **Restore**: Select an app and click "Restore" to bring it back.
    *   **Kill**: Select an app and click "Kill" to close it permanently.

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
