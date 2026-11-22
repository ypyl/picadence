# The rename's names are gone from the tests (L0)

## Why

Two tests still name their temporary folders after the product's old name. They
are folder names, so nothing fails, which is exactly why they survived the rename
and will be read as evidence that the old name is still current.

## What Changes

Two string literals in `extensions/picadence.test.mjs`. Same tests, same
assertions, a name that matches the product.

## Capabilities

None. A test's scratch folder is not behavior, so `skip_specs: true`.

## Impact

- `extensions/picadence.test.mjs`.

## Routing

0. **Does observable behavior change?** No.
1. **Can it be undone easily?** Yes, one line each.
2. **Can a machine prove it correct?** Yes: the suite runs the same 21 tests.
3. **Does anything outside the change depend on it?** No.

Level: **L0**.
