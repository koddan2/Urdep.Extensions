# Urdep.Extensions

Collection of useful C# stuff.

## Urdep.Extensions.Augmentation

```csharp
var aug = Augment.C(new MyPoco("hello", 1, true, new object()));
Debug.Assert(aug is IAugmented<MyPoco>, "Should be true");
```

## Urdep.Extensions.Data

```csharp
var aug = Augment.C(new Anything {Name = "Superman"});
var dict = aug.AsDictionary();
Debug.Assert(dict["Name"] == "Superman", "Should be true");
```

```
dotnet nuget push C:\...\.build-output\ -s C:\...\.local-nuget --skip-duplicate
```