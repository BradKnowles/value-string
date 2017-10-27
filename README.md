ValueString allows you to encapsulate an object as a culture-invariant string
and parse it to any type that implements the Parse/TryParse pattern.

It is intended to be used for convenience when there is a need to initialize
typed instances from culture-neutral (invariant) strings - often read from a
simple configuration file or a database table that contains data as string pairs.

```c#
// ValueString.As method uses the invariant culture by default.
// It also has an overload accepting an IFormatProvider.
var value = new ValueString("1.5");
var number = value.As<double>(); // Calls double.Parse.

// Nullable<T> values are supported.
value = new ValueString(null);
number = value.As<double>(); // Throws an InvalidCastException.
var nullable = value.As<double?>(); // null.

// ValueString.Is is just like ValueString.As - the only difference
// being that it calls the type's TryParse method instead of Parse.
value = new ValueString("1.1.1.1");
IPAddress address;
if (value.Is(out address)) // Calls IPAddress.TryParse.
    Console.WriteLine("The IP address is: {0}", address);

// ValueString constructor accepts a System.Object, and uses the invariant
// culture when converting it to a string if it implements IFormattable.
// So the following value contains "1.5" (dot-separated) even if 1.5.ToString()
// returns "1,5" (comma-separated) due to the current culture.
value = new ValueString(1.5);

// An implicit operator exists converting strings to ValueString instances.
value = "1.5";
```

ValueString builds lambda expressions that call the type's original
parsing methods, and cache the compiled delegates for future use.
Therefore they essentially work as fast as the underlying parsing methods themselves.

There are also extension methods for parsing key/ValueString pairs.  
Consider you inject the configuration parameters to your service like below:

```c#
private readonly Uri uri;
private readonly int? timeout;

public SomeService(IReadOnlyDictionary<string, ValueString> config)
{
    // TryGetValue extension finds the value and converts it to the desired type.
    if (!config.TryGetValue("Uri", out this.uri) ||
        !config.TryGetValue("Timeout", out this.timeout))
        {
            // fast fail/throw exception
        }
}
```

This is equal to the following code without ValueString:


```c#
private readonly Uri uri;
private readonly int? timeout;

public SomeService(IReadOnlyDictionary<string, string> config)
{
    var invariant = CultureInfo.InvariantCulture;

    string temp;
    if (!config.TryGetValue("Uri", out temp) ||
        !Uri.TryCreate(temp, UriKind.Absolute, out this.uri) ||
        !config.TryGetValue("Timeout", out temp) ||
        !int.TryParse(temp, NumberStyles.Integer, invariant, out timeout))
        {
            // fast fail/throw exception
        }
}

```

`As` overloads search for a parsing method to cache, in the following order:

1. `static T Parse(string, IFormatProvider)` method
2. `static T Parse(string)` method
3. `static bool TryParse(string, IFormatProvider, out T)` method
4. `static bool TryParse(string, NumberStyles, IFormatProvider, out T)` method
5. `static bool TryParse(string, IFormatProvider, DateTimeStyles, out T)` method
6. `static bool TryParse(string, out T)` method
7. [TypeConverterAttribute][2] via `TypeDescriptor.GetConverter`
8. `T(string)` constructor

`Is` overloads search for a parsing method to cache, in the following order:

1. `static bool TryParse(string, IFormatProvider, out T)` method
2. `static bool TryParse(string, NumberStyles, IFormatProvider, out T)` method
3. `static bool TryParse(string, IFormatProvider, DateTimeStyles, out T)` method
4. `static bool TryParse(string, out T)` method
5. `static T Parse(string, IFormatProvider)` method
6. `static T Parse(string)` method
7. [TypeConverterAttribute][2] via `TypeDescriptor.GetConverter`
8. `T(string)` constructor

| Method family    | Parsing method not found or parsing method failed         |
| ---------------- | --------------------------------------------------------- |
| `ValueString.As` | Throws an `InvalidCastException`                          |
| `ValueString.Is` | Returns `false` (TryParse methods are assumed to be safe).|

See [the unit tests][1] for details.

[1]: tests/ValueStringTests.cs
[2]: https://docs.microsoft.com/dotnet/api/system.componentmodel.typeconverterattribute