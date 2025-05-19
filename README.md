# StringPool

[![NuGet](https://img.shields.io/nuget/v/StringPool.svg?style=flat-square)](https://www.nuget.org/packages/StringPool/)

---

## Introduction

- A high-performance string pooling library based on .NET9's `ArrayPool` design, optimized for reusing string instances.
- Reduces memory allocations and GC pressure by efficiently managing string lifetimes in performance-sensitive scenarios.

---

## Features

- ✅ **GC Optimization**: Minimizes string allocations by reusing pooled instances.
- ✅ **Thread-Safe**: Supports concurrent operations for multithreaded applications.
- ✅ **Lightweight**: Low-overhead design with minimal runtime impact.
- ✅ **Seamless Integration**: Simple API to replace ad-hoc string caching or frequent allocations.

---

## Why?

- Frequent short-lived string allocations can degrade performance in high-throughput applications.
- Existing solutions like `ArrayPool` or manual caching lack dedicated support for string pooling.
- `StringPool` provides a standardized, efficient way to manage reusable strings without GC overhead.

---

## How to use?

### `StringPool`

- `public string Rent(int minimumLength)`

  - Rent a string with at least the specified length from the pool.

- `public void Return(string array, bool clearArray = true)`

  - Return a string to the pool,
  - when `clearArray` is true (default), the string will be zeroed.

- `public string Rent(ReadOnlySpan<char> buffer)`

  - Rent a string with at least the specified buffer's length from the pool,
  - and copies the content from the buffer.

### `UnsafeString`

- `public bool SetText(ReadOnlySpan<char> buffer)`

  - Rents a new internal buffer if the current buffer is `null`;
  - or resizes the internal buffer if the current buffer's capacity is insufficient.
  - Then copies characters from the provided `ReadOnlySpan<char> buffer` into the internal buffer,
  - returns `true` if a new buffer was rented,
  - returns `false` if no new buffer was rented.

  <br>

  - You can use `public static implicit operator string?(UnsafeString? @string)` to convert an `UnsafeString` to a `string`,
  - the string's `Length` will be equal to `buffer.Length`.

- `public void Dispose()`

  - Resets the internal string and returns it to the pool,
  - then sets the internal string to `null`.

### `Customization`

- You can use `StringPool.Custom` to override:

  - `public static unsafe void Custom(Func<double> getMemoryPressure)`.

    or

  - `public static unsafe void Custom(delegate* managed<double> getMemoryPressure)`.

  to customize how `StringPool` determines memory pressure for its automatic `Trim()` behavior.

---

## Note:

⚠️ The actual length of the string rented from `StringPool`'s`Rent` may not match the requested length.

- If you need the rented string’s actual length to equal the requested length,
- use `UnsafeString`'s `public bool SetText(ReadOnlySpan<char> buffer)`,
- and be sure to call `UnsafeString`'s `public void Dispose()`.

<br>

⚠️ The `string` converted from `UnsafeString`:

- Does not guarantee the same lifetime or content consistency as the `UnsafeString` itself.
- The safest practice is to update all `string`s converted from `UnsafeString` every time you update the `UnsafeString`.