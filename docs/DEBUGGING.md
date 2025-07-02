# Debugging Guide for AdventureWorks API

## Visual Studio 2022 Exception Settings

### FluentValidation Exception Handling

The AdventureWorks API uses **FluentValidation** for input validation with centralized exception handling via middleware. Validation exceptions are **expected** and should not break the debugger.

### Quick Setup - Import Exception Settings

**Method 1: Import Pre-configured Settings (Recommended)**

1. Open Visual Studio 2022
2. Navigate to: `Debug` → `Windows` → `Exception Settings` (or press `Ctrl+Alt+E`)
3. Click the **"Import"** icon (folder with arrow) in the Exception Settings toolbar
4. Browse to: `...\AdventureWorks\.vs\ExceptionSettings.xml`
5. Click **Open**

The debugger will now ignore `FluentValidation.ValidationException` and allow your exception middleware to handle it gracefully.

---

**Method 2: Manual Configuration**

If the import file is not available:

1. Open Exception Settings: `Debug` → `Windows` → `Exception Settings` (`Ctrl+Alt+E`)
2. Right-click **"Common Language Runtime Exceptions"** → **Add Exception**
3. Type: `FluentValidation.ValidationException`
4. Click **OK**
5. **Uncheck the box** next to `FluentValidation.ValidationException`

---

## Expected Debugging Behavior

### Before Configuration ❌
- Debugger breaks every time validation fails
- Slows down development and testing
- Interrupts normal API flow

### After Configuration ✅
- Debugger ignores validation exceptions
- Exception middleware catches and returns `400 Bad Request`
- Only breaks on **unhandled** exceptions
- Smooth debugging experience

---

## Verify Configuration

**Test with Invalid Request:**

```bash
# Send invalid employee data (missing required fields)
curl -X POST https://localhost:5001/api/v1/employees \
  -H "Content-Type: application/json" \
  -d '{"firstName":"","lastName":""}'
```

**Expected Result:**
- ✅ No debugger break
- ✅ API returns `400 Bad Request` with validation errors
- ✅ Exception logged by middleware

---

## Other Expected Exceptions

The following exceptions are also handled by middleware and typically don't require debugger breaks:

- **`System.Collections.Generic.KeyNotFoundException`** - Entity not found (returns `404 Not Found`)
- **`System.UnauthorizedAccessException`** - Authorization failure (returns `401 Unauthorized`)
- **`FluentValidation.ValidationException`** - Input validation failure (returns `400 Bad Request`)

You can add these to Exception Settings if they interrupt your debugging workflow.

---

## Exporting Your Settings

To share your exception settings with the team:

1. Open Exception Settings (`Ctrl+Alt+E`)
2. Click the **"Export"** icon (folder with arrow pointing out)
3. Save to: `...\mick\AdventureWorks\.vs\ExceptionSettings.xml`
4. Commit this file if you've made improvements

---

## Troubleshooting

**Problem:** Debugger still breaks on validation exceptions

**Solution:**
1. Verify the exception name is **exactly**: `FluentValidation.ValidationException`
2. Ensure the checkbox is **unchecked**
3. Restart Visual Studio if settings don't apply
4. Check that "Break When Thrown" is disabled (not just "User-unhandled")

**Problem:** Settings lost after Visual Studio update

**Solution:**
- Re-import `ExceptionSettings.xml` after major VS updates
- Or manually re-apply using Method 2 above

---

## Additional Resources

- [Visual Studio Exception Settings Documentation](https://learn.microsoft.com/en-us/visualstudio/debugger/managing-exceptions-with-the-debugger)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- AdventureWorks API Exception Middleware: `src/AdventureWorks.API/Middleware/`
