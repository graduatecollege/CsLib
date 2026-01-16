# CsLib

Graduate College web application common library, primarily
for web application APIs.

## Publish

Publishing is automated via GitHub Actions. To publish a new version:

1. Push a tag in the format `vX.Y.Z` (e.g., `v1.0.9`):
   ```bash
   git tag v1.0.9
   git push origin v1.0.9
   ```

2. The GitHub Actions workflow will automatically:
   - Build the project
   - Create a NuGet package with the version from the tag
   - Publish to GitHub Packages
   - Create a GitHub release

### Manual Publishing (if needed)

To manually publish the library:

```bash
dotnet clean -c Release
dotnet build -c Release /p:Version=X.Y.Z
dotnet pack -c Release --no-build /p:Version=X.Y.Z -o ./packages
dotnet nuget push ./packages/*.nupkg --source "github" --api-key YOUR_GITHUB_PAT
```
