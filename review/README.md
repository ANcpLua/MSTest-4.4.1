# Test review

`test-review.json` records each declared test method and each native lifecycle fixture class.
The completed source ledger has 791 test methods and 75 lifecycle fixture classes.
The inventory includes deliberately undiscoverable declarations used to verify generator and
adapter behavior. Those declarations are inputs to an owning assertion, not independent
passing tests.

A `Pass` disposition describes the source review. It does not certify a runner, operating
system, or native binary that was not executed. Grades bound the example's assertion,
cleanup and structure; a narrow API demonstration is not exhaustive application coverage.

The review checks concrete outcomes, exception types, shared state, cancellation, resource
ownership and parallel execution. Child probes that deliberately fail, skip or time out are
checked by their parent against native output and structured results. Their individual
bodies need not manufacture a successful assertion.

The native inventory can be refreshed with:

```sh
dotnet run --file scripts/InventoryTests.cs -- . artifacts/test-inventory.json
```

This reads syntax and identifies declarations. It does not assign review grades. Refresh
rows after changing their source and retain the source hash so stale reviews are detectable.

MTP 2.5.1 has a reproduced MSBuild/TRX controller handshake defect. The strict reproduction
is `scripts/verify-msbuild-trx.sh`; its failing result is not covered by a passing desktop
suite. Windows application activation also requires a Windows host. See `API.md` for the
current boundaries of the examples.
