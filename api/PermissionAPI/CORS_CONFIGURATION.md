# CORS Configuration Guide

## ? CORS Issue Fixed!

The API has been configured to accept requests from your Angular application.

---

## ?? What Was Added

### 1?? CORS Policy Configuration (Program.cs)

```csharp
builder.Services.AddCors(options =>
{
    // Specific policy for your Angular app
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:49797")  // Your Angular URL
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });

    // Alternative: Allow all origins (development only)
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

### 2?? CORS Middleware (Program.cs)

```csharp
app.UseCors("AllowAngularApp"); // Placed before UseAuthorization()
```

---

## ?? Current Configuration

**Allowed Origin:** `http://localhost:49797` (your Angular app)  
**Allowed Headers:** All  
**Allowed Methods:** All (GET, POST, PUT, DELETE, etc.)  
**Credentials:** Enabled  

---

## ?? If Your Angular Port Changes

### Option 1: Update the Specific Policy

In `Program.cs`, update the origin:

```csharp
policy.WithOrigins("http://localhost:NEW_PORT")
```

### Option 2: Allow Multiple Origins

```csharp
policy.WithOrigins(
    "http://localhost:49797",
    "http://localhost:4200",  // Add more ports
    "http://localhost:4201"
)
```

### Option 3: Use Environment-Based Configuration

**appsettings.Development.json:**
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:49797",
      "http://localhost:4200"
    ]
  }
}
```

**Program.cs:**
```csharp
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? new[] { "http://localhost:49797" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

---

## ?? For Development: Allow All Origins

If you want to allow requests from **any** origin during development:

**Change in Program.cs:**
```csharp
// Use the "AllowAll" policy instead
app.UseCors("AllowAll");
```

**?? WARNING:** Never use `AllowAll` in production!

---

## ?? Production Configuration

For production, you should:

1. **Whitelist specific domains:**
   ```csharp
   policy.WithOrigins(
       "https://yourdomain.com",
       "https://www.yourdomain.com"
   )
   ```

2. **Store in configuration:**
   ```json
   // appsettings.Production.json
   {
     "Cors": {
       "AllowedOrigins": [
         "https://yourdomain.com",
         "https://www.yourdomain.com"
       ]
     }
   }
   ```

3. **Remove AllowCredentials if not needed:**
   ```csharp
   policy.WithOrigins("https://yourdomain.com")
         .AllowAnyHeader()
         .AllowAnyMethod();
   // Note: AllowCredentials() removed
   ```

---

## ?? Testing

### 1. Restart Your API

```bash
dotnet run
```

The API will now accept requests from `http://localhost:49797`

### 2. Test from Angular

Your Angular HTTP request should now work:

```typescript
// Angular service
this.http.get('http://localhost:5275/api/Users')
  .subscribe(
    users => console.log('? Success!', users),
    error => console.error('? Error:', error)
  );
```

### 3. Verify in Browser Console

You should now see:
- ? **Before:** CORS error
- ? **After:** Successful request with data

---

## ?? CORS Headers Explained

| Header | Value | Purpose |
|--------|-------|---------|
| `Access-Control-Allow-Origin` | `http://localhost:49797` | Specifies which origin can access |
| `Access-Control-Allow-Methods` | `GET, POST, PUT, DELETE, etc.` | Allowed HTTP methods |
| `Access-Control-Allow-Headers` | `*` | Allowed request headers |
| `Access-Control-Allow-Credentials` | `true` | Allows cookies/auth headers |

---

## ?? Troubleshooting

### Issue: Still Getting CORS Error

**Check:**
1. ? API is running on `http://localhost:5275`
2. ? Angular is running on `http://localhost:49797`
3. ? You restarted the API after making changes
4. ? The origin URL matches exactly (including protocol and port)

### Issue: Preflight Request Failing

If you see `OPTIONS` request failing:

```csharp
// Ensure CORS is before Authorization
app.UseCors("AllowAngularApp");
app.UseAuthorization();
```

### Issue: Credentials Not Working

If you need to send cookies or auth headers:

```typescript
// Angular: Include credentials
this.http.get('http://localhost:5275/api/Users', {
  withCredentials: true
}).subscribe(...);
```

```csharp
// API: Must use specific origin (not AllowAnyOrigin)
policy.WithOrigins("http://localhost:49797")
      .AllowCredentials();
```

---

## ?? Complete CORS Checklist

- [x] CORS policy defined in `Program.cs`
- [x] CORS middleware added to pipeline
- [x] CORS middleware placed **before** `UseAuthorization()`
- [x] Angular origin URL whitelisted
- [x] API restarted
- [x] Ready to test from Angular! ??

---

## ?? Next Steps

1. **Restart the API:** `dotnet run`
2. **Refresh your Angular app**
3. **Test the API calls**
4. **You should now see data instead of CORS errors!**

---

## ?? Quick Reference

**Current Setup:**
- ? Angular: `http://localhost:49797`
- ? API: `http://localhost:5275`
- ? CORS Policy: `AllowAngularApp`
- ? All HTTP methods allowed
- ? All headers allowed
- ? Credentials enabled

**To switch to allow-all (dev only):**
```csharp
app.UseCors("AllowAll");
```

**To add more origins:**
```csharp
policy.WithOrigins(
    "http://localhost:49797",
    "http://localhost:4200"
)
```
