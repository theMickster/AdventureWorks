using Microsoft.Win32.SafeHandles;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace AdventureWorks.Test.Common.Setup;

/// <summary>
/// Base class for all tests. 
/// All from another abstract class in this class's hierarchy.
/// </summary>
[ExcludeFromCodeCoverage]
public abstract class TestBase : IDisposable
{
    #region Private Fields 

    // Used to detect redundant calls to the Dispose
    private bool _disposed = false;
    private readonly SafeHandle _safeHandle = new SafeFileHandle(IntPtr.Zero, true);

    #endregion Private Fields 

    #region Public Properties

    public DateTime TestRunDate => new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 1, 11, 11, 11);

    #endregion Public Properties

    #region Public Methods 

    /// <summary>
    /// Created if/when a particular test class needs to add functionality to dispose.
    /// </summary>
    public virtual void ConcreteDispose()
    {

    }

    public virtual void Dispose()
    {
        ConcreteDispose();
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Used to create objects before a test is executed.
    /// </summary>
    protected virtual void Setup()
    {

    }

    /// <summary>
    /// Used to clean-up objects after a test is executed.
    /// </summary>
    public virtual void Teardown()
    {

    }

    #endregion Public Methods 

    #region Protected Methods 

    /// <summary>
    /// Dispose of everything created during an integration test.
    /// </summary>
    /// <remarks>
    /// Do not override or hide this method in child-classes unless you've got a darn good reason to do so.
    /// </remarks>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            ConcreteDispose();
            Teardown();
            _safeHandle?.Dispose();
        }

        _disposed = true;
    }

    #endregion Protected Methods 
}
