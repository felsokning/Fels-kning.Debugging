//-----------------------------------------------------------------------
// <copyright file="ProcessExtensionsTests.cs" company="Felsökning">
//     Copyright (c) Felsökning. All rights reserved.
// </copyright>
// <author>John Bailey</author>
//-----------------------------------------------------------------------
namespace Felsökning.Debugging.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ProcessExtensionsTests
    {
        [TestMethod]
        public void DumpProcessThreads_Handles_Native()
        {
            var processStartInfo = new ProcessStartInfo();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                processStartInfo.FileName = "notepad.exe";
                processStartInfo.UseShellExecute = false;
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                processStartInfo.FileName = "top";
                processStartInfo.UseShellExecute = false;
            }

            var sut = Process.Start(processStartInfo);

            var clrWasFound = sut!.DumpProcessThreads(out string threads);
            sut!.Kill();
            sut!.Dispose();

            clrWasFound.Should().BeFalse();
            threads.Should().NotBeNullOrWhiteSpace();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                threads.Should().Contain("No CLR Versions found for Notepad. Process is most likely native.");
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                threads.Should().Contain("No CLR Versions found for top. Process is most likely native.");
            }
        }

        [TestMethod]
        public async Task DumpProcessThreads_Handles_Net()
        {
            var processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = "dotnet";
            processStartInfo.Arguments = "DotNet.Docker.dll";
            processStartInfo.UseShellExecute = false;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processStartInfo.RedirectStandardOutput = false;
            processStartInfo.RedirectStandardError = false;
            processStartInfo.RedirectStandardInput = false;

            var sut = Process.Start(processStartInfo);

            // Sleep required due to JIT'ing timing[s].
            await Task.Delay(TimeSpan.FromSeconds(1));

            var clrWasFound = sut!.DumpProcessThreads(out string threads);
            sut!.Kill();
            sut!.Dispose();

            clrWasFound.Should().BeTrue();
            threads.Should().NotBeNullOrWhiteSpace();
        }
    }
}