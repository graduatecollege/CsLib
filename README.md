# CsLib

Graduate College web application common library, primarily
for web application APIs.

## Publish

First build in Release mode:

```powershell
dotnet clean -c Release
dotnet build -c Release
```

Then publish the library:
```powershell
dotnet nuget push --source "npm_feed" --api-key az .\CsLib\bin\Release\Grad.CsLib.*
```
