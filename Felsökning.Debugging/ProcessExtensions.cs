//-----------------------------------------------------------------------
// <copyright file="ProcessExtensions.cs" company="Felsökning">
//     Copyright (c) Felsökning. All rights reserved.
// </copyright>
// <author>John Bailey</author>
//-----------------------------------------------------------------------
namespace Felsökning.Debugging
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ProcessExtensions"/> class.
    /// </summary>
    public static class ProcessExtensions
    {
        /// <summary>
        ///     Extends the <see cref="Process"/> class to leverage <see cref="Microsoft.Diagnostics.Runtime"/> (a.k.a.: ClrMD) and returns the frames for all threads in a given (managed) process.
        ///     <para>NOTE: Your target architecture must match the architecture of the process being debugged.</para>
        /// </summary>
        /// <param name="process">The current process context.</param>
        /// <param name="outString">The string containing the threads.</param>
        /// <returns>A boolean indicating success (based on if any CLR Versions were found).</returns>
        public static bool DumpProcessThreads(this Process process, out string outString)
        {
            bool succeeded = false;
            StringBuilder threads = new StringBuilder();
            using (DataTarget target = DataTarget.AttachToProcess(process.Id, true))
            {
                target.CacheOptions.CacheMethods = true;
                target.CacheOptions.CacheMethodNames = StringCaching.Cache;
                target.CacheOptions.CacheStackRoots = true;
                target.CacheOptions.CacheStackTraces = true;
                target.CacheOptions.CacheTypes = true;
                target.CacheOptions.UseOSMemoryFeatures = true;

                if (target.ClrVersions.Any())
                {
                    // Set the symbol file path, so we can debug with some sources.
                    target.SetSymbolPath("SRV*https://msdl.microsoft.com/download/symbols");

                    // Use the first CLR Runtime available due to SxS.
                    ClrInfo? clrInfo = target.ClrVersions.SingleOrDefault();
                    if (clrInfo != null) 
                    {
                        ClrRuntime clrRuntime = clrInfo.CreateRuntime();
                        foreach (ClrThread thread in clrRuntime.Threads)
                        {
                            if (!thread.IsAlive)
                            {
                                continue;
                            }

                            var clrStackFrames = thread.EnumerateStackTrace(true);
                            if (clrStackFrames.Any())
                            {
                                threads.AppendLine(string.Format(format: "{0:X}", arg0: thread.OSThreadId));
                                foreach (ClrStackFrame frame in clrStackFrames)
                                {
                                    threads.AppendLine(
                                        string.Format(
                                        "{0,12:x} {1,12:x} {2} {3}",
                                        frame.StackPointer,
                                        frame.InstructionPointer,
                                        frame.Method,
                                        frame));
                                }
                            }
                        }
                    }

                    succeeded = true;
                }
                else
                {
                    threads.AppendLine($"No CLR Versions found for {process.ProcessName}. Process is most likely native.");
                }
            }

            outString = threads.ToString();
            return succeeded;
        }
    }
}