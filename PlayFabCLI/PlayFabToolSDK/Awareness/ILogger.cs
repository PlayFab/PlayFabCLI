using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Awareness
{

    /// <summary>
    /// Interface to be implemented on any type to have named instances
    /// </summary>
    public interface INamedInstance
    {
        string Name { get; }
    }

    /// <summary>
    /// Basic logger interface
    /// </summary>
    public interface ILogger
    {
        void Append(string message);
        void Log(string message, object caller);
        void Verbose(string message, object caller);
        void Debug(string message, object caller);
        void Error(string message, object caller);
    }
}
