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

- `public string Rent(int minimumLength)`

  - Rent a string with at least the specified length from the pool.

- `public void Return(string array, bool clearArray = true)`

  - Return a string to the pool,
  - When `clearArray` is true (default), the string will be zeroed.

- `public string Rent(ReadOnlySpan<char> buffer)`

  - Rent a string with at least the specified buffer's length from the pool,
  - And copies the content from the buffer.

- You can use `StringPool.Custom` to override:

  - `public static unsafe void Custom(Func<double> getMemoryPressure)`.

    or

  - `public static unsafe void Custom(delegate* managed<double> getMemoryPressure)`.

  to customize how `StringPool` determines memory pressure for its automatic `Trim()` behavior.