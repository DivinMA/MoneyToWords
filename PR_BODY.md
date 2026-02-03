This PR:

- Improves `MoneyError.Description` to include actual values for `InvalidKopecks` and `RublesTooLarge`, uses `sprintf` and handles empty `CompositionError`.
- Adds unit tests (`MoneyErrorPropertiesTests`) to assert stability of `MoneyError.Code` and `Description` formatting.
- Adds CI checks: `dotnet format --verify-no-changes` step and coverage collection + Codecov upload in `.github/workflows/ci.yml`.

All tests pass locally (259/259).

Please review and let me know if you want me to also add a pre-commit hook to run `dotnet format` locally.
