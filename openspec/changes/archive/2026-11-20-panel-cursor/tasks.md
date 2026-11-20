# Tasks

## 1. The fix

- [x] 1.1 Add `LoadCursorW`, `IDC_ARROW`, and the class cursor field to the Win32
      surface. Verified: the build names them, and the interop test still allows
      only the system libraries it already allowed.
- [x] 1.2 Move the panel class definition into `Panel.CreateWindowClass`, which
      sets the arrow cursor. Verified: the test below reads the cursor from it.
- [x] 1.3 Test that the class names a non-null cursor, and that it is the arrow.
      Verified: `dotnet test` fails if the cursor is dropped again.

## 2. Finish

- [x] 2.1 Run the tray with the panel open and hover it. Verified: the pointer is
      the normal arrow, captured in the smoke.
- [x] 2.2 `bash scripts/verify.sh`, changelog entry, archive, and merge.
      Verified: the check passes and the archive carries the result.
