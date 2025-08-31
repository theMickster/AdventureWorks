using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Data.SqlClient;

namespace AdventureWorks.DbReset.Console.Tests.Verbs.Handlers;

/// <summary>
/// Test-only factory for fabricating <see cref="SqlException"/> instances. Required because
/// neither <see cref="SqlException"/> nor <see cref="SqlError"/> exposes a public constructor.
/// We invoke the internal <c>SqlException.CreateException(SqlErrorCollection, string)</c> via
/// reflection — the same pattern long used in dotnet/SqlClient's own tests.
/// </summary>
internal static class SqlExceptionFactory
{
    /// <summary>
    /// Creates a <see cref="SqlException"/> whose <see cref="SqlException.Number"/> is
    /// <paramref name="number"/> and whose <see cref="Exception.Message"/> is
    /// <paramref name="message"/>. Class is fixed at 11 (user-actionable error severity); other
    /// detail fields are stubbed.
    /// </summary>
    public static SqlException Create(int number, string message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var error = BuildError(number, message);
        var collection = BuildCollection(error);
        return BuildException(collection);
    }

    private static SqlError BuildError(int number, string message)
    {
        // SqlError exposes several internal constructors across SqlClient versions; try the
        // most-specific shapes first and fall back. Each shape produces an equivalent SqlError
        // for our test purposes (Number + Message are the only fields we assert on).
        var candidates = new (Type[] Sig, object?[] Args)[]
        {
            // (int, byte, byte, string, string, string, int, uint, Exception) — Microsoft.Data.SqlClient with win32 code as uint.
            (new[] { typeof(int), typeof(byte), typeof(byte), typeof(string), typeof(string), typeof(string), typeof(int), typeof(uint), typeof(Exception) },
             new object?[] { number, (byte)0, (byte)11, "test-server", message, "test-proc", 0, (uint)0, null }),
            // (int, byte, byte, string, string, string, int, int, Exception) — int win32 code variant.
            (new[] { typeof(int), typeof(byte), typeof(byte), typeof(string), typeof(string), typeof(string), typeof(int), typeof(int), typeof(Exception) },
             new object?[] { number, (byte)0, (byte)11, "test-server", message, "test-proc", 0, 0, null }),
            // (int, byte, byte, string, string, string, int, Exception) — no win32 code.
            (new[] { typeof(int), typeof(byte), typeof(byte), typeof(string), typeof(string), typeof(string), typeof(int), typeof(Exception) },
             new object?[] { number, (byte)0, (byte)11, "test-server", message, "test-proc", 0, null }),
            // (int, byte, byte, string, string, string, int, uint) — older shape without inner exception.
            (new[] { typeof(int), typeof(byte), typeof(byte), typeof(string), typeof(string), typeof(string), typeof(int), typeof(uint) },
             new object?[] { number, (byte)0, (byte)11, "test-server", message, "test-proc", 0, (uint)0 }),
            // (int, byte, byte, string, string, string, int) — minimal.
            (new[] { typeof(int), typeof(byte), typeof(byte), typeof(string), typeof(string), typeof(string), typeof(int) },
             new object?[] { number, (byte)0, (byte)11, "test-server", message, "test-proc", 0 }),
        };

        foreach (var (sig, args) in candidates)
        {
            var ctor = typeof(SqlError).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                binder: null,
                sig,
                modifiers: null);
            if (ctor is not null)
            {
                return (SqlError)ctor.Invoke(args);
            }
        }

        throw new InvalidOperationException(
            "No usable SqlError constructor found via reflection. The Microsoft.Data.SqlClient version in use does not match any known shape.");
    }

    private static SqlErrorCollection BuildCollection(SqlError error)
    {
        var collection = (SqlErrorCollection)RuntimeHelpers.GetUninitializedObject(typeof(SqlErrorCollection));
        var addMethod = typeof(SqlErrorCollection).GetMethod(
            "Add",
            BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            new[] { typeof(SqlError) },
            modifiers: null)
            ?? throw new InvalidOperationException("SqlErrorCollection.Add(SqlError) not found.");

        // The collection backs onto a private List<object>; SqlClient's parameterless ctor
        // initializes it. GetUninitializedObject skips that, so we set the field manually if
        // present, otherwise let Add() lazily initialize. Some builds use a List<SqlError>
        // private field named "errors".
        var listField = typeof(SqlErrorCollection).GetField(
            "_errors",
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? typeof(SqlErrorCollection).GetField(
                "errors",
                BindingFlags.NonPublic | BindingFlags.Instance);

        if (listField is not null && listField.GetValue(collection) is null)
        {
            listField.SetValue(collection, Activator.CreateInstance(listField.FieldType));
        }

        addMethod.Invoke(collection, new object?[] { error });
        return collection;
    }

    /// <summary>
    /// Creates a <see cref="SqlException"/> with <see cref="SqlException.Number"/> set to
    /// <paramref name="number"/> and a non-null <see cref="Exception.InnerException"/>. Used to
    /// exercise the <c>Number==0 &amp;&amp; InnerException is not null</c> transport-failure branch.
    /// </summary>
    public static SqlException CreateWithInner(int number, string message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var error = BuildError(number, message);
        var collection = BuildCollection(error);
        var ex = BuildException(collection);

        var field = typeof(Exception).GetField(
            "_innerException",
            BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException("Exception._innerException field not found.");

        field.SetValue(ex, new InvalidOperationException("transport failure"));
        return ex;
    }

    private static SqlException BuildException(SqlErrorCollection collection)
    {
        // CreateException(SqlErrorCollection errorCollection, string serverVersion)
        var create = typeof(SqlException).GetMethod(
            "CreateException",
            BindingFlags.NonPublic | BindingFlags.Static,
            binder: null,
            new[] { typeof(SqlErrorCollection), typeof(string) },
            modifiers: null)
            ?? throw new InvalidOperationException("SqlException.CreateException(SqlErrorCollection, string) not found.");

        return (SqlException)create.Invoke(null, new object?[] { collection, "11.0.0" })!;
    }
}
