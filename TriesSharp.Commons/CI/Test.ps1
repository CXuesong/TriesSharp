param (
    [string]
    $Configuration = "Release"
)
trap {
    Write-Error $_
    Exit 1
}

# Assumes $PWD is the repo root
dotnet test ./TriesSharp.Tests.UnitTestProject1/TriesSharp.Tests.UnitTestProject1.csproj `
    --no-build -c $Configuration `
    --logger "console;verbosity=normal" `
    -- RunConfiguration.TestSessionTimeout=1800000
Exit $LASTEXITCODE
