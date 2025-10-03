# 🚀 Debug Mode - Quick Login

## Overview

Debug mode allows developers to skip the login process for faster development iteration.

## Features

- ✅ One-click login with pre-configured test user
- ✅ Visual debug badge in top-right corner
- ✅ Highlighted debug button on login page
- ✅ Only available in development environment
- ✅ Automatically disabled in production

## Usage

### Enable Debug Mode

Debug mode is automatically enabled when running in development:

```bash
npm run dev
```

### Quick Login

1. Navigate to the login page (`/login`)
2. Click the **🚀 Debug: Skip Login** button
3. You'll be instantly logged in as a test user

### Debug User Credentials

```json
{
  "id": 1,
  "phone": "13800138000",
  "nickName": "Debug User",
  "email": "debug@catcat.com",
  "role": 1,
  "avatar": "🐱"
}
```

## Environment Variables

### `.env.development`
```env
VITE_APP_TITLE=CatCat - Development
VITE_API_BASE_URL=http://localhost:5000
VITE_DEBUG_MODE=true  # Enable debug mode
```

### `.env.production`
```env
VITE_APP_TITLE=CatCat
VITE_API_BASE_URL=/api
VITE_DEBUG_MODE=false  # Disable debug mode
```

## Visual Indicators

### Debug Badge
A pulsing orange badge appears in the top-right corner when debug mode is active:
```
🚀 DEBUG MODE
```

### Debug Login Button
On the login page, a special orange button with dashed border appears:
```
🚀 Debug: Skip Login
```

## Security

⚠️ **Important Security Notes:**

1. Debug mode is **completely disabled** in production builds
2. The debug button is **not rendered** when `VITE_DEBUG_MODE=false`
3. No sensitive credentials are hardcoded in production code
4. The debug token is client-side only and not validated by the backend

## Implementation Details

### Files Modified

1. **src/stores/user.ts**
   - Added `debugLogin()` function
   - Generates temporary debug token and user

2. **src/views/auth/Login.vue**
   - Added debug button (conditional rendering)
   - Added `handleDebugLogin()` method
   - Checks `import.meta.env.VITE_DEBUG_MODE`

3. **src/App.vue**
   - Added debug mode badge
   - Visual indicator of debug environment

4. **Environment Files**
   - `.env.development` - Debug enabled
   - `.env.production` - Debug disabled

## Development Workflow

```bash
# Start development with debug mode
npm run dev

# Build for production (debug mode disabled)
npm run build

# Preview production build (no debug mode)
npm run preview
```

## Troubleshooting

### Debug button not showing?

1. Check `.env.development` exists with `VITE_DEBUG_MODE=true`
2. Restart the dev server after changing env files
3. Clear browser cache and localStorage

### Debug login not working?

1. Check browser console for errors
2. Verify localStorage is enabled
3. Check that userStore.debugLogin() is called

## Future Enhancements

Potential improvements:

- [ ] Keyboard shortcut for debug login (e.g., Ctrl+Shift+D)
- [ ] Multiple debug user profiles (admin, customer, provider)
- [ ] Debug mode settings panel
- [ ] Mock API responses in debug mode
- [ ] Debug mode logger

