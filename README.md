![Licensed under the MIT License](https://img.shields.io/github/license/teo-tsirpanis/recursiont.svg)
[![NuGet](https://img.shields.io/nuget/v/Recursiont.svg)](https://nuget.org/packages/Recursiont)
[![Test](https://github.com/teo-tsirpanis/Recursiont/actions/workflows/ci.yml/badge.svg?branch=mainstream&event=push)](https://github.com/teo-tsirpanis/Recursiont/actions/workflows/ci.yml)

# Recursion't

Recursion't is a C# library that allows efficiently implementing recursive algorithms without worrying for stack overflows. It achieves this by creatively using `async` method builders.

## Features

* ~~__Lightweight until you need it.__ If recursion in a function does not go too deep, Recursion't will have minimal overhead.~~ Initial benchmarks have shown that on simple cases Recursion't's overhead is significant. Releasing the library on NuGet is delayed until they are resolved or it is realized that on real-world cases the overhead is negligible.
* __Single-threaded.__ A common way to tackle infinite recursion is by keeping recursing in a different thread. Recursion't runs entirely in one thread.
* __Memory-efficient.__ Recursion't uses techniques like object pooling to keep steady-state memory allocations low.

## How to use

You can convert a recursive function to use Recursion't by following these steps:

1. Add `using Recursiont;` to your code.
2. Make the function `async`.
3. Change the function's return type to `RecursiveOp` if it returns `void`, or to `RecursiveOp<T>` if it returns `T`.
4. `await` the recursive calls.
5. Create a helper function that uses `RecursiveRunner.Run` that serves as the entry point for your recursive function.

To see an example, imagine the following recursive function that computes the [Ackermann function](https://en.wikipedia.org/wiki/Ackermann_function):

```csharp
static uint Ackermann(uint m, uint n)
{
    if (m == 0)
    {
        return n + 1;
    }
    if (n == 0)
    {
        return Ackermann(m - 1, 1);
    }
    return Ackermann(m - 1, Ackermann(m, n - 1));
}
```

If you call `Ackermann(4, 1)` your code will crash with a stack overflow. Here's the same function, rewritten to use Recursion't:

```csharp
using Recursiont;

static uint Ackermann(uint m, uint n)
{
    return RecursiveRunner.Run(AckermannImpl, m, n);

    static async RecursiveOp<uint> AckermannImpl(uint m, uint n)
    {
        if (m == 0)
        {
            return n + 1;
        }
        if (n == 0)
        {
            return await AckermannImpl(m - 1, 1);
        }
        return await AckermannImpl(m - 1, await AckermannImpl(m, n - 1));
    }
}
```

Now, calling `Ackermann(4, 1)` will not cause stack overflows but will still take an extraordinary amount of time to complete.

`RecursiveRunner.Run` has overloads that accept functions with up to six parameters. If your function has more you can create a lambda that accepts a tuple:

```csharp
RecursiveRunner.Run(static ((int X1, int X2, int X3, int X4, int X5, int X6, int X7) x) =>
    F(x.X1, x.X2, x.X3, x.X4, x.X5, x.X6, x.X7), (0, 0, 0, 0, 0, 0, 0));

static async RecursiveOp F(int x1, int x2, int x3, int x4, int x5, int x6, int x7)
{
    // ...
}
```

## The rules

Using Recursion't comes with a set of simple rules that you have to follow.

> The term "recursive functions" refers to methods that return `RecursiveOp` or `RecursiveOp<T>`.

*
    __Immediately `await` after calling recursive functions.__ Recursion't is not a coroutine library. The following code might either fail or produce unexpected results:

    ```csharp
    static async RecursiveOp Foo()
    {
        RecursiveOp op1 = Bar();
        RecursiveOp op2 = Bar();
        await op1;
        await op2;
    }
    ```

*
    __Do not call recursive functions outside of other recursive functions.__ The only way to call a recursive function from a non-recursive one is by passing a delegate to it in `RecursiveRunner.Run`. Consider the following code:

    ```csharp
    // ❌ Wrong
    F();
    // ✅ Correct
    RecursiveRunner.Run(F);

    static async RecursiveOp F()
    {
        // ...
    }
    ```

*
    __Exercise caution when using `RecursiveRunner.Run` inside recursive functions.__ Let's say you have the following two functions:

    ```csharp
    static void A()
    {
        RecursiveRunner.Run(AImpl);

        static async RecursiveOp AImpl()
        {
            // ...
        }
    }

    static void B()
    {
        RecursiveRunner.Run(BImpl);

        static async RecursiveOp BImpl()
        {
            // ...
            A();
            // ...
        }
    }
    ```

    `B` is inefficient and -if `A` and `B` are mutually recursive- downright dangerous and prone to stack overflows. The way Recursion't avoids stack overflows is by storing the stack to the heap when it goes too deep, up to the last point where `RecursiveRunner.Run` was called. Using it in recursive functions limits how many stack frames can be moved to the heap.

    What you can do is to make `A()` directly return `RecursiveOp` and call and `await` it from B.

    ```csharp
    static async RecursiveOp A()
    {
        // ...
    }

    static void B()
    {
        RecursiveRunner.Run(BImpl);

        static async RecursiveOp BImpl()
        {
            // ...
            await A();
            // ...
        }
    }
    ```

    > Unless you are doing things like recursive user callbacks, you don't have to expose recursive functions in a public API. You can create a public wrapper function that calls `RecursiveRunner.Run` and keep Recursion't as an implementation detail.

## Maintainer(s)

- [@teo-tsirpanis](https://github.com/teo-tsirpanis)
