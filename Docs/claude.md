Here's my code review of the Sambar project:

## Architecture & Design

**Strengths:**
- Clean separation of concerns with the widget system
- Modular design allowing theme/layout customization
- Smart use of Win32 interception for taskbar replacement

**Issues:**
- **Heavy coupling to static API instance** - `Sambar.api` is accessed globally throughout widgets, making testing difficult and creating tight coupling
- **Mixed responsibilities** - The `Api` class is massive (15+ files) and handles everything from audio to windowing to system monitoring
- **Unsafe threading** - Many UI updates use `Dispatcher.Invoke` without proper error handling

## Critical Issues

**Memory Management:**
```csharp
public static FileStream logFile = File.Open(Paths.logFile, FileMode.OpenOrCreate);
public static StreamWriter logFileWriter = new(logFile);
```
These static resources are never disposed, causing resource leaks.

**Security:**
```csharp
ScriptOptions options = ScriptOptions.Default
    .AddReferences(typeof(Sambar).Assembly)
    .WithImports("sambar");
obj = await CSharpScript.EvaluateAsync<T>(script, options: options);
```
Runtime compilation of user scripts without sandboxing poses security risks.

**Error Handling:**
Most Win32 API calls lack proper error checking:
```csharp
User32.SetWindowLong(hWnd, (int)GETWINDOWLONG.GWL_EXSTYLE, (int)(exStyles | (uint)WINDOWSTYLE.WS_EX_TOOLWINDOW));
// No check of return value or GetLastError()
```

## Performance Concerns

**Inefficient Updates:**
```csharp
public void UpdateText()
{
    Task.Run(async () =>
    {
        while (true)
        {
            // UI update every second in background thread
            this.Thread.Invoke(() => { textBlock.Text = text; });
            await Task.Delay(1000);
        }
    });
}
```
Creates long-running tasks that never clean up.

**Resource-Heavy Audio Processing:**
The audio visualization creates multiple threads and continuous FFT processing without throttling or resource limits.

## Code Quality Issues

**Magic Numbers:**
```csharp
const int SAMPLE_RATE = 44100;
const int BITS = 16;
const int CHANNELS = 2;
```
Should be configurable or at least documented why these specific values.

**Inconsistent Naming:**
Mix of PascalCase, camelCase, and SCREAMING_CASE without clear conventions.

**Poor Separation of Concerns:**
```csharp
public partial class Api // Split across 14 files
```
This god class violates SRP badly.

## Specific Recommendations

1. **Implement proper DI container** instead of static `Sambar.api`
2. **Add comprehensive error handling** for all Win32 calls
3. **Implement IDisposable** for resource cleanup
4. **Sandbox script execution** or validate inputs
5. **Use CancellationTokens** for proper task cleanup
6. **Split the API class** into focused services
7. **Add unit tests** - currently there are none
8. **Implement proper logging** instead of static file handles

## Positive Aspects

- Clever taskbar interception approach
- Comprehensive Windows API integration  
- Flexible widget system design
- Good use of WPF for UI rendering
- Solid audio processing implementation

## Overall Assessment

This is an ambitious project with some clever solutions, but it needs significant refactoring for production use. The core concepts are sound, but execution has serious issues around resource management, error handling, and code organization that could cause crashes and security vulnerabilities.

The biggest wins would come from breaking up the monolithic API class and implementing proper resource cleanup patterns.
